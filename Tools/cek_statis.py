#!/usr/bin/env python3
"""Cek statis C# sebelum PR (docs/tech/18 bagian 5). Keluar 1 bila ada pelanggaran."""
import os, re, sys
LARANG = [r"\bTMPro\b", r"\bNavMesh", r"\bAnimator\b", r"using\s+UnityEngine\.InputSystem", r"\bCinemachine\b",
          r"//\s*TODO", r"NotImplementedException", r"FindObjectOfType\b", r"\brb\.velocity\b"]
masalah, kelas = [], {}

def kutip_rusak(baris):
    """True bila sebuah string literal langsung disambung huruf/angka/kutip (mis. "("Andi..."" tanpa escape)."""
    i, q, verb = 0, False, False
    while i < len(baris):
        c = baris[i]
        if not q:
            if baris.startswith("//", i): return False
            if c == "'":
                j = baris.find("'", i + 2 if baris[i + 1:i + 2] == "\\" else i + 1)
                i = j + 1 if j > 0 else i + 1
                continue
            if c == '"': q, verb = True, (i > 0 and baris[i - 1] == "@")
        else:
            if c == "\\" and not verb:
                i += 2
                continue
            if c == '"':
                q = False
                nx = baris[i + 1:i + 2]
                if nx.isalnum() or nx == "_" or (nx == '"' and not verb): return True
        i += 1
    return False
for akar, _, files in os.walk("Assets"):
    if "ThirdParty" in akar:
        continue
    for n in files:
        p = os.path.join(akar, n)
        if n.endswith(".shader"):
            if "Fallback" not in open(p, encoding="utf-8").read():
                masalah.append(p + ": shader tanpa Fallback")
            continue
        if not n.endswith(".cs"):
            continue
        teks = open(p, encoding="utf-8").read()
        baris = teks.count("\n") + 1
        if baris > 900: masalah.append("%s: %d baris (>900)" % (p, baris))
        if teks.count("{") != teks.count("}"): masalah.append(p + ": kurung kurawal tidak seimbang")
        for pola in LARANG:
            if pola == r"\brb\.velocity\b" and "PhysicsCompat" in n: continue
            if re.search(pola, teks): masalah.append("%s: dilarang %s" % (p, pola))
        for s in re.findall(r'"[^"\n]*"', teks):
            if any(ord(c) > 0x2FF for c in s): masalah.append(p + ": simbol non-Latin di string " + s[:40])
        for nb, b in enumerate(teks.split("\n"), 1):
            if kutip_rusak(b): masalah.append("%s:%d: tanda kutip di dalam string belum di-escape (pakai \\\")" % (p, nb))
        parsial = bool(re.search(r"\bpartial\b", teks))
        for k in set(re.findall(r"\b(?:class|struct|enum|interface)\s+(\w+)", teks)):
            kelas.setdefault(k, []).append((p, parsial))
# Nama tipe ganda dinilai setelah semua file terbaca, jadi hasilnya tidak bergantung urutan os.walk.
for k, daftar in kelas.items():
    if len(daftar) > 1 and not all(x[1] for x in daftar):
        masalah.append("%s: nama ganda %s (juga %s)" % (daftar[1][0], k, daftar[0][0]))
# Komponen/aset Unity (turunan MonoBehaviour/ScriptableObject) WAJIB di file bernama sama dengan kelasnya.
# Bila tidak, Unity tidak membuat MonoScript; data komponen tetap tersimpan di scene dan player Android crash
# "level0 is corrupted / Position out of bounds" setelah logo Unity (kasus ForagingExtraNode, Samsung A16).
DASAR_UNITY = {"MonoBehaviour", "ScriptableObject", "Graphic", "MaskableGraphic", "BaseMeshEffect", "Image", "Text",
               "RawImage", "Selectable", "UIBehaviour", "Button", "Shadow", "Outline", "ScrollRect", "StateMachineBehaviour"}
turunan = {}
for akar, _, fs in os.walk("Assets"):
    if "ThirdParty" in akar or os.sep + "Editor" in akar: continue
    for n in fs:
        if not n.endswith(".cs"): continue
        pth = os.path.join(akar, n)
        t = open(pth, encoding="utf-8", errors="ignore").read()
        for m in re.finditer(r"\bclass\s+(\w+)\s*(?:<[^>]*>)?\s*:\s*([\w\.]+)", t):
            turunan.setdefault(m.group(1), []).append((pth, m.group(2).split(".")[-1]))
def komponen(c, jejak=()):
    if c in DASAR_UNITY: return True
    if c in jejak or c not in turunan: return False
    return any(komponen(b, jejak + (c,)) for _, b in turunan[c])
for c, daftar in turunan.items():
    if not komponen(c): continue
    nama_file = {os.path.basename(f).split(".")[0] for f, _ in daftar}
    if c not in nama_file:
        masalah.append("%s: komponen %s harus di file %s.cs (crash level0 corrupted di Android)" % (daftar[0][0], c, c))
for m in masalah: print("[AK][GAGAL]", m)
print("[AK] cek statis: %d masalah" % len(masalah))
sys.exit(1 if masalah else 0)
