@echo off
REM set bin folder and current folder
set BinFolder=%1
set CurFolder=%~dp0

if not "%BinFolder%"=="" goto run 
else goto err

:run
copy "%CurFolder%"\*.dll "%BinFolder%"\
regsvr32 "%CurFolder%zkemkeeper.dll"
goto end

:err
echo Missing bin folder, input bin path please.
goto end

:end
echo run complete.