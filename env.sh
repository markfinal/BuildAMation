#!/bin/bash

DefaultFlavour=Release
# use the incoming argument as the sub-directory, or fall back on the default
flavour=${1:-$DefaultFlavour}
curPath=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
ExecutablePath="$curPath/bin/$flavour"

if [ ! -d "$ExecutablePath" ]; then
  echo "*** ERROR: BuildAMation directory '$ExecutablePath' does not exist ***"
else
  export PATH="$ExecutablePath":$PATH
  bam --version
fi
