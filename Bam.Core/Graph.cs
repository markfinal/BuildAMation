#region License
// Copyright (c) 2010-2015, Mark Final
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
    /// Singleton representing the single point of reference for all build functionality
    /// </summary>
    public sealed class Graph :
        System.Collections.Generic.IEnumerable<ModuleCollection>
    {
        static Graph()
        {
            Instance = new Graph();
        }

        public static Graph Instance
        {
            get;
            private set;
        }

        private Graph()
        {
            this.Modules = new System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>>();
            this.ReferencedModules = new System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>>();
            this.TopLevelModules = new System.Collections.Generic.List<Module>();
            this.Macros = new MacroList();
            if (null != State.BuildMode)
            {
                this.Macros.AddVerbatim("buildroot", State.BuildRoot);
            }
            this.BuildEnvironmentInternal = null;
            this.CommonModuleType = new System.Collections.Generic.Stack<System.Type>();
            this.DependencyGraph = new DependencyGraph();
            this.Mode = null;
            this.MetaData = null;
        }

        public void
        AddModule(
            Module m)
        {
            this.Modules[this.BuildEnvironmentInternal].Add(m);
        }

        public System.Collections.Generic.Stack<System.Type> CommonModuleType
        {
            get;
            private set;
        }

        public T
        FindReferencedModule<T>() where T : Module, new()
        {
            var referencedModules = this.ReferencedModules[this.BuildEnvironmentInternal];
            var matches = referencedModules.Where(item => item.GetType() == typeof(T));
            var matchedModule = matches.FirstOrDefault();
            if (null != matchedModule)
            {
                return matchedModule as T;
            }
            this.CommonModuleType.Push(typeof(T));
            var newModule = Module.Create<T>(preInitCallback: module =>
                {
                    if (null != module)
                    {
                        referencedModules.Add(module);
                    }
                });
            this.CommonModuleType.Pop();
            return newModule;
        }

        private Module
        MakeModuleOfType(
            System.Type moduleType)
        {
            try
            {
                var findReferencedModuleMethod = typeof(Graph).GetMethod("FindReferencedModule");
                var genericVersionForModuleType = findReferencedModuleMethod.MakeGenericMethod(moduleType);
                var newModule = genericVersionForModuleType.Invoke(Graph.Instance, null) as Module;
                return newModule;
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                var exModuleType = (ex.InnerException is ModuleCreationException) ? (ex.InnerException as ModuleCreationException).ModuleType : moduleType;
                var inner = ex.InnerException;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                throw new Exception(inner, "Failed to create module of type {0}", exModuleType.ToString());
            }
        }

        public ModuleType
        MakeModuleOfType<ModuleType>(
            System.Type moduleType) where ModuleType : Module
        {
            return this.MakeModuleOfType(moduleType) as ModuleType;
        }

        public void
        CreateTopLevelModules(
            System.Reflection.Assembly assembly,
            Environment env,
            string ns)
        {
            this.BuildEnvironment = env;
            var includeTests = CommandLineProcessor.Evaluate(new UseTests());
            var allTypes = assembly.GetTypes();
            var allPackageTypes = allTypes.Where(type => ((type.Namespace == ns) || (includeTests && (type.Namespace == ns + ".tests"))) && type.IsSubclassOf(typeof(Module)) && type.IsSealed);
            foreach (var moduleType in allPackageTypes)
            {
                var newModule = MakeModuleOfType(moduleType);
                if (newModule != null)
                {
                    this.TopLevelModules.Add(newModule);
                }
            }
            this.BuildEnvironment = null;
            if (0 == this.TopLevelModules.Count)
            {
                throw new Exception("No modules found in the namespace '{0}'", ns);
            }
            // remove all top level modules that have a reference count > 1
            foreach (var tlm in this.TopLevelModules.Reverse<Module>())
            {
                if (!tlm.TopLevel)
                {
                    this.TopLevelModules.Remove(tlm);
                }
            }
        }

        public void
        ApplySettingsPatches()
        {
            Log.Detail("Applying settings patches");
            foreach (var rank in this.DependencyGraph.Reverse())
            {
                foreach (var module in rank.Value)
                {
                    module.ApplySettingsPatches();
                }
            }
        }

        private System.Collections.Generic.List<Module> TopLevelModules
        {
            get;
            set;
        }

        private System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>> Modules
        {
            get;
            set;
        }

        private System.Collections.Generic.Dictionary<Environment, System.Collections.Generic.List<Module>> ReferencedModules
        {
            get;
            set;
        }

        public string Mode
        {
            get;
            set;
        }

        public MacroList Macros
        {
            get;
            private set;
        }

        public object MetaData
        {
            get;
            set;
        }

        private Environment BuildEnvironmentInternal = null;
        public Environment BuildEnvironment
        {
            get
            {
                return this.BuildEnvironmentInternal;
            }

            private set
            {
                this.BuildEnvironmentInternal = value;
                if (null != value)
                {
                    this.Modules.Add(value, new System.Collections.Generic.List<Module>());
                    this.ReferencedModules.Add(value, new System.Collections.Generic.List<Module>());
                }
            }
        }

        public DependencyGraph DependencyGraph
        {
            get;
            private set;
        }

        private void ApplyGroupDependenciesToChildren(
            System.Collections.ObjectModel.ReadOnlyCollection<Module> children,
            System.Collections.Generic.IEnumerable<Module> dependencies)
        {
            var nonChildDependents = dependencies.Where(item => !(item is IChildModule));
            foreach (var c in children)
            {
                c.DependsOn(nonChildDependents);
            }
        }

        private void ApplyGroupRequirementsToChildren(
            System.Collections.ObjectModel.ReadOnlyCollection<Module> children,
            System.Collections.Generic.IEnumerable<Module> dependencies)
        {
            var nonChildDependents = dependencies.Where(item => !(item is IChildModule));
            foreach (var c in children)
            {
                c.Requires(nonChildDependents);
            }
        }

        private void
        InternalArrangeDependents(
            Module m,
            int rank)
        {
            // predicate required, because eventually there will be a module without a Tool, e.g. a Tool itself
            if (m.Tool != null)
            {
                m.Requires(m.Tool);
                var child = m as IChildModule;
                if ((null == child) || (null == child.Parent))
                {
                    // children inherit the settings from their parents
                    m.UsePublicPatches(m.Tool);
                }
                m.Settings = (m.Tool as ITool).CreateDefaultSettings(m);
            }
            if ((0 == m.Dependents.Count) && (0 == m.Requirements.Count))
            {
                return;
            }
            if (m is IModuleGroup)
            {
                var children = m.Children;
                this.ApplyGroupDependenciesToChildren(children, m.Dependents);
                this.ApplyGroupRequirementsToChildren(children, m.Requirements);
            }
            var nextRank = rank + 1;
            var currentRank = this.DependencyGraph[nextRank];
            foreach (var c in m.Dependents)
            {
                currentRank.Add(c);
                this.InternalArrangeDependents(c, nextRank);
            }
            foreach (var c in m.Requirements)
            {
                currentRank.Add(c);
                this.InternalArrangeDependents(c, nextRank);
            }
        }

        public void
        SortDependencies()
        {
            Log.Detail("Constructing dependency graph");
            var currentRank = this.DependencyGraph[0];
            foreach (var m in this.TopLevelModules)
            {
                currentRank.Add(m);
                this.InternalArrangeDependents(m, 0);
            }
            Module.CompleteModules();
        }

        public void
        Dump()
        {
            foreach (var rank in this.DependencyGraph)
            {
                var text = new System.Text.StringBuilder();
                text.AppendFormat("{2}Rank {0}: {1} modules{2}", rank.Key, rank.Value.Count(), System.Environment.NewLine);
                text.AppendLine(new string('-', 80));
                foreach (var m in rank.Value)
                {
                    text.AppendLine(m.ToString());
                    if (m is IInputPath)
                    {
                        text.AppendFormat("\tInput: {0}{1}", (m as IInputPath).InputPath, System.Environment.NewLine);
                    }
                    foreach (var s in m.GeneratedPaths)
                    {
                        text.AppendFormat("\t{0} : {1}{2}", s.Key, s.Value, System.Environment.NewLine);
                    }
                }
                Log.DebugMessage(text.ToString());
            }
        }

        private void
        InternalValidateGraph(
            int parentRank,
            System.Collections.ObjectModel.ReadOnlyCollection<Module> modules)
        {
            foreach (var c in modules)
            {
                var childCollection = c.OwningRank;
                if (null == childCollection)
                {
                    throw new Exception("Dependency has no rank");
                }
                var found = this.DependencyGraph.Where(item => item.Value == childCollection);
                if (0 == found.Count())
                {
                    throw new Exception("Module collection not found in graph");
                }
                if (found.Count() > 1)
                {
                    throw new Exception("Module collection found more than once in graph");
                }
                var childRank = found.First().Key;
                if (childRank <= parentRank)
                {
                    throw new Exception("Dependent module {0} found at a lower rank than the dependee", c);
                }
            }
        }

        public void
        Validate()
        {
            foreach (var rank in this.DependencyGraph)
            {
                foreach (var m in rank.Value)
                {
                    this.InternalValidateGraph(rank.Key, m.Dependents);
                    this.InternalValidateGraph(rank.Key, m.Requirements);
                }
            }
        }

        public System.Collections.Generic.IEnumerator<ModuleCollection>
        GetEnumerator()
        {
            foreach (var rank in this.DependencyGraph)
            {
                yield return rank.Value;
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool
        IsReferencedModule(
            Module module)
        {
            return this.ReferencedModules[module.BuildEnvironment].Contains(module);
        }

        public System.Collections.Generic.List<Environment> BuildEnvironments
        {
            get
            {
                return this.Modules.Keys.ToList();
            }
        }

        private Array<PackageDefinition> PackageDefinitions
        {
            get;
            set;
        }

        public PackageDefinition MasterPackage
        {
            get
            {
                return this.PackageDefinitions[0];
            }
        }

        public void
        SetPackageDefinitions(
            Array<PackageDefinition> packages)
        {
            this.PackageDefinitions = packages;
            this.Macros.AddVerbatim("masterpackagename", this.MasterPackage.Name);
        }

        public System.Collections.Generic.IEnumerable<PackageDefinition> Packages
        {
            get
            {
                foreach (var package in this.PackageDefinitions)
                {
                    yield return package;
                }
            }
        }

        public IBuildModeMetaData
        BuildModeMetaData
        {
            get;
            set;
        }
    }
}
