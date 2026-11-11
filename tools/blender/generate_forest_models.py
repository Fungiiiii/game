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

    def build(self, name):
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
    m.face([left[0], left[1], left[2]], BARK_PALE)
    m.face([right[2], right[1], right[0]], BARK_PALE)
    m.face([left[1], right[1], right[2], left[2]], BARK_PALE)  # sloped back
    m.face([left[0], left[2], right[2], right[0]], BARK_PALE)  # inside the trunk


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
        bake_space_transform=False,
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


def main():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    for build in (tree_broad, tree_pine, tree_giant, rock_boulder, rock_spire):
        export(build())


main()
