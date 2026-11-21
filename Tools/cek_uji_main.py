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
    bot_frames = sorted(Path('Logs/foto_kualitas').glob('bot_*.png'))
    if bot_frames:
        gif_imgs = []
        for bf in bot_frames:
            with Image.open(bf) as im:
                if im.format != 'PNG' or im.size != (960, 540):
                    raise RuntimeError('Frame bot PlayMode tidak sah: ' + bf.name)
                gif_imgs.append(im.convert('RGB').resize((480, 270), Image.Resampling.BILINEAR))
        gif_imgs[0].save(Path('Logs/foto_kualitas') / 'bot_uji_main.gif', save_all=True, append_images=gif_imgs[1:], duration=900, loop=0)
        print('[AK] PlayMode: GIF bot uji main dibuat (' + str(len(gif_imgs)) + ' frame)')
    print('[AK] PlayMode: urutan kontrol/panel/restore dan tiga render HUD tersedia')

if __name__ == '__main__':
    main()
