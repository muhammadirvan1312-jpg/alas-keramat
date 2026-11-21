"""Panggang pemindaian Poly Haven menjadi LOD untuk Unity; dijalankan Blender -b --python."""
import bpy, bmesh, struct, json, sys, hashlib, math, random
from pathlib import Path
from mathutils import Matrix, Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/ThirdParty/Prepared'
OUT.mkdir(parents=True,exist_ok=True)

def is_leaf_mat(mat):
    if not mat:return False
    n=mat.name.lower()
    return 'leaves' in n or 'leaf' in n or 'foliage' in n or 'frond' in n

def is_branch_mat(mat):
    return bool(mat and 'branch' in mat.name.lower())

def tree_parts(src,limit):
    """Batang disederhanakan sendiri; daun menjadi kartu utuh dengan rentang UV asal.

    Collapse seluruh scan 1,6 juta tris menjadi 14 ribu menghancurkan daun dan
    ranting menjadi pecahan. Material cabang tipis dibuang, bukan diruntuhkan.
    """
    obj=src.copy();obj.data=src.data.copy();bpy.context.collection.objects.link(obj)
    obj.matrix_world=src.matrix_world.copy();bpy.context.view_layer.update()
    mats=obj.data.materials
    bm=bmesh.new();bm.from_mesh(obj.data)
    keep=set(f for f in bm.faces if f.material_index<len(mats) and not is_leaf_mat(mats[f.material_index]) and not is_branch_mat(mats[f.material_index]))
    if not keep:
        keep=set(f for f in bm.faces if f.material_index==0 and (not mats or not is_leaf_mat(mats[0])))
    bmesh.ops.delete(bm,geom=[f for f in bm.faces if f not in keep],context='FACES')
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
        rows.append((0,[(obj.matrix_world@mesh.vertices[vi].co,(wood_normal@mesh.vertices[vi].normal).normalized(),tuple(uv.data[li].uv) if uv else (0.0,0.0)) for vi,li in zip(tri.vertices,tri.loops)]))
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
        mat=mesh.materials[tri.material_index] if tri.material_index<len(mesh.materials) else None
        if is_leaf_mat(mat):groups.setdefault(find(tri.vertices[0]),[]).append(tri)
    cards=sorted(groups)
    if cards:
        random.Random(7331).shuffle(cards)  # subset LOD jauh selalu berasal dari urutan yang sama.
        card_limit=min(len(cards),max(1,(limit-len(rows))//2))
        expand=math.sqrt(len(cards)/card_limit)
        normal_matrix=src.matrix_world.to_3x3().inverted().transposed()
        for key in cards[:card_limit]:
            # Least-squares affine UV -> ruang mesh mempertahankan atlas, bukan UV tris hasil collapse.
            samples={}
            for tri in groups[key]:
                for vi,li in zip(tri.vertices,tri.loops):samples[vi]=(mesh.vertices[vi].co,uv.data[li].uv if uv else Vector((0.0,0.0)))
            center=sum((p for p,t in samples.values()),Vector())/len(samples)
            tc=sum((t for p,t in samples.values()),Vector((0,0)))/len(samples)
            suu=svv=suv=0.;pu=Vector();pv=Vector()
            for p,t in samples.values():
                du,dv=t-tc;suu+=du*du;svv+=dv*dv;suv+=du*dv;pu+=(p-center)*du;pv+=(p-center)*dv
            det=suu*svv-suv*suv
            if abs(det)<1e-14:continue
            u=(pu*svv-pv*suv)/det;v=(pv*suu-pu*suv)/det
            lo=[min(t[a] for p,t in samples.values()) for a in range(2)];hi=[max(t[a] for p,t in samples.values()) for a in range(2)]
            normal=sum((tri.normal for tri in groups[key]),Vector())
            if normal.length<1e-8:normal=Vector((0,0,1))
            normal=normal.normalized()
            corners=[(lo[0],lo[1]),(hi[0],lo[1]),(hi[0],hi[1]),(lo[0],hi[1])]
            if u.cross(v).dot(normal)<0:corners.reverse()
            quad=[(src.matrix_world@(center+(u*(t[0]-tc.x)+v*(t[1]-tc.y))*expand),(normal_matrix@normal).normalized(),t) for t in corners]
            rows.append((1,[quad[0],quad[1],quad[2]]));rows.append((1,[quad[0],quad[2],quad[3]]))
        print('[AK] Kartu daun utuh',card_limit,'dari',len(cards),'perbesaran',round(expand,2),flush=True)
    return rows

def write_mesh(path,objects,limit,tree=False,twist=0.0,target_height=18.0):
    vertices=[]; triangles=[[],[]]
    points=[o.matrix_world@v.co for o in objects for v in o.data.vertices]
    cx=(min(p.x for p in points)+max(p.x for p in points))*0.5 if tree else 0.0
    cy=(min(p.y for p in points)+max(p.y for p in points))*0.5 if tree else 0.0
    zmin=min(p.z for p in points);height=max(p.z for p in points)-zmin
    scale=target_height/max(height,0.001) if tree else 1.0
    for src in objects:
        if tree:
            rows=tree_parts(src,limit)
            for sub,tri in rows:
                base=len(vertices)
                for p,n,tex in tri:
                    p=Vector((p.x-cx,p.y-cy,p.z))
                    if twist!=0.0:
                        ang=twist*((p.z-zmin)/max(height,0.001));c=math.cos(ang);s=math.sin(ang)
                        p=Vector((p.x*c-p.y*s,p.x*s+p.y*c,p.z));n=Vector((n.x*c-n.y*s,n.x*s+n.y*c,n.z))
                    q=(p.x*scale*(1.12 if twist else 1),(p.z-zmin)*scale*(0.92 if twist else 1),-p.y*scale*(1.12 if twist else 1))
                    vertices.append((*q,n.x,n.z,-n.y,*tex))
                triangles[sub].extend([base,base+1,base+2])
            continue
        obj=src.copy();obj.data=src.data.copy();bpy.context.collection.objects.link(obj)
        obj.matrix_world=src.matrix_world.copy();bpy.context.view_layer.update()
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
            sub=1 if not tree or (material and is_leaf_mat(material)) else 0
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

def prop_bounds(o):
    pts=[o.matrix_world@v.co for v in o.data.vertices]
    if not pts:return (0,0,0,0,0,0)
    return (min(p.x for p in pts),max(p.x for p in pts),min(p.y for p in pts),max(p.y for p in pts),min(p.z for p in pts),max(p.z for p in pts))

def split_by_spatial_gaps(obj):
    pts=[obj.matrix_world@v.co for v in obj.data.vertices]
    if not pts:return [obj]
    for axis in (0,1):
        vals=[p[axis] for p in pts]
        lo,hi=min(vals),max(vals)
        span=hi-lo
        if span<1e-4:continue
        nbins=32
        used=[False]*nbins
        for v in vals:used[min(nbins-1,int((v-lo)/span*nbins))]=True
        spans=[];start=None
        for i,u in enumerate(used):
            if u and start is None:start=i
            elif not u and start is not None:spans.append((start,i));start=None
        if start is not None:spans.append((start,nbins))
        if len(spans)>=2:
            out=[]
            for si,(b0,b1) in enumerate(spans):
                x0=lo+(b0/nbins)*span-1e-4;x1=lo+(b1/nbins)*span+1e-4
                copy=obj.copy();copy.data=obj.data.copy();copy.name=f"{obj.name}_seg{si}"
                bpy.context.collection.objects.link(copy)
                copy.matrix_world=obj.matrix_world.copy();bpy.context.view_layer.update()
                bm=bmesh.new();bm.from_mesh(copy.data)
                kill=[f for f in bm.faces if not (x0<=(copy.matrix_world@f.calc_center_median())[axis]<=x1)]
                if kill:bmesh.ops.delete(bm,geom=kill,context='FACES')
                bm.to_mesh(copy.data);bm.free()
                if len(copy.data.polygons)>=4:out.append(copy)
                else:bpy.data.objects.remove(copy,do_unlink=True)
            if out:return out
    return [obj]

def write_prop(path,objects,limit,target_size,cluster=1,pick=0,sink=0.0,size_axis='h',split_rocks=False):
    valid=sorted([o for o in objects if o.data and len(o.data.polygons)>=4],key=lambda o:o.name)
    if not valid:valid=list(objects)
    if split_rocks and len(valid)==1:
        bpy.ops.object.select_all(action='DESELECT');valid[0].select_set(True);bpy.context.view_layer.objects.active=valid[0]
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.separate(type='LOOSE')
        bpy.ops.object.mode_set(mode='OBJECT')
        valid=sorted([o for o in bpy.data.objects if o.type=='MESH' and len(o.data.polygons)>=12],key=lambda o:len(o.data.polygons),reverse=True)
    elif len(valid)==1 and (cluster>1 or pick>0):
        valid=split_by_spatial_gaps(valid[0])
    if cluster>1 and len(valid)==1:cluster=1

    # Mode 'whole' jika cluster==1 dan beberapa sub-bagian bertumpukan pada benda tunggal; jika terpisah jarak -> ambil varian pick.
    if len(valid)>1 and cluster==1:
        b0=prop_bounds(valid[0]);b1=prop_bounds(valid[1])
        c0=((b0[0]+b0[1])*0.5,(b0[2]+b0[3])*0.5);c1=((b1[0]+b1[1])*0.5,(b1[2]+b1[3])*0.5)
        span=max(b0[1]-b0[0],b0[3]-b0[2],0.01)
        if math.hypot(c0[0]-c1[0],c0[1]-c1[1])<span*0.45 and not split_rocks and pick==0:
            # Bagikan seluruh sub-part yang bertumpukan sebagai satu model utuh.
            chosen=[(valid,0.0,0.0,0.0)]
        else:
            chosen=[([valid[pick%len(valid)]],0.0,0.0,0.0)]
    elif cluster>1:
        chosen=[]
        for ci in range(cluster):
            ang=ci*2.094395+pick*0.7;rad=0.0 if ci==0 else 0.16
            chosen.append(([valid[(pick+ci)%len(valid)]],math.cos(ang)*rad,math.sin(ang)*rad,ci*117.0*math.pi/180.0))
    else:
        chosen=[(valid,0.0,0.0,0.0)]

    ref_objs=chosen[0][0]
    ref_pts=[o.matrix_world@v.co for o in ref_objs for v in o.data.vertices]
    ref_w=max(max(p.x for p in ref_pts)-min(p.x for p in ref_pts),max(p.y for p in ref_pts)-min(p.y for p in ref_pts),0.001)
    ref_h=max(max(p.z for p in ref_pts)-min(p.z for p in ref_pts),0.001)
    ref_scale=target_size/(ref_w if size_axis=='w' else ref_h)

    vertices=[];triangles=[]
    per_limit=max(80,limit//len(chosen))
    for grp,ox,oy,yaw in chosen:
        gpts=[o.matrix_world@v.co for o in grp for v in o.data.vertices]
        cx=(min(p.x for p in gpts)+max(p.x for p in gpts))*0.5
        cy=(min(p.y for p in gpts)+max(p.y for p in gpts))*0.5
        zmin=min(p.z for p in gpts)
        cs,sn=math.cos(yaw),math.sin(yaw)
        each_limit=max(60,per_limit//len(grp))
        for src in grp:
            obj=src.copy();obj.data=src.data.copy();bpy.context.collection.objects.link(obj)
            obj.matrix_world=src.matrix_world.copy();bpy.context.view_layer.update()
            bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
            count=sum(len(p.vertices)-2 for p in obj.data.polygons)
            if count>each_limit:
                dec=obj.modifiers.new('LOD','DECIMATE');dec.ratio=each_limit/count;dec.use_collapse_triangulate=True
                bpy.ops.object.modifier_apply(modifier=dec.name)
            mesh=obj.data;mesh.calc_loop_triangles();uv=mesh.uv_layers.active
            nmat=obj.matrix_world.to_3x3().inverted().transposed()
            for tri in mesh.loop_triangles:
                base=len(vertices)
                for vi,li in zip(tri.vertices,tri.loops):
                    v=mesh.vertices[vi];p=obj.matrix_world@v.co
                    n=(nmat@v.normal).normalized() if v.normal.length>1e-7 else Vector((0,0,1))
                    px=(p.x-cx)*ref_scale;py=(p.y-cy)*ref_scale;pz=(p.z-zmin)*ref_scale
                    rx=px*cs-py*sn+ox;ry=px*sn+py*cs
                    nx=n.x*cs-n.y*sn;ny=n.x*sn+n.y*cs;nz=n.z
                    q=(rx,pz-sink,-ry-oy)
                    norm=(nx,nz,-ny)
                    tex=uv.data[li].uv if uv else (0.0,0.0)
                    vertices.append((*q,*norm,*tex))
                triangles.extend([base,base+1,base+2])
            bpy.data.objects.remove(obj,do_unlink=True)
    with open(path,'wb') as f:
        f.write(b'AKV1');f.write(struct.pack('<3i',len(vertices),1,0))
        for v in vertices:f.write(struct.pack('<8f',*v))
        f.write(struct.pack('<i',len(triangles)));f.write(struct.pack('<%di'%len(triangles),*triangles))
    print('[AK] Prop',path.name,len(vertices)//3,'segitiga',flush=True)

for name in ['island_tree_01','fern_02','island_tree_02','grass_medium_01','grass_medium_02','anthurium_botany_01','boulder_01','rock_moss_set_01','dead_tree_trunk','root_cluster_01','tree_stump_01','dry_branches_medium_01','rock_moss_set_02','shrub_01','shrub_03']:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    path=ROOT/'Assets/ThirdParty/CC0/Model'/f'{name}.fbx.source'
    if not path.exists():raise RuntimeError('Aset scan hilang: '+str(path))
    bpy.ops.import_scene.fbx(filepath=str(path))
    objects=[o for o in bpy.data.objects if o.type=='MESH']
    if name=='island_tree_01':
        write_mesh(OUT/'PohonScan_LOD0.bytes',objects,5200,True,0.0)
        write_mesh(OUT/'PohonScan_LOD1.bytes',objects,950,True,0.0)
        write_mesh(OUT/'PohonScanB_LOD0.bytes',objects,4800,True,0.65)
        write_mesh(OUT/'PohonScanB_LOD1.bytes',objects,900,True,0.65)
    elif name=='island_tree_02':
        write_mesh(OUT/'PohonScanC_LOD0.bytes',objects,5200,True,0.0,20.5)
        write_mesh(OUT/'PohonScanC_LOD1.bytes',objects,950,True,0.0,20.5)
    elif name=='fern_02':
        write_mesh(OUT/'PakisScan_LOD0.bytes',objects[1:2],680)
        write_mesh(OUT/'PakisScan_LOD1.bytes',objects[1:2],180)
    elif name=='grass_medium_01':
        write_prop(OUT/'RumputScanA.bytes',objects,440,0.44,cluster=3,pick=0,sink=0.02,size_axis='h')
    elif name=='grass_medium_02':
        write_prop(OUT/'RumputScanB.bytes',objects,360,0.32,cluster=3,pick=0,sink=0.02,size_axis='h')
    elif name=='anthurium_botany_01':
        write_prop(OUT/'KeladiScan.bytes',objects,520,0.78,cluster=1,pick=0,sink=0.03,size_axis='h')
    elif name=='boulder_01':
        write_prop(OUT/'BatuBesarScan.bytes',objects,640,1.9,cluster=1,pick=0,sink=0.10,size_axis='w')
    elif name=='rock_moss_set_01':
        if len(objects)==1:
            bpy.ops.object.select_all(action='DESELECT');objects[0].select_set(True);bpy.context.view_layer.objects.active=objects[0]
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.mesh.select_all(action='SELECT')
            bpy.ops.mesh.separate(type='LOOSE')
            bpy.ops.object.mode_set(mode='OBJECT')
            objects=sorted([o for o in bpy.data.objects if o.type=='MESH' and len(o.data.polygons)>=12],key=lambda o:len(o.data.polygons),reverse=True)
        write_prop(OUT/'BatuLumutA.bytes',objects,380,1.05,cluster=1,pick=0,sink=0.06,size_axis='w')
        write_prop(OUT/'BatuLumutB.bytes',objects,380,0.85,cluster=1,pick=1,sink=0.05,size_axis='w')
    elif name=='dead_tree_trunk':
        write_prop(OUT/'KayuTumbangScan.bytes',objects,620,3.4,cluster=1,pick=0,sink=0.06,size_axis='w')
    elif name=='root_cluster_01':
        write_prop(OUT/'AkarScan.bytes',objects,720,2.6,cluster=1,pick=0,sink=0.06,size_axis='w')
    elif name=='tree_stump_01':
        write_prop(OUT/'TunggulScan.bytes',objects,700,1.15,cluster=1,pick=0,sink=0.10,size_axis='w')
    elif name=='dry_branches_medium_01':
        write_prop(OUT/'RantingScanA.bytes',objects,260,1.7,cluster=1,pick=0,sink=0.02,size_axis='w')
        write_prop(OUT/'RantingScanB.bytes',objects,260,1.3,cluster=1,pick=1,sink=0.02,size_axis='w')
    elif name=='shrub_01':
        write_mesh(OUT/'SemakScanA_LOD0.bytes',objects,900)
        write_mesh(OUT/'SemakScanA_LOD1.bytes',objects,220)
    elif name=='shrub_03':
        write_mesh(OUT/'SemakScanB_LOD0.bytes',objects,700)
        write_mesh(OUT/'SemakScanB_LOD1.bytes',objects,180)
    elif name=='rock_moss_set_02':
        write_prop(OUT/'BatuLumutC.bytes',objects,420,1.25,cluster=1,pick=0,sink=0.07,size_axis='w',split_rocks=True)
        write_prop(OUT/'BatuLumutD.bytes',objects,360,0.75,cluster=1,pick=1,sink=0.05,size_axis='w',split_rocks=True)
