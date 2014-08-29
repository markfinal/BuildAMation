#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace Bam.Core
{
    [System.Flags]
    public enum EPlatform
    {
        Invalid = 0,
        Win32   = (1 << 0),
        Win64   = (1 << 1),
        Unix32  = (1 << 2),
        Unix64  = (1 << 3),
        OSX32   = (1 << 4),
        OSX64   = (1 << 5),

        Windows = Win32 | Win64,
        Unix    = Unix32 | Unix64,
        OSX     = OSX32 | OSX64,
        Posix   = Unix | OSX,

        NotWindows = ~Windows,
        NotUnix    = ~Unix,
        NotOSX     = ~OSX,
        NotPosix   = ~Posix,

        All        = Windows | Unix | OSX
    }
}
