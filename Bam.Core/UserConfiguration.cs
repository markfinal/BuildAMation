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
using Microsoft.Extensions.Configuration;
namespace Bam.Core
{
    /// <summary>
    /// Users may configure BuildAMation operations by several methods. These take this order of precendece:
    ///  - Environment variables prefixed with 'BAM'. The configuration names use colons, :, as scoping, which are invalid
    ///  for environment variables, so a double underscore __ may be safely used instead, e.g.
    ///  configuration = Packages:SourceDir aka environment variable = BAMPackages__SourceDir
    ///  - An .ini file in your home directory called buildamation.ini
    ///  - Defaults
    ///
    /// Configuration variables defined:
    ///  Packages:SourceDir
    /// </summary>
    public static class UserConfiguration
    {
        /// <summary>
        /// Configuration option
        /// This is the directory in which package sources are downloaded to. (default = $HOME/.bam.package.sources)
        /// </summary>
        public const string SourcesDir = "Packages:SourceDir";

        private static Microsoft.Extensions.Configuration.IConfiguration InternalConfiguration;

        static UserConfiguration()
        {
            InternalConfiguration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile))
                .AddInMemoryCollection(
                    new System.Collections.Generic.Dictionary<string, string>
                    {
                        { SourcesDir, System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".bam.package.sources") }
                    }
                )
                .AddIniFile("buildamation.ini", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("BAM")
                .Build();
        }

        /// <summary>
        /// Get the configuration interface to query.
        /// </summary>
        public static Microsoft.Extensions.Configuration.IConfiguration Configuration
        {
            get
            {
                return InternalConfiguration;
            }
        }
    }
}
