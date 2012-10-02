@echo off
:: Build script that extract the iids from generated COM header files for use on Linux
:: The new files are checked in to Perforce
:: This script is intented to be called by a Hudson job. A few environment variables have to
:: be set:
::    set P4CLIENT=hudson-cal-ww-idl-josta
::    set P4USER=Hudson
::    set P4PASSWD=password
::    set P4PORT=src.sil.org:1934
::    PATH="C:\Program Files\Perforce";%PATH%
::    p4 sync %WORKSPACE%\BldSys
::    BldSys\build_tlb.cmd

:: set environment
set AssertUiEnabled=false
set TMPDIR=%TEMP%\%BUILD_TAG%
PATH=%WORKSPACE%\Bin;%WORKSPACE%\BldSys;%PATH%

:: Check out the files from Perforce
echo Syncing files and opening for edit...
p4 sync

p4 edit %WORKSPACE%\Lib\linux\Common\*Tlb.*
p4 edit %WORKSPACE%\Lib\linux\Common\idhfiles.MD5
p4 edit %WORKSPACE%\Src\Kernel\FwKernel_GUIDs.cpp
p4 edit %WORKSPACE%\Src\Language\Language_GUIDs.cpp
p4 edit %WORKSPACE%\Src\views\Views_GUIDs.cpp

:: Rebuild generated header files
echo Building...
cd Bld
:: we have to call checkTLBsUpToDate so that idhfiles.MD5 gets rebuilt
..\Bin\nant\bin\nant.exe -D:localsys-workaround=True buildtlb checkTLBsUpToDate
if errorlevel 1 goto REVERT

:: Check in any changed files to Perforce
echo Submitting files to Perforce...
p4 submit -f revertunchanged -d "Automatically regenerated idl files for use on Linux"
exit /b 0

:REVERT
echo Got error! Reverting files...
p4 revert %WORKSPACE%\Lib\linux\Common\*Tlb.*
p4 revert %WORKSPACE%\Lib\linux\Common\idhfiles.MD5
p4 revert %WORKSPACE%\Src\Kernel\FwKernel_GUIDs.cpp
p4 revert %WORKSPACE%\Src\Language\Language_GUIDs.cpp
p4 revert %WORKSPACE%\Src\views\Views_GUIDs.cpp
exit /b 1
