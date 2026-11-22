using AlasKeramat.Building;
using AlasKeramat.Core;
using UnityEngine;
namespace AlasKeramat.Player
{
    public partial class AndiFPSController : ISaveable
    {
        public const float KecepatanRakit = 2.5f;
        public FungsiBangunan RakitAktif { get; private set; }
        public bool Duduk { get; private set; }
        Transform _rakitRoot;
        ParticleSystem _buihRakit, _gelembungSelam;

        public static Quaternion HitungAyunOmbakRakit(float waktu, float laju01, float yawDerajat)
        {
            float pitch = Mathf.Sin(waktu * 2.1f) * (1.6f + Mathf.Clamp01(laju01) * 2.0f);
            float roll = Mathf.Cos(waktu * 1.5f) * 1.5f;
            return Quaternion.Euler(pitch, yawDerajat, roll);
        }

        public static float HitungLajuBuihRakit(float kecepatanDatar)
        {
            return kecepatanDatar < 0.05f ? 0f : Mathf.Lerp(5f, 18f, Mathf.Clamp01(kecepatanDatar / KecepatanRakit));
        }

        public static float HitungLajuGelembungSelam(bool menyelam, float napas01)
        {
            return !menyelam ? 0f : Mathf.Lerp(14f, 5f, Mathf.Clamp01(napas01));
        }

        void PerbaruiGelembungSelam()
        {
            if (kamera == null) return;
            float laju = HitungLajuGelembungSelam(Menyelam, Napas / Mathf.Max(1f, NapasMaks));
            if (laju > 0f)
            {
                if (_gelembungSelam == null)
                {
                    var go = new GameObject("GelembungSelam");
                    go.transform.SetParent(kamera, false);
                    go.transform.localPosition = new Vector3(0f, -0.15f, 0.35f);
                    _gelembungSelam = go.AddComponent<ParticleSystem>();
                    var m = _gelembungSelam.main;
                    m.loop = true; m.startLifetime = 1.1f; m.startSpeed = 0.45f; m.startSize = 0.025f;
                    m.maxParticles = 24; m.gravityModifier = -0.35f; m.simulationSpace = ParticleSystemSimulationSpace.World;
                    m.startColor = new Color(0.85f, 0.95f, 1f, 0.55f);
                    var sh = _gelembungSelam.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.14f;
                    var pr = go.GetComponent<ParticleSystemRenderer>();
                    pr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_GelembungSelam", new Color(0.88f, 0.96f, 1f, 0.55f), 0f, 0.9f, null, null, null, SurfaceMode.Transparent);
                    pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                var em = _gelembungSelam.emission; em.rateOverTime = laju;
                if (!_gelembungSelam.isPlaying) _gelembungSelam.Play();
            }
            else if (_gelembungSelam != null && _gelembungSelam.isPlaying) _gelembungSelam.Stop();
        }
        public string SaveKey { get { return "posisi_andi"; } }
        [System.Serializable] class PosisiAndiT6 { public Vector3 posisi; public float yaw, pitch; }
        public string CaptureJson() { return JsonUtility.ToJson(new PosisiAndiT6 { posisi=transform.position, yaw=_yaw, pitch=_pitch }); }
        public void RestoreJson(string json)
        {
            var d=JsonUtility.FromJson<PosisiAndiT6>(json); if(d==null)return;
            TurunRakit(); Duduk=false; Teleport(d.posisi); _yaw=d.yaw; _pitch=d.pitch;
            transform.rotation=Quaternion.Euler(0f,_yaw,0f);
        }
        public void Teleport(Vector3 posisi)
        {
            HentikanInputGerak(); KecepatanDatar=0f; DiTanah=false; Berenang=false; Menyelam=false;
            var c=GetComponent<CharacterController>(); bool aktif=c!=null && c.enabled;
            if(c!=null)c.enabled=false;
            transform.position=posisi; _vDatar=Vector3.zero; _vy=0f; _diUdara=false; _puncakY=posisi.y;
            if(c!=null)c.enabled=aktif;
        }
        public void NaikRakit(FungsiBangunan rakit)
        {
            if(rakit==null || RakitAktif!=null)return;
            var id=rakit.GetComponentInParent<IdentitasBangunan>(); if(id==null)return;
            Duduk=false; RakitAktif=rakit; _rakitRoot=id.transform;
            if(_cc!=null)_cc.enabled=false;
            Berenang=false; Menyelam=false; _diUdara=false;
            transform.position=_rakitRoot.position+Vector3.up*0.4f;
        }
        public void TurunRakit()
        {
            if(RakitAktif!=null && _rakitRoot!=null)
            {
                Vector3 p=_rakitRoot.position+_rakitRoot.right*1.8f+Vector3.up*0.5f;
                RakitAktif=null; _rakitRoot=null;
                Teleport(p);
            }
            if(_cc!=null)_cc.enabled=true;
        }
        public void DudukDi(Vector3 p) { TurunRakit(); Teleport(p); Duduk=true; }
        public void Berdiri() { Duduk=false; }
        bool GerakKendaraan(float dt)
        {
            if(Duduk) { KecepatanDatar=0f; SedangLari=false; return true; }
            if(RakitAktif==null || _rakitRoot==null) return false;
            Vector3 gerak=transform.TransformDirection(new Vector3(InputGerak.x,0f,InputGerak.y));
            gerak=Vector3.ClampMagnitude(gerak,1f)*KecepatanRakit*dt;
            Vector3 p=_rakitRoot.position, calon=p+gerak;
            float? air=WorldTerrainGenerator.GetWaterLevelAt(calon.x,calon.z);
            bool sah=air.HasValue && air.Value>WorldTerrainGenerator.SampleHeight(calon.x,calon.z)+0.25f;
            for(int i=0;i<4 && sah;i++)
            {
                Vector3 q=calon+_rakitRoot.right*(i%2==0?-1.2f:1.2f)+_rakitRoot.forward*(i<2?-1.8f:1.8f);
                float? w=WorldTerrainGenerator.GetWaterLevelAt(q.x,q.z);
                sah=w.HasValue && w.Value>WorldTerrainGenerator.SampleHeight(q.x,q.z)+0.15f;
            }
            float lajuRakit = 0f;
            if(sah && gerak.sqrMagnitude>0f)
            {
                RaycastHit hit;
                int mask=(1<<GameLayers.Ground)|(1<<GameLayers.Structure);
                if(!Physics.BoxCast(p+Vector3.up*0.8f,new Vector3(1.1f,0.3f,1.7f),gerak.normalized,out hit,_rakitRoot.rotation,gerak.magnitude,mask,QueryTriggerInteraction.Ignore))
                {
                    calon.y=air.Value + Mathf.Sin(Time.time * 2.1f) * 0.045f;
                    _rakitRoot.position=calon;
                    lajuRakit = gerak.magnitude / Mathf.Max(0.0001f, dt);
                }
            }
            _rakitRoot.rotation = Quaternion.Slerp(_rakitRoot.rotation, HitungAyunOmbakRakit(Time.time, lajuRakit / KecepatanRakit, _yaw), dt * 4.5f);
            float lajuBuih = HitungLajuBuihRakit(lajuRakit);
            if (lajuBuih > 0f)
            {
                if (_buihRakit == null)
                {
                    var go = new GameObject("BuihRakit");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(0f, -0.35f, -1.2f);
                    _buihRakit = go.AddComponent<ParticleSystem>();
                    var m = _buihRakit.main;
                    m.loop = true; m.startLifetime = 0.75f; m.startSpeed = 0.8f; m.startSize = 0.16f;
                    m.maxParticles = 30; m.simulationSpace = ParticleSystemSimulationSpace.World;
                    m.startColor = new Color(0.92f, 0.96f, 1f, 0.62f);
                    var sh = _buihRakit.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(1.4f, 0.08f, 0.4f);
                    var pr = go.GetComponent<ParticleSystemRenderer>();
                    pr.sharedMaterial = SafeMaterialFactory.CreatePBR("M_BuihRakit", new Color(0.94f, 0.97f, 1f, 0.6f), 0f, 0.5f, null, null, null, SurfaceMode.Transparent);
                    pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                var em = _buihRakit.emission; em.rateOverTime = lajuBuih;
                if (!_buihRakit.isPlaying) _buihRakit.Play();
            }
            else if (_buihRakit != null && _buihRakit.isPlaying) _buihRakit.Stop();
            transform.position=_rakitRoot.position+Vector3.up*0.4f;
            Berenang=false; Menyelam=false; SedangLari=false; KecepatanDatar=0f; Napas=NapasMaks;
            return true;
        }
    }
}
