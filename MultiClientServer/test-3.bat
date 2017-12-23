@echo off
cd obj\x86\Debug
start MultiClientServer.exe 1101 1102 1103
start MultiClientServer.exe 1102 1101 1103
start MultiClientServer.exe 1103 1102 1101 1104

start MultiClientServer.exe 1104 1105 1106 1103
start MultiClientServer.exe 1105 1104 1106
start MultiClientServer.exe 1106 1105 1104
