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
        private readonly Array<PackageDefinition> packages = new Array<PackageDefinition>();

        /// <summary>
        /// Convert the PackageRepository to a string.
        /// </summary>
        /// <returns>Root path of the package repository.</returns>
        public override string ToString()
        {
            if (this.IsStructured)
            {
                if (this.HasTests)
                {
                    return $"{this.RootPath}\t(Structured+Tests)";
                }
                else
                {
                    return $"{this.RootPath}\t(Structured)";
                }
            }
            else
            {
                return this.RootPath;
            }
        }

        private System.Collections.Generic.IEnumerable<string>
        ScanForPackages(
            string dirPath)
        {
            if (this.IsStructured)
            {
                var packagesDir = System.IO.Path.Combine(dirPath, "packages");
                foreach (var path in System.IO.Directory.GetDirectories(packagesDir, "*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    yield return path;
                }
                if (this.HasTests)
                {
                    var testsDir = System.IO.Path.Combine(dirPath, "tests");
                    foreach (var path in System.IO.Directory.GetDirectories(testsDir, "*", System.IO.SearchOption.TopDirectoryOnly))
                    {
                        yield return path;
                    }
                }
            }
            else
            {
                foreach (var path in System.IO.Directory.GetDirectories(dirPath, "*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    yield return path;
                }
            }
        }

        /// <summary>
        /// Create a package repository rooted at specified path.
        /// Optionally insert some definition files first.
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="insertedDefinitionFiles"></param>
        public PackageRepository(
            string rootPath,
            params PackageDefinition[] insertedDefinitionFiles)
        {
            if (!System.IO.Directory.Exists(rootPath))
            {
                throw new Exception($"Package repository directory '{rootPath}' does not exist");
            }
            Log.DebugMessage("Adding package repository rooted at '{0}'", rootPath);
            this.RootPath = rootPath;
            if (System.IO.Directory.Exists(System.IO.Path.Combine(rootPath, "packages")))
            {
                this.IsStructured = true;
                this.HasTests = System.IO.Directory.Exists(System.IO.Path.Combine(rootPath, "tests"));
            }
            else
            {
                this.IsStructured = false;
            }

            foreach (var defn in insertedDefinitionFiles)
            {
                this.packages.Add(defn);
            }

            var candidatePackageDirs = this.ScanForPackages(rootPath);
            foreach (var packageDir in candidatePackageDirs)
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
                Log.DebugMessage($"\t{packageDir}");

                if (this.packages.Any(item => item.XMLFilename == packageDefinitionPath))
                {
                    continue;
                }

                var definitionFile = new PackageDefinition(packageDefinitionPath, this);
                definitionFile.Read();
                this.packages.Add(definitionFile);
            }
        }

        /// <summary>
        /// Create a package repository rooted at specified path.
        /// </summary>
        /// <param name="rootPath"></param>
        public PackageRepository(
            string rootPath)
            :
            this(rootPath, System.Linq.Enumerable.Empty<PackageDefinition>().ToArray())
        { }

        /// <summary>
        /// Returns the root path of the repository.
        /// </summary>
        public string RootPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Does the repository have a structured layout, i.e. (root)/packages and optionally (root)/tests.
        /// </summary>
        public bool IsStructured
        {
            get;
            private set;
        }

        /// <summary>
        /// Does a structured repository have a tests directory?
        /// </summary>
        public bool HasTests
        {
            get;
            private set;
        }

        /// <summary>
        /// Find the package by its description in this repository.
        /// </summary>
        /// <param name="packageDescription">Description of the package to find.</param>
        /// <returns>Returns the PackageDefinition if found, or null if not.</returns>
        public PackageDefinition
        FindPackage(
            (string name, string version) packageDescription)
        {
            if (packageDescription.version != null)
            {
                return this.packages.FirstOrDefault(item => item.Name == packageDescription.name && item.Version == packageDescription.version);
            }
            else
            {
                return this.packages.FirstOrDefault(item => item.Name == packageDescription.name);
            }
        }

        /// <summary>
        /// Find the package given a definition file path.
        /// </summary>
        /// <param name="definitionFilePath">Path o the definition file path.</param>
        /// <returns>Package associated with the definition file, or null if not found.</returns>
        public PackageDefinition
        FindPackage(
            string definitionFilePath)
        {
            return this.packages.FirstOrDefault(item => item.XMLFilename == definitionFilePath);
        }

        private static string
        GetPackageDefinitionPathname(
            string possibleBamFolder)
        {
            // <package>/bam/<packagedefinitionfile>.xml
            var xmlFiles = System.IO.Directory.GetFiles(possibleBamFolder, "*.xml", System.IO.SearchOption.TopDirectoryOnly);
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
