"""Panggang pemindaian Poly Haven menjadi LOD untuk Unity; dijalankan Blender -b --python."""
import bpy, bmesh, struct, json, sys, hashlib, math, random
from pathlib import Path
from mathutils import Matrix, Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/ThirdParty/Prepared'
OUT.mkdir(parents=True,exist_ok=True)

def tree_parts(src,limit):
    """Batang disederhanakan sendiri; daun menjadi kartu utuh dengan rentang UV asal.

    Collapse seluruh scan 1,6 juta tris menjadi 14 ribu menghancurkan daun dan
    ranting menjadi pecahan. Material cabang tipis dibuang, bukan diruntuhkan.
    """
    obj=src.copy();obj.data=src.data.copy();bpy.context.collection.objects.link(obj)
    bm=bmesh.new();bm.from_mesh(obj.data)
    bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.material_index!=0],context='FACES')
    bm.to_mesh(obj.data);bm.free()
    bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
    count=sum(len(p.vertices)-2 for p in obj.data.polygons)
    wood_limit=max(200,int(limit*0.30))
    if count>wood_limit:
        dec=obj.modifiers.new('BatangLOD','DECIMATE');dec.ratio=wood_limit/count;dec.use_collapse_triangulate=True
        bpy.ops.object.modifier_apply(modifier=dec.name)
    mesh=obj.data;mesh.calc_loop_triangles();uv=mesh.uv_layers.active
    wood_normal=obj.matrix_world.to_3x3().inverted().transposed()
    rows=[]
    for tri in mesh.loop_triangles:
        rows.append((0,[(obj.matrix_world@mesh.vertices[vi].co,(wood_normal@mesh.vertices[vi].normal).normalized(),tuple(uv.data[li].uv)) for vi,li in zip(tri.vertices,tri.loops)]))
    bpy.data.objects.remove(obj,do_unlink=True)

    mesh=src.data;mesh.calc_loop_triangles();uv=mesh.uv_layers.active
    parent=list(range(len(mesh.vertices)))
    def find(i):
        while parent[i]!=i:parent[i]=parent[parent[i]];i=parent[i]
        return i
    for edge in mesh.edges:
        a,b=map(find,edge.vertices);parent[a]=b
    groups={}
    for tri in mesh.loop_triangles:
        mat=mesh.materials[tri.material_index]
        if mat and 'leaves' in mat.name.lower():groups.setdefault(find(tri.vertices[0]),[]).append(tri)
    cards=sorted(groups)
    random.Random(7331).shuffle(cards)  # subset LOD jauh selalu berasal dari urutan yang sama.
    card_limit=min(len(cards),max(1,(limit-len(rows))//2))
    expand=math.sqrt(len(cards)/card_limit)
    normal_matrix=src.matrix_world.to_3x3().inverted().transposed()
    for key in cards[:card_limit]:
        # Least-squares affine UV -> ruang mesh mempertahankan atlas, bukan UV tris hasil collapse.
        samples={}
        for tri in groups[key]:
            for vi,li in zip(tri.vertices,tri.loops):samples[vi]=(mesh.vertices[vi].co,uv.data[li].uv)
        center=sum((p for p,t in samples.values()),Vector())/len(samples)
        tc=sum((t for p,t in samples.values()),Vector((0,0)))/len(samples)
        suu=svv=suv=0.;pu=Vector();pv=Vector()
        for p,t in samples.values():
            du,dv=t-tc;suu+=du*du;svv+=dv*dv;suv+=du*dv;pu+=(p-center)*du;pv+=(p-center)*dv
        det=suu*svv-suv*suv
        if abs(det)<1e-14:raise RuntimeError('Kartu daun tanpa luas UV')
        u=(pu*svv-pv*suv)/det;v=(pv*suu-pu*suv)/det
        lo=[min(t[a] for p,t in samples.values()) for a in range(2)];hi=[max(t[a] for p,t in samples.values()) for a in range(2)]
        normal=sum((tri.normal for tri in groups[key]),Vector()).normalized()
        corners=[(lo[0],lo[1]),(hi[0],lo[1]),(hi[0],hi[1]),(lo[0],hi[1])]
        if u.cross(v).dot(normal)<0:corners.reverse()
        quad=[(src.matrix_world@(center+(u*(t[0]-tc.x)+v*(t[1]-tc.y))*expand),(normal_matrix@normal).normalized(),t) for t in corners]
        rows.append((1,[quad[0],quad[1],quad[2]]));rows.append((1,[quad[0],quad[2],quad[3]]))
    print('[AK] Kartu daun utuh',card_limit,'dari',len(cards),'perbesaran',round(expand,2),flush=True)
    return rows

def write_mesh(path,objects,limit,tree=False,twist=0.0):
    vertices=[]; triangles=[[],[]]
    points=[o.matrix_world@v.co for o in objects for v in o.data.vertices]
    zmin=min(p.z for p in points);height=max(p.z for p in points)-zmin
    scale=18.0/max(height,0.001) if tree else 1.0
    for src in objects:
        if tree:
            rows=tree_parts(src,limit)
            for sub,tri in rows:
                base=len(vertices)
                for p,n,tex in tri:
                    if twist!=0.0:
                        ang=twist*((p.z-zmin)/max(height,0.001));c=math.cos(ang);s=math.sin(ang)
                        p=Vector((p.x*c-p.y*s,p.x*s+p.y*c,p.z));n=Vector((n.x*c-n.y*s,n.x*s+n.y*c,n.z))
                    q=(p.x*scale*(1.12 if twist else 1),(p.z-zmin)*scale*(0.92 if twist else 1),-p.y*scale*(1.12 if twist else 1))
                    vertices.append((*q,n.x,n.z,-n.y,*tex))
                triangles[sub].extend([base,base+1,base+2])
            continue
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
