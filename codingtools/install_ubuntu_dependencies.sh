#!/bin/bash

ubuntu_version=`lsb_release -rs`
echo Installing dependencies for Ubuntu $ubuntu_version

if [ "$ubuntu_version" = "14.04" ]; then
# need at least git 2.x
sudo add-apt-repository -y ppa:git-core/ppa
fi

wget -q https://packages.microsoft.com/config/ubuntu/$ubuntu_version/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get install apt-transport-https
sudo apt-get -q update

# git
# dotnet core sdk
# G++
# 32-bit GCC and G++
# GCC ObjectiveC
# GNUStep
# 32 and 64-bit X11 development files for Test12
# chrpath for ChrpathTest1
# doxygen for doc generation
sudo apt-get -q -y --no-install-recommends install \
git \
dotnet-sdk-2.1 \
g++ \
gcc-multilib \
g++-multilib \
gobjc \
gnustep-devel \
libx11-dev \
libx11-dev:i386 \
chrpath \
doxygen \
python
