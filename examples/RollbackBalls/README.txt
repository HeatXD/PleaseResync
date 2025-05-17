run the example

@echo off
REM Launch spectating instances
start "" cmd /k RollbackBalls.exe id=0 /s
start "" cmd /k RollbackBalls.exe id=1 /s

REM Launch non-spectating instances
start "" cmd /k RollbackBalls.exe id=0
start "" cmd /k RollbackBalls.exe id=1

pause