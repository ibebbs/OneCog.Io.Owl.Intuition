@echo Off

echo BUILD.BAT - FAKE build started.
".\packages\FAKE.4.20.0\tools\Fake.exe" build.fsx
echo BUILD.BAT - FAKE build finished.