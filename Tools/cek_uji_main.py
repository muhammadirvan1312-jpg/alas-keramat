#!/usr/bin/env python3
"""Gerbang bukti PlayMode; tidak menganggap waktu runner sebagai FPS HP."""
import argparse, json
from pathlib import Path
from PIL import Image

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--commit', required=True)
    args = parser.parse_args()
    report = json.loads(Path('Logs/ci/uji_main.json').read_text())
    if report.get('commit') != args.commit or report.get('passed') is not True:
        raise RuntimeError('PlayMode gagal atau sumber revisi tidak cocok')
    if len(report.get('checks', [])) < 10 or any(not check.startswith('[LULUS]') for check in report['checks']):
        raise RuntimeError('Urutan uji PlayMode belum lengkap')
    for name in ('main_01_hud', 'main_02_kontrol', 'main_03_audio'):
        with Image.open(Path('Logs/foto_kualitas') / (name + '.png')) as image:
            if image.format != 'PNG' or image.size != (960, 540):
                raise RuntimeError('Bukti HUD PlayMode tidak sah: ' + name)
    print('[AK] PlayMode: urutan kontrol/panel/restore dan tiga render HUD tersedia')

if __name__ == '__main__':
    main()
