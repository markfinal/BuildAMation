#!/bin/bash

ubuntu_version=`lsb_release -rs`
echo Installing Gcc 8 dependencies for Ubuntu $ubuntu_version

sudo apt-get -q update
sudo apt-get -q -y --no-install-recommends install g++-8 g++-8-multilib g++-multilib
sudo update-alternatives --remove-all gcc
sudo update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-8 10 --slave /usr/bin/g++ g++ /usr/bin/g++-8 --slave /usr/bin/gcov gcov /usr/bin/gcov-8 --slave /usr/bin/gcov-dump gcov-dump /usr/bin/gcov-dump-8 --slave /usr/bin/gcov-tool gcov-tool /usr/bin/gcov-tool-8 --slave /usr/bin/gcc-ar gcc-ar /usr/bin/gcc-ar-8 --slave /usr/bin/gcc-nm gcc-nm /usr/bin/gcc-nm-8 --slave /usr/bin/gcc-ranlib gcc-ranlib /usr/bin/gcc-ranlib-8
sudo update-alternatives --set gcc /usr/bin/gcc-8
