#!/bin/bash

root=$PWD
find $root -maxdepth 1 -mindepth 1 -type d \( ! -iname ".*" \) | while read FILENAME;
do
  package=`basename $FILENAME`
  if [[ "$package" != "$root" ]]; then
    find $root/${package} -maxdepth 1 -type d \( ! -iname ".*" \) | while read FILENAME;
    do
      version=`basename $FILENAME`
      if [[ "$version" != "${package}" ]]; then
        if [ -d "$root/$package/$version/build" ]; then
            echo "Deleting '$root/$package/$version/build' directory and all children"
            rm -fr $root/$package/$version/build
        fi
        if [ -d "$root/$package/$version/debug_build" ]; then
            echo "Deleting '$root/$package/$version/debug_build' directory and all children"
            rm -fr $root/$package/$version/debug_build
        fi
        if [ -d "$root/$package/$version/BamProject" ]; then
            echo "Deleting '$root/$package/$version/BamProject' directory and all children"
            rm -fr $root/$package/$version/BamProject
        fi
      fi
    done
  fi
done

