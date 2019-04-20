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
    /// A package repository is a collection of packages and optionally tests.
    /// The structure on disk is
    /// [rootdir]/packages
    /// [rootdir]/tests
    /// Or
    /// [rootdir]
    /// where packages are subdirectories in any of those folders.
    /// </summary>
    public class PackageRepository
    {
        private readonly StringArray candidatePackageDirs;
        private readonly Array<PackageDefinition> packages = new Array<PackageDefinition>();

        /// <summary>
        /// Create a package repository rooted at specified path.
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="requiresSourceDownload"></param>
        public PackageRepository(
            string rootPath,
            bool requiresSourceDownload)
        {
            if (!System.IO.Directory.Exists(rootPath))
            {
                throw new Exception($"Package repository directory '{rootPath}' does not exist");
            }
            this.RootPath = rootPath;

            Log.MessageAll("Adding package repository rooted at '{0}'", rootPath);
            this.candidatePackageDirs = new StringArray();
            var possiblePackages = System.IO.Directory.GetDirectories(rootPath, "*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var packageDir in possiblePackages)
            {
                var possibleBamFolder = System.IO.Path.Combine(packageDir, PackageUtilities.BamSubFolder);
                if (!System.IO.Directory.Exists(possibleBamFolder))
                {
                    continue;
                }
                var packageDefinitionPath = GetPackageDefinitionPathname(possibleBamFolder);
                if (null == packageDefinitionPath)
                {
                    continue;
                }
                Log.MessageAll($"\t{packageDir}");
                this.candidatePackageDirs.Add(packageDir);

                var definitionFile = new PackageDefinition(packageDefinitionPath, requiresSourceDownload);
                definitionFile.Read();
                this.packages.Add(definitionFile);
            }
        }

        /// <summary>
        /// Returns the root path of the repository.
        /// </summary>
        public string RootPath
        {
            get;
            private set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="packageDescription"></param>
        /// <returns></returns>
        public PackageDefinition
        FindPackage(
            System.Tuple<string, string, bool?> packageDescription)
        {
            if (packageDescription.Item2 != null)
            {
                return this.packages.FirstOrDefault(item => item.Name == packageDescription.Item1 && item.Version == packageDescription.Item2);
            }
            else
            {
                return this.packages.FirstOrDefault(item => item.Name == packageDescription.Item1);
            }
        }

        private static string
        GetPackageDefinitionPathname(
            string possibleBamFolder)
        {
            var xmlFiles = System.IO.Directory.GetFiles(possibleBamFolder, "*.xml", System.IO.SearchOption.AllDirectories);
            if (0 == xmlFiles.Length)
            {
                return null;
            }
            if (xmlFiles.Length > 1)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("Too many .xml files found under {0}", possibleBamFolder);
                message.AppendLine();
                foreach (var file in xmlFiles)
                {
                    message.AppendFormat("\t{0}", file);
                    message.AppendLine();
                }
                throw new Exception(message.ToString());
            }
            return xmlFiles[0];
        }
    }
}
