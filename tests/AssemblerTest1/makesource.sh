#!/bin/bash

xcrun --sdk macosx10.12 clang -mmacosx-version-min=10.6 -O2 -arch i386 -S -o source/clang/helloworld32.s -DBUILD32 reference/helloworld.c
xcrun --sdk macosx10.12 clang -mmacosx-version-min=10.6 -O2 -arch x86_64 -S -o source/clang/helloworld64.s reference/helloworld.c
