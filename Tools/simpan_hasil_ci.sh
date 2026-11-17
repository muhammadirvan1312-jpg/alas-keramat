#!/usr/bin/env bash
# Simpan laporan/foto CI ke cabang terpisah `ci-hasil` (folder per cabang PR), BUKAN ke cabang PR,
# agar cabang tahap tidak bentrok dengan main karena file Logs/.
# Pemakaian: Tools/simpan_hasil_ci.sh <nama-cabang> <file/folder> [file/folder ...]
set -u
CAB="${1//\//_}"; shift
WT="${RUNNER_TEMP:-/tmp}/ci-hasil"
git config --global --add safe.directory "$GITHUB_WORKSPACE" 2>/dev/null || true
git config user.name "ak-ci-bot"; git config user.email "ak-ci-bot@users.noreply.github.com"
for coba in 1 2 3; do
  rm -rf "$WT"; git worktree prune
  if git fetch -q origin ci-hasil 2>/dev/null; then
    git worktree add -q -B ci-hasil-lokal "$WT" FETCH_HEAD
  else
    git worktree add -q --detach "$WT" && (cd "$WT" && git checkout -q --orphan ci-hasil-lokal && git rm -rqf . >/dev/null 2>&1; true)
  fi
  mkdir -p "$WT/$CAB"
  for f in "$@"; do [ -e "$f" ] && cp -r "$f" "$WT/$CAB/" ; done
  ( cd "$WT" && git -c user.name=ak-ci-bot -c user.email=ak-ci-bot@users.noreply.github.com add -A \
    && { git diff --cached --quiet && echo "Tidak ada hasil baru."; true; } \
    && { git diff --cached --quiet || git -c user.name=ak-ci-bot -c user.email=ak-ci-bot@users.noreply.github.com commit -q -m "hasil CI $CAB"; } \
    && git push -q origin HEAD:ci-hasil ) && { echo "[AK] hasil CI disimpan di cabang ci-hasil/$CAB"; exit 0; }
  sleep $((coba * 5))
done
echo "[AK] gagal menyimpan hasil CI (tidak memblokir)"; exit 0
