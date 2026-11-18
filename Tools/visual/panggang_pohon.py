"""Panggang pemindaian Poly Haven menjadi LOD untuk Unity; dijalankan Blender -b --python."""
import bpy, struct, json, sys, hashlib, math
from pathlib import Path
from mathutils import Matrix
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/ThirdParty/Prepared'
OUT.mkdir(parents=True,exist_ok=True)

def write_mesh(path,objects,limit,tree=False,twist=0.0):
    vertices=[]; triangles=[[],[]]
    points=[o.matrix_world@v.co for o in objects for v in o.data.vertices]
    zmin=min(p.z for p in points);height=max(p.z for p in points)-zmin
    scale=18.0/max(height,0.001) if tree else 1.0
    for src in objects:
        obj=src.copy();obj.data=src.data.copy();bpy.context.collection.objects.link(obj)
        bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
        weld=obj.modifiers.new('Weld','WELD');weld.merge_threshold=0.00015
        bpy.ops.object.modifier_apply(modifier=weld.name)
        count=sum(len(p.vertices)-2 for p in obj.data.polygons)
        if count>limit:
            dec=obj.modifiers.new('LOD','DECIMATE');dec.ratio=limit/count;dec.use_collapse_triangulate=True
            bpy.ops.object.modifier_apply(modifier=dec.name)
        mesh=obj.data;mesh.calc_loop_triangles();uv=mesh.uv_layers.active
        for tri in mesh.loop_triangles:
            material=mesh.materials[tri.material_index] if tri.material_index<len(mesh.materials) else None
            sub=1 if not tree or (material and 'leaves' in material.name) else 0
            base=len(vertices)
            for vi,li in zip(tri.vertices,tri.loops):
                v=mesh.vertices[vi];p=obj.matrix_world@v.co;n=(obj.matrix_world.to_3x3().inverted().transposed()@v.normal).normalized()
                if twist!=0.0:
                    ang=twist*((p.z-zmin)/max(height,0.001));c=math.cos(ang);s=math.sin(ang)
                    px=p.x*c-p.y*s;py=p.x*s+p.y*c;nx=n.x*c-n.y*s;ny=n.x*s+n.y*c
                    p=(px,py,p.z);n=(nx,ny,n.z)
                    q=(p[0]*scale*1.12,(p[2]-zmin)*scale*0.92,-p[1]*scale*1.12)
                    norm=(n[0],n[2],-n[1])
                else:
                    q=(p.x*scale,(p.z-zmin)*scale,-p.y*scale)
                    norm=(n.x,n.z,-n.y)
                tex=uv.data[li].uv if uv else (0,0)
                vertices.append((*q,*norm,*tex))
            triangles[sub].extend([base,base+1,base+2])
        bpy.data.objects.remove(obj,do_unlink=True)
    with open(path,'wb') as f:
        f.write(b'AKV1');f.write(struct.pack('<3i',len(vertices),2,0))
        for v in vertices:f.write(struct.pack('<8f',*v))
        for t in triangles:f.write(struct.pack('<i',len(t)));f.write(struct.pack('<%di'%len(t),*t))
    print('[AK] LOD',path.name,len(vertices)//3,'segitiga',flush=True)

for name in ['island_tree_01','fern_02']:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    path=ROOT/'Assets/ThirdParty/CC0/Model'/f'{name}.fbx.source'
    if not path.exists():raise RuntimeError('Aset scan hilang: '+str(path))
    bpy.ops.import_scene.fbx(filepath=str(path))
    objects=[o for o in bpy.data.objects if o.type=='MESH']
    if name=='island_tree_01':
        write_mesh(OUT/'PohonScan_LOD0.bytes',objects,14000,True,0.0)
        write_mesh(OUT/'PohonScan_LOD1.bytes',objects,2200,True,0.0)
        write_mesh(OUT/'PohonScanB_LOD0.bytes',objects,12500,True,0.65)
        write_mesh(OUT/'PohonScanB_LOD1.bytes',objects,2000,True,0.65)
    else:
        write_mesh(OUT/'PakisScan_LOD0.bytes',objects[1:2],2200)
        write_mesh(OUT/'PakisScan_LOD1.bytes',objects[1:2],400)
