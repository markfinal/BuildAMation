#!/bin/bash

ubuntu_version=`lsb_release -rs`
echo Installing Gcc 5 dependencies for Ubuntu $ubuntu_version

apt-get -q update
apt-get -q -y --no-install-recommends install g++-5 g++-5-multilib g++-multilib
update-alternatives --remove-all gcc
update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-5 10 --slave /usr/bin/g++ g++ /usr/bin/g++-5 --slave /usr/bin/gcov gcov /usr/bin/gcov-5 --slave /usr/bin/gcov-dump gcov-dump /usr/bin/gcov-dump-5 --slave /usr/bin/gcov-tool gcov-tool /usr/bin/gcov-tool-5 --slave /usr/bin/gcc-ar gcc-ar /usr/bin/gcc-ar-5 --slave /usr/bin/gcc-nm gcc-nm /usr/bin/gcc-nm-5 --slave /usr/bin/gcc-ranlib gcc-ranlib /usr/bin/gcc-ranlib-5
sudo update-alternatives --set gcc /usr/bin/gcc-5
