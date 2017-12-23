@echo off
cd obj\x86\Debug
start MultiClientServer.exe 1100 1101 1102 1110
start MultiClientServer.exe 1101 1100 1102
start MultiClientServer.exe 1102 1100 1101 1108
start MultiClientServer.exe 1108 1102