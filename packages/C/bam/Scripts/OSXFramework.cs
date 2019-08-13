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
using System.Linq;
namespace C
{
    /// <summary>
    /// Derive this module to represent an OSX framework.
    /// </summary>
    public abstract class OSXFramework :
        CModule
    {
        /// <summary>
        /// Path key for this module type
        /// </summary>
        public const string FrameworkKey = "macOS Framework";

        private void
        GetIDName()
        {
            var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Bam.Core.PackageMetaData>("Clang");

            var frameworkPath = this.CreateTokenizedString("$(0)/$(FrameworkLibraryPath)", this.FrameworkPath);

            this.RegisterGeneratedFile(
                FrameworkKey,
                this.CreateTokenizedString(
                    "$(0)/$(1)",
                    new[] { this.FrameworkPath, this.FrameworkBundleName }
                )
            );

            if (!frameworkPath.IsParsed)
            {
                frameworkPath.Parse();
            }
            var idName = Bam.Core.OSUtilities.RunExecutable(
                Bam.Core.OSUtilities.GetInstallLocation("xcrun").First(),
                $"--sdk {clangMeta["SDK"]} otool -DX {frameworkPath.ToString()}" // should use clangMeta.SDK, but this avoids a compile time dependency
            ).StandardOutput;
            this.Macros["IDName"] = Bam.Core.TokenizedString.CreateVerbatim(idName);
        }

        /// <summary>
        /// Initialize this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();
            this.Macros["FrameworkLibraryPath"] = this.FrameworkLibraryPath;
            this.GetIDName();
        }

        /// <summary>
        /// Directory in which the framework is stored.
        /// </summary>
        /// <value>The framework path.</value>
        public abstract Bam.Core.TokenizedString FrameworkPath { get; }

        /// <summary>
        /// Name of the framework bundle itself, ending in .framework
        /// </summary>
        /// <value>The name of the framework bundle.</value>
        protected abstract Bam.Core.TokenizedString FrameworkBundleName { get; }

        /// <summary>
        /// Relative path from the framework directory to the .dylib
        /// </summary>
        /// <value>The framework library path.</value>
        protected abstract Bam.Core.TokenizedString FrameworkLibraryPath { get; }
    }
}
