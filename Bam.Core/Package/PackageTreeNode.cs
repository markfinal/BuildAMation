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
    /// Representation of a package within a tree structure.
    /// </summary>
    public class PackageTreeNode
    {
        private Array<PackageTreeNode> InternalParents { get; set; } = new Array<PackageTreeNode>();
        private Array<PackageTreeNode> InternalChildren { get; set; } = new Array<PackageTreeNode>();

        /// <summary>
        /// Convert a PackageTreeNode to a string.
        /// </summary>
        /// <returns>The package definition's full name</returns>
        public override string
        ToString()
        {
            if (null != this.Definition)
            {
                return this.Definition.FullName;
            }
            else
            {
                return $"{this.Name}-{this.Version} (not yet discovered)";
            }
        }

        /// <summary>
        /// Construct a new PackageTreeNode from an existing package definition file.
        /// As the package definition file exists, this means that the package has been
        /// discovered in a repository.
        /// </summary>
        /// <param name="definition">The PackageDefinition to use as source for the PackageTreeNode.</param>
        public PackageTreeNode(
            PackageDefinition definition)
        {
            this.Definition = definition;
            this.Name = definition.Name;
            this.Version = definition.Version;
        }

        /// <summary>
        /// Construct a new PackageTreeNode from a name and version.
        /// This means that the package has not yet been discovered in a repository, so it should be
        /// considered a placeholder for future discovery.
        /// </summary>
        /// <param name="name">Name of the package to find.</param>
        /// <param name="version">Version ot the package to find, or null if there is no version.</param>
        public PackageTreeNode(
            string name,
            string version)
        {
            this.Definition = null;
            this.Name = name;
            this.Version = version;
        }

        /// <summary>
        /// Name of the package represented by this PackageTreeNode.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Version of the package represented by the PackageTreeNode.
        /// </summary>
        public string Version
        {
            get;
            private set;
        }

        /// <summary>
        /// PackageDefinition represented by the PackageTreeNode.
        /// </summary>
        public PackageDefinition Definition
        {
            get;
            private set;
        }

        /// <summary>
        /// Add a child PackageTreeNode to this.
        /// </summary>
        /// <param name="child">PackageTreeNode to add.</param>
        public void
        AddChild(
            PackageTreeNode child)
        {
            this.InternalChildren.Add(child);
            child.InternalParents.Add(this);
        }

        /// <summary>
        /// Remove this PackageTreeNode from all of its parents.
        /// </summary>
        public void
        RemoveFromParents()
        {
            foreach (var parent in this.InternalParents)
            {
                parent.InternalChildren.Remove(this);
            }
            this.InternalParents.Clear();
        }

        /// <summary>
        /// Enumerate the children of this PackageTreeNode.
        /// </summary>
        public System.Collections.Generic.IEnumerable<PackageTreeNode> Children
        {
            get
            {
                foreach (var child in this.InternalChildren)
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Enumerate the parents of this PackageTreeNode.
        /// </summary>
        public System.Collections.Generic.IEnumerable<PackageTreeNode> Parents
        {
            get
            {
                foreach (var parent in this.InternalParents)
                {
                    yield return parent;
                }
            }
        }

        /// <summary>
        /// Enumerate the duplicate package names recursively from this PackageTreeNode.
        /// </summary>
        public System.Collections.Generic.IEnumerable<string> DuplicatePackageNames
        {
            get
            {
                var packageNames = new Array<(string name, string version)>();

                void getPackageNames(
                    Array<PackageTreeNode> parents,
                    PackageTreeNode node)
                {
                    packageNames.AddUnique((node.Name, node.Version));
                    foreach (var child in node.InternalChildren)
                    {
                        // check for cyclic dependencies
                        if (parents.Contains(child))
                        {
                            continue;
                        }
                        parents.Add(node);
                        getPackageNames(parents, child);
                    }
                    if (parents.Any())
                    {
                        parents.Remove(parents.Last());
                    }
                }

                getPackageNames(new Array<PackageTreeNode>(), this);
                return packageNames.GroupBy(item => item.name).Where(item => item.Count() > 1).Select(item => item.Key);
            }
        }

        /// <summary>
        /// Enumerate all the PackageTreeNodes for all packages with the given name.
        /// </summary>
        /// <param name="packageName">Name of the package to search for duplicates.</param>
        /// <returns>Enumeration of duplicate packages with the given name.</returns>
        public System.Collections.Generic.IEnumerable<PackageTreeNode>
        DuplicatePackages(
            string packageName)
        {
            var packages = new Array<PackageTreeNode>();

            void getPackageByName(
                Array<PackageTreeNode> parents,
                PackageTreeNode node)
            {
                if (node.Name.Equals(packageName, System.StringComparison.Ordinal))
                {
                    packages.AddUnique(node);
                }
                foreach (var child in node.InternalChildren)
                {
                    // check for cyclic dependencies
                    if (parents.Contains(child))
                    {
                        continue;
                    }
                    parents.Add(node);
                    getPackageByName(parents, child);
                }
                if (parents.Any())
                {
                    parents.Remove(parents.Last());
                }
            }

            getPackageByName(new Array<PackageTreeNode>(), this);
            return packages;
        }

        /// <summary>
        /// Enumerate all packages that have not been discovered in repositories.
        /// </summary>
        public System.Collections.Generic.IEnumerable<PackageTreeNode> UndiscoveredPackages
        {
            get
            {
                var packages = new Array<PackageTreeNode>();

                void getPackageWithNoDefinition(
                    Array<PackageTreeNode> parents,
                    PackageTreeNode node)
                {
                    if (null == node.Definition)
                    {
                        packages.AddUnique(node);
                    }
                    foreach (var child in node.InternalChildren)
                    {
                        // check for cyclic dependencies
                        if (parents.Contains(child))
                        {
                            continue;
                        }
                        parents.Add(node);
                        getPackageWithNoDefinition(parents, child);
                    }
                    if (parents.Any())
                    {
                        parents.Remove(parents.Last());
                    }
                }

                getPackageWithNoDefinition(new Array<PackageTreeNode>(), this);
                return packages;
            }
        }

        /// <summary>
        /// Enumerate all package repository paths recursively.
        /// </summary>
        public System.Collections.Generic.IEnumerable<string> PackageRepositoryPaths
        {
            get
            {
                var repositoryPaths = new StringArray();

                void getRepositoryPaths(
                    Array<PackageTreeNode> parents,
                    PackageTreeNode node)
                {
                    if (null != node.Definition)
                    {
                        // add relative repository directories
                        var packageDir = node.Definition.GetPackageDirectory();
                        foreach (var repo in node.Definition.PackageRepositories)
                        {
                            // faking TokenizedString replacement
                            var repoPath = repo.Replace("$(packagedir)", packageDir);
                            repoPath = System.IO.Path.GetFullPath(repoPath);
                            if (!System.IO.Directory.Exists(repoPath))
                            {
                                Log.Info($"Repository path '{repo}', expanding to '{repoPath}', from package '{node.Definition.Name}' does not exist");
                                continue;
                            }
                            repositoryPaths.AddUnique(repoPath);
                        }

                        // add repository directories found via a search path
                        var config = UserConfiguration.Configuration;
                        var searchDirs = $"{config[UserConfiguration.RepositorySearchDirs]}";
                        var splitSearchDirs = searchDirs.Split(System.IO.Path.PathSeparator).Where(
                            item => !System.String.IsNullOrEmpty(item) && System.IO.Directory.Exists(item)
                        );
                        foreach (var namedRepo in node.Definition.NamedPackageRepositories)
                        {
                            var foundNamedRepo = false;
                            foreach (var searchDir in splitSearchDirs)
                            {
                                var proposedDir = System.IO.Path.Combine(searchDir, namedRepo);
                                if (!System.IO.Directory.Exists(proposedDir))
                                {
                                    continue;
                                }
                                repositoryPaths.AddUnique(proposedDir);
                                foundNamedRepo = true;
                                break;
                            }
                            if (!foundNamedRepo)
                            {
                                Log.Info(
                                    $"Unable to locate named package repository '{namedRepo}', required by '{node.Definition.FullName}', after looking in {splitSearchDirs.Count()} search directories:"
                                );
                                foreach (var searchDir in splitSearchDirs)
                                {
                                    Log.Info($"\t'{searchDir}'");
                                }
                            }
                        }
                    }
                    foreach (var child in node.InternalChildren)
                    {
                        // check for cyclic dependencies
                        if (parents.Contains(child))
                        {
                            continue;
                        }
                        parents.Add(node);
                        getRepositoryPaths(parents, child);
                    }
                    if (parents.Any())
                    {
                        parents.Remove(parents.Last());
                    }
                }

                getRepositoryPaths(new Array<PackageTreeNode>(), this);
                return repositoryPaths;
            }
        }

        /// <summary>
        /// Enumerate all unique package definitions recursively.
        /// </summary>
        public System.Collections.Generic.IEnumerable<PackageDefinition> UniquePackageDefinitions
        {
            get
            {
                var definitions = new Array<PackageDefinition>();

                void getUniquePackageDefinitions(
                    Array<PackageTreeNode> parents,
                    PackageTreeNode node)
                {
                    if (null != node.Definition)
                    {
                        definitions.AddUnique(node.Definition);
                    }
                    foreach (var child in node.InternalChildren)
                    {
                        // check for cyclic dependencies
                        if (parents.Contains(child))
                        {
                            continue;
                        }
                        parents.Add(node);
                        getUniquePackageDefinitions(parents, child);
                    }
                    if (parents.Any())
                    {
                        parents.Remove(parents.Last());
                    }
                }

                getUniquePackageDefinitions(new Array<PackageTreeNode>(), this);
                return definitions;
            }
        }
    }
}
