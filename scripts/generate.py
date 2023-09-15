#!/usr/bin/env python3
# Copyright (c)  2023  Xiaomi Corporation

import argparse
import re
from pathlib import Path

import jinja2


def get_version():
    return "0.5.0"


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


def process_macos(s):
    libs = "libportaudio.dylib"

    d = get_dict()
    d["dotnet_rid"] = "osx-x64"
    d["libs"] = libs

    environment = jinja2.Environment()
    template = environment.from_string(s)
    s = template.render(**d)
    with open("./macos/portaudio.runtime.csproj", "w") as f:
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
    process_macos(s)
    process_linux(s)
    process_windows(s)


if __name__ == "__main__":
    main()
