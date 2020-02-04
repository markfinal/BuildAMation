#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace C
{
    /// <summary>
    /// Tool for linking code.
    /// </summary>
    abstract class LinkerTool :
        Bam.Core.PreBuiltTool
    {
        /// <summary>
        /// Get the library path for the specified library.
        /// </summary>
        /// <param name="library">Library module</param>
        /// <returns>Path to library</returns>
        public abstract Bam.Core.TokenizedString GetLibraryPath(
            CModule library);

        /// <summary>
        /// Process dependency between executable and library.
        /// </summary>
        /// <param name="executable">Executable</param>
        /// <param name="library">Library</param>
        public abstract void ProcessLibraryDependency(
            CModule executable,
            CModule library);

        /// <summary>
        /// Process dependency between executable and an SDK.
        /// </summary>
        /// <param name="executable">Executable</param>
        /// <param name="sdk">SDK </param>
        /// <param name="direct">Is this a direct dependency</param>
        public virtual void ProcessSDKDependency(
            ConsoleApplication executable,
            SDKTemplate sdk,
            bool direct)
        {
            foreach (var sdkLib in executable.SDKLibrariesToLink(sdk))
            {
                this.ProcessLibraryDependency(executable as CModule, sdkLib as CModule);
            }
        }

        /// <summary>
        /// Get the version of the toolchain for this tool
        /// </summary>
        public ToolchainVersion Version { get; protected set; }
    }
}
