@echo off
cd obj\x86\Debug
start MultiClientServer.exe 1101 1102 1105
start MultiClientServer.exe 1102 1101 1103
start MultiClientServer.exe 1103 1104 1102
start MultiClientServer.exe 1104 1105 1103
start MultiClientServer.exe 1105 1101 1104
