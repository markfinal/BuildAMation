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
namespace Bam.Core
{
    /// <summary>
    /// Utility class wrapping NuGet package information.
    /// Use this to query information regarding the specified NuGet name and version.
    /// </summary>
    public class NuGetInfo
    {
        private static readonly NuGet.Repositories.NuGetv3LocalRepository repo =
            new NuGet.Repositories.NuGetv3LocalRepository(System.IO.Path.Combine(NuGet.Common.NuGetEnvironment.GetFolderPath(NuGet.Common.NuGetFolderPath.NuGetHome), "packages"));

        private readonly NuGet.Repositories.LocalPackageInfo nuget;

        /// <summary>
        /// Collect information for the specified NuGet name and version.
        /// </summary>
        /// <param name="nuGetName">Name of the NuGet to query.</param>
        /// <param name="requiredVersion">Version of th NuGet to query.</param>
        public NuGetInfo(
            string nuGetName,
            string requiredVersion)
        {
            var installs = repo.FindPackagesById(nuGetName);
            if (!installs.Any())
            {
                throw new Bam.Core.Exception($"Unable to locate any NuGet package for {nuGetName}");
            }
            this.nuget = installs.FirstOrDefault(item => item.Version.ToNormalizedString().Equals(requiredVersion, System.StringComparison.Ordinal));
            if (null == this.nuget)
            {
                throw new Exception($"Unable to find NuGet {nuGetName} version {requiredVersion}");
            }
        }

        /// <summary>
        /// Returns the path to the 'tools' subdirectory of the NuGet's package.
        /// </summary>
        public string ToolsDir => System.IO.Path.Combine(this.nuget.ExpandedPath, "tools");
    }
}
