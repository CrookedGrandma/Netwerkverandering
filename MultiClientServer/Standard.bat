@echo off
cd obj\x86\Debug
start MultiClientServer.exe 1100 1102 1101
start MultiClientServer.exe 1101 1100 1102
start MultiClientServer.exe 1102 1100 1101 1106 1104
start MultiClientServer.exe 1104 1102 1105
start MultiClientServer.exe 1105 1104 1106
start MultiClientServer.exe 1106 1105 1102