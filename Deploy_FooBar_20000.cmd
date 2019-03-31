: # Compile and run code from generate_player.cs
: #
: # This script can be executed either as Windows batch script, or as bash script
: # Bash lines need to have # at the end so bash can ignore \r\n characters
: # See: https://stackoverflow.com/a/45340765/9206248
: #
:;<<WINDOWS_BATCH_SECTION
: Here starts Windows batch version
@echo off

set BASE=%~dp0
set SRC_DIR=%BASE%\src\Acme.FooBarPlayer\bin\Debug
set DEST_DIR=%BASE%\players\FooBar_20000

if not exist "%DEST_DIR%" mkdir "%DEST_DIR%"

xcopy "%SRC_DIR%" "%DEST_DIR%" /e /f /y /exclude:%BASE%\xcopy.exclude

exit /b 0
WINDOWS_BATCH_SECTION
#
# Here starts bash version
if [ ! -n "$BASH" ];then exec bash $0 $*; fi #
#
set -e #
BASE="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )" #
SRC_DIR=${BASE}/src/Acme.FooBarPlayer/bin/Debug/ #
DEST_DIR=${BASE}/players/FooBar_20000 #
mkdir -p "$DEST_DIR" #
rsync -av "$SRC_DIR" "$DEST_DIR" --exclude-from=${BASE}/xcopy.exclude #
#
