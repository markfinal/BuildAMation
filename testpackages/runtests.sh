#!/bin/bash -e

root=$PWD
platform=$1
builder=$2
buildroot=build

function deleteBuildRoot
{
    if [ -d "$1/$2/$3/$buildroot" ]; then
        echo "Deleting '$1/$2/$3/$buildroot' directory and all children"
        rm -fr $1/$2/$3/$buildroot
    fi
}

function runTest
{
    inputFile=$1/$2/$3/test$platform.txt
    if [ -f $inputFile ]; then
        pushd $1/$2/$3
        while IFS=$'\r\n' read responseFile; do
            Opus @$responseFile -platforms=$platform -configurations="debug;optimized" -buildroot=$buildroot -builder=$builder -verbosity=1;
            if [ $? != 0 ]; then
                exit $? # see first line
            fi
        done < "$inputFile"
        popd
    fi
}

find $root -maxdepth 1 -type d \( ! -iname ".*" \) | while read FILENAME;
do
  package=`basename $FILENAME`
  if [[ "$package" != "$root" ]]; then
    find $root/${package} -maxdepth 1 -type d \( ! -iname ".*" \) | while read FILENAME;
    do
      version=`basename $FILENAME`
      if [[ "$version" != "${package}" ]]; then
        deleteBuildRoot $root $package $version
        runTest $root $package $version
        deleteBuildRoot $root $package $version
      fi
    done
  fi
done

