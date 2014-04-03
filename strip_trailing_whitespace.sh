#!/bin/bash

find . -type f -name '*.cs' | xargs -ifile sed --in-place 's/[[:space:]]\+$//' 'file'

