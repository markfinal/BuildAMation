#region License
// Copyright (c) 2010-2016, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    /// <summary>
    /// Platform enumeration
    /// </summary>
    [System.Flags]
    public enum EPlatform
    {
        /// <summary>
        /// No such platform
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// 32-bit Windows
        /// </summary>
        Win32   = (1 << 0),

        /// <summary>
        /// 64-bit Windows
        /// </summary>
        Win64   = (1 << 1),

        /// <summary>
        /// 32-bit Linux.
        /// </summary>
        Linux32 = (1 << 2),

        /// <summary>
        /// 64-bit Linux.
        /// </summary>
        Linux64 = (1 << 3),

        /// <summary>
        /// 32-bit OSX
        /// </summary>
        OSX32   = (1 << 4),

        /// <summary>
        /// 64-bit OSX
        /// </summary>
        OSX64   = (1 << 5),

        /// <summary>
        /// Alias for any Windows
        /// </summary>
        Windows = Win32 | Win64,

        /// <summary>
        /// Alias for any Linux
        /// </summary>
        Linux   = Linux32 | Linux64,

        /// <summary>
        /// Alias for any OSX
        /// </summary>
        OSX     = OSX32 | OSX64,

        /// <summary>
        /// Alias for Linux or OSX.
        /// </summary>
        Posix   = Linux | OSX,

        /// <summary>
        /// Alias for any platform other than Windows
        /// </summary>
        NotWindows = ~Windows,

        /// <summary>
        /// Alias for any platform other than Linux.
        /// </summary>
        NotLinux   = ~Linux,

        /// <summary>
        /// Alias for any platform other than OSX.
        /// </summary>
        NotOSX     = ~OSX,

        /// <summary>
        /// Alias for any non-posix platform.
        /// </summary>
        NotPosix   = ~Posix,

        /// <summary>
        /// Alias for all platforms.
        /// </summary>
        All        = Windows | Linux | OSX
    }
}
