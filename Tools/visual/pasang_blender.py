#!/usr/bin/env python3
"""Blender resmi versi tetap; verifikasi arsip sebelum ekstraksi."""
import hashlib,tarfile,urllib.request,time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
FOLDER=ROOT/'.tools/blender43'
FILE='blender-4.3.2-linux-x64.tar.xz'
SHA='4da1c956673c0485e63054e563ee69198cc8f80d8157dd7592dffc8a6a5592e6'
URLS=['https://download.blender.org/release/Blender4.3/'+FILE,'https://ftp.halifax.rwth-aachen.de/blender/release/Blender4.3/'+FILE]
def valid(path):
    if not path.exists():return False
    with path.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()==SHA

def main():
    FOLDER.mkdir(parents=True,exist_ok=True)
    archive=FOLDER/'archive.tar.xz';exe=FOLDER/'blender-4.3.2-linux-x64/blender'
    if exe.exists():print('[AK] Blender 4.3.2 siap');return
    if not valid(archive):
        for url in URLS:
            try:
                temp=archive.with_suffix('.part')
                with urllib.request.urlopen(url,timeout=90) as response,temp.open('wb') as output:
                    while True:
                        data=response.read(1024*1024)
                        if not data:break
                        output.write(data)
                if not valid(temp):raise ValueError('SHA256 Blender tidak cocok')
                temp.replace(archive);break
            except Exception as error:print('[AK] Sumber Blender dilewati:',error)
        if not valid(archive):raise RuntimeError('Blender resmi gagal diunduh')
    with tarfile.open(archive) as tar:tar.extractall(FOLDER,filter='data')
    if not exe.exists():raise RuntimeError('Executable Blender tidak ditemukan')
    print('[AK] Blender resmi 4.3.2, arsip SHA256 cocok')
if __name__=='__main__':main()
