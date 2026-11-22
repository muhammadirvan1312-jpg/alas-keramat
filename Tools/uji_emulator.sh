#!/usr/bin/env bash
# Pasang APK, buka game, rekam logcat 120 detik, ringkas crash. Dipanggil dari ak-emulator.yml.
APK="$1"; PAKET="$2"; KELUAR="$3"; mkdir -p "$KELUAR"
adb shell getprop ro.product.cpu.abilist | tee "$KELUAR/abi.txt"
adb install -r -g "$APK" 2>&1 | tee "$KELUAR/install.txt"
PAKET_ASLI=$(adb shell pm list packages | tr -d '\r' | grep -i alaskeramat | head -n1 | sed 's/package://')
[ -n "$PAKET_ASLI" ] && PAKET="$PAKET_ASLI"; echo "paket: $PAKET" | tee -a "$KELUAR/install.txt"
adb logcat -c
AKT=$(adb shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER "$PAKET" | tr -d '\r' | tail -n1)
echo "activity: $AKT" | tee -a "$KELUAR/install.txt"
adb shell am start -W -n "$AKT" 2>&1 | tee -a "$KELUAR/install.txt"
for t in 15 30 60 90 120; do
  sleep 15
  adb shell pidof "$PAKET" > /dev/null && echo "t~${t}s: proses hidup" || echo "t~${t}s: proses MATI"
done | tee "$KELUAR/status.txt"
adb exec-out screencap -p > "$KELUAR/layar.png" || true
adb logcat -d -v threadtime > "$KELUAR/logcat_lengkap.txt"
grep -E "alaskeramat|FATAL|AndroidRuntime|DEBUG   :|backtrace|signal [0-9]+|Abort message|Unity  |CRASH|lowmemorykiller|Out of memory|il2cpp|\[AK\]" "$KELUAR/logcat_lengkap.txt" | head -n 400 > "$KELUAR/ringkas.txt" || true
echo "--- ringkas ---"; head -n 120 "$KELUAR/ringkas.txt"
exit 0
