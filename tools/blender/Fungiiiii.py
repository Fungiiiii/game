# Blender FBX export preset for Fungiiiii — operator preset for export_scene.fbx.
#
# This file is authoritative for the project's export settings. See
# docs/art-pipeline.md for the human-readable table and the reasoning, and
# docs/adrs/0005-pipeline-et-nommage-des-assets-3d.md for the decision.
#
# Install: see tools/blender/README.md
import bpy

op = bpy.context.active_operator

op.path_mode = 'AUTO'
op.embed_textures = False

# Include — one logical asset per file, geometry and skeleton only.
op.use_selection = True
op.use_visible = False
op.use_active_collection = False
op.object_types = {'ARMATURE', 'MESH'}
op.use_custom_props = False

# Transform — 1 Blender unit lands as 1 Unity metre, in Unity's axes.
# bake_space_transform ("Apply Transform") stays off for every asset: the axis
# conversion is done once, on the Unity side, by Bake Axis Conversion. Doing
# both risks converting twice, and the option is experimental and breaks rigs.
op.global_scale = 1.0
op.apply_unit_scale = True
op.apply_scale_options = 'FBX_SCALE_ALL'
op.axis_forward = '-Z'
op.axis_up = 'Y'
op.use_space_transform = True
op.bake_space_transform = False

# Geometry — Blender's default smoothing ('OFF', i.e. Normals Only) drops
# smoothing information and shows up as shading artefacts in Unity.
# Modifiers are applied at their viewport level, not their render level.
op.mesh_smooth_type = 'FACE'
op.use_mesh_modifiers = True
op.use_mesh_edges = False
op.use_tspace = False
op.use_triangles = False

# Armature — leaf bones are useless to Unity. use_armature_deform_only stays
# off because the root bone deforms no vertex and would be dropped, taking the
# hierarchy with it; the cost is that IK and control bones come across too.
op.primary_bone_axis = 'Y'
op.secondary_bone_axis = 'X'
op.armature_nodetype = 'NULL'
op.use_armature_deform_only = False
op.add_leaf_bones = False

# Animation — nothing is animated yet. Turning this on is part of the change
# that introduces animations, together with the <Character>@<Clip>.fbx naming.
op.bake_anim = False
