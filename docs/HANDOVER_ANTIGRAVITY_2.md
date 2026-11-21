# Handover ke Antigravity (21 Nov 2026, malam)

## Status
- **main**: APK `uji-20261121-1505` (275 MB). Isi: G3 + performa 30 FPS + 30+ rekaman audio CC0 (Kenney, OpenGameArt) + musik jelajah + hujan ikut cuaca + tekstur tanah/serasah/lumut Poly Haven + semak/pohon/props scan + sebaran props se-pulau + langit senja HDRI + daun gugur/debu/percik hujan + cookie senter + dinding gua kapur + rumah bilik bambu + UI bulat + FOV lari.
- **PR #44 (cabang `imp-karakter`)** sedang di AK CI (~80 menit). Isi:
  - 7 karakter Mixamo untuk musuh (`ModelUntuk` di `Editor/BuilderSteps.Mixamo.cs`): Genderuwo=Warrok, AbdiButo=Maw, AbdiWanita=Zombiegirl, AbdiSekte=Nightshade, AbdiLumpur/MutanGembung=Mutant, PemimpinPatroli=Parasite, MutanJemari=Mremireh. Model diputar langsung (legacy) di `AI/AnimasiMixamo.Karakter.cs`, roboh pakai Zombie_Dying/Dying.
  - Hewan FBX CC0 (`AI/ModelHewanAsli.cs`, `Editor/BuilderSteps.HewanAsli.cs`): AyamHutan=ayam_jago/ayam_betina, Rusa & Kancil=rusa, UlarPiton/Kobra=ular.
  - Ikon tombol HUD (`Editor/BuilderSteps.IkonKontrol.cs`, ikon game-icons.net CC BY 3.0 di `Assets/UI/IkonKontrol`, kredit di MenuScreens), joystick profesional, indikator luka (`UI/IndikatorLuka.cs`), efek HP kritis (`Core/AKPascaProses.cs`), penanda benda (`UI/PenandaInteraksi.cs`), jejak tebasan (`Player/CombatAndLeftHandController.Jejak.cs`), peta mini redup, timeout CI 120/140 menit.
  - Run CI sebelumnya gagal hanya di cek "HUD: tombol pakai ikon (0)"; sudah diperbaiki (Sprite cadangan di `MuatIkon`). Bila masih merah: `gh api .../actions/jobs/<id>/logs | grep -E "error CS|\[GAGAL\]"`.

## Aset berlisensi (JANGAN masuk repo publik)
- Repo privat **`muhammadirvan1312-jpg/alas-keramat-aset`**: `Mixamo/` (55 klip), `Karakter/` (7 musuh), `Hewan/` (ayam_jago, ayam_betina, rusa, ular, kelelawar FBX + tekstur, CC0).
- AK CI menyalinnya lewat secret `ASET_TOKEN` (fine-grained, read-only Contents ke repo aset, **kedaluwarsa 21 Des 2026**) ke `Assets/Mixamo`, `Assets/MixamoKarakter`, `Assets/HewanAsli` (semua di .gitignore dan dikecualikan dari artefak publik).

## Belum selesai (prioritas)
1. **Karakter utama**: Deden=Mixamo *Remy*, Ningsih=*Peasant Girl*, Ki Rogo=*Peasant Man*. FBX sudah di `C:\Users\muhammad.irvan\Downloads` (Remy mungkin perlu diunduh ulang). Langkah: salin ke `alas-keramat-aset/Karakter/`, tambah peta di `ModelUntuk` + `PasangDiAdegan` untuk NPC Deden/Ningsih/KiRogo.
2. **Kelelawar** gua (`Hewan/kelelawar.fbx`, klip Bat_Flying/Idle/Attack/Die) belum dipasang.
3. Hewan masih prosedural: babi hutan, harimau, monyet, biawak, buaya. Sumber gratis realistis: Sketchfab WildMesh_3D (demo gratis Deer/Boar/Alligator/Buffalo, login Google).
4. Ide visual: god rays hutan, buih/riak air.
5. Tanya model HP pengguna untuk tuning performa. Lisensi Unity berakhir 16 Des 2026.

## Pelajaran
- `python3 Tools/cek_statis.py` wajib 0 masalah (kata Animator/NavMesh terlarang termasuk komentar, tanpa karakter non-ASCII di string).
- Build penuh ~75 menit; foto kualitas hanya pada event PR; AK Autopilot squash-merge PR hijau.
- Saat menggabung banyak cabang: konflik `Tools/aset_cc0.lock.json` gabungkan per `path` (union); hati-hati union otomatis pada array C# (pernah merusak `PasanganFoto`).
- Pengguna non-coder, bahasa santai; jangan ketik password/token; download butuh izin.
