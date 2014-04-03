#!/bin/bash

grep -IUrl --color '
' . --include=*.cs | xargs -ifile fromdos 'file'

