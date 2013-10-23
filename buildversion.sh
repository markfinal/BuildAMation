#!/bin/bash

if [ -n "$1" ]
then
  echo "Creating Opus distribution for version $1"
else
  echo "Please supply a version number"
  exit 1
fi

directory=opus-$1

svn export http://opus.googlecode.com/svn/trunk $directory
rm $directory/.gitignore

find $directory -name "AssemblyInfo.cs" | xargs sed 's/AssemblyInformationalVersion("[0-9.]*")/AssemblyInformationalVersion("'$1'")/g' -i

tar -czf opus-$1-source.tgz $directory

pushd $directory
mdtool build --target:Build --configuration:Release Opus.sln
popd

tar -czf opus-$1-binary.tgz $directory/bin $directory/Changelog.txt $directory/env.bat $directory/env.sh $directory/License.txt $directory/packages $directory/testpackages
rm -fr $directory

