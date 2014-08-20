#!/bin/bash

if [ -n "$1" ]
then
  echo "Creating BuildAMation distribution for version $1"
else
  echo "Please supply a version number"
  exit 1
fi

directory=BuildAMation-$1

git clone https://github.com/markfinal/BuildAMation.git $directory
# TODO: checkout to a branch or tag
# even better... test if there is a tag, if not, create it, after the version update below
# this would allow creating a fixed point of code with the correct version number, but also being able to
# recreate the exact code again
rm $directory/.gitignore
rm $directory/.git

# Update version number to that specified
find $directory -name "AssemblyInfo.cs" | xargs sed 's/AssemblyInformationalVersion("[0-9.]*")/AssemblyInformationalVersion("'$1'")/g' -i

# tar up the source tree
tar -czf BuildAMation-$1-source.tgz $directory

# now compile the source tree, optimized
pushd $directory
mdtool build --target:Build --configuration:Release BuildAMation.sln
popd

# now tar up the 'bin' folder, and any additional files necessary to ship
tar -czf BuildAMation-$1-binary.tgz $directory/bin $directory/Changelog.txt $directory/COPYING.LESSER $directory/env.bat $directory/env.sh $directory/License.txt $directory/packages $directory/testpackages

# remove checkout folder
rm -fr $directory
