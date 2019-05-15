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
    internal class PackageTreeNode
    {
        private Array<PackageTreeNode> InternalParents { get; set; } = new Array<PackageTreeNode>();
        private Array<PackageTreeNode> InternalChildren { get; set; } = new Array<PackageTreeNode>();

        public override string
        ToString()
        {
            return this.Definition.FullName;
        }

        public PackageTreeNode(
            PackageDefinition definition)
        {
            this.Definition = definition;
            this.Name = definition.Name;
            this.Version = definition.Version;
        }

        public PackageTreeNode(
            string name,
            string version)
        {
            this.Definition = null;
            this.Name = name;
            this.Version = version;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public PackageDefinition Definition
        {
            get;
            private set;
        }

        public void
        AddChild(
            PackageTreeNode child)
        {
            this.InternalChildren.Add(child);
            child.InternalParents.Add(this);
        }

        public void
        RemoveFromParents()
        {
            foreach (var parent in this.InternalParents)
            {
                parent.InternalChildren.Remove(this);
            }
            this.InternalParents.Clear();
        }

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

        public System.Collections.Generic.IEnumerable<PackageTreeNode> UnresolvedPackages
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
                        repositoryPaths.AddRangeUnique(node.Definition.PackageRepositories);
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

        public System.Collections.Generic.IEnumerable<PackageDefinition> UniquePackageDefinitions
        {
            get
            {
                var definitions = new Array<PackageDefinition>();

                void getUniquePackageDefinitions(
                    Array<PackageTreeNode> parents,
                    PackageTreeNode node)
                {
                    definitions.AddUnique(node.Definition);
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
