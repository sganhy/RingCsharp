"C:\Program Files\dotnet\dotnet" --version
"C:\Program Files\dotnet\dotnet" build "Ring.Core.2017.csproj" /p:Configuration=Release /p:Platform="x64" --output ".\bin\x64-core\Release\"
"..\Tools\Obfuscar.Console" "-V"
"..\Tools\Obfuscar.Console" ".\x64.Core.Obfuscar.config"
pause