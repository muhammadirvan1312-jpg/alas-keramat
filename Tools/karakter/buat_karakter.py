"""Panggang inti MakeHuman CC0 ke rig gameplay 24 sendi, dijalankan Blender headless.
Tidak menyertakan kode plugin di APK. Semua topology, target, proxy dan skin dikunci hash.
"""
import bpy, json, struct, gzip, re, math
from pathlib import Path
from mathutils import Vector, Matrix
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Assets/ThirdParty/Characters'
OUT=ROOT/'Assets/ThirdParty/Prepared'
OUT.mkdir(parents=True,exist_ok=True)
BONES=['pelvis','spine_01','spine_03','neck_01','head',None,'clavicle_l','clavicle_r','upperarm_l','upperarm_r','lowerarm_l','lowerarm_r','hand_l','hand_r','thigh_l','thigh_r','calf_l','calf_r','foot_l','foot_r',None,None,None,None]

def obj_read(path):
    vertices=[];uv=[];faces=[];groups={};group=''
    for line in path.read_text().splitlines():
        a=line.split()
        if not a:continue
        if a[0]=='v':vertices.append(Vector(map(float,a[1:4])))
        elif a[0]=='vt':uv.append(tuple(map(float,a[1:3])))
        elif a[0]=='g':group=a[1]
        elif a[0]=='f':
            f=[tuple(int(x)-1 for x in token.split('/')[:2]) for token in a[1:]]
            faces.append((group,f));groups.setdefault(group,set()).update(p[0] for p in f)
    return vertices,uv,faces,groups

BASE,UV,FACES,GROUP=obj_read(SOURCE/'Core/base.obj')
WEIGHTS=json.loads((SOURCE/'Core/weights.game_engine.json').read_text())['weights']
RIG=json.loads((SOURCE/'Core/rig.game_engine.json').read_text())
MAP={n:i for i,n in enumerate(BONES) if n}
for n in WEIGHTS:
    if n not in MAP:
        if n.startswith(('index','middle','pinky','ring','thumb')):MAP[n]=12 if n.endswith('_l') else 13
        elif n=='spine_02':MAP[n]=1
        elif n.startswith('ball_'):MAP[n]=18 if n.endswith('_l') else 19
        else:MAP[n]=0
W=[{} for _ in BASE]
for n,entries in WEIGHTS.items():
    for vi,weight in entries:W[vi][MAP[n]]=W[vi].get(MAP[n],0)+weight
for weights in W:
    s=sum(weights.values())
    if not s:weights[4]=1
    else:
        for k in weights:weights[k]/=s

def target(vertices,name,weight):
    if not weight:return
    with gzip.open(SOURCE/'Core'/name,'rt') as f:
        for line in f:
            a=line.split()
            if len(a)==4:vertices[int(a[0])]+=Vector(map(float,a[1:]))*weight

def proxy(name,kind,base):
    folder=SOURCE/'System'/kind/name
    v,uv,faces,groups=obj_read(folder/(name+'.obj'))
    maps=[];delete=set();section='';scale=Vector((1,1,1))
    for line in (folder/(name+'.mhclo')).read_text().splitlines():
        a=line.split()
        if not a or a[0].startswith('#'):continue
        if a[0] in ['x_scale','y_scale','z_scale']:
            axis='xyz'.index(a[0][0]);scale[axis]=abs(base[int(a[1])][axis]-base[int(a[2])][axis])/float(a[3])
        elif a[0] in ['verts','delete_verts']:section=a[0]
        elif a[0][0].isdigit() or a[0][0]=='-':
            if section=='verts':maps.append(a)
            elif section=='delete_verts':
                i=0
                while i<len(a):
                    if i+2<len(a) and a[i+1]=='-':delete.update(range(int(a[i]),int(a[i+2])+1));i+=3
                    else:delete.add(int(a[i]));i+=1
    if len(v)!=len(maps):raise RuntimeError('Jumlah proxy tidak cocok '+name)
    weights=[]
    for i,a in enumerate(maps):
        if len(a)==1:idx=[int(a[0])];blend=[1.0];delta=Vector((0,0,0))
        else:idx=list(map(int,a[:3]));blend=list(map(float,a[3:6]));delta=Vector(map(float,a[6:9]))
        v[i]=sum((base[k]*b for k,b in zip(idx,blend)),Vector((0,0,0)))+Vector(tuple(delta[d]*scale[d] for d in range(3)))
        mix={}
        for k,b in zip(idx,blend):
            for bone,weight in W[k].items():mix[bone]=mix.get(bone,0)+max(0,b)*weight
        weights.append(mix)
    return v,uv,faces,weights,delete

def reduce_part(posed,uv,faces,ws,limit):
    count=sum(len(f)-2 for _,f in faces)
    if count<=limit:return posed,uv,faces,ws
    mesh=bpy.data.meshes.new('GarmentLOD');mesh.from_pydata(posed,[],[[p[0] for p in f] for _,f in faces]);mesh.update()
    layer=mesh.uv_layers.new()
    for poly,(_,face) in zip(mesh.polygons,faces):
        for li,(_,ui) in zip(poly.loop_indices,face):layer.data[li].uv=uv[ui]
    obj=bpy.data.objects.new('GarmentLOD',mesh);bpy.context.collection.objects.link(obj)
    for bone in range(24):
        group=obj.vertex_groups.new(name=str(bone))
        for i,w in enumerate(ws):
            if w.get(bone,0)>0:group.add([i],w[bone],'REPLACE')
    bpy.context.view_layer.objects.active=obj
    mod=obj.modifiers.new('LOD','DECIMATE');mod.ratio=limit/count;mod.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=mod.name)
    m=obj.data;newp=[v.co.copy() for v in m.vertices];neww=[{int(obj.vertex_groups[g.group].name):g.weight for g in v.groups} for v in m.vertices]
    newuv=[];newfaces=[];layer=m.uv_layers.active
    for poly in m.polygons:
        face=[]
        for vi,li in zip(poly.vertices,poly.loop_indices):newuv.append(tuple(layer.data[li].uv));face.append((vi,len(newuv)-1))
        newfaces.append(('',face))
    bpy.data.objects.remove(obj,do_unlink=True)
    return newp,newuv,newfaces,neww

def make(name,recipe):
    base=[v.copy() for v in BASE]
    target(base,'asian-female-young.target.gz' if recipe['female'] else 'asian-male-young.target.gz',1-recipe['old'])
    target(base,'asian-male-old.target.gz',recipe['old'])
    body_index=GROUP['body'];bottom=min(base[i].y for i in body_index);height=max(base[i].y for i in body_index)-bottom
    scale=recipe['height']/height
    def to_unity(v):return Vector((-v.x*scale*recipe['width'],(v.y-bottom)*scale,v.z*scale))
    def center(group):
        points=GROUP[group];return to_unity(sum((base[i] for i in points),Vector((0,0,0)))/len(points))
    rest=[];tail=[]
    for bone in BONES:
        if bone:
            spec=RIG[bone]
            def joint(info):
                if 'cube_name' in info:return center(info['cube_name'])
                return to_unity(sum((base[i] for i in info['vertex_indices']),Vector((0,0,0)))/len(info['vertex_indices']))
            rest.append(joint(spec['head']));tail.append(joint(spec['tail']))
        else:rest.append(Vector((0,0,0)));tail.append(Vector((0,1,0)))
    source=[p.copy() for p in rest];dest=[p.copy() for p in rest];rot=[Matrix.Identity(3) for _ in BONES]
    for upper,fore,hand in [(8,10,12),(9,11,13)]:
        # Rotate the source A-pose into the gameplay I-pose, preserving segment lengths.
        rot[upper]=(tail[upper]-source[upper]).normalized().rotation_difference(Vector((0,-1,0))).to_matrix()
        dest[fore]=dest[upper]+rot[upper]@(source[fore]-source[upper])
        rot[fore]=(tail[fore]-source[fore]).normalized().rotation_difference(Vector((0,-1,0))).to_matrix()
        dest[hand]=dest[fore]+rot[fore]@(source[hand]-source[fore]);rot[hand]=rot[fore]
    for i in [5,20,21,22,23]:
        dest[i]=dest[4].copy()
    dest[5]+=Vector((0,0.01,0.025));dest[20]+=Vector((-0.032,0.08,0.08));dest[21]+=Vector((0.032,0.08,0.08));dest[22]=dest[20]+Vector((0,0.025,0));dest[23]=dest[21]+Vector((0,0.025,0))
    def deform(v,w):
        p=to_unity(v);s=sum(w.values())
        return sum(((rot[k]@(p-source[k])+dest[k])*(value/s) for k,value in w.items()),Vector((0,0,0))) if s else p
    clothes=proxy(recipe['outfit'],'clothes',base);shoes=proxy('shoes01','clothes',base);hair=proxy(recipe['hair'],'hair',base);eyes=proxy('low-poly','eyes',base)
    hidden=clothes[-1]|shoes[-1]
    parts=[(base,UV,[(g,f) for g,f in FACES if g=='body' and not any(p[0] in hidden for p in f)],W,0),(*clothes[:4],1),(*shoes[:4],2),(*hair[:4],3),(*eyes[:4],4)]
    if name=='Ningsih':
        body=[(g,f) for g,f in FACES if g=='body' and all((base[vi].y-bottom)/height>0.815 or W[vi].get(12,0)+W[vi].get(13,0)>0.5 for vi,_ in f)]
        garment,guv,gfaces,gweights,_=clothes
        hem=min((v.y-bottom)/height for v in garment)
        for v in garment:
            t=(v.y-bottom)/height
            if t<0.53:v.y=bottom+height*(0.055+(t-hem)/(0.53-hem)*(0.53-0.055))
        tiled=[];faces=[]
        for group,face in gfaces:
            skirt=sum((garment[vi].y-bottom)/height for vi,_ in face)/len(face)<0.53
            newface=[]
            for vi,_ in face:
                q=garment[vi];tiled.append(((q.x*0.3)%0.48+(0.5 if skirt else 0),(q.y*0.3)%1));newface.append((vi,len(tiled)-1))
            faces.append((group,newface))
        parts=[(base,UV,body,W,0),(garment,tiled,faces,gweights,1),(*shoes[:4],2),(*hair[:4],3),(*eyes[:4],4)]
    output=[];original=[];indices=[[] for _ in range(5)]
    for vs,uv,faces,ws,material in parts:
        posed=[deform(v,w) for v,w in zip(vs,ws)]
        if material in [1,2,3]:posed,uv,faces,ws=reduce_part(posed,uv,faces,ws,{1:6500,2:1400,3:2000}[material])
        # Real topology with smooth vertex normals; retain UV seams at output corners.
        normals=[Vector((0,0,0)) for _ in posed]
        triangles=[]
        for group,face in faces:
            for j in range(1,len(face)-1):
                tri=[face[0],face[j+1],face[j]] # reflection of X reverses winding
                a,b,c=(posed[p[0]] for p in tri);normal=(b-a).cross(c-a)
                for vi,_ in tri:normals[vi]+=normal
                triangles.append(tri)
        for tri in triangles:
            for vi,ui in tri:
                weight=sorted(ws[vi].items(),key=lambda p:p[1],reverse=True)[:4];total=sum(max(0,w) for _,w in weight)
                if not total:weight=[(4,1)];total=1
                bone=[k for k,w in weight]+[0]*4;value=[max(0,w)/total for k,w in weight]+[0]*4
                original.append(vi if material==0 else -1)
                n=normals[vi].normalized();output.append((*posed[vi],*n,*uv[ui],*bone[:4],*value[:4]));indices[material].append(len(output)-1)
    with open(OUT/(name+'.bytes'),'wb') as f:
        f.write(b'AKV1');f.write(struct.pack('<3i',len(output),5,24))
        for p in dest:f.write(struct.pack('<3f',*p))
        for row in output:f.write(struct.pack('<8f4i4f',*row))
        for t in indices:f.write(struct.pack('<i',len(t)));f.write(struct.pack('<%di'%len(t),*t))
        shapes={
            'Kedip':{'eye-left-closure':1,'eye-right-closure':1},
            'Takut':{'eye-left-opened-up':0.65,'eye-right-opened-up':0.65,'eyebrows-left-inner-up':0.5,'eyebrows-right-inner-up':0.5},
            'Ngeri':{'eye-left-opened-up':1,'eye-right-opened-up':1,'mouth-open':0.3},
            'Marah':{'eyebrows-left-down':0.9,'eyebrows-right-down':0.9},
            'Sedih':{'eyebrows-left-inner-up':0.6,'eyebrows-right-inner-up':0.6,'mouth-depression':0.5},
            'Menangis':{'eyebrows-left-inner-up':0.8,'eyebrows-right-inner-up':0.8,'eye-left-slit':0.5,'eye-right-slit':0.5},
            'Lega':{'mouth-corner-puller':0.2},'Senyum':{'mouth-corner-puller':0.6},
            'Sakit':{'eye-left-slit':0.7,'eye-right-slit':0.7,'mouth-open':0.2},
            'Lelah':{'eye-left-slit':0.55,'eye-right-slit':0.55},
            'Curiga':{'eyebrows-left-down':0.6,'eye-right-slit':0.4},
            'Teriak':{'mouth-open':0.4,'eyebrows-left-down':0.5,'eyebrows-right-down':0.5},
            'Bicara':{'mouth-open':0.22}
        }
        f.write(struct.pack('<i',len(shapes)))
        for shape,targets in shapes.items():
            deltas={}
            for targetname,value in targets.items():
                with gzip.open(SOURCE/'Core/Expressions'/(targetname+'.target.gz'),'rt') as targetfile:
                    for line in targetfile:
                        a=line.split()
                        if len(a)==4:
                            vi=int(a[0]);delta=Vector(map(float,a[1:]));deltas[vi]=deltas.get(vi,Vector((0,0,0)))+Vector((-delta.x*recipe['width'],delta.y,delta.z))*scale*value
            entries=[(i,deltas[vi]) for i,vi in enumerate(original) if vi in deltas]
            encoded=shape.encode('utf8');f.write(struct.pack('<i',len(encoded)));f.write(encoded);f.write(struct.pack('<i',len(entries)))
            for index,delta in entries:f.write(struct.pack('<i3f',index,*delta))

    report={'name':name,'triangles':len(output)//3,'height':recipe['height'],'materials':5,'maxInfluences':4,'source':'MakeHuman CC0'}
    (OUT/(name+'.json')).write_text(json.dumps(report,indent=2));print('[AK]',report,flush=True)

if __name__=='__main__':
    for name,recipe in json.loads((ROOT/'Tools/karakter/resep.json').read_text()).items():make(name,recipe)
