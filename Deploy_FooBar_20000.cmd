: # This script can be executed either as Windows batch script, or as bash script
: # Bash lines need to have # at the end so bash can ignore \r\n characters
: # See: https://stackoverflow.com/a/45340765/9206248
: #
:;<<WINDOWS_BATCH_SECTION
: Here starts Windows batch version
@echo off

set BASE=%~dp0
set PROJECT=%BASE%\src\Acme.FooBarPlayer
set DEST_DIR=%BASE%\players\FooBar_20000

dotnet publish "%PROJECT%" -c Debug -o "%DEST_DIR%"

exit /b 0
WINDOWS_BATCH_SECTION
#
# Here starts bash version
if [ ! -n "$BASH" ];then exec bash $0 $*; fi #
#
set -e #
BASE="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )" #
PROJECT=${BASE}/src/Acme.FooBarPlayer #
DEST_DIR=${BASE}/players/FooBar_20000 #
dotnet publish "${PROJECT}" -c Debug -o "$DEST_DIR" #
#
