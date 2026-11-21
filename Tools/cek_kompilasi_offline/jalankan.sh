#!/usr/bin/env bash
# Cek kompilasi C# offline (tanpa Unity) dengan Roslyn + referensi Unity 2021.3 dari NuGet + stub uGUI/URP.
# Menangkap error CS (sintaks, member ganda, salah nama/parameter, using hilang) sebelum menunggu AK CI ~75 menit.
# Pakai: bash Tools/cek_kompilasi_offline/jalankan.sh   (butuh dotnet-sdk-8.0: apt-get install -y dotnet-sdk-8.0)
# Catatan: API khusus Unity 6 (linearVelocity dll.) tidak ada di referensi 2021, jadi mode Unity 6 tidak dijalankan.
set -euo pipefail
AKAR=$(cd "$(dirname "$0")/../.." && pwd); KERJA=${KERJA:-/tmp/ak_kompil}; REF=$KERJA/ref
mkdir -p "$REF" "$KERJA/stub"
ambil(){ local id=$1 ver=$2; [ -d "$REF/$id" ] && return; curl -sfL -o "$REF/$id.zip" "https://api.nuget.org/v3-flatcontainer/$id/$ver/$id.$ver.nupkg"; mkdir -p "$REF/$id"; (cd "$REF/$id" && unzip -q -o "../$id.zip"); }
ambil unityengine.modules 2021.3.33; ambil unity3d.sdk 2021.1.14.1
rm -f "$REF/unity3d.sdk/lib/UnityEngine.dll"  # hanya UnityEditor.dll yang dipakai; UnityEngine dari paket modul
cp "$AKAR/Tools/cek_kompilasi_offline/StubUIURP.cs.txt" "$KERJA/stub/StubUIURP.cs"
M=$REF/unityengine.modules/lib/netstandard2.0
buat(){ # $1=nama csproj, $2=pakai editor (1/0)
  { echo '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>netstandard2.1</TargetFramework><LangVersion>9.0</LangVersion><Nullable>disable</Nullable><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>$(DEFINES)</DefineConstants><NoWarn>CS0414;CS0649;CS0169;CS0168;CS0219;CS0162;CS0618;CS0067;CS0108;CS0114</NoWarn><AllowUnsafeBlocks>true</AllowUnsafeBlocks><GenerateAssemblyInfo>false</GenerateAssemblyInfo></PropertyGroup><ItemGroup>'
    echo '<Compile Include="$(SRC)/**/*.cs" /><Compile Include="'"$KERJA"'/stub/*.cs" /><PackageReference Include="Microsoft.CSharp" Version="4.7.0" />'
    for d in "$M"/UnityEngine*.dll; do echo "<Reference Include=\"$(basename "$d" .dll)\"><HintPath>$d</HintPath></Reference>"; done
    [ "$2" = 1 ] && echo "<Reference Include=\"UnityEditor\"><HintPath>$REF/unity3d.sdk/lib/UnityEditor.dll</HintPath></Reference>"
    echo '</ItemGroup></Project>'; } > "$KERJA/$1"
}
buat CekEditor.csproj 1; buat CekAndroid.csproj 0
rm -rf "$KERJA/ed" "$KERJA/rt"; mkdir -p "$KERJA/ed" "$KERJA/rt"; cp -r "$AKAR/Assets" "$KERJA/ed/"; cp -r "$AKAR/Assets" "$KERJA/rt/"
find "$KERJA/rt/Assets" -type d -name Editor -prune -exec rm -rf {} +
ABAIKAN="NamedBuildTarget|customReflectionTexture|Handheld|UnityEventTools|ThirdParty/"
hasil=0
for mode in "CekEditor.csproj ed UNITY_EDITOR%3BUNITY_ANDROID%3BENABLE_LEGACY_INPUT_MANAGER" "CekAndroid.csproj rt UNITY_ANDROID%3BENABLE_LEGACY_INPUT_MANAGER"; do
  set -- $mode; echo "== $1"
  (cd "$KERJA" && dotnet restore "$1" -v q >/dev/null 2>&1 || true)
  out=$(cd "$KERJA" && dotnet build "$1" --no-incremental -nologo -v q -p:SRC="$KERJA/$2/Assets" "-p:DEFINES=$3" 2>&1 | grep -E "error CS" | sed -E "s#^$KERJA/$2/##; s# \[/.*##" | sort -u | grep -vE "$ABAIKAN" || true)
  if [ -n "$out" ]; then echo "$out"; hasil=1; else echo "[AK] 0 error"; fi
done
exit $hasil
