@echo off
cd obj\x86\Debug
start MultiClientServer.exe 55500 55501 55502
start MultiClientServer.exe 55501 55500 55502
start MultiClientServer.exe 55502 55500 55501 55503 55504
start MultiClientServer.exe 55503 55502 55505
start MultiClientServer.exe 55504 55502 55505
start MultiClientServer.exe 55505 55503 55504