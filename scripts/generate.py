#!/usr/bin/env python3
# Copyright (c)  2023  Xiaomi Corporation

import argparse
import re
from pathlib import Path

import jinja2


def get_version():
    return "1.0.3"


def read_proj_file(filename):
    with open(filename) as f:
        return f.read()


def get_dict():
    version = get_version()
    return {
        "version": get_version(),
    }


def process_linux(s):
    libs = "libportaudio.so"

    d = get_dict()
    d["dotnet_rid"] = "linux-x64"
    d["libs"] = libs

    environment = jinja2.Environment()
    template = environment.from_string(s)
    s = template.render(**d)
    with open("./linux/portaudio.runtime.csproj", "w") as f:
        f.write(s)


def process_macos(s, arch):
    libs = "libportaudio.dylib"

    d = get_dict()
    d["dotnet_rid"] = f"osx-{arch}"
    d["libs"] = libs

    environment = jinja2.Environment()
    template = environment.from_string(s)
    s = template.render(**d)
    with open(f"./macos-{arch}/portaudio.runtime.csproj", "w") as f:
        f.write(s)

def process_ios(s):
    libs = "libportaudio.a"

    d = get_dict()
    d["dotnet_rid"] = f"ios-arm64"
    d["libs"] = libs

    environment = jinja2.Environment()
    template = environment.from_string(s)
    s = template.render(**d)
    with open(f"./ios-arm64/portaudio.runtime.ios.csproj", "w") as f:
        f.write(s)

def process_windows(s):
    libs = "portaudio.dll"

    d = get_dict()
    d["dotnet_rid"] = "win-x64"
    d["libs"] = libs

    environment = jinja2.Environment()
    template = environment.from_string(s)
    s = template.render(**d)
    with open("./windows/portaudio.runtime.csproj", "w") as f:
        f.write(s)


def main():
    s = read_proj_file("./portaudio.csproj.runtime.in")
    process_macos(s, "x64")
    process_macos(s, "arm64")
    process_linux(s)
    process_windows(s)
    process_ios(s)


if __name__ == "__main__":
    main()
