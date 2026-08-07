@echo off
setlocal

REM Publishes the generator as single-file self-contained executables for all
REM compatible platforms into .\bins\<rid>. Just double-click or run:
REM  build.bat
REM To build a single RID:  build.bat linux-x64
set "ROOT=%~dp0..\.."
set "PROJ=%ROOT%\Kolpa.Generator\Kolpa.Generator.csproj"
set "OUT=%ROOT%\bins"

if not "%1"=="" (
  set RIDLIST=%1
  ) else (
  set RIDLIST=win-x64 linux-x64 linux-arm64 osx-x64 osx-arm64
)

for %%R in (%RIDLIST%) do (
  echo.
  echo ^>^> Publishing %%R -^> %OUT%\%%R
  dotnet publish "%PROJ%" -c Release -r %%R --self-contained true -o "%OUT%\%%R" ^
  /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:EnableCompressionInSingleFile=true ^
  /p:DebugType=embedded ^
  /p:InvariantGlobalization=true ^
  /p:UseAppHost=true
  if errorlevel 1 (
    echo [ERROR] Publish of %%R failed.
    set FAILED=1
  )
)

echo.
echo ^>^> Done. Executables under %OUT%:
for /d %%D in ("%OUT%\*") do (
  for %%F in ("%%D\Kolpa.Generator*") do (
    if exist %%F echo %%F
  )
)

endlocal & exit /b %FAILED%
