#!/bin/bash

root=$PWD
find $root -maxdepth 1 -mindepth 1 -type d \( ! -iname ".*" \) | while read FILENAME;
do
  package=`basename $FILENAME`
  if [[ "$package" != "$root" ]]; then
    find $root/${package} -maxdepth 1 -type d \( ! -iname ".*" \) | while read FILENAME;
    do
      subDir=`basename $FILENAME`
      if [[ "$subDir" == "bam" ]]; then
        if [ -d "$root/$package/build" ]; then
            echo "Deleting '$root/$package/build' directory and all children"
            rm -fr $root/$package/build
        fi
        if [ -d "$root/$package/debug_build" ]; then
            echo "Deleting '$root/$package/debug_build' directory and all children"
            rm -fr $root/$package/debug_build
        fi
        if [ -d "$root/$package/PackageDebug" ]; then
            echo "Deleting '$root/$package/PackageDebug' directory and all children"
            rm -fr $root/$package/PackageDebug
        fi
      fi
    done
  fi
done

