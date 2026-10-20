"""Render our original Power Klaw FBX as a transparent inventory icon.

Run with Blender: blender --background --python scripts/render_power_klaw_icon.py
Pass -- --previews to render alternative views into /private/tmp/ork-klaw/icons.
"""
from pathlib import Path
import sys
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/Modifications/ArmouryOrks/Art'
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(ART / 'PowerKlaw.fbx'))
meshes = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']
material = bpy.data.materials.new('PowerKlaw_Inventory')
material.use_nodes = True
shader = material.node_tree.nodes.get('Principled BSDF')
texture = material.node_tree.nodes.new('ShaderNodeTexImage')
texture.image = bpy.data.images.load(str(ART / 'PowerKlaw_BaseColor.png'))
material.node_tree.links.new(texture.outputs['Color'], shader.inputs['Base Color'])
shader.inputs['Metallic'].default_value = 0.8
shader.inputs['Roughness'].default_value = 0.68
normal_texture = material.node_tree.nodes.new('ShaderNodeTexImage')
normal_texture.image = bpy.data.images.load(str(ART / 'PowerKlaw_Normal_Source.png'))
normal_texture.image.colorspace_settings.name = 'Non-Color'
normal = material.node_tree.nodes.new('ShaderNodeNormalMap')
normal.inputs['Strength'].default_value = 0.7
material.node_tree.links.new(normal_texture.outputs['Color'], normal.inputs['Color'])
material.node_tree.links.new(normal.outputs['Normal'], shader.inputs['Normal'])
for mesh in meshes:
    mesh.data.materials.clear()
    mesh.data.materials.append(material)
points = [obj.matrix_world @ Vector(p) for obj in meshes for p in obj.bound_box]
minimum = Vector(tuple(min(p[i] for p in points) for i in range(3)))
maximum = Vector(tuple(max(p[i] for p in points) for i in range(3)))
center = (minimum + maximum) / 2
span = (maximum - minimum).length
print('MODEL BOUNDS:', tuple(minimum), tuple(maximum), flush=True)
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 24
scene.cycles.use_denoising = True
scene.render.resolution_x = scene.render.resolution_y = 512
scene.render.resolution_percentage = 100
scene.render.film_transparent = True
scene.render.image_settings.file_format = 'PNG'
scene.render.image_settings.color_mode = 'RGBA'
scene.view_settings.view_transform = 'AgX'
world = bpy.data.worlds.new('Inventory ambient')
world.use_nodes = True
world.node_tree.nodes['Background'].inputs['Color'].default_value = (0.8, 0.83, 0.9, 1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value = 0.4
scene.world = world
cam_data = bpy.data.cameras.new('InventoryCamera')
cam_data.type = 'ORTHO'
cam = bpy.data.objects.new('InventoryCamera', cam_data)
scene.collection.objects.link(cam)
scene.camera = cam
cam_data.clip_start = span * 0.001
cam_data.clip_end = span * 20

def light(name, direction, energy, size):
    data = bpy.data.lights.new(name, 'AREA')
    data.energy = energy * span * span
    data.shape = 'DISK'
    data.size = size * span
    obj = bpy.data.objects.new(name, data)
    scene.collection.objects.link(obj)
    obj.location = center + Vector(direction).normalized() * span * 1.5
    obj.rotation_euler = (center - obj.location).to_track_quat('-Z', 'Y').to_euler()
    return obj

light('Key', (2, -4, 5), 170, 1.5)
light('Fill', (-3, 1, 2), 70, 2)
light('Rim', (1, 4, 3), 120, 1)
previews = '--previews' in sys.argv
views = [(4, -6, 3), (6, 0, 2), (0, 6, 2), (-6, 0, 2), (4, 6, 3), (-4, -6, 3)] if previews else [(-4, -6, 3)]
for i, direction in enumerate(views):
    cam.location = center + Vector(direction).normalized() * span * 3
    cam.rotation_euler = (center - cam.location).to_track_quat('-Z', 'Y').to_euler()
    inverse = cam.rotation_euler.to_matrix().transposed()
    projection = [inverse @ (point - center) for point in points]
    width = max(p.x for p in projection) - min(p.x for p in projection)
    height = max(p.y for p in projection) - min(p.y for p in projection)
    cam_data.ortho_scale = max(width, height) * 1.12
    output = Path('/private/tmp/ork-klaw/icons') / f'view-{i}.png' if previews else ART / 'PowerKlaw_InventoryIcon.png'
    output.parent.mkdir(parents=True, exist_ok=True)
    scene.render.filepath = str(output)
    bpy.ops.render.render(write_still=True)
    print('RENDERED:', output, flush=True)
