setlocal
set root=d:\codeql
set databaseDir=%root%\databases\sdv-fd-wdm
set logDir=%root%\logs
for %%i in (%logDir%) do if not exist %%i md %%i
for %%i in (%databaseDir%) do if exist %%i rd /s /q %%i
set path=%path%;%root%\codeql
REM Disable Windows tracer to avoid build hangs where Windows tracer.exe and CodeQl tracer.exe can interact and deadlock while merging config files early in a build
set TRACER_ENABLED=0
codeql.exe database create --language=cpp --source-root=D:\os\src\onecore\drivers\wdf\tools\sdv\samples\SDV-FailDriver-WDM\driver --threads=0 --command="build.cmd /BuildDateAlreadySet /getSlowestDirs 20 /ZPh /c /dirinclude:!nonprod /codeset:cs_windows /jpath d:\os\obj\amd64fre /j buildfre /0 /L /SkipTestCode" --logdir=%logDir% %databaseDir%
endlocal
