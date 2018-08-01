#!/bin/bash

sysinfo=$( uname )
if [[ $sysinfo == Darwin ]]
then
xcrun --sdk macosx10.12 clang -mmacosx-version-min=10.6 -O2 -arch i386 -S -o source/clang/helloworld32.s -DBUILD32 reference/helloworld.c
xcrun --sdk macosx10.12 clang -mmacosx-version-min=10.6 -O2 -arch x86_64 -S -o source/clang/helloworld64.s reference/helloworld.c
fi

if [[ $sysinfo == Linux ]]
then
gcc -O2 -m32 -S -o source/gcc/helloworld32.S -DBUILD32 reference/helloworld.c
gcc -O2 -m64 -S -o source/gcc/helloworld64.S reference/helloworld.c
fi

if [[ $sysinfo == 'MINGW32_NT-6.2' ]]
then
# requires Mingw to be pathed in
gcc -O2 -m32 -S -o source/mingw/helloworld32.S -DBUILD32 reference/helloworld.c
fi
