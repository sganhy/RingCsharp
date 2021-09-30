REM "C:\Program Files\Mono\bin\xbuild" 
REM  "%~dp0\mono.exe" %MONO_OPTIONS% "%~dp0\..\lib\mono\xbuild\14.0\bin\xbuild.exe" %*
"C:\Program Files\Mono\bin\mono.exe" "C:\Program Files\Mono\bin\..\lib\mono\xbuild\14.0\bin\xbuild.exe" Ring.Adapters.SQLite.csproj /p:Configuration=Release-Mono /p:Platform="x64" /p:outputdir=".\bin\x64-mono\Release\"
pause
