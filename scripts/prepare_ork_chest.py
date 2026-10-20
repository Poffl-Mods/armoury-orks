"""Reduce the original Ork War Chest, bake high-poly detail and export the game mesh.

Run: blender --background --python scripts/prepare_ork_chest.py -- /path/to/source_chest_folder
Original input files remain untouched. Outputs retain the input UV layout and 2K maps.
"""
from pathlib import Path
import hashlib
import json
import shutil
import sys
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/Modifications/ArmouryOrks/Art'
SOURCE = ROOT / 'SourceAssets/ArmouryCache/OrkWarChest'
SOURCE.mkdir(parents=True, exist_ok=True)
INPUT = Path(sys.argv[sys.argv.index('--') + 1])
FBX = next(INPUT.glob('*.fbx'))
TARGET = 40000
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(FBX))
meshes = [o for o in bpy.context.selected_objects if o.type == 'MESH']
if len(meshes) != 1: raise RuntimeError('Expected the single Ork War Chest mesh.')
high = meshes[0]
high.name = 'OrkChest_Original_High'
original_triangles = sum(len(p.vertices)-2 for p in high.data.polygons)
bpy.context.view_layer.objects.active=high
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
points=[Vector(p) for p in high.bound_box]
minimum=Vector(tuple(min(p[i] for p in points) for i in range(3)))
maximum=Vector(tuple(max(p[i] for p in points) for i in range(3)))
scale=1.02/(maximum.x-minimum.x)
origin=Vector(((minimum.x+maximum.x)/2,(minimum.y+maximum.y)/2,minimum.z))
for v in high.data.vertices: v.co=(v.co-origin)*scale
high.data.update()
# Construct PBR nodes explicitly so the original authored normal participates in the bake.
material=bpy.data.materials.new('OrkChest_PBR');material.use_nodes=True
nodes=material.node_tree.nodes;links=material.node_tree.links;bsdf=nodes.get('Principled BSDF')
texture_files={}
for suffix,key,socket,linear in [('', 'BaseColor','Base Color',False),('_metallic','Metallic','Metallic',True),('_roughness','Roughness','Roughness',True),('_normal','Normal_Original',None,True)]:
    path=next(INPUT.glob('*_texture'+suffix+'.png'))
    destination=SOURCE/('OrkChest_'+key+'.png')
    shutil.copy2(path,destination)
    texture_files[key]=destination
    node=nodes.new('ShaderNodeTexImage');node.name=key
    node.image=bpy.data.images.load(str(destination));node.image.colorspace_settings.name='Non-Color' if linear else 'sRGB'
    if socket: links.new(node.outputs['Color'],bsdf.inputs[socket])
normal=nodes.new('ShaderNodeNormalMap');links.new(nodes['Normal_Original'].outputs['Color'],normal.inputs['Color']);links.new(normal.outputs['Normal'],bsdf.inputs['Normal'])
high.data.materials.clear();high.data.materials.append(material)
for p in high.data.polygons:p.use_smooth=True
low=high.copy();low.data=high.data.copy();low.name='OrkChest_40k';bpy.context.scene.collection.objects.link(low)
low_material=material.copy();low.data.materials.clear();low.data.materials.append(low_material)
bpy.ops.object.select_all(action='DESELECT');low.select_set(True);bpy.context.view_layer.objects.active=low
modifier=low.modifiers.new('40k game mesh','DECIMATE');modifier.ratio=TARGET/original_triangles;modifier.use_collapse_triangulate=True
bpy.ops.object.modifier_apply(modifier=modifier.name)
triangles=sum(len(p.vertices)-2 for p in low.data.polygons)
if abs(triangles-TARGET)>2:raise RuntimeError(f'Unexpected triangle count {triangles}')
for p in low.data.polygons:p.use_smooth=True
image=bpy.data.images.new('OrkChest_Normal_40k',width=2048,height=2048,alpha=False);image.colorspace_settings.name='Non-Color'
node=low_material.node_tree.nodes.new('ShaderNodeTexImage');node.image=image;low_material.node_tree.nodes.active=node
low_normal=next(n for n in low_material.node_tree.nodes if n.type=='NORMAL_MAP')
for link in list(low_material.node_tree.links):
    if link.to_node==low_normal:low_material.node_tree.links.remove(link)
bpy.ops.object.select_all(action='DESELECT');high.select_set(True);low.select_set(True);bpy.context.view_layer.objects.active=low
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=8
scene.render.bake.use_selected_to_active=True;scene.render.bake.cage_extrusion=.008;scene.render.bake.max_ray_distance=.025;scene.render.bake.margin=12
bpy.ops.object.bake(type='NORMAL')
image.filepath_raw=str(SOURCE/'OrkChest_Normal_40k.png');image.file_format='PNG';image.save()
low_material.node_tree.links.new(node.outputs['Color'],low_normal.inputs['Color'])
# Decimation interpolates UV coordinates. Reproject every PBR channel from the
# untouched high mesh so checker patterns, paint and metal regions do not warp.
emitter=nodes.new('ShaderNodeEmission');emitter.inputs['Strength'].default_value=1
output=nodes.get('Material Output');links.new(emitter.outputs['Emission'],output.inputs['Surface'])
for key in ['BaseColor','Metallic','Roughness']:
    target=bpy.data.images.new('OrkChest_'+key+'_40k',width=2048,height=2048,alpha=False)
    target.colorspace_settings.name='sRGB' if key=='BaseColor' else 'Non-Color'
    bake_node=low_material.node_tree.nodes.new('ShaderNodeTexImage');bake_node.image=target
    low_material.node_tree.nodes.active=bake_node
    links.new(nodes[key].outputs['Color'],emitter.inputs['Color'])
    bpy.ops.object.bake(type='EMIT')
    target.filepath_raw=str(SOURCE/('OrkChest_'+key+'_40k.png'));target.file_format='PNG';target.save()
    low_material.node_tree.nodes[key].image=target
    shutil.copy2(target.filepath_raw,ART/('OrkChest_'+key+'.png'))
bpy.data.objects.remove(high,do_unlink=True)
shutil.copy2(SOURCE/'OrkChest_Normal_40k.png',ART/'OrkChest_Normal_Source.png')
bpy.ops.object.select_all(action='DESELECT');low.select_set(True);bpy.context.view_layer.objects.active=low
bpy.ops.export_scene.fbx(filepath=str(ART/'OrkChest.fbx'),use_selection=True,object_types={'MESH'},bake_anim=False,path_mode='STRIP',axis_forward='-Z',axis_up='Y')
# Keep a clean reduced editable source and compareable studio views of the actual mesh.
scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.resolution_x=scene.render.resolution_y=900;scene.render.resolution_percentage=100;scene.render.film_transparent=True;scene.render.image_settings.color_mode='RGBA'
points=[low.matrix_world@Vector(p) for p in low.bound_box];center=sum(points,Vector())/8;span=max(low.dimensions)
bpy.ops.object.camera_add();camera=bpy.context.object;camera.data.type='ORTHO';camera.data.ortho_scale=span*1.45;scene.camera=camera
scene.world.color=(.15,.15,.15)
for direction,energy in [((1,-2,3),500),((-2,1,2),500)]:
    bpy.ops.object.light_add(type='AREA',location=center+Vector(direction)*span);lamp=bpy.context.object;lamp.data.energy=energy*span*span;lamp.data.shape='DISK';lamp.data.size=span*3;lamp.rotation_euler=(center-lamp.location).to_track_quat('-Z','Y').to_euler()
for name,direction in [('Front',(1,-2,1.3)),('Back',(-1,2,1.3))]:
    camera.location=center+Vector(direction)*span*2;camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(SOURCE/('OrkChest_40k_'+name+'.png'));bpy.ops.render.render(write_still=True)
report={'source_asset':'OrkWarChest_Source','source_sha256':hashlib.sha256(FBX.read_bytes()).hexdigest(),'original_triangles':original_triangles,'reduced_triangles':triangles,'reduction_percent':round((1-triangles/original_triangles)*100,2),'dimensions_metres_blender_xyz':list(low.dimensions),'front_axis_blender':'-Y','unity_front_correction_degrees':180,'texture_size':[2048,2048],'uv_layers':len(low.data.uv_layers),'pbr_reprojection':['BaseColor','Metallic','Roughness','Normal'],'normal_bake':{'selected_to_active':True,'cage_extrusion':.008,'max_ray_distance':.025,'margin':12}}
(SOURCE/'reduction-report.json').write_text(json.dumps(report,indent=2))
# Pack source textures so the reduced Blender file survives moving the repository.
bpy.data.orphans_purge(do_recursive=True)
for source_image in bpy.data.images:
    if source_image.source=='FILE' and source_image.filepath:
        source_image.filepath=bpy.path.relpath(source_image.filepath,start=str(SOURCE))
bpy.ops.file.pack_all();bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'OrkChest_40k.blend'))
print('CHEST_REDUCED',json.dumps(report),flush=True)
