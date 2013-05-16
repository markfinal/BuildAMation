#!/bin/bash

DefaultFlavour=Release
# use the incoming argument as the sub-directory, or fall back on the default
flavour=${1:-$DefaultFlavour}
OpusPath=$PWD/bin/$flavour

if [ ! -d "$OpusPath" ]
then
  echo "*** ERROR: Opus directory '$OpusPath' does not exist ***"
else
  echo "Added '$OpusPath' to the start of PATH"
  export OPUSPATH=$OpusPath
  export PATH=$OPUSPATH:$PATH
fi

