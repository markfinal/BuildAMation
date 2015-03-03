/*
Copyright 2010-2015 Mark Final

This file is part of BuildAMation.

BuildAMation is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

BuildAMation is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
*/
#ifdef MAIN_C
#error MAIN_C has been defined
#endif

#ifndef D_BAM_CONFIGURATION_DEBUG
#error This file can only be compiled in BuildAMation debug configuration builds
#endif

#include "header.h"

const char *GetConfiguration()
{
    return "Debug";
}
