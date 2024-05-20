@echo off

cd "../RankCalculator"
start "" dotnet run

cd "../Valuator"

start dotnet run --urls "http://0.0.0.0:5001" 
start dotnet run --urls "http://0.0.0.0:5002"

cd "C:\nats-server"
start "" "nats-server.exe"

cd "C:\nginx-1.26.0"
start "" "nginx.exe"