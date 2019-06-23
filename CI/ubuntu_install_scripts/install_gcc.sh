#!/bin/bash

if [[ $EUID -ne 0 ]]; then
   echo "This script must be run as root"
   exit 1
fi

gcc_version=$1
ubuntu_version=`lsb_release -rs`
echo Installing Gcc ${gcc_version} dependencies for Ubuntu $ubuntu_version

add-apt-repository -y ppa:ubuntu-toolchain-r/test > /dev/null 2>&1

apt-get -q update > /dev/null
apt-get -q -y --no-install-recommends install g++-${gcc_version} g++-${gcc_version}-multilib g++-multilib gobjc++-${gcc_version} gobjc++-${gcc_version}-multilib > /dev/null
update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-${gcc_version} 10 --slave /usr/bin/g++ g++ /usr/bin/g++-${gcc_version} --slave /usr/bin/gcov gcov /usr/bin/gcov-${gcc_version} --slave /usr/bin/gcov-dump gcov-dump /usr/bin/gcov-dump-${gcc_version} --slave /usr/bin/gcov-tool gcov-tool /usr/bin/gcov-tool-${gcc_version} --slave /usr/bin/gcc-ar gcc-ar /usr/bin/gcc-ar-${gcc_version} --slave /usr/bin/gcc-nm gcc-nm /usr/bin/gcc-nm-${gcc_version} --slave /usr/bin/gcc-ranlib gcc-ranlib /usr/bin/gcc-ranlib-${gcc_version}
