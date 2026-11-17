@echo off
setlocal
if "%UNITY_EXE%"=="" (echo [AK] Variabel UNITY_EXE belum diatur. & exit /b 2)
set PROJ=%~dp0..
if not exist "%PROJ%\Logs" mkdir "%PROJ%\Logs"
"%UNITY_EXE%" -batchmode -nographics -quit -projectPath "%PROJ%" -logFile "%PROJ%\Logs\cek_kompilasi.log" -executeMethod AlasKeramat.EditorTools.NusantaraSelfTest.RunFromCommandLine
set CODE=%ERRORLEVEL%
findstr /C:"error CS" "%PROJ%\Logs\cek_kompilasi.log"
if %ERRORLEVEL%==0 (echo [AK] ADA ERROR KOMPILASI & exit /b 1)
if not %CODE%==0 (echo [AK] Self-test gagal, lihat Logs\nusantara_selftest.txt & exit /b %CODE%)
echo [AK] BERSIH & exit /b 0
