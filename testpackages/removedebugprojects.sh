#!/bin/bash

set root ${PWD}
for package in $(find $root -maxdepth 1 -type d \( ! -iname ".*" \) ); do
  if [[ "$package" != "$root" ]]; then
    for version in $(find ${package} -maxdepth 1 -type d \( ! -iname ".*" \) ); do
      if [[ "$version" != "${package}" ]]; then
        if [ -d "$version/build" ]; then
            rm -fr $version/build
        fi
        if [ -d "$version/Opus" ]; then
            rm -fr $version/Opus
        fi
      fi
    done
  fi
done

