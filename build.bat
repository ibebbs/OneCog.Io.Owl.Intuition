@echo Off

echo BUILD.BAT - NuGet package restore started.
".\.nuget\NuGet.exe" restore ".\OneCog.Io.Owl.Intuition.sln" -OutputDirectory ".\packages"
echo BUILD.BAT - NuGet package restore finished.

echo BUILD.BAT - FAKE build started.
".\packages\FAKE.4.20.0\tools\Fake.exe" build.fsx
echo BUILD.BAT - FAKE build finished.