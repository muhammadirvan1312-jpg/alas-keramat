#!/usr/bin/env python3
"""Metadata stabil untuk sumber Unity buatan kode; tidak menimpa GUID yang sudah ada."""
import hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
def main():
    count=0
    for p in sorted((ROOT/'Assets').rglob('*')):
        if p.suffix not in ('.cs','.shader','.hlsl'):continue
        dest=Path(str(p)+'.meta')
        if dest.exists():continue
        rel=p.relative_to(ROOT).as_posix()
        guid=hashlib.md5(('AlasKeramat/source/'+rel).encode()).hexdigest()
        if p.suffix=='.cs':
            importer='MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n'
        elif p.suffix=='.shader':
            importer='ShaderImporter:\n  externalObjects: {}\n  defaultTextures: []\n  nonModifiableTextures: []\n  preprocessorOverride: 0\n'
        else:importer='ShaderIncludeImporter:\n  externalObjects: {}\n'
        dest.write_text('fileFormatVersion: 2\nguid: '+guid+'\n'+importer+'  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
        count+=1
    print('[AK] Metadata sumber baru:',count)
if __name__=='__main__':main()
