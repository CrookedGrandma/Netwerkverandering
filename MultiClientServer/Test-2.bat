@echo off
cd obj\x86\Debug
start MultiClientServer.exe 1101 1102
start MultiClientServer.exe 1102 1101 1103
start MultiClientServer.exe 1103 1102 1104
start MultiClientServer.exe 1104 1103 1105
start MultiClientServer.exe 1105 1104 1106
start MultiClientServer.exe 1106 1104
