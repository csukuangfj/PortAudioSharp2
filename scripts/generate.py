#!/usr/bin/env python3
# Copyright (c)  2023  Xiaomi Corporation

import argparse
import re
from pathlib import Path

import jinja2


def get_version():
    return "1.0.6"


def read_proj_file(filename):
    with open(filename) as f:
        return f.read()


def get_dict():
    version = get_version()
    return {
        "version": get_version(),
    }


def process_linux(s, arch):
    libs = "libportaudio.so"

    d = get_dict()
    d["dotnet_rid"] = f"linux-{arch}"
    d["dotnet_rid2"] = f"linux-{arch}"
    if arch == "arm64":
        d["dotnet_rid2"] = f"linux-aarch64"
    d["libs"] = libs

    environment = jinja2.Environment()
    template = environment.from_string(s)
    s = template.render(**d)
    with open(f"./linux-{arch}/portaudio.runtime.csproj", "w") as f:
        f.write(s)


def process_macos(s, arch):
    libs = "libportaudio.dylib"

    d = get_dict()
    d["dotnet_rid"] = f"osx-{arch}"
    d["dotnet_rid2"] = f"osx-{arch}"
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
    d["dotnet_rid2"] = f"ios-arm64"
    d["libs"] = libs

    environment = jinja2.Environment()
    template = environment.from_string(s)
    s = template.render(**d)
    with open(f"./ios-arm64/portaudio.runtime.ios.csproj", "w") as f:
        f.write(s)


def process_windows(s, arch):
    libs = "portaudio.dll"

    d = get_dict()
    d["dotnet_rid"] = f"win-{arch}"
    d["dotnet_rid2"] = f"win-{arch}"
    d["libs"] = libs

    environment = jinja2.Environment()
    template = environment.from_string(s)
    s = template.render(**d)
    with open(f"./windows-{arch}/portaudio.runtime.csproj", "w") as f:
        f.write(s)


def main():
    s = read_proj_file("./portaudio.csproj.runtime.in")
    process_macos(s, "x64")
    process_macos(s, "arm64")

    process_linux(s, "x64")
    process_linux(s, "arm64")

    process_windows(s, "x64")
    process_ios(s)


if __name__ == "__main__":
    main()
