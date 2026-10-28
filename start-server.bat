@echo off
chcp 65001 > nul
title فروشگاه مارکت — سرور
echo Starting shop server on http://localhost:5200 ...
echo (این پنجره را نبندید؛ برای توقف Ctrl+C)
dotnet run --project "D:\Projects\Motoreto\src\Shop.Presentation" --no-build --urls http://localhost:5200
pause
