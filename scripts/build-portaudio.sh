#!/usr/bin/env bash

if [ ! -f ./libportaudio.dylib ]; then
  wget https://files.pythonhosted.org/packages/ca/e6/2cd0d856eac10bda6339859b14c489c43a47ceff35b317eb9bb8139b3246/sherpa_onnx-1.4.6-cp311-cp311-macosx_10_14_x86_64.whl

  unzip sherpa_onnx-1.4.6-cp311-cp311-macosx_10_14_x86_64.whl

  cp sherpa_onnx/lib/libsherpa-onnx-portaudio.dylib ./libportaudio.dylib
  rm -rf sherpa_onnx
fi

if [ ! -f ./libportaudio.so ]; then
  wget https://files.pythonhosted.org/packages/f4/91/503548ccb85b2cd4271b0ca4bd9fd2ba126169e68a457d834b2da2ab7c9d/sherpa_onnx-1.4.6-cp311-cp311-manylinux_2_17_x86_64.manylinux2014_x86_64.whl

  unzip ./sherpa_onnx-1.4.6-cp311-cp311-manylinux_2_17_x86_64.manylinux2014_x86_64.whl

  cp sherpa_onnx/lib/libsherpa-onnx-portaudio.so ./libportaudio.so
  rm -rf sherpa_onnx
fi


wget http://files.portaudio.com/archives/pa_stable_v190700_20210406.tgz
tar xvf pa_stable_v190700_20210406.tgz
cd portaudio
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
cmake --build . --config Release
