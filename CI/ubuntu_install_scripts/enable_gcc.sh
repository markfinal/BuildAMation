#!/bin/bash

if [[ $EUID -ne 0 ]]; then
   echo "This script must be run as root"
   exit 1
fi

gcc_version=$1
ubuntu_version=`lsb_release -rs`
echo Enabling Gcc ${gcc_version} for Ubuntu $ubuntu_version

update-alternatives --set gcc /usr/bin/gcc-${gcc_version}

gcc --version
