#!/bin/bash

root=$PWD
find $root -maxdepth 1 -type d \( ! -iname ".*" \) | while read FILENAME;
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
        if [ -d "$root/$package/$version/Opus" ]; then
            echo "Deleting '$root/$package/$version/Opus' directory and all children"
            rm -fr $root/$package/$version/Opus
        fi
      fi
    done
  fi
done

