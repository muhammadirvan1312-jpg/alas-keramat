"""Panggang MakeHuman CC0 & model horor ke rig gameplay 24 sendi (format AKV1), dijalankan Blender headless.
Tidak menyertakan kode plugin di APK. Semua topology, target, proxy dan skin dikunci hash.
"""
import bpy, json, struct, gzip, math
from pathlib import Path
from mathutils import Vector, Matrix

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Assets/ThirdParty/Characters'
OUT=ROOT/'Assets/ThirdParty/Prepared'
OUT.mkdir(parents=True,exist_ok=True)
BONES=['pelvis','spine_01','spine_03','neck_01','head',None,'clavicle_l','clavicle_r','upperarm_l','upperarm_r','lowerarm_l','lowerarm_r','hand_l','hand_r','thigh_l','thigh_r','calf_l','calf_r','foot_l','foot_r',None,None,None,None]
BONES_LENGAN=tuple(i for i,name in enumerate(BONES) if name and name.startswith(('upperarm_','lowerarm_','hand_')))

def kulit_ningsih_terlihat(tinggi_rel,x,z,weights):
    # Mask pakaian tetap diperiksa pemanggil; lengan telanjang tidak ikut dipotong bersama dada.
    lengan=sum(weights.get(bone,0) for bone in BONES_LENGAN)>0.25
    return tinggi_rel>0.822 or (abs(x)<0.65 and z>0.1 and tinggi_rel>0.75) or lengan

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
    if count<=limit or count<12:return posed,uv,faces,ws
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

class PartBuilder:
    """Pembangun mesh tambahan dalam ruang MakeHuman CCW (otomatis terlipat X ke Unity)."""
    def __init__(self):
        self.vs=[];self.uv=[];self.faces=[];self.ws=[]
    def add_v(self,p,u,v,w):
        idx=len(self.vs);self.vs.append(Vector(p));self.uv.append((u,v));self.ws.append(dict(w));return idx
    def add_quad(self,i00,i10,i11,i01):
        self.faces.append(('',[(i00,i00),(i10,i10),(i11,i11),(i01,i01)]))
    def add_tri(self,i0,i1,i2):
        self.faces.append(('',[(i0,i0),(i1,i1),(i2,i2)]))
    def tube(self,rings,radial,uv_rect=(0.05,0.05,0.9,0.9),closed=False):
        u0,v0,du,dv=uv_rect
        grid=[]
        nr=len(rings)
        for ri,(pts,w) in enumerate(rings):
            vv=v0+dv*(ri/max(1,nr-1))
            row=[]
            for si,p in enumerate(pts):
                uu=u0+du*(si/radial)
                row.append(self.add_v(p,uu,vv,w))
            grid.append(row)
        for ri in range(nr-1):
            for si in range(radial):
                self.add_quad(grid[ri][si],grid[ri][si+1],grid[ri+1][si+1],grid[ri+1][si])
        if closed and nr>=2:
            cb=self.add_v(sum(rings[0][0][:-1],Vector((0,0,0)))/radial,u0+du*0.5,v0,rings[0][1])
            ct=self.add_v(sum(rings[-1][0][:-1],Vector((0,0,0)))/radial,u0+du*0.5,v0+dv,rings[-1][1])
            for si in range(radial):
                self.add_tri(cb,grid[0][si+1],grid[0][si])
                self.add_tri(ct,grid[-1][si],grid[-1][si+1])
    def ribbon_double(self,spine,half_w,bone_fn,uv_rect=(0.05,0.05,0.9,0.9)):
        """Pita dua-sisi untuk helai rambut panjang kuntilanak & bulu genderuwo."""
        u0,v0,du,dv=uv_rect
        n=len(spine)
        f_rows=[];b_rows=[]
        for i,p in enumerate(spine):
            t=i/max(1,n-1)
            tang=(spine[min(i+1,n-1)]-spine[max(i-1,0)]).normalized()
            side=tang.cross(Vector((0,0,1)) if abs(tang.z)<0.85 else Vector((1,0,0))).normalized()
            w=half_w(t);vv=v0+dv*(1.0-t);bw=bone_fn(p,t)
            f_rows.append((self.add_v(p-side*w,u0,vv,bw),self.add_v(p+side*w,u0+du,vv,bw)))
            b_rows.append((self.add_v(p+side*w,u0+du,vv,bw),self.add_v(p-side*w,u0,vv,bw)))
        for i in range(n-1):
            self.add_quad(f_rows[i][0],f_rows[i][1],f_rows[i+1][1],f_rows[i+1][0])
            self.add_quad(b_rows[i][0],b_rows[i][1],b_rows[i+1][1],b_rows[i+1][0])
    def build(self):
        return self.vs,self.uv,self.faces,self.ws

def append_part(part,extra):
    vs,uv,faces,ws=part[:4]
    ev,euv,efaces,ews=extra
    voff=len(vs);uoff=len(uv)
    vs=list(vs)+list(ev);uv=list(uv)+list(euv);ws=list(ws)+list(ews)
    faces=list(faces)+[(g,[(vi+voff,ui+uoff) for vi,ui in f]) for g,f in efaces]
    return (vs,uv,faces,ws,*part[4:])

def sculpt_corpse(base,role):
    le=Vector((-0.308,7.284,1.310));re=Vector((0.308,7.284,1.310))
    lc=Vector((-0.44,6.76,1.08));rc=Vector((0.44,6.76,1.08))
    if role in ('pocong','kuntilanak'):
        for i in GROUP['body']:
            p=base[i]
            if p.y>6.15 and p.z>0.4:
                for eye in (le,re):
                    d=(p-eye).length
                    if d<0.44:
                        k=(1.0-d/0.44)**1.5
                        base[i].z-=0.24*k
                        base[i].y-=0.02*k
                for cheek in (lc,rc):
                    d=(p-cheek).length
                    if d<0.45:
                        k=(1.0-d/0.45)**1.4
                        base[i].x*=1.0-0.14*k
                        base[i].z-=0.18*k
                # Buka rahang bawah sedikit menyeringai memperlihatkan gigi mayat.
                if 6.18<p.y<6.58 and p.z>0.85 and abs(p.x)<0.42:
                    base[i].y-=0.12
                    base[i].z+=0.04
        for gi in GROUP.get('helper-lower-teeth',()):
            base[gi].y-=0.13
        if role=='kuntilanak':
            # Runcingkan jemari tangan agar menyerupai cakar mayat.
            for i in GROUP['body']:
                wh=W[i].get(12,0)+W[i].get(13,0)
                if wh>0.65 and base[i].y<2.6:
                    side=-1.0 if base[i].x<0 else 1.0
                    base[i].x+=side*0.18*wh
                    base[i].y-=0.22*wh
                    base[i].z+=0.14*wh
    elif role=='genderuwo':
        for i in GROUP['body']:
            p=base[i]
            if 7.38<p.y<7.82 and p.z>0.95:
                k=max(0.0,1.0-abs(p.x)/0.65)
                base[i].z+=0.22*k;base[i].y+=0.08*k
            elif 6.08<p.y<6.68 and p.z>0.75:
                base[i].y-=0.12;base[i].z+=0.16;base[i].x*=1.16
            elif 2.5<p.y<6.0:
                base[i].x*=1.10;base[i].z*=1.12
                if p.z<0:base[i].z-=0.15*((p.y-2.5)/3.5)
    elif role=='kuyang':
        sculpt_corpse(base,'kuntilanak')
    elif role=='tuyul':
        # Kepala membesar bulat & gundul, telinga runcing tajam seperti kelelawar/goblin, wajah tirus cekung!
        for i in GROUP['body']:
            p=base[i]
            if p.y>6.15:
                dx=p.x;dy=p.y-7.1;dz=p.z-0.35
                # Perbesar batok kepala 1.32x & tarik telinga kiri/kanan jadi runcing panjang!
                ear=max(0.0,(abs(p.x)-0.66)/0.25)
                base[i]=Vector((dx*1.28+(0.52*ear if p.x>0 else -0.52*ear),7.1+dy*1.22+0.42*ear,0.35+dz*1.25-0.30*ear))
        for side in (-1,1):
            pass
    elif role=='hantuair':
        sculpt_corpse(base,'pocong')
        for i in GROUP['body']:
            p=base[i]
            if 0.2<p.y<5.5:base[i].x*=1.14;base[i].z*=1.16
            if p.y>6.4 and p.z>0.3:base[i].x*=1.06;base[i].z+=0.06

def ring_ellipse(cx,cy,cz,rx,rz,radial,wiggle=0.0):
    pts=[]
    for s in range(radial+1):
        a=s*2.0*math.pi/radial
        wr=1.0+wiggle*math.sin(a*7.0)*0.08
        pts.append(Vector((cx+math.sin(a)*rx*wr,cy,cz+math.cos(a)*rz*wr)))
    return pts

def remap_u(part,scale_u,offset_u=0.0):
    vs,uv,faces,ws=part[:4]
    nuv=[(u*scale_u+offset_u,v) for u,v in uv]
    return (vs,nuv,faces,ws,*part[4:])

def add_deden_accessories(clothes,base):
    clothes=remap_u(clothes,0.88,0.0)
    pb=PartBuilder()
    head_w={4:1.0}
    rings=[]
    for ri,y,rad_s in [(0,7.46,1.02),(1,7.58,1.04),(2,7.71,1.03),(3,7.82,1.00)]:
        pts=[]
        for s in range(29):
            a=s*2.0*math.pi/28
            dy=-0.04*math.cos(a) if ri<2 else 0.0
            rx=0.77*rad_s;rz=0.95*rad_s
            pts.append(Vector((math.sin(a)*rx,y+dy,0.34+math.cos(a)*rz)))
        rings.append((pts,head_w))
    pb.tube(rings,28,(0.91,0.54,0.07,0.42),closed=False)
    return append_part(clothes,pb.build())

def add_kirogo_blangkon(clothes,base):
    clothes=remap_u(clothes,0.88,0.0)
    pb=PartBuilder()
    head_w={4:1.0}
    rings=[]
    for y,rx,rz,cz in [(7.46,0.79,0.97,0.33),(7.68,0.82,1.00,0.31),(7.98,0.77,0.95,0.28),(8.28,0.62,0.77,0.25),(8.50,0.34,0.42,0.24)]:
        rings.append((ring_ellipse(0.0,y,cz,rx,rz,28,0.15),head_w))
    pb.tube(rings,28,(0.91,0.04,0.07,0.90),closed=True)
    mrings=[]
    for dz,rx,ry in [(0.0,0.16,0.16),(-0.18,0.27,0.26),(-0.36,0.22,0.22),(-0.46,0.08,0.08)]:
        pts=[Vector((math.sin(s*2.0*math.pi/12)*rx,7.38+math.cos(s*2.0*math.pi/12)*ry,-0.62+dz)) for s in range(13)]
        mrings.append((pts,head_w))
    pb.tube(mrings,12,(0.91,0.04,0.07,0.90),closed=True)
    return append_part(clothes,pb.build())

def add_kantil(hair):
    hair=remap_u(hair,0.88,0.0)
    pb=PartBuilder();hw={4:1.0}
    uv_rambut=(0.40,0.80,0.12,0.14)
    # Volume rambut mahkota & pelipis membingkai wajah agar tidak tampak botak dari depan.
    for cx,cy,cz,rx,ry,rz in [(0.0,7.94,0.22,0.84,0.56,0.92),(-0.66,7.48,0.34,0.24,0.56,0.46),(0.66,7.48,0.34,0.24,0.56,0.46)]:
        crown=[(ring_ellipse(cx,cy+dy*ry,cz,rx*rs,rz*rs,20),hw) for dy,rs in [(-0.6,0.68),(0.0,1.0),(0.55,0.84),(0.92,0.42)]]
        pb.tube(crown,20,uv_rambut,closed=True)
    for side in (-1.0,1.0):
        part=[(ring_ellipse(side*(0.24+t*0.42),8.12-t*0.72,0.98-t*0.26,0.22*(1.0-0.3*t),0.14*(1.0-0.2*t),12),hw) for t in (0.0,0.4,0.8,1.0)]
        pb.tube(part,12,uv_rambut,closed=True)
    # Sanggul konde Jawa padat di belakang kepala + 3 kuncup melati kantil putih.
    bun=[(ring_ellipse(0.0,7.20+math.sin(t*math.pi)*0.04,-0.78-t*0.58,0.52*math.sin(t*math.pi*0.85+0.25),0.46*math.sin(t*math.pi*0.85+0.25),20),hw) for t in (0.0,0.28,0.62,0.90,1.0)]
    pb.tube(bun,20,uv_rambut,closed=True)
    for k in range(3):
        by=7.22-k*0.14;bz=-0.76-k*0.04;bx=-0.56
        pts=[]
        for ri,dy,r in [(0,0.0,0.025),(1,-0.08,0.078),(2,-0.17,0.045),(3,-0.23,0.012)]:
            pts.append((ring_ellipse(bx,by+dy,bz,r,r,8),hw))
        pb.tube(pts,8,(0.90,0.12,0.07,0.75),closed=True)
    return append_part(hair,pb.build())

def add_topeng(shoes,base,male=True):
    shoes=remap_u(shoes,0.62,0.0)
    pb=PartBuilder();hw={4:1.0}
    sx=0.68 if male else 0.62
    cols=11;rows=9
    grid=[]
    for r in range(rows):
        v_t=r/(rows-1)
        y=6.30+v_t*1.65
        row=[]
        for c in range(cols):
            u_t=(c/(cols-1))*2.0-1.0
            ang=u_t*0.88
            rx=sx*(1.0-0.25*((v_t-0.55)**2))
            x=math.sin(ang)*rx
            nose=0.22*math.exp(-((u_t/0.22)**2)-(((v_t-0.46)/0.20)**2))
            eye_pit=-0.12*(math.exp(-(((u_t-0.38)/0.16)**2)-(((v_t-0.64)/0.11)**2))+math.exp(-(((u_t+0.38)/0.16)**2)-(((v_t-0.64)/0.11)**2)))
            brow=0.10*math.exp(-(((v_t-0.78)/0.09)**2))*(1.0-0.4*abs(u_t))
            mouth=-0.09*math.exp(-((u_t/0.32)**2)-(((v_t-0.20)/0.08)**2))
            z=0.68+math.cos(ang)*0.82+nose+eye_pit+brow+mouth
            row.append(pb.add_v((x,y,z),0.66+0.31*(c/(cols-1)),0.05+0.90*v_t,hw))
        grid.append(row)
    for r in range(rows-1):
        for c in range(cols-1):
            if r==5 and c in (2,3,6,7):continue
            pb.add_quad(grid[r][c],grid[r][c+1],grid[r+1][c+1],grid[r+1][c])
    return append_part(shoes,pb.build())

def build_pocong(base,bottom,height):
    # Kain kafan anatomis (submesh 1) membungkus tubuh terikat + 5 tali pocong (submesh 2) + jambul ikat kepala atas + bukaan wajah cekung.
    shroud=PartBuilder();ropes=PartBuilder();wisps=PartBuilder()
    radial=32
    def bw(y):
        t=(y-bottom)/height
        if t<0.52:return {0:1.0}
        if t<0.62:return {0:0.4,1:0.6}
        if t<0.78:return {1:0.3,2:0.7}
        if t<0.86:return {2:0.3,3:0.7}
        return {4:1.0}
    # Profil kafan dari kaki terikat hingga puncak jambul ikat atas kepala: (y_mh, rx, rz, cz, cinch_factor)
    prof=[
        (-8.05,0.48,0.52,0.18,0.72),(-7.20,0.68,0.72,0.16,0.92),(-6.85,0.46,0.50,0.15,0.68), # tali pergelangan kaki
        (-5.20,0.92,0.78,0.14,1.00),(-3.65,0.64,0.62,0.14,0.75),(-3.30,0.98,0.84,0.15,1.00), # tali lutut
        (-1.20,1.22,0.98,0.16,1.00),(0.55,1.34,1.06,0.18,1.00),(1.15,0.96,0.82,0.18,0.82),   # tali pinggang/siku terikat
        (2.80,1.46,1.08,0.18,1.00),(4.55,1.68,1.12,0.18,1.00),(5.40,1.22,0.96,0.22,0.94),
        (5.75,0.56,0.62,0.24,0.76),                                                           # tali leher
        (6.18,0.76,0.95,0.30,0.98),(6.75,0.82,1.06,0.34,1.00),(7.35,0.84,1.08,0.34,1.00),
        (7.92,0.80,1.00,0.32,0.98),(8.42,0.34,0.38,0.28,0.55),                                # tali ikatan atas kepala (jambul)
        (8.85,0.48,0.50,0.22,0.90),(9.38,0.64,0.56,0.12,1.05),(9.65,0.26,0.24,0.04,0.65)     # kuncir kain mekar atas
    ]
    grid=[]
    for ri,(y,rx,rz,cz,cinch) in enumerate(prof):
        w=bw(y);row=[]
        v_uv=(ri)/(len(prof)-1)
        for s in range(radial+1):
            a=s*2.0*math.pi/radial
            fold=1.0+0.045*math.sin(a*9.0+ri*0.7)+0.025*math.cos(a*15.0)
            flare=1.22 if y>8.75 else 1.0
            px=math.sin(a)*rx*fold*flare;pz=cz+math.cos(a)*rz*fold*flare
            py=y
            # Buka jendela wajah dengan lipatan kain mundur di sisi pipi/dahi/dagu.
            if 6.20<y<7.90 and math.cos(a)>0.38:
                px*=1.06;pz=min(pz,0.92)
            row.append(shroud.add_v((px,py,pz),s/radial,v_uv,w))
        grid.append(row)
    for ri in range(len(prof)-1):
        y_mid=(prof[ri][0]+prof[ri+1][0])*0.5
        for s in range(radial):
            a_mid=(s+0.5)*2.0*math.pi/radial
            # Kosongkan poligon di jendela wajah depan agar wajah busuk & rongga mata terlihat jelas.
            if 6.24<=y_mid<=7.88 and math.cos(a_mid)>0.52:continue
            shroud.add_quad(grid[ri][s],grid[ri][s+1],grid[ri+1][s+1],grid[ri+1][s])
    # Tutup alas bawah & ujung jambul atas.
    cb=shroud.add_v((0,-8.08,0.18),0.5,0.0,bw(-8.05))
    ct=shroud.add_v((0,9.72,0.02),0.5,1.0,bw(9.65))
    for s in range(radial):
        shroud.add_tri(cb,grid[0][s+1],grid[0][s])
        shroud.add_tri(ct,grid[-1][s],grid[-1][s+1])

    # 5 tali pengikat kain kafan 3D nyata.
    for ry,rx,rz,cz in [(-6.85,0.50,0.54,0.15),(-3.65,0.68,0.66,0.14),(1.15,1.00,0.86,0.18),(5.75,0.60,0.66,0.24),(8.42,0.38,0.42,0.28)]:
        rrings=[(ring_ellipse(0.0,ry+dy,cz,rx+dr,rz+dr,24,0.06),bw(ry)) for dy,dr in [(-0.08,0.0),(0.0,0.07),(0.08,0.0)]]
        ropes.tube(rrings,24,(0.1,0.1,0.8,0.8),closed=True)
    # Rumbai ikatan atas masuk ke submesh 3 agar 5 submesh terisi bersih.
    for k in range(4):
        a=k*math.pi*0.5+0.4
        sp=[Vector((math.sin(a)*(0.12+0.12*t),8.42+t*1.15,0.24+math.cos(a)*(0.12+0.12*t)-0.16*t*t)) for t in (0.0,0.4,0.8,1.0)]
        wisps.ribbon_double(sp,lambda t:0.06*(1.0-0.5*t),lambda p,t:{4:1.0},(0.1,0.1,0.8,0.8))
    return shroud.build(),ropes.build(),wisps.build()

def add_kuntilanak_hair(hair):
    pb=PartBuilder()
    def bw(p,t):
        if p.y>6.8:return {4:1.0}
        if p.y>5.2:return {3:0.5,4:0.5}
        if p.y>2.2:return {2:0.65,3:0.35}
        return {1:0.55,2:0.45}
    # 24 helai rambut belakang & bahu menjuntai sampai pinggang/lutut + 10 helai liar menutupi sebagian wajah!
    for i in range(24):
        f=(i/23.0)*2.0-1.0 # kiri ke kanan melengkung di ubun-ubun belakang/samping
        ang=math.pi+f*1.85 # -Z adalah punggung; sisi +/-X adalah bahu
        sx=math.sin(ang)*0.76;sz=0.34+math.cos(ang)*0.94
        length=5.6+1.3*math.sin(i*2.1)
        out=Vector((math.sin(ang),0.0,math.cos(ang))).normalized()
        pts=[]
        for seg in range(8):
            t=seg/7.0
            rad_push=0.14+0.68*math.sin(t*math.pi*0.75)
            wave=0.16*math.sin(t*7.5+i*1.3)
            p=Vector((sx+out.x*rad_push+wave,8.32-t*length,sz+out.z*rad_push-0.18*t))
            pts.append(p)
        pb.ribbon_double(pts,lambda t:0.24*(1.0-0.35*t),bw,(0.05,0.05,0.9,0.9))
    # Tirai rambut depan menjuntai liar melewati pelipis & setengah wajah (mata mengintip di sela helai!).
    for i in range(10):
        side=-1.0 if i<6 else 1.0 # lebih rapat di sisi kiri wajah seperti ciri khas kuntilanak
        fx=side*(0.10+(i%5)*0.15)
        pts=[]
        for seg in range(7):
            t=seg/6.0
            pts.append(Vector((fx+0.06*math.sin(t*5.0+i),8.22-t*(2.8+(i%3)*0.65),1.16+0.34*math.sin(t*math.pi*0.6)+0.06*math.cos(t*4.0+i))))
        pb.ribbon_double(pts,lambda t:0.13*(1.0-0.45*t),lambda p,t:{4:0.85,3:0.15} if p.y>6.0 else {3:0.5,2:0.5},(0.05,0.05,0.9,0.9))
    return append_part(hair,pb.build())

def add_genderuwo_features(shoes,hair):
    horn_b=PartBuilder();hw={4:1.0};jw={5:1.0}
    # 2 tanduk iblis melengkung dari pelipis dahi + 2 taring rahang bawah mencuat!
    for side in (-1.0,1.0):
        hr=[]
        for k in range(6):
            t=k/5.0;r=0.16*(1.0-0.78*t)+0.02
            cx=side*(0.42+0.62*t*t);cy=7.72+1.25*t-0.15*t*t;cz=0.95-0.95*t*t
            pts=[Vector((cx+math.sin(s*2*math.pi/8)*r,cy,cz+math.cos(s*2*math.pi/8)*r)) for s in range(9)]
            hr.append((pts,hw))
        horn_b.tube(hr,8,(0.1,0.1,0.5,0.5),closed=True)
        # Taring bawah.
        tr=[]
        for k in range(4):
            t=k/3.0;r=0.055*(1.0-0.85*t)
            pts=[Vector((side*(0.21+0.03*t)+math.sin(s*2*math.pi/6)*r,6.32+0.26*t,1.26+0.08*t+math.cos(s*2*math.pi/6)*r)) for s in range(7)]
            tr.append((pts,jw))
        horn_b.tube(tr,6,(0.1,0.1,0.5,0.5),closed=True)
    shoes=append_part(shoes,horn_b.build())

    # Surai & bulu kasar lebat di kepala, tengkuk, bahu, punggung & lengan!
    fur=PartBuilder()
    zones=[(0.0,7.9,0.1,0.75,0.7,4,22),(0.0,6.1,0.0,1.15,0.85,3,20),(0.0,4.8,-0.4,1.65,0.75,2,28),(-2.2,4.4,0.1,0.55,0.45,8,12),(2.2,4.4,0.1,0.55,0.45,9,12)]
    for cx,cy,cz,rx,ry,bone,cnt in zones:
        for k in range(cnt):
            a=(k/cnt)*2.0*math.pi+bone*0.7
            ox=cx+math.sin(a)*rx*0.85;oy=cy+(((k%3)-1)*0.45)*ry;oz=cz+math.cos(a)*rx*0.55
            out=Vector((math.sin(a)*0.4,-0.85,math.cos(a)*0.35)).normalized()
            pts=[Vector((ox,oy,oz))+out*(0.38*t) for t in (0.0,0.4,0.8,1.0)]
            fur.ribbon_double(pts,lambda t:0.24*(1.0-0.7*t),lambda p,t,b=bone:{b:1.0},(0.05,0.05,0.9,0.9))
    hair=append_part(hair,fur.build())
    return shoes,hair

def build_kuyang():
    # Organ dalam menggantung basah (trakea, dua paru-paru, jantung, usus melilit berayun di bawah leher terputus!)
    org=PartBuilder();hair=PartBuilder();fangs=PartBuilder()
    def obw(y):return {4:1.0} if y>5.5 else {3:0.6,2:0.4} if y>4.2 else {2:0.7,1:0.3} if y>2.2 else {1:0.6,0:0.4}
    # Batang trakea + kerongkongan dari pangkal leher putus (5.9 -> 3.8).
    t_rings=[]
    for k in range(7):
        y=5.92-k*0.36;r=0.26+0.05*math.sin(k*2.1)
        t_rings.append((ring_ellipse(0.06*math.sin(k),y,0.15,r,r*0.95,12,0.15),obw(y)))
    org.tube(t_rings,12,(0.05,0.1,0.4,0.8),closed=True)
    # Sepasang paru-paru basah menggantung di kiri-kanan dada.
    for side in (-1.0,1.0):
        lr=[]
        for k in range(5):
            t=k/4.0;y=4.55-t*1.25;r=0.14+0.34*math.sin(t*math.pi*0.92)
            lr.append((ring_ellipse(side*0.42,y,0.18,r*0.95,r*0.80,12,0.2),obw(y)))
        org.tube(lr,12,(0.45,0.1,0.5,0.8),closed=True)
    # 3 untaian usus panjang melilit turun ~1.1 m di bawah leher!
    for idx,ang0,rad_coil,len_c in [(0,0.0,0.34,4.4),(1,2.1,0.26,5.0),(2,4.2,0.20,4.8)]:
        ir=[]
        for k in range(15):
            t=k/14.0;y=3.95-t*len_c;a=ang0+t*9.5
            cx=math.cos(a)*rad_coil*(1.0-0.3*t);cz=0.15+math.sin(a)*rad_coil*0.85
            r=0.11+0.025*math.sin(t*12.0)
            ir.append((ring_ellipse(cx,y,cz,r,r,8),obw(y)))
        org.tube(ir,8,(0.1,0.1,0.8,0.8),closed=True)
    # Rambut panjang kuyang berkibar liar ke belakang & samping kepala terpenggal!
    for i in range(18):
        f=(i/17.0)*2.0-1.0;ang=math.pi+f*1.65
        sx=math.sin(ang)*0.72;sz=0.34+math.cos(ang)*0.88
        out=Vector((math.sin(ang),0.0,math.cos(ang))).normalized()
        pts=[Vector((sx+out.x*(0.2+t*1.4),8.25-t*3.8+0.35*math.sin(t*5.0+i),sz+out.z*(0.2+t*2.1))) for t in [s/6.0 for s in range(7)]]
        hair.ribbon_double(pts,lambda t:0.20*(1.0-0.4*t),lambda p,t:{4:0.85,3:0.15},(0.05,0.05,0.9,0.9))
    # Taring tajam mencuat dari rahang bawah/atas.
    for side in (-1.0,1.0):
        fr=[(ring_ellipse(side*0.20,6.65-t*0.24,1.25+0.05*t,0.04*(1.0-0.8*t),0.04*(1.0-0.8*t),6),{4:1.0}) for t in (0.0,0.5,1.0)]
        fangs.tube(fr,6,(0.2,0.2,0.4,0.4),closed=True)
    return (*org.build(),set()),(*fangs.build(),set()),(*hair.build(),set())

def add_hantu_air_algae(hair):
    pb=PartBuilder()
    def bw(p,t):return {4:1.0} if p.y>6.6 else {3:0.5,2:0.5} if p.y>4.4 else {2:0.7,1:0.3}
    for i in range(22):
        f=(i/21.0)*2.0-1.0;ang=math.pi+f*1.9
        sx=math.sin(ang)*0.78;sz=0.34+math.cos(ang)*0.96
        out=Vector((math.sin(ang),0.0,math.cos(ang))).normalized()
        pts=[Vector((sx+out.x*(0.16+0.65*t)+0.12*math.sin(t*6+i),8.35-t*5.2,sz+out.z*(0.16+0.65*t))) for t in [s/6.0 for s in range(7)]]
        pb.ribbon_double(pts,lambda t:0.22*(1.0-0.3*t),bw,(0.05,0.05,0.9,0.9))
    return append_part(hair,pb.build())

def bake_banaspati_skull():
    """Panggang tengkorak iblis hangus (non-skinned submesh 2: 0=tulang hangus, 1=mata/retak api) untuk Banaspati."""
    base=[v.copy() for v in BASE]
    target(base,'asian-male-old.target.gz',0.8)
    sculpt_corpse(base,'pocong')
    body_vs=GROUP['body'];bottom=min(base[i].y for i in body_vs);height=max(base[i].y for i in body_vs)-bottom
    scale=1.75/height;neck_y=5.95;head_c=sum((base[i] for i in body_vs if base[i].y>neck_y),Vector((0,0,0)))/max(1,sum(1 for i in body_vs if base[i].y>neck_y))
    def loc(v):return Vector((-(v.x-head_c.x)*scale*1.5,(v.y-head_c.y)*scale*1.5,(v.z-head_c.z)*scale*1.5))
    hb=PartBuilder()
    for side in (-1.0,1.0):
        hr=[]
        for k in range(5):
            t=k/4.0;r=0.14*(1.0-0.8*t)
            cx=side*(0.36+0.42*t);cy=head_c.y+1.25+0.95*t;cz=head_c.z+0.25-0.7*t*t
            hr.append(([Vector((cx+math.sin(s*2*math.pi/8)*r,cy,cz+math.cos(s*2*math.pi/8)*r)) for s in range(9)],{}))
        hb.tube(hr,8,(0.2,0.2,0.6,0.6),closed=True)
    skull_faces=[(g,f) for g,f in FACES if (g=='body' and all(base[vi].y>6.22 and abs(base[vi].x)<0.72 for vi,_ in f)) or g in ('helper-upper-teeth','helper-lower-teeth')]
    eyes=proxy('low-poly','eyes',base)
    out=[];tris=[[],[]]
    parts=[(base,UV,skull_faces,0),(*hb.build()[:3],0),(eyes[0],eyes[1],eyes[2],1)]
    for vs,uv,faces,sub in parts:
        pverts=[loc(v) for v in vs];norms=[Vector((0,0,0)) for _ in pverts];ft=[]
        for _,f in faces:
            for j in range(1,len(f)-1):
                t=[f[0],f[j+1],f[j]];a,b,c=(pverts[p[0]] for p in t);n=(b-a).cross(c-a)
                for vi,_ in t:norms[vi]+=n
                ft.append(t)
        for t in ft:
            for vi,ui in t:
                n=norms[vi].normalized();out.append((*pverts[vi],*n,*uv[ui]));tris[sub].append(len(out)-1)
    with open(OUT/'Banaspati.bytes','wb') as f:
        f.write(b'AKV1');f.write(struct.pack('<3i',len(out),2,0))
        for row in out:f.write(struct.pack('<8f',*row))
        for tr in tris:f.write(struct.pack('<i',len(tr)));f.write(struct.pack('<%di'%len(tr),*tr))
    print('[AK]',{'name':'Banaspati','triangles':len(out)//3,'materials':2,'source':'MakeHuman CC0 skull'},flush=True)

def make(name,recipe):
    role=recipe.get('role','')
    base=[v.copy() for v in BASE]
    target(base,'asian-female-young.target.gz' if recipe['female'] else 'asian-male-young.target.gz',1-recipe['old'])
    target(base,'asian-male-old.target.gz',recipe['old'])
    if role:sculpt_corpse(base,role)
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
        rot[upper]=(tail[upper]-source[upper]).normalized().rotation_difference(Vector((0,-1,0))).to_matrix()
        dest[fore]=dest[upper]+rot[upper]@(source[fore]-source[upper])
        rot[fore]=(tail[fore]-source[fore]).normalized().rotation_difference(Vector((0,-1,0))).to_matrix()
        dest[hand]=dest[fore]+rot[fore]@(source[hand]-source[fore]);rot[hand]=rot[fore]
    for i in [5,20,21,22,23]:dest[i]=dest[4].copy()
    dest[5]+=Vector((0,0.01,0.025));dest[20]+=Vector((-0.032,0.08,0.08));dest[21]+=Vector((0.032,0.08,0.08));dest[22]=dest[20]+Vector((0,0.025,0));dest[23]=dest[21]+Vector((0,0.025,0))
    def deform(v,w):
        p=to_unity(v);s=sum(w.values())
        return sum(((rot[k]@(p-source[k])+dest[k])*(value/s) for k,value in w.items()),Vector((0,0,0))) if s else p

    clothes=proxy(recipe['outfit'],'clothes',base);shoes=proxy('shoes01','clothes',base);hair=proxy(recipe['hair'],'hair',base);eyes=proxy('low-poly','eyes',base)
    hidden=clothes[-1]|shoes[-1]
    body_faces=[(g,f) for g,f in FACES if g=='body' and not any(p[0] in hidden for p in f)]

    if name=='Deden':
        clothes=add_deden_accessories(clothes,base)
    elif name=='KiRogo':
        clothes=add_kirogo_blangkon(clothes,base)
    elif name=='Ningsih':
        body_faces=[(g,f) for g,f in FACES if g=='body' and not any(p[0] in hidden for p in f) and all(
            kulit_ningsih_terlihat((base[vi].y-bottom)/height,base[vi].x,base[vi].z,W[vi])
            for vi,_ in f)]
        garment,guv,gfaces,gweights,_=clothes
        hem=min((v.y-bottom)/height for v in garment)
        for v in garment:
            t=(v.y-bottom)/height
            if t<0.53:v.y=bottom+height*(0.055+(t-hem)/(0.53-hem)*(0.53-0.055))
        # Pertahankan UV asli pakaian MakeHuman sambil membagi atlas kiri=kebaya, kanan=jarik batik!
        split_uv=[];faces=[]
        for group,face in gfaces:
            skirt=sum((garment[vi].y-bottom)/height for vi,_ in face)/len(face)<0.53
            newface=[]
            for vi,ui in face:
                u0,v0=guv[ui];split_uv.append((u0*0.47+(0.52 if skirt else 0.01),v0));newface.append((vi,len(split_uv)-1))
            faces.append((group,newface))
        clothes=(garment,split_uv,faces,gweights,clothes[-1])
        hair=add_kantil(hair)
    elif role=='pocong':
        # Wajah busuk terbuka & gigi menyeringai; badan lain tertutup rapat kain kafan + 5 tali pocong.
        body_faces=[(g,f) for g,f in FACES if (g=='body' and all(6.18<base[vi].y<7.98 and base[vi].z>0.70 for vi,_ in f)) or g in ('helper-upper-teeth','helper-lower-teeth')]
        shroud_part,rope_part,wisp_part=build_pocong(base,bottom,height)
        clothes=(*shroud_part,set());shoes=(*rope_part,set());hair=(*wisp_part,set())
    elif role=='kuntilanak':
        # Daster panjang tercabik sampai mata kaki (kaki mayat telanjang) + tirai rambut panjang acak-acakan & gigi!
        body_faces=[(g,f) for g,f in FACES if (g=='body' and not any(p[0] in clothes[-1] for p in f) and all(
            (base[vi].y-bottom)/height>0.78 or (base[vi].y-bottom)/height<0.10 or W[vi].get(12,0)+W[vi].get(13,0)>0.5
            for vi,_ in f)) or g in ('helper-upper-teeth','helper-lower-teeth')]
        garment,guv,gfaces,gweights,_=clothes
        hem=min((v.y-bottom)/height for v in garment)
        for v in garment:
            t=(v.y-bottom)/height
            if t<0.53:
                ang=math.atan2(v.x,v.z)
                tear=0.032*math.sin(ang*7.0)+0.022*math.cos(ang*13.0)
                v.x*=1.16;v.z*=1.14
                v.y=bottom+height*(0.035+tear+(t-hem)/(0.53-hem)*(0.53-0.035))
        clothes=(garment,guv,gfaces,gweights,clothes[-1])
        # Kaki telanjang: kurangi sepatu ke 2 segitiga jari di dalam daster, gunakan mesh tubuh kaki telanjang.
        shoes=([Vector((0,0.02,0)),Vector((0.01,0.02,0)),Vector((0,0.03,0))],[(0.5,0.5)]*3,[('',[(0,0),(1,1),(2,2)])],[{18:1.0},{18:1.0},{18:1.0}],set())
        hair=add_kuntilanak_hair(hair)
    elif role=='genderuwo':
        # Hanya cawat robek di pinggang & paha; dada kekar berbulu + taring + tanduk + kaki cakar!
        garment,guv,gfaces,gweights,_=clothes
        gfaces=[(g,f) for g,f in gfaces if all((garment[vi].y-bottom)/height<0.57 for vi,_ in f)]
        for v in garment:
            t=(v.y-bottom)/height
            if t<0.34:v.y+=0.08*math.sin(v.x*18.0)*height
        clothes=(garment,guv,gfaces,gweights,clothes[-1])
        body_faces=[(g,f) for g,f in FACES if (g=='body' and not any((base[vi].y-bottom)/height>0.29 and (base[vi].y-bottom)/height<0.57 for vi,_ in f)) or g in ('helper-upper-teeth','helper-lower-teeth')]
        shoes=([Vector((0,0.02,0)),Vector((0.01,0.02,0)),Vector((0,0.03,0))],[(0.5,0.5)]*3,[('',[(0,0),(1,1),(2,2)])],[{18:1.0},{18:1.0},{18:1.0}],set())
        hair=([],[],[],[],set())
        shoes,hair=add_genderuwo_features(shoes,hair)
    elif role=='sekte':
        shoes=add_topeng(shoes,base,not recipe['female'])
    elif role=='kuyang':
        # Kepala terpenggal di pangkal leher + taring + rambut melambai + isi perut menggantung basah!
        body_faces=[(g,f) for g,f in FACES if (g=='body' and all(base[vi].y>5.82 for vi,_ in f)) or g in ('helper-upper-teeth','helper-lower-teeth')]
        clothes,shoes,hair=build_kuyang()
    elif role=='tuyul':
        # Tubuh kecil bungkuk gundul, cawat pinggul robek, tanpa rambut & mata raksasa hitam mengilap!
        garment,guv,gfaces,gweights,_=clothes
        gfaces=[(g,f) for g,f in gfaces if all(0.32<(garment[vi].y-bottom)/height<0.55 for vi,_ in f)]
        clothes=(garment,guv,gfaces,gweights,clothes[-1])
        body_faces=[(g,f) for g,f in FACES if g=='body' and not any(0.36<(base[vi].y-bottom)/height<0.52 for vi,_ in f)]
        shoes=([Vector((0,0.02,0)),Vector((0.01,0.02,0)),Vector((0,0.03,0))],[(0.5,0.5)]*3,[('',[(0,0),(1,1),(2,2)])],[{18:1.0},{18:1.0},{18:1.0}],set())
        hair=([Vector((0,1.4,0)),Vector((0.01,1.4,0)),Vector((0,1.41,0))],[(0.5,0.5)]*3,[('',[(0,0),(1,1),(2,2)])],[{4:1.0},{4:1.0},{4:1.0}],set())
        # Perbesar mata Tuyul 1.4x mengelilingi pusat mata kiri/kanan hasil proxy.
        ev=eyes[0]
        l_pts=[p for p in ev if p.x<0];r_pts=[p for p in ev if p.x>=0]
        lc=sum(l_pts,Vector((0,0,0)))/max(1,len(l_pts));rc=sum(r_pts,Vector((0,0,0)))/max(1,len(r_pts))
        eyes=([((lc if v.x<0 else rc)+(v-(lc if v.x<0 else rc))*1.45) for v in ev],*eyes[1:])
    elif role=='hantuair':
        # Mayat tenggelam bengkak berkain robek & tirai lumut/ganggang sungai di kepala & bahu.
        garment,guv,gfaces,gweights,_=clothes
        gfaces=[(g,f) for g,f in gfaces if all((garment[vi].y-bottom)/height<0.58 for vi,_ in f)]
        clothes=(garment,guv,gfaces,gweights,clothes[-1])
        body_faces=[(g,f) for g,f in FACES if (g=='body' and not any(p[0] in clothes[-1] for p in f) and not any(0.35<(base[vi].y-bottom)/height<0.54 for vi,_ in f)) or g in ('helper-upper-teeth','helper-lower-teeth')]
        shoes=([Vector((0,0.02,0)),Vector((0.01,0.02,0)),Vector((0,0.03,0))],[(0.5,0.5)]*3,[('',[(0,0),(1,1),(2,2)])],[{18:1.0},{18:1.0},{18:1.0}],set())
        hair=add_hantu_air_algae(hair)

    parts=[(base,UV,body_faces,W,0),(*clothes[:4],1),(*shoes[:4],2),(*hair[:4],3),(*eyes[:4],4)]
    output=[];indices=[[],[],[],[],[]];original=[]
    limits={1:5200,2:1400,3:2400}
    for vs,uv,faces,ws,material in parts:
        posed=[deform(v,w) for v,w in zip(vs,ws)]
        if material in limits:posed,uv,faces,ws=reduce_part(posed,uv,faces,ws,limits[material])
        normals=[Vector((0,0,0)) for _ in posed];triangles=[]
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
                n=normals[vi].normalized() if normals[vi].length>1e-7 else Vector((0,1,0))
                output.append((*posed[vi],*n,*uv[ui],*bone[:4],*value[:4]));indices[material].append(len(output)-1)
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
    bake_banaspati_skull()
