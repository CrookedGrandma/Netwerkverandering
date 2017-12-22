@echo off
cd obj\x86\Debug
start MultiClientServer.exe 55500 55501
start MultiClientServer.exe 55501 55500 55502
start MultiClientServer.exe 55502 55501 55503
start MultiClientServer.exe 55503 55502 55504
start MultiClientServer.exe 55504 55503 55505
start MultiClientServer.exe 55505 55504 55506
start MultiClientServer.exe 55506 55505 55507
start MultiClientServer.exe 55507 55506 55508
start MultiClientServer.exe 55508 55507 55509
start MultiClientServer.exe 55509 55508 55510
start MultiClientServer.exe 55510 55509 55511
start MultiClientServer.exe 55511 55510 55512
start MultiClientServer.exe 55512 55511 55513
start MultiClientServer.exe 55513 55512 55514
start MultiClientServer.exe 55514 55513

