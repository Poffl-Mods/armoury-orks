"""Render the generated Unity cache geometry/material colours for placement-independent review."""
import math
from pathlib import Path
import re
import bpy
from mathutils import Vector, Quaternion
ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/Modifications/ArmouryOrks/Art'
OUT = ROOT / 'SourceAssets/ArmouryCache'
OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
materials = {}
for file in ART.glob('OrkArmouryCache_*.mat'):
    guid = re.search(r'guid: (\w+)', file.with_suffix('.mat.meta').read_text()).group(1)
    color = re.search(r'- _BaseColor: \{r: ([\d.]+), g: ([\d.]+), b: ([\d.]+)', file.read_text())
    mat = bpy.data.materials.new(file.stem)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = tuple(map(float, color.groups())) + (1,)
    bsdf.inputs['Metallic'].default_value = float(re.search(r'- _Metallic: ([\d.]+)', file.read_text()).group(1))
    bsdf.inputs['Roughness'].default_value = .85
    materials[guid] = mat
objects = {}
for section in re.split(r'^--- ', (ART / 'OrkArmouryCache.prefab').read_text(), flags=re.M):
    owner = re.search(r'm_GameObject: \{fileID: (\d+)\}', section)
    if not owner: continue
    obj = objects.setdefault(owner.group(1), {})
    if section.startswith('!u!4 '):
        for key in ['Position', 'Scale', 'Rotation']:
            raw = re.search(r'm_Local' + key + r': \{([^}]+)\}', section).group(1)
            obj[key] = {k: float(v) for k, v in re.findall(r'(\w): ([-\d.eE]+)', raw)}
    elif section.startswith('!u!23 '):
        obj['material'] = re.search(r'm_Materials:\s+- \{fileID: \d+, guid: (\w+)', section).group(1)
for obj in objects.values():
    if 'material' not in obj: continue
    p, s, q = obj['Position'], obj['Scale'], obj['Rotation']
    bpy.ops.mesh.primitive_cube_add(size=1, location=(p['x'], p['z'], p['y']))
    cube = bpy.context.object
    cube.scale = (s['x'], s['z'], s['y'])
    cube.rotation_mode = 'QUATERNION'
    cube.rotation_quaternion = Quaternion((q['w'], -q['x'], -q['z'], -q['y']))
    cube.data.materials.append(materials[obj['material']])
bpy.ops.object.camera_add(location=(1.55,-2.25,1.65))
camera = bpy.context.object
camera.rotation_euler = (Vector((0,0,.35))-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 1.65
scene = bpy.context.scene
scene.camera = camera
scene.render.engine = 'CYCLES'
scene.cycles.samples = 40
scene.world.color = (.16,.16,.16)
for position, energy, size in [((1,-2,3),280,3),((-2,-.5,1.8),160,2),((0,2,2),240,2)]:
    bpy.ops.object.light_add(type='AREA',location=position)
    light=bpy.context.object
    light.data.energy=energy
    light.data.shape='DISK'
    light.data.size=size
    light.rotation_euler=(Vector((0,0,.3))-light.location).to_track_quat('-Z','Y').to_euler()
scene.render.resolution_x=900
scene.render.resolution_y=700
scene.render.resolution_percentage=100
scene.render.film_transparent=True
scene.render.filepath=str(OUT / 'OrkArmouryCache_Preview.png')
bpy.ops.render.render(write_still=True)
