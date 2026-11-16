#!/usr/bin/env python3
"""Pengunduh aset CC0 & mocap sesuai lock file (docs/gdd/19 bagian 4).
Pemakaian: python3 Tools/ambil_aset.py --semua --keluar Assets/ThirdParty
Lock file: Tools/aset_cc0.lock.json {"aset":[{"url","path","sha256"}]} dan Tools/mocap.lock.json {"klip":[...]}.
Aset yang gagal diunduh dilewati (game memakai cadangan prosedural)."""
import argparse, hashlib, json, os, sys, urllib.request

UA = "AlasKeramat-AsetCC0/1.0 (+github.com/muhammadirvan1312-jpg/alas-keramat)"
AKAR = os.path.dirname(os.path.abspath(__file__))

def muat(nama, kunci):
    p = os.path.join(AKAR, nama)
    if not os.path.exists(p):
        return []
    with open(p, encoding="utf-8") as f:
        return json.load(f).get(kunci, [])

def unduh(entri, keluar):
    tujuan = os.path.join(keluar, entri["path"])
    if os.path.exists(tujuan) and entri.get("sha256"):
        with open(tujuan, "rb") as f:
            if hashlib.sha256(f.read()).hexdigest() == entri["sha256"]:
                return "ada"
    os.makedirs(os.path.dirname(tujuan), exist_ok=True)
    req = urllib.request.Request(entri["url"], headers={"User-Agent": UA})
    with urllib.request.urlopen(req, timeout=120) as r:
        data = r.read()
    if entri.get("sha256") and hashlib.sha256(data).hexdigest() != entri["sha256"]:
        raise ValueError("sha256 tidak cocok")
    with open(tujuan, "wb") as f:
        f.write(data)
    return "unduh"

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--semua", action="store_true")
    ap.add_argument("--keluar", default="Assets/ThirdParty")
    a = ap.parse_args()
    daftar = muat("aset_cc0.lock.json", "aset") + muat("mocap.lock.json", "klip")
    os.makedirs(a.keluar, exist_ok=True)
    gagal = 0
    for e in daftar:
        try:
            print("[AK] %s %s" % (unduh(e, a.keluar), e["path"]))
        except Exception as ex:
            gagal += 1
            print("[AK] LEWAT %s: %s" % (e.get("path"), ex))
    print("[AK] selesai: %d entri, %d dilewati" % (len(daftar), gagal))
    return 0

if __name__ == "__main__":
    sys.exit(main())
