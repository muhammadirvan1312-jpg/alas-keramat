#!/usr/bin/env python3
"""Unduh aset CC0 yang dikunci SHA256; tidak ada panggilan internet saat game dimainkan."""
import io, zipfile
import argparse, concurrent.futures, hashlib, json, os, struct, sys, time, urllib.request, zlib
from pathlib import Path
UA='AlasKeramatBuilder/1.0 (+github.com/muhammadirvan1312-jpg/alas-keramat)'
ROOT=Path(__file__).resolve().parent

def muat(nama,kunci):
    p=ROOT/nama
    return json.loads(p.read_text(encoding='utf-8')).get(kunci,[]) if p.exists() else []

def unduh(entry,output):
    root=Path(output).resolve();target=(root/entry['path']).resolve()
    if not target.is_relative_to(root):raise ValueError('Path aset keluar folder')
    expected=entry.get('sha256')
    if target.exists() and expected and hashlib.sha256(target.read_bytes()).hexdigest()==expected:return 'ada'
    headers={'User-Agent':UA}
    if 'range_offset' in entry:
        a=entry['range_offset'];headers['Range']=f"bytes={a}-{a+entry['range_size']-1}"
    with urllib.request.urlopen(urllib.request.Request(entry['url'],headers=headers),timeout=90) as response:
        if 'range_offset' in entry and response.status!=206:raise ValueError('Server mengabaikan Range ZIP')
        data=response.read()
    if 'range_offset' in entry:
        header=struct.unpack_from('<4s5H3L2H',data)
        if header[0]!=b'PK\x03\x04':raise ValueError('Header arsip tidak cocok')
        offset=30+header[-2]+header[-1]
        data=data[offset:offset+header[7]]
        if header[3]==8:data=zlib.decompress(data,-15)
        elif header[3]!=0:raise ValueError('Kompresi ZIP tidak didukung')
    if 'zip_entry' in entry:
        if hashlib.sha256(data).hexdigest()!=entry['archive_sha256']:raise ValueError('SHA256 arsip tidak cocok')
        with zipfile.ZipFile(io.BytesIO(data)) as archive:data=archive.read(entry['zip_entry'])
    if expected and hashlib.sha256(data).hexdigest()!=expected:raise ValueError('SHA256 tidak cocok')
    target.parent.mkdir(parents=True,exist_ok=True);temp=target.with_suffix(target.suffix+'.part');temp.write_bytes(data);temp.replace(target)
    return 'unduh'

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--semua',action='store_true');ap.add_argument('--keluar',default='Assets/ThirdParty');ap.add_argument('--wajib',action='store_true');args=ap.parse_args()
    entries=muat('aset_cc0.lock.json','aset')+muat('mocap.lock.json','klip')+muat('karakter.lock.json','aset')
    def task(e):
        for attempt in range(3):
            try:return True,'[AK] '+unduh(e,args.keluar)+' '+e['path']
            except Exception as error:
                if attempt==2:return False,'[AK] LEWAT '+e.get('path','?')+': '+str(error)
                time.sleep(attempt+1)
    failed=0
    with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:
        for ok,message in pool.map(task,entries):print(message,flush=True);failed+=not ok
    print(f'[AK] selesai: {len(entries)} entri, {failed} dilewati')
    return 1 if failed and args.wajib else 0
if __name__=='__main__':sys.exit(main())
