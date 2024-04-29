@echo off
cd ..
cd Valuator

start dotnet run --urls "http://0.0.0.0:5001" 
start dotnet run --urls "http://0.0.0.0:5002"

start "C:\nginx-1.26.0\nginx.exe"