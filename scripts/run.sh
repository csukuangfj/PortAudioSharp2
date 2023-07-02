#!/usr/bin/env bash
# Copyright (c)  2023  Xiaomi Corporation

set -ex

mkdir -p macos linux windows all
rm -rf packages

cp -v ./libportaudio.dylib ./macos
cp -v ./libportaudio.so ./linux
cp -v ./portaudio.dll ./windows

./generate.py

pushd linux
dotnet build -c Release
dotnet pack -c Release -o ../../PortAudioSharp/packages
popd

pushd macos
dotnet build -c Release
dotnet pack -c Release -o ../../PortAudioSharp/packages
popd

pushd windows
dotnet build -c Release
dotnet pack -c Release -o ../../PortAudioSharp/packages
popd

pushd ../PortAudioSharp
dotnet build -c Release
dotnet pack -c Release -o ./packages
popd

mv ../PortAudioSharp/packages .
