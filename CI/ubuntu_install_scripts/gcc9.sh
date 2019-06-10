#!/bin/bash

ubuntu_version=`lsb_release -rs`
echo Installing Gcc 9 dependencies for Ubuntu $ubuntu_version

# this assumes Ubuntu 18
sudo add-apt-repository ppa:ubuntu-toolchain-r/test

sudo apt-get -q update
sudo apt-get -q -y --no-install-recommends install g++-9 g++-9-multilib g++-multilib
sudo update-alternatives --remove-all gcc
sudo update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-9 10 --slave /usr/bin/g++ g++ /usr/bin/g++-9 --slave /usr/bin/gcov gcov /usr/bin/gcov-9 --slave /usr/bin/gcov-dump gcov-dump /usr/bin/gcov-dump-9 --slave /usr/bin/gcov-tool gcov-tool /usr/bin/gcov-tool-9 --slave /usr/bin/gcc-ar gcc-ar /usr/bin/gcc-ar-9 --slave /usr/bin/gcc-nm gcc-nm /usr/bin/gcc-nm-9 --slave /usr/bin/gcc-ranlib gcc-ranlib /usr/bin/gcc-ranlib-9
sudo update-alternatives --set gcc /usr/bin/gcc-9
