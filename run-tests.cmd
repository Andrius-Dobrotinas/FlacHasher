dotnet build "FlacHasher.sln" --configuration Release /p:EnableWindowsTargeting=true

dotnet test "Andy.Cmd.Tests\\Cmd.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\src\TestResults\Cmd.Tests.html"
dotnet test "Common.Tests\\Common.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\src\TestResults\Common.Tests.html"
dotnet test "ExternalProcess\ExternalProcess.Tests\ExternalProcess.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\src\TestResults\ExternalProcess.Tests.html"
dotnet test "Configuration.Ini\\Configuration.Ini.Tests\\Configuration.Ini.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\src\TestResults\Configuration.Ini.Tests.html"
dotnet test "FlacHash.Tests\\FlacHash.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\src\TestResults\FlacHash.Tests.html"
dotnet test "FlacHasher.Cmd.Tests\\FlacHasher.Cmd.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\src\TestResults\FlacHasher.Cmd.Tests.html"
dotnet test "FlacHasher.Win.Tests\\FlacHasher.Win.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\src\TestResults\FlacHasher.Win.Tests.html"
dotnet test "Utils\\CompressionLevelChecker.Win.Tests\\CompressionLevelChecker.Win.Tests.csproj" --configuration Release --no-build --logger "html;LogFileName=c:\src\TestResults\CompressionLevelChecker.Win.Tests.html"