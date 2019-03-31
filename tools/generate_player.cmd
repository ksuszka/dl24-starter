: # Compile and run code from generate_player.cs
: #
: # This script can be executed either as Windows batch script, or as bash script
: # Bash lines need to have # at the end so bash can ignore \r\n characters
: # See: https://stackoverflow.com/a/45340765/9206248
: #
:;<<WINDOWS_BATCH_SECTION
: Here starts Windows batch version
@echo off

cd %~dp0
dotnet run -p generate_player.csproj

exit /b 0
WINDOWS_BATCH_SECTION
#
# Here starts bash version
if [ ! -n "$BASH" ];then exec bash $0 $*; fi #
#
set -e #
cd "$( dirname "${BASH_SOURCE[0]}" )" #
dotnet run -p generate_player.csproj #
#
