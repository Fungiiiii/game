# Generates the Mystic Forest low poly models and exports them as FBX.
#
# Run headless from the repository root:
#   blender -b -P tools/blender/generate_forest_models.py
#
# Writes into Assets/Art/Models/Environment/:
#   TreeBroad.fbx    warm broadleaf, 14 to 18 m, canopy from 6 m (spawn zone)
#   TreePine.fbx     tiered conifer, 20 to 24 m (dense zone)
#   TreeGiant.fbx    unique landmark, ~29 m: a massive trunk on root fins that
#                    splits into three crowned limbs
#   Rock_Boulder.fbx landmark boulder, ~3.5 m wide, moss on top
#   Rock_Spire.fbx   standing stone, ~5 m tall
#   GrassTuft.fbx    a tuft of blades, ~0.5 m, drawn as Terrain detail
#   Bush.fbx         low bush, ~1.3 m
#   Fern.fbx         arching fern, ~0.5 m
#   WaterFern.fbx    the water fern of the mist: six wide open fronds, lighter
#                    than the decor fern, ~0.4 m high and ~1.2 m across. Full of
#                    water, the game darkens it and makes drops fall from it.
#   MushroomCluster.fbx  four mushrooms, red and violet caps, ~0.6 m
#   MushroomCluster_02.fbx  three red caps, one tall and two small, ~0.7 m
#   MushroomCluster_03.fbx  five low violet caps spread wide, ~0.35 m
#   MushroomCluster_04.fbx  one large red cap with a small violet one, ~0.85 m
#
# Pass model names after "--" to export only those, for instance:
#   blender -b -P tools/blender/generate_forest_models.py -- MushroomCluster_02
#
# Every model follows what was measured on Tree.fbx: origin at the foot, Z up in
# Blender, 1 unit = 1 m, one material named M_Fungi_Palette (remapped to
# Palette.mat by each .fbx.meta), flat faces, and UVs pointing at the centre of
# a Palette.png cell. Cells are (row, col) as in tools/palette/generate_palette.py.
# Export settings match the table in docs/art-pipeline.md.
#
# Deterministic: every jitter comes from a seeded random.Random, so running the
# script twice gives the same meshes.
import math
import os
import random
import sys

import bmesh
import bpy
from mathutils import Vector

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(REPO, "Assets", "Art", "Models", "Environment")
MATERIAL_NAME = "M_Fungi_Palette"

# Palette cells (row, col).
WOOD = (3, 2)
WOOD_DARK = (3, 3)
GREEN = (3, 0)
GREEN_DARK = (3, 1)
ROCK_LIGHT = (4, 0)
ROCK_BASE = (4, 1)
ROCK_SHADOW = (4, 2)
LEAF_WARM = (6, 0)
LEAF_WARM_SHADE = (6, 1)
PINE = (6, 2)
PINE_SHADE = (6, 3)
BARK_PALE = (6, 4)
GIANT_LEAF = (6, 5)
GRASS = (6, 6)
GRASS_SHADE = (6, 7)
GRASS_TIP = (7, 1)
MUSHROOM_VIOLET = (7, 0)
MUSHROOM_RED = (2, 0)
BODY_LIGHT = (0, 0)
BODY_BASE = (0, 1)
SPOTS = (1, 1)

GRID = 8


def cell_uv(cell):
    row, col = cell
    return ((col + 0.5) / GRID, 1.0 - (row + 0.5) / GRID)


class MeshBuilder:
    """Collects faces with their palette cell, then builds one flat-shaded mesh."""

    def __init__(self):
        self.verts = []
        self.faces = []  # (vertex indices, cell)

    def vert(self, co):
        self.verts.append(Vector(co))
        return len(self.verts) - 1

    def face(self, indices, cell):
        self.faces.append((list(indices), cell))

    def outward_face(self, indices, cell, inside):
        """A face of a closed shape, wound so it faces away from a point inside
        the shape. Unity draws one side only: a face wound the wrong way is
        invisible from outside, and the model looks hollow."""
        a, b, c = (self.verts[i] for i in indices[:3])
        normal = (b - a).cross(c - a)
        centre = sum((self.verts[i] for i in indices), Vector()) / len(indices)
        if normal.dot(centre - Vector(inside)) < 0:
            indices = list(reversed(indices))
        self.face(indices, cell)

    def ring(self, centre, radius, sides, rng=None, jitter=0.0, phase=0.0, axis=None, squash=1.0):
        """A ring of vertices around centre, perpendicular to axis (default Z)."""
        centre = Vector(centre)
        axis = Vector(axis or (0, 0, 1)).normalized()
        ref = Vector((1, 0, 0)) if abs(axis.x) < 0.9 else Vector((0, 1, 0))
        u = axis.cross(ref).normalized()
        v = axis.cross(u).normalized()
        out = []
        for i in range(sides):
            a = phase + 2 * math.pi * i / sides
            r = radius * (1 + (rng.uniform(-jitter, jitter) if rng else 0))
            p = centre + (u * math.cos(a) + v * math.sin(a) * squash) * r
            out.append(self.vert(p))
        return out

    def bridge(self, lower, upper, cell):
        n = len(lower)
        for i in range(n):
            j = (i + 1) % n
            self.face([lower[i], lower[j], upper[j], upper[i]], cell)

    def cap(self, ring, centre, cell, up=True):
        c = self.vert(centre)
        n = len(ring)
        for i in range(n):
            j = (i + 1) % n
            self.face([ring[i], ring[j], c] if up else [ring[j], ring[i], c], cell)

    def tube(self, points, radii, sides, cell, rng, jitter=0.05, close_top=True, close_bottom=False):
        """A tapered tube through a list of points, one ring per point."""
        rings = []
        for k, (p, r) in enumerate(zip(points, radii)):
            if k == 0:
                axis = Vector(points[1]) - Vector(points[0])
            elif k == len(points) - 1:
                axis = Vector(points[-1]) - Vector(points[-2])
            else:
                axis = Vector(points[k + 1]) - Vector(points[k - 1])
            rings.append(self.ring(p, r, sides, rng, jitter, axis=axis))
        for a, b in zip(rings, rings[1:]):
            self.bridge(a, b, cell)
        if close_top:
            self.cap(rings[-1], points[-1], cell, up=True)
        if close_bottom:
            self.cap(rings[0], points[0], cell, up=False)
        return rings

    def blob(self, centre, radii, cell_top, cell_bottom, rng, subdivisions=2, jitter=0.12):
        """A faceted ellipsoid: an icosphere scaled and jittered. In bmesh,
        subdivisions=1 is the bare icosahedron (20 faces), 2 gives 80 faces."""
        bm = bmesh.new()
        bmesh.ops.create_icosphere(bm, subdivisions=subdivisions, radius=1.0)
        base = len(self.verts)
        for v in bm.verts:
            co = Vector((v.co.x * radii[0], v.co.y * radii[1], v.co.z * radii[2]))
            co *= 1 + rng.uniform(-jitter, jitter)
            self.verts.append(Vector(centre) + co)
        for f in bm.faces:
            normal_z = f.normal.z
            self.face([base + v.index for v in f.verts], cell_top if normal_z > -0.2 else cell_bottom)
        bm.free()

    def build(self, name, up_normals=False):
        mesh = bpy.data.meshes.new(name)
        mesh.from_pydata([tuple(v) for v in self.verts], [], [f for f, _ in self.faces])
        mesh.update()
        uv_layer = mesh.uv_layers.new(name="UVMap")
        for poly, (_, cell) in zip(mesh.polygons, self.faces):
            uv = cell_uv(cell)
            for li in poly.loop_indices:
                uv_layer.data[li].uv = uv
            poly.use_smooth = False
        mesh.validate()
        if up_normals:
            # Every normal straight up: the model is lit like the ground under it,
            # without a mosaic of light and dark facets.
            mesh.normals_split_custom_set([(0.0, 0.0, 1.0)] * len(mesh.loops))
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.scene.collection.objects.link(obj)
        obj.data.materials.append(palette_material())
        return obj


def palette_material():
    mat = bpy.data.materials.get(MATERIAL_NAME)
    if mat is None:
        mat = bpy.data.materials.new(MATERIAL_NAME)
    return mat


def tree_broad():
    rng = random.Random(1201)
    m = MeshBuilder()
    # Trunk with a root flare, leaning a little, then three uneven limbs carrying
    # the canopy. Nothing is symmetric, so the random rotation of each instance
    # reads as a different tree.
    m.tube([(0, 0, 0), (0, 0, 0.7), (0.15, 0.05, 4.0), (0.4, 0.15, 7.0)],
           [0.95, 0.62, 0.5, 0.42], 6, WOOD, rng, close_top=True)
    limbs = []
    for a, length, height in ((0.3, 3.6, 9.0), (2.25, 2.4, 10.6), (4.3, 3.0, 9.5)):
        end = (0.3 + math.cos(a) * length, 0.1 + math.sin(a) * length, height)
        m.tube([(0.3, 0.1, 5.6), end], [0.32, 0.18], 5, WOOD, rng)
        limbs.append(end)
    # Canopy: three big blobs of different sizes on the limbs, small ones filling the gaps.
    for k, (end, radii) in enumerate(zip(limbs, ((3.9, 3.5, 2.8), (3.0, 2.8, 2.4), (3.5, 3.3, 2.7)))):
        top, bottom = (LEAF_WARM, LEAF_WARM_SHADE) if k != 1 else (LEAF_WARM_SHADE, LEAF_WARM_SHADE)
        m.blob((end[0], end[1], end[2] + 1.2), radii, top, bottom, rng)
    for k in range(3):
        a = 2 * math.pi * k / 3 + 1.45
        m.blob((math.cos(a) * 2.2, math.sin(a) * 2.2, 12.6 + k * 0.5), (2.4, 2.4, 2.0),
               LEAF_WARM, LEAF_WARM_SHADE, rng, subdivisions=1, jitter=0.08)
    m.blob((0.2, 0.0, 14.4), (2.6, 2.6, 1.8), LEAF_WARM, LEAF_WARM_SHADE, rng, subdivisions=1)
    return m.build("TreeBroad")


def pine_tier(m, z, radius, height, sides, rng, phase):
    """One drooping tier of a conifer: apex, shoulder ring, drooping rim, underside."""
    apex = m.vert((rng.uniform(-0.1, 0.1), rng.uniform(-0.1, 0.1), z + height))
    shoulder = m.ring((0, 0, z + height * 0.25), radius * 0.72, sides, rng, 0.08, phase)
    rim = m.ring((0, 0, z), radius, sides, rng, 0.12, phase + math.pi / sides)
    under = m.ring((0, 0, z + height * 0.12), radius * 0.28, sides, rng, 0.05, phase)
    n = sides
    for i in range(n):
        j = (i + 1) % n
        m.face([shoulder[i], shoulder[j], apex], PINE)
    m.bridge(rim, shoulder, PINE)
    m.bridge(under, rim, PINE_SHADE)
    m.cap(under, (0, 0, z + height * 0.2), PINE_SHADE, up=False)


def tree_pine():
    rng = random.Random(2203)
    m = MeshBuilder()
    m.tube([(0, 0, 0), (0, 0, 0.6), (0, 0, 12.0), (0, 0, 21.0)],
           [0.75, 0.5, 0.36, 0.14], 6, WOOD_DARK, rng, jitter=0.03)
    tiers = [(4.0, 4.6, 4.2), (7.3, 4.0, 3.9), (10.4, 3.4, 3.6), (13.3, 2.8, 3.3),
             (16.0, 2.1, 3.0), (18.6, 1.4, 3.4)]
    for k, (z, radius, height) in enumerate(tiers):
        pine_tier(m, z, radius, height, 9, rng, phase=k * 0.7)
    return m.build("TreePine")


def pad(m, centre, radius, thickness, sides, rng, cell_top, cell_side):
    """A flat, stacked canopy pad: a squat faceted disc."""
    cx, cy, cz = centre
    top = m.ring((cx, cy, cz + thickness * 0.5), radius * 0.78, sides, rng, 0.1)
    rim = m.ring((cx, cy, cz), radius, sides, rng, 0.14, math.pi / sides)
    bottom = m.ring((cx, cy, cz - thickness * 0.5), radius * 0.62, sides, rng, 0.1)
    m.cap(top, (cx, cy, cz + thickness * 0.65), cell_top, up=True)
    m.bridge(rim, top, cell_top)
    m.bridge(bottom, rim, cell_side)
    m.cap(bottom, (cx, cy, cz - thickness * 0.55), cell_side, up=False)


def buttress(m, angle, rng):
    """A flat root fin: a wedge from the trunk foot out onto the ground."""
    dx, dy = math.cos(angle), math.sin(angle)
    px, py = -dy * 0.22, dx * 0.22  # half thickness, sideways
    reach = 4.0 + rng.uniform(-0.4, 0.5)
    height = 3.6 + rng.uniform(-0.5, 0.6)
    pts = [(dx * 1.5, dy * 1.5, 0.0), (dx * reach, dy * reach, 0.0), (dx * 1.5, dy * 1.5, height)]
    left = [m.vert((x + px, y + py, z)) for x, y, z in pts]
    right = [m.vert((x - px, y - py, z)) for x, y, z in pts]
    inside = (dx * (1.5 + reach) / 3, dy * (1.5 + reach) / 3, height / 3)
    m.outward_face([left[0], left[1], left[2]], BARK_PALE, inside)
    m.outward_face([right[0], right[1], right[2]], BARK_PALE, inside)
    m.outward_face([left[1], right[1], right[2], left[2]], BARK_PALE, inside)  # sloped back
    m.outward_face([left[0], left[2], right[2], right[0]], BARK_PALE, inside)  # against the trunk
    m.outward_face([left[0], right[0], right[1], left[1]], BARK_PALE, inside)  # under the ground


def tree_giant():
    rng = random.Random(3307)
    m = MeshBuilder()
    # Same reading as Tree.fbx: one straight trunk, the crown high up. Only
    # bigger: a massive trunk held by root fins, splitting into three limbs at
    # 19 m, each carrying a wide flat crown.
    m.tube([(0, 0, 0), (0, 0, 1.4), (0.2, 0, 9.0), (-0.1, 0.1, 16.5), (0, 0, 19.5)],
           [2.5, 1.9, 1.6, 1.35, 1.25], 8, BARK_PALE, rng, jitter=0.05)
    for k in range(5):
        buttress(m, 2 * math.pi * k / 5 + rng.uniform(-0.2, 0.2), rng)
    for k in range(3):
        a = 2 * math.pi * k / 3 + math.pi / 6
        dx, dy = math.cos(a), math.sin(a)
        m.tube([(dx * 0.3, dy * 0.3, 18.8), (dx * 2.4, dy * 2.4, 22.6), (dx * 5.2, dy * 5.2, 25.0)],
               [0.95, 0.62, 0.35], 6, BARK_PALE, rng)
        pad(m, (dx * 5.6, dy * 5.6, 25.8), 5.4, 2.0, 11, rng, GIANT_LEAF, GREEN_DARK)
        pad(m, (dx * 4.8 + dy * 0.8, dy * 4.8 - dx * 0.8, 27.6), 3.3, 1.4, 9, rng, GIANT_LEAF, GREEN_DARK)
    return m.build("TreeGiant")


def rock_boulder():
    rng = random.Random(4409)
    m = MeshBuilder()
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=2, radius=1.0)
    base = len(m.verts)
    for v in bm.verts:
        co = Vector((v.co.x * 1.9, v.co.y * 1.6, v.co.z * 1.35))
        co *= 1 + rng.uniform(-0.16, 0.16)
        co.z = max(co.z + 0.9, 0.0)  # sit on the ground, flat bottom
        m.verts.append(co)
    for f in bm.faces:
        nz = f.normal.z
        cell = GREEN if nz > 0.75 else ROCK_BASE if nz > -0.1 else ROCK_SHADOW
        m.face([base + v.index for v in f.verts], cell)
    bm.free()
    return m.build("Rock_Boulder")


def rock_spire():
    rng = random.Random(5501)
    m = MeshBuilder()
    rings = []
    for z, r in [(0, 1.15), (1.6, 0.95), (3.4, 0.78), (4.6, 0.45)]:
        rings.append(m.ring((rng.uniform(-0.1, 0.1), rng.uniform(-0.1, 0.1), z), r, 6, rng, 0.14))
    cells = [ROCK_SHADOW, ROCK_BASE, ROCK_LIGHT]
    for k, (a, b) in enumerate(zip(rings, rings[1:])):
        m.bridge(a, b, cells[k])
    m.cap(rings[-1], (0.1, 0.0, 5.2), GREEN, up=True)
    return m.build("Rock_Spire")


def grass_tuft():
    """A soft tuft, as in stylized games: twelve flat blades curving outward,
    each in three tones from a foot the colour of the moss to a light tip, and
    every normal pointing up so the tuft is lit like the ground. Each blade has
    a front and a back a few millimetres apart: solid from both sides."""
    rng = random.Random(6101)
    m = MeshBuilder()
    rows = (0.0, 0.4, 0.75, 1.0)
    widths = (1.0, 0.8, 0.45, 0.0)
    cells = (GRASS_SHADE, GRASS, GRASS_TIP)
    for k in range(12):
        a = 2 * math.pi * k / 12 + rng.uniform(-0.25, 0.25)
        dx, dy = math.cos(a), math.sin(a)
        out = Vector((dx, dy, 0.0))
        side = Vector((-dy, dx, 0.0))
        root = out * rng.uniform(0.0, 0.1)
        height = rng.uniform(0.22, 0.45)
        bend = rng.uniform(0.08, 0.2)
        width = rng.uniform(0.05, 0.08)
        for skin in (0.004, -0.004):
            left, right = [], []
            for t, w in zip(rows, widths):
                centre = root + out * (bend * t * t + skin) + Vector((0.0, 0.0, height * t))
                left.append(m.vert(centre + side * width * w * 0.5))
                right.append(m.vert(centre - side * width * w * 0.5))
            # One winding for the whole skin: the front faces outward, the back inward.
            normal = (m.verts[right[0]] - m.verts[left[0]]).cross(m.verts[left[1]] - m.verts[left[0]])
            flip = (normal.dot(out) > 0) != (skin > 0)
            for i, cell in enumerate(cells):
                if i < 2:
                    face = [left[i], right[i], right[i + 1], left[i + 1]]
                else:
                    face = [left[i], right[i], right[i + 1]]
                m.face(list(reversed(face)) if flip else face, cell)
    return m.build("GrassTuft", up_normals=True)


def bush():
    rng = random.Random(6203)
    m = MeshBuilder()
    m.blob((0.0, 0.0, 0.7), (1.0, 0.95, 0.75), GREEN, GREEN_DARK, rng)
    m.blob((0.75, 0.25, 0.45), (0.6, 0.55, 0.5), GREEN, GREEN_DARK, rng, subdivisions=1)
    m.blob((-0.6, -0.4, 0.4), (0.55, 0.6, 0.45), GREEN_DARK, GREEN_DARK, rng, subdivisions=1)
    return m.build("Bush")


def fern():
    """Seven fronds, each a flat closed diamond with a raised rib: solid from
    above and below."""
    rng = random.Random(6307)
    m = MeshBuilder()
    for k in range(7):
        a = 2 * math.pi * k / 7 + rng.uniform(-0.2, 0.2)
        dx, dy = math.cos(a), math.sin(a)
        px, py = -dy, dx
        reach = rng.uniform(0.8, 1.05)
        rise = rng.uniform(0.38, 0.5)
        width = 0.16
        base = m.vert((0.0, 0.0, 0.05))
        mid = Vector((dx * reach * 0.5, dy * reach * 0.5, rise))
        left = m.vert(mid + Vector((px * width, py * width, 0.0)))
        right = m.vert(mid - Vector((px * width, py * width, 0.0)))
        rib = m.vert(mid + Vector((0.0, 0.0, 0.05)))
        keel = m.vert(mid - Vector((0.0, 0.0, 0.04)))
        tip = m.vert((dx * reach, dy * reach, rise * 0.45))
        for a_, b_ in ((base, left), (left, tip), (tip, right), (right, base)):
            m.outward_face([a_, b_, rib], GREEN_DARK, mid)
            m.outward_face([a_, b_, keel], PINE_SHADE, mid)
    return m.build("Fern")


def water_fronds():
    """The water fern's six fronds, as (direction, side, reach, rise, width)."""
    rng = random.Random(6311)
    fronds = []
    for k in range(6):
        a = 2 * math.pi * k / 6 + rng.uniform(-0.15, 0.15)
        direction = Vector((math.cos(a), math.sin(a), 0.0))
        side = Vector((-direction.y, direction.x, 0.0))
        fronds.append((direction, side, rng.uniform(1.0, 1.2), rng.uniform(0.3, 0.36), 0.24))
    return fronds


def frond_corners(frond):
    """The five corners of a water frond: foot, left and right edges at its middle,
    raised rib over its middle, and tip. The rib makes the upper face a shallow
    ridge, like the decor fern's."""
    direction, side, reach, rise, width = frond
    mid = direction * (reach * 0.5) + Vector((0.0, 0.0, 0.05 + rise))
    return {
        "base": Vector((0.0, 0.0, 0.05)),
        "left": mid + side * width,
        "right": mid - side * width,
        "mid": mid,
        "rib": mid + Vector((0.0, 0.0, 0.04)),
        # The tip droops back to a little over half the height of the middle.
        "tip": direction * reach + Vector((0.0, 0.0, 0.05 + rise * 0.55)),
    }


def water_fern():
    """Six wide fronds opened flat like a cup, in light green with a dark
    underside: it reads apart from the decor fern, closed and dark."""
    m = MeshBuilder()
    for frond in water_fronds():
        c = frond_corners(frond)
        base, left, right, tip = (m.vert(c[k]) for k in ("base", "left", "right", "tip"))
        rib = m.vert(c["rib"])
        keel = m.vert(c["mid"] - Vector((0.0, 0.0, 0.035)))
        for a_, b_ in ((base, left), (left, tip), (tip, right), (right, base)):
            m.outward_face([a_, b_, rib], GREEN, c["mid"])
            m.outward_face([a_, b_, keel], GREEN_DARK, c["mid"])
    return m.build("WaterFern")


def mushroom(m, rng, x, y, height, cap_radius, cap_cell):
    stem = m.ring((x, y, 0.0), cap_radius * 0.28, 6)
    top = m.ring((x, y, height), cap_radius * 0.22, 6)
    m.bridge(stem, top, BODY_LIGHT)
    # A rounded cap: rim, shoulder, crown, then the pale gills underneath. Larger caps
    # get more sides, so their rim still reads as a circle.
    sides = 10 if cap_radius > 0.25 else 8
    rim = m.ring((x, y, height - cap_radius * 0.1), cap_radius, sides, rng, 0.02)
    shoulder = m.ring((x, y, height + cap_radius * 0.35), cap_radius * 0.82, sides, rng, 0.02, math.pi / sides)
    crown = m.vert((x + rng.uniform(-0.02, 0.02), y + rng.uniform(-0.02, 0.02), height + cap_radius * 0.62))
    # The shoulder is turned half a side from the rim: the band between them is made of
    # triangles, never of twisted quads, which fold and open a notch in the silhouette.
    for i in range(sides):
        j = (i + 1) % sides
        m.face([rim[i], rim[j], shoulder[i]], cap_cell)
        m.face([shoulder[i], rim[j], shoulder[j]], cap_cell)
    for i in range(sides):
        j = (i + 1) % sides
        m.face([shoulder[i], shoulder[j], crown], cap_cell)
    m.cap(rim, (x, y, height - cap_radius * 0.02), BODY_BASE, up=False)
    # Pale spots on the cap, the Fungiiiii signature. Each one is a small square drawn
    # in the plane of one face of the crown and lifted a hair off it: it lies on the
    # cap, never sticks out of it nor sinks into it.
    for k in (1, sides // 2, sides - 2):
        a, b, c = m.verts[shoulder[k]], m.verts[shoulder[(k + 1) % sides]], m.verts[crown]
        normal = (b - a).cross(c - a).normalized()
        across = (b - a).normalized()
        up = normal.cross(across)
        centre = a.lerp(b, 0.5).lerp(c, 0.2) + normal * cap_radius * 0.015
        half = cap_radius * 0.12
        spot = [m.vert(centre + across * du * half + up * dv * half) for du, dv in ((-1, -1), (1, -1), (1, 1), (-1, 1))]
        m.face(spot, SPOTS)


def cluster(name, seed, caps):
    """A group of mushrooms, one (x, y, height, cap radius, cap cell) per cap."""
    rng = random.Random(seed)
    m = MeshBuilder()
    for x, y, height, radius, cell in caps:
        mushroom(m, rng, x, y, height, radius, cell)
    return m.build(name)


def mushroom_cluster():
    return cluster("MushroomCluster", 6409, ((0.0, 0.0, 0.55, 0.26, MUSHROOM_RED), (0.32, 0.12, 0.32, 0.17, MUSHROOM_VIOLET),
                                             (-0.22, 0.24, 0.26, 0.13, MUSHROOM_RED), (-0.1, -0.3, 0.4, 0.2, MUSHROOM_VIOLET)))


# Variants of the gatherable mushrooms, so that a grove of them does not look stamped.
# Each fits in the same footprint as MushroomCluster, about 1 m across.
def mushroom_cluster_02():
    return cluster("MushroomCluster_02", 6410, ((0.0, 0.0, 0.6, 0.3, MUSHROOM_RED), (0.3, -0.12, 0.3, 0.16, MUSHROOM_RED),
                                                (-0.24, 0.16, 0.22, 0.12, MUSHROOM_RED)))


def mushroom_cluster_03():
    return cluster("MushroomCluster_03", 6411, ((0.0, 0.0, 0.3, 0.17, MUSHROOM_VIOLET), (0.26, 0.1, 0.22, 0.13, MUSHROOM_VIOLET),
                                                (-0.22, 0.14, 0.2, 0.12, MUSHROOM_VIOLET), (0.08, -0.28, 0.24, 0.14, MUSHROOM_VIOLET),
                                                (-0.18, -0.2, 0.16, 0.1, MUSHROOM_VIOLET)))


def mushroom_cluster_04():
    return cluster("MushroomCluster_04", 6412, ((0.0, 0.0, 0.72, 0.34, MUSHROOM_RED), (0.26, 0.18, 0.18, 0.1, MUSHROOM_VIOLET)))


# Terrain details draw the raw mesh with position, yaw and scale only, ignoring
# the -90 X root rotation every other model imports with: these are exported
# with Apply Transform on, so the mesh itself is Y up.
BAKED_AXES = {"GrassTuft"}


def export(obj):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    path = os.path.join(OUT, obj.name + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=path,
        path_mode="AUTO",
        use_selection=True,
        object_types={"MESH"},
        use_custom_props=False,
        global_scale=1.0,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        apply_unit_scale=True,
        use_space_transform=True,
        bake_space_transform=obj.name in BAKED_AXES,
        mesh_smooth_type="FACE",
        use_mesh_modifiers=True,
        use_mesh_edges=False,
        use_tspace=False,
        use_triangles=False,
        primary_bone_axis="Y",
        secondary_bone_axis="X",
        armature_nodetype="NULL",
        use_armature_deform_only=False,
        add_leaf_bones=False,
        bake_anim=False,
    )
    tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    print(f"exported {obj.name}: {tris} triangles -> {path}")


BUILDS = {
    "TreeBroad": tree_broad,
    "TreePine": tree_pine,
    "TreeGiant": tree_giant,
    "Rock_Boulder": rock_boulder,
    "Rock_Spire": rock_spire,
    "GrassTuft": grass_tuft,
    "Bush": bush,
    "Fern": fern,
    "WaterFern": water_fern,
    "MushroomCluster": mushroom_cluster,
    "MushroomCluster_02": mushroom_cluster_02,
    "MushroomCluster_03": mushroom_cluster_03,
    "MushroomCluster_04": mushroom_cluster_04,
}


def main():
    # Blender keeps its own arguments before "--"; the script's come after it.
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    unknown = [name for name in argv if name not in BUILDS]
    if unknown:
        raise SystemExit(f"unknown models: {', '.join(unknown)}")

    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    for name in argv or BUILDS:
        export(BUILDS[name]())


main()
