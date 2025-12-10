dotnet build "FlacHasher.sln" --configuration Release /p:EnableWindowsTargeting=true

dotnet test "Andy.Cmd.Tests\\Cmd.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\TestResults\Cmd.Tests.html"
