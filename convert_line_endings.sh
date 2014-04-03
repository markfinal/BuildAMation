#!/bin/bash

grep -IUrl --color '^M' . --include=*.cs | xargs -ifile fromdos 'file'

