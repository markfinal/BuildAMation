#!/bin/bash

ubuntu_version=`lsb_release -rs`
echo Installing Gcc 6 dependencies for Ubuntu $ubuntu_version

apt-get -q update > /dev/null
apt-get -q -y --no-install-recommends install g++-6 g++-6-multilib g++-multilib > /dev/null
update-alternatives --remove-all gcc
update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-6 10 --slave /usr/bin/g++ g++ /usr/bin/g++-6 --slave /usr/bin/gcov gcov /usr/bin/gcov-6 --slave /usr/bin/gcov-dump gcov-dump /usr/bin/gcov-dump-6 --slave /usr/bin/gcov-tool gcov-tool /usr/bin/gcov-tool-6 --slave /usr/bin/gcc-ar gcc-ar /usr/bin/gcc-ar-6 --slave /usr/bin/gcc-nm gcc-nm /usr/bin/gcc-nm-6 --slave /usr/bin/gcc-ranlib gcc-ranlib /usr/bin/gcc-ranlib-6
sudo update-alternatives --set gcc /usr/bin/gcc-6
