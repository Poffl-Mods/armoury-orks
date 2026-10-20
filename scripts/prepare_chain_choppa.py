"""Prepare the reduced Chain Choppa for Unity; run with Blender --background --python."""
from pathlib import Path
import bpy
from mathutils import Vector, Matrix
from math import radians

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'SourceAssets/ChainChoppa'
ART = ROOT / 'Assets/Modifications/ArmouryOrks/Art'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE / 'ChainChoppa_60k.blend'))
weapon = bpy.data.objects['ChainChoppa']
scene = bpy.context.scene
# Render the actual reduced asset, using its rebaked surface normals.
points = [weapon.matrix_world @ Vector(v) for v in weapon.bound_box]
center = sum(points, Vector()) / 8
span = weapon.dimensions.length
camera = scene.camera
camera.location = center + Vector((.6, -5, .25)).normalized() * span * 2
camera.rotation_euler = (center - camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.ortho_scale = weapon.dimensions.z * 1.12
scene.render.resolution_x = scene.render.resolution_y = 512
scene.render.film_transparent = True
scene.render.image_settings.color_mode = 'RGBA'
scene.render.filepath = str(ART / 'ChainChoppa_InventoryIcon.png')
bpy.ops.render.render(write_still=True)
# Move the hand point 0.11 source units (about 6 cm in game) toward the blade. Keeping the imported
# FBX basis in Unity then maps Blender Z-up to the game's Y-up without size hacks.
grip = Vector((.08, 0, -.64))
scale = 1.04 / weapon.dimensions.z
# Reverse the 0.5.7 roll by another 180 degrees around the blade axis.
# Bake into vertices so the correction survives doll-room attachment.
rotation = Matrix.Rotation(radians(90), 4, 'Z')
weapon.data.transform(rotation @ Matrix.Scale(scale, 4) @ Matrix.Translation(-grip))
weapon.data.update()
weapon.name = 'ChainChoppa_GripAligned'
bpy.ops.object.select_all(action='DESELECT')
weapon.select_set(True)
bpy.context.view_layer.objects.active = weapon
bpy.ops.export_scene.fbx(filepath=str(ART / 'ChainChoppa.fbx'), use_selection=True,
                        object_types={'MESH'}, bake_anim=False, path_mode='STRIP',
                        axis_forward='-Z', axis_up='Y')
print('PREPARED ChainChoppa: 60000 triangles, 1.04 metres, grip at origin', flush=True)
