#!/usr/bin/env bash
# Cek kompilasi + self-test lokal (Unity Editor harus tertutup). Atur UNITY_EXE dulu.
set -u
if [ -z "${UNITY_EXE:-}" ]; then echo "[AK] Variabel UNITY_EXE belum diatur."; exit 2; fi
PROJ="$(cd "$(dirname "$0")/.." && pwd)"
mkdir -p "$PROJ/Logs"
"$UNITY_EXE" -batchmode -nographics -quit -projectPath "$PROJ" -logFile "$PROJ/Logs/cek_kompilasi.log" -executeMethod AlasKeramat.EditorTools.NusantaraSelfTest.RunFromCommandLine
CODE=$?
if grep -n "error CS" "$PROJ/Logs/cek_kompilasi.log"; then echo "[AK] ADA ERROR KOMPILASI"; exit 1; fi
if [ $CODE -ne 0 ]; then echo "[AK] Self-test gagal, lihat Logs/nusantara_selftest.txt"; exit $CODE; fi
echo "[AK] BERSIH"; exit 0
