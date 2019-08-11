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
namespace VisualC
{
    /// <summary>
    /// Class representing meta data for this package
    /// </summary>
    public class MetaData :
        VisualCCommon.MetaData,
        VisualCCommon.IRuntimeLibraryPathMeta
    {
        /// <summary>
        /// Create the default instance
        /// </summary>
        public MetaData()
        {
            this.SolutionFormatVersion = "12.00";
            this.PlatformToolset = "v110";
            this.VCXProjToolsVersion = "4.0";
            this.VCXProjFiltersToolsVersion = "4.0";
        }

        /// <summary>
        /// Path within the installation to vcvars.bat
        /// </summary>
        protected override string Subpath_to_vcvars => "VC";

        /// <summary>
        /// This version does not have native 64-bit tools
        /// </summary>
        protected override bool HasNative64BitTools => false;

        /// <summary>
        /// The VisualStudio solution format version
        /// </summary>
        public string
        SolutionFormatVersion
        {
            get
            {
                return this.Meta["SolutionFormatVersion"] as string;
            }

            private set
            {
                this.Meta["SolutionFormatVersion"] = value;
            }
        }

        /// <summary>
        /// Platform toolset for the VisualStudio projects
        /// </summary>
        public string
        PlatformToolset
        {
            get
            {
                return this.Meta["PlatformToolset"] as string;
            }

            set
            {
                this.Meta["PlatformToolset"] = value;
            }
        }

        /// <summary>
        /// VCXProject tools version
        /// </summary>
        public string
        VCXProjToolsVersion
        {
            get
            {
                return this.Meta["VCXProjToolsVersion"] as string;
            }

            private set
            {
                this.Meta["VCXProjToolsVersion"] = value;
            }
        }

        /// <summary>
        /// VCXProject filter tools version
        /// </summary>
        public string
        VCXProjFiltersToolsVersion
        {
            get
            {
                return this.Meta["VCXProjFiltersToolsVersion"] as string;
            }

            private set
            {
                this.Meta["VCXProjFiltersToolsVersion"] = value;
            }
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CRuntimePaths(
            C.EBit depth)
        {
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/redist/x86/Microsoft.VC110.CRT/msvcr110.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
                    break;

                case C.EBit.SixtyFour:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/redist/x64/Microsoft.VC110.CRT/msvcr110.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
                    break;

                default:
                    throw new Bam.Core.Exception($"Unrecognized bit depth, {depth}");
            }
            return dynamicLibPaths;
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CxxRuntimePaths(
            C.EBit depth)
        {
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/redist/x86/Microsoft.VC110.CRT/msvcp110.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
                    break;

                case C.EBit.SixtyFour:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/redist/x64/Microsoft.VC110.CRT/msvcp110.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
                    break;

                default:
                    throw new Bam.Core.Exception($"Unrecognized bit depth, {depth}");
            }
            return dynamicLibPaths;
        }
    }
}
