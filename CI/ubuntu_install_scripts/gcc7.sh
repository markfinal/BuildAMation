#!/bin/bash

ubuntu_version=`lsb_release -rs`
echo Installing Gcc 7 dependencies for Ubuntu $ubuntu_version

apt-get -q update > /dev/null
apt-get -q -y --no-install-recommends install g++-7 g++-7-multilib g++-multilib gobjc++-7 gobjc++-7-multilib > /dev/null
update-alternatives --remove-all gcc
update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-7 10 --slave /usr/bin/g++ g++ /usr/bin/g++-7 --slave /usr/bin/gcov gcov /usr/bin/gcov-7 --slave /usr/bin/gcov-dump gcov-dump /usr/bin/gcov-dump-7 --slave /usr/bin/gcov-tool gcov-tool /usr/bin/gcov-tool-7 --slave /usr/bin/gcc-ar gcc-ar /usr/bin/gcc-ar-7 --slave /usr/bin/gcc-nm gcc-nm /usr/bin/gcc-nm-7 --slave /usr/bin/gcc-ranlib gcc-ranlib /usr/bin/gcc-ranlib-7
sudo update-alternatives --set gcc /usr/bin/gcc-7
