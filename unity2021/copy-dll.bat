
@echo off

REM !!! Generated by the fmp-cli 1.86.0.  DO NOT EDIT!

md VideoSee\Assets\3rd\fmp-xtc-videosee

cd ..\vs2022
dotnet build -c Release

copy fmp-xtc-videosee-lib-mvcs\bin\Release\netstandard2.1\*.dll ..\unity2021\VideoSee\Assets\3rd\fmp-xtc-videosee\
