#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace C.Cxx
{
    // TOOD: rename enum as structured?
    /// <summary>
    /// Which exception handler to use
    /// </summary>
    public enum EExceptionHandler
    {
        /// <summary>
        /// Exception handling is not enabled
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Use synchronous exception handling.
        /// </summary>
        Synchronous = 1,

        /// <summary>
        /// Use asynchronous exception handling.
        /// </summary>
        Asynchronous = 2,

        /// <summary>
        /// Use synchronous exception handle with C externs
        /// </summary>
        SyncWithCExternFunctions = 3
    }

    /// <summary>
    /// C++ language standard varieties
    /// </summary>
    public enum ELanguageStandard
    {
        /// <summary>
        /// No standard defined, use compiler default
        /// </summary>
        NotSet,

        /// <summary>
        /// Compile against the C++ 98 standard.
        /// </summary>
        Cxx98,

        /// <summary>
        /// Compile against the GNU C++ 98 standard.
        /// </summary>
        GnuCxx98,

        /// <summary>
        /// Compile against the C++11 standard.
        /// </summary>
        Cxx11,
        // TODO: GnuCxx11
    }

    /// <summary>
    /// Specify the C++ standard library to compile against
    /// </summary>
    public enum EStandardLibrary
    {
        /// <summary>
        /// Undefined, use compiler standard library.
        /// </summary>
        NotSet,

        /// <summary>
        /// Use the libstdc++ standard library.
        /// </summary>
        libstdcxx,

        /// <summary>
        /// Use the libc++ standard library.
        /// </summary>
        libcxx
    }
}
