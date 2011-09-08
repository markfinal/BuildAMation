#!/bin/bash

DefaultFlavour=Release

if [ -n "$1" ]
then
  flavour=$1
else
  flavour=$DefaultFlavour
fi

OpusPath=$PWD/bin/$flavour

if [ ! -d "$OpusPath" ]
then
  echo "Opus directory '$OpusPath' does not exist"
else
  echo "Adding '$OpusPath' to the start of PATH"
  export OPUSPATH=$OpusPath
  export PATH=$OPUSPATH:$PATH
fi

