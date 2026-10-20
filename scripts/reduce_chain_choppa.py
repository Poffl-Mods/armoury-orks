"""Rebuild the 60k Chain Choppa directly from the original source FBX.

Run: blender --background --python scripts/reduce_chain_choppa.py -- /path/to/original.fbx
The original FBX and the previous 25k source remain untouched.
"""
from pathlib import Path
import sys
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'SourceAssets/ChainChoppa'
original_path = Path(sys.argv[sys.argv.index('--') + 1])
bpy.ops.wm.open_mainfile(filepath=str(SOURCE / 'ChainChoppa_25k.blend'))
previous = bpy.data.objects['ChainChoppa']
material = previous.data.materials[0].copy()
# Restore the original normal map for projection from the full-resolution source.
normal_texture = next(n for n in material.node_tree.nodes if n.type == 'TEX_IMAGE' and 'Normal' in n.image.name)
normal_texture.image = bpy.data.images.load(str(SOURCE / 'ChainChoppa_Normal_Source.png'))
normal_texture.image.colorspace_settings.name = 'Non-Color'
bpy.data.objects.remove(previous, do_unlink=True)
bpy.ops.import_scene.fbx(filepath=str(original_path))
high = next(o for o in bpy.context.selected_objects if o.type == 'MESH')
high.name = 'Original_HighResolution'
high.data.materials.clear()
high.data.materials.append(material)
original_triangles = sum(len(p.vertices) - 2 for p in high.data.polygons)
low = high.copy()
low.data = high.data.copy()
low.name = 'ChainChoppa'
bpy.context.scene.collection.objects.link(low)
low_material = material.copy()
low.data.materials.clear()
low.data.materials.append(low_material)
bpy.ops.object.select_all(action='DESELECT')
low.select_set(True)
bpy.context.view_layer.objects.active = low
modifier = low.modifiers.new('Reduce original to 60k', 'DECIMATE')
modifier.ratio = 60000 / original_triangles
modifier.use_collapse_triangulate = True
bpy.ops.object.modifier_apply(modifier=modifier.name)
triangles = sum(len(p.vertices) - 2 for p in low.data.polygons)
assert triangles == 60000, triangles
for face in low.data.polygons:
    face.use_smooth = True

normal_image = bpy.data.images.new('ChainChoppa_Normal_60k', width=2048, height=2048, alpha=False)
normal_image.colorspace_settings.name = 'Non-Color'
node = low_material.node_tree.nodes.new('ShaderNodeTexImage')
node.image = normal_image
low_material.node_tree.nodes.active = node
normal = next(n for n in low_material.node_tree.nodes if n.type == 'NORMAL_MAP')
for link in list(low_material.node_tree.links):
    if link.to_node == normal:
        low_material.node_tree.links.remove(link)
bpy.ops.object.select_all(action='DESELECT')
high.select_set(True)
low.select_set(True)
bpy.context.view_layer.objects.active = low
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 8
scene.render.bake.use_selected_to_active = True
scene.render.bake.cage_extrusion = .008
scene.render.bake.max_ray_distance = .02
scene.render.bake.margin = 8
bpy.ops.object.bake(type='NORMAL')
normal_image.filepath_raw = str(SOURCE / 'ChainChoppa_Normal_60k.png')
normal_image.file_format = 'PNG'
normal_image.save()
low_material.node_tree.links.new(node.outputs['Color'], normal.inputs['Color'])
# Remove the unused normal input, leaving only the 60k normal in the exported material.
for n in list(low_material.node_tree.nodes):
    if n.type == 'TEX_IMAGE' and n != node and 'Normal' in n.image.name:
        low_material.node_tree.nodes.remove(n)
bpy.data.objects.remove(high, do_unlink=True)
bpy.data.orphans_purge(do_recursive=True)
scene.cycles.samples = 32
points = [low.matrix_world @ Vector(v) for v in low.bound_box]
center = sum(points, Vector()) / 8
span = low.dimensions.length
scene.render.film_transparent = False
scene.render.resolution_x = 650
scene.render.resolution_y = 1000
scene.render.resolution_percentage = 100
scene.camera.data.ortho_scale = low.dimensions.z * 1.12
for filename, direction in [('ChainChoppa_60k_Front.png', (.6,-5,.25)), ('ChainChoppa_60k_Back.png', (.6,5,.25))]:
    camera = scene.camera
    camera.location = center + Vector(direction).normalized() * span * 2
    camera.rotation_euler = (center - camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath = str(SOURCE / filename)
    bpy.ops.render.render(write_still=True)
bpy.ops.object.select_all(action='DESELECT')
low.select_set(True)
bpy.context.view_layer.objects.active = low
bpy.ops.export_scene.fbx(filepath=str(SOURCE / 'ChainChoppa_60k.fbx'), use_selection=True,
                        object_types={'MESH'}, bake_anim=False, path_mode='RELATIVE',
                        axis_forward='-Z', axis_up='Y')
for image in bpy.data.images:
    if image.source == 'FILE' and image.filepath:
        image.filepath = bpy.path.relpath(image.filepath, start=str(SOURCE))
bpy.context.preferences.filepaths.save_version = 0
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE / 'ChainChoppa_60k.blend'))
print(f'REBUILT: {original_triangles} -> {triangles} triangles, UVs: {len(low.data.uv_layers)}', flush=True)
