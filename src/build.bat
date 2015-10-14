@..\packages\NuGet.CommandLine.2.8.6\tools\NuGet.exe restore Microbus.sln -PackagesDirectory "..\packages"
@msbuild Microbus.msbuild /t:Build;Test;Package