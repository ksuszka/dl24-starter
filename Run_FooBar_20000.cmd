: # This script can be executed either as Windows batch script, or as bash script
: # Bash lines need to have # at the end so bash can ignore \r\n characters
: # See: https://stackoverflow.com/a/45340765/9206248
: #
:;<<WINDOWS_BATCH_SECTION
: Here starts Windows batch version
@echo off

set BASE=%~dp0\players\FooBar_20000

echo ---------------------------------------------------------------------------
echo %DATE% %TIME% Starting FooBar 20000
echo %DATE% %TIME% Starting FooBar 20000 >> %BASE%\Run_FooBar_20000.log

:loop
cd /D %BASE%
dotnet Acme.FooBarPlayer.dll
echo ---------------------------------------------------------------------------
echo %DATE% %TIME% Restarting FooBar 20000
echo %DATE% %TIME% Restarting FooBar 20000 >> %BASE%\Run_FooBar_20000.log
goto :loop

WINDOWS_BATCH_SECTION
#
# Here starts bash version
if [ ! -n "$BASH" ];then exec bash $0 $*; fi #
#
set -e #
BASE="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )/players/FooBar_20000" #
echo --------------------------------------------------------------------------- #
echo $(date '+%Y-%m-%d %H:%M:%S') Starting FooBar 20000 #
echo $(date '+%Y-%m-%d %H:%M:%S') Starting FooBar 20000 >> $BASE/Run_FooBar_20000.log #
while true; do #
(cd $BASE && dotnet Acme.FooBarPlayer.dll) #
echo --------------------------------------------------------------------------- #
echo $(date '+%Y-%m-%d %H:%M:%S') Restarting FooBar 20000 #
echo $(date '+%Y-%m-%d %H:%M:%S') Restarting FooBar 20000 >> $BASE/Run_FooBar_20000.log #
done #
#
