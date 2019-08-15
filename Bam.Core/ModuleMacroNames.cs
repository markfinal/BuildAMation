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
namespace Bam.Core
{
    /// <summary>
    /// Common macro names used for Modules.
    /// </summary>
    public static class ModuleMacroNames
    {
        /// <summary>
        /// Directory of the Module's package (i.e. that containing the 'bam' folder).
        /// </summary>
        public const string BamPackageDirectory = "bampackagedir";

        /// <summary>
        /// Name of the Module's package.
        /// </summary>
        public const string PackageName = "packagename";

        /// <summary>
        /// Root directory that package build files are written to.
        /// </summary>
        public const string PackageBuildDirectory = "packagebuilddir";

        /// <summary>
        /// Directory of the Module's package.
        /// This may be the same as \p BamPackageDirectory, unless the package has
        /// downloadable or redirected source, and then it is as specified.
        /// </summary>
        public const string PackageDirectory = "packagedir";

        /// <summary>
        /// The name of this Module
        /// </summary>
        public const string ModuleName = "modulename";

        /// <summary>
        /// The basename used for any file written for this Module.
        /// </summary>
        public const string OutputName = "OutputName";

        /// <summary>
        /// The name of the configuration for this Module instance.
        /// </summary>
        public const string ConfigurationName = "config";

        /// <summary>
        /// Directory that this Module's output files are written to.
        /// </summary>
        public const string ModuleOutputDirectory = "moduleoutputdir";
    }
}
