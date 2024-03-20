# Dockerfile
#
# Build PortAudio v19.7.0, Linux
#
# Use this file in an empty directory with commands:
#
#     podman build . -t portaudio-build
#     mkdir out
#     podman run --mount type=bind,source=out,dst=/out -t portaudio-build
#
# That should produce shared library in out/ directory.
# Using Ubuntu 16.04 means it hopefully will work in most modern Linux systems.

FROM ubuntu:16.04

RUN apt update
RUN apt install -y build-essential cmake git libasound2-dev libjack-dev
RUN git clone https://github.com/PortAudio/portaudio.git --branch v19.7.0 --single-branch
RUN mkdir build && cd build && cmake -DCMAKE_BUILD_TYPE=Release ../portaudio
RUN echo "#!/bin/bash\ncd /build && make -j12 && cp libportaudio.so /out" >> /script.sh
RUN chmod a+x /script.sh
ENTRYPOINT /script.sh
