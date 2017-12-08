#region License
// Copyright (c) 2010-2017, Mark Final
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
using Bam.Core;
using System.Linq;
namespace Publisher
{
    /// <summary>
    /// Derive from this module to collate files and directories into a runnable distribution.
    /// Collation occurs within a folder in the build root called the 'publishing root'.
    /// An initial file is collated into the publishing root, and this file determines the structure of the
    /// subsequent files and folders, and the application type. This is the reference file.
    /// Add subsequent files and folders, specifying paths relative to the reference file. For example, to
    /// place a dynamic library in a plugins subfolder next to the main executable, specify a subdirectory of
    /// 'plugins'. To place a framework in the Contents/Frameworks sub-folder of an application bundle, specify
    /// a subdirectory of '../Frameworks', as the executable is in Contents/MacOS.
    /// </summary>
#if D_NEW_PUBLISHING
    public abstract class Collation :
        Bam.Core.Module
    {
        static Collation()
        {
            Bam.Core.TokenizedString.registerPostUnaryFunction("readlink", argument =>
                {
#if __MonoCS__
                    var symlink = new Mono.Unix.UnixSymbolicLinkInfo(argument);
                    return symlink.ContentsPath;
#else
                    throw new System.NotSupportedException("Unable to get symbolic link target on Windows");
#endif
                });
        }

        private ICollationPolicy Policy = null;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.ModuleTypePublishDirectory = this.DefaultModuleTypePublishDirectory;

            // TODO: if this is used as a position argument, it might end up in an infinite recursion during string parsing
            this.PublishRoot = Bam.Core.TokenizedString.Create("$(publishdir)", null);
        }

        public sealed override void
        Evaluate()
        {
            // TODO
        }

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.Collate(this, context);
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "Collation";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<ICollationPolicy>.Create(className);
                    }
                    break;
            }
        }

        /// <summary>
        /// The type of application being published.
        /// </summary>
        public enum EPublishingType
        {
            /// <summary>
            /// Application in a console application.
            /// </summary>
            ConsoleApplication,

            /// <summary>
            /// Application is a GUI application.
            /// On OSX, this is an application bundle, and will automatically appear in a <name>.app/Contents/MacOS folder
            /// under the publishing root.
            /// </summary>
            WindowedApplication,

            /// <summary>
            /// Distributing a library, with headers, import/static/dynamic libraries, and potentially tools or tests.
            /// </summary>
            Library

            // TODO: macosFramework
        }

        // this is doubling up the cost of the this.Requires list, but at less runtime cost
        // for expanding each CollatedObject2 to peek as it's properties
        private System.Collections.Generic.Dictionary<System.Tuple<Bam.Core.Module, Bam.Core.PathKey>, CollatedObject> collatedObjects = new System.Collections.Generic.Dictionary<System.Tuple<Module, PathKey>, CollatedObject>();

        private Bam.Core.TokenizedString PublishRoot
        {
            get;
            set;
        }

        public Bam.Core.TokenizedString ExecutableDir
        {
            get
            {
                return this.Macros["ExecutableDir"];
            }

            set
            {
                this.Macros["ExecutableDir"] = value;
            }
        }

        public Bam.Core.TokenizedString DynamicLibraryDir
        {
            get
            {
                return this.Macros["DynamicLibraryDir"];
            }

            set
            {
                this.Macros["DynamicLibraryDir"] = value;
            }
        }

        public Bam.Core.TokenizedString StaticLibraryDir
        {
            get
            {
                return this.Macros["StaticLibraryDir"];
            }

            set
            {
                this.Macros["StaticLibraryDir"] = value;
            }
        }

        public Bam.Core.TokenizedString ImportLibraryDir
        {
            get
            {
                return this.Macros["ImportLibraryDir"];
            }

            set
            {
                this.Macros["ImportLibraryDir"] = value;
            }
        }

        public Bam.Core.TokenizedString PluginDir
        {
            get
            {
                return this.Macros["PluginDir"];
            }

            set
            {
                this.Macros["PluginDir"] = value;
            }
        }

        public Bam.Core.TokenizedString HeaderDir
        {
            get
            {
                return this.Macros["HeaderDir"];
            }

            set
            {
                this.Macros["HeaderDir"] = value;
            }
        }

        private void
        SetConsoleApplicationDefaultMacros()
        {
            this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            if (Bam.Core.OSUtilities.CurrentOS == Bam.Core.EPlatform.Windows)
            {
                this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
            }
            this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
        }

        private void
        setWindowedApplicationDefaultMacros()
        {
            switch (Bam.Core.OSUtilities.CurrentOS)
            {
                case Bam.Core.EPlatform.Windows:
                    {
                        this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)/plugins", new[] { this.PublishRoot }));
                    }
                    break;

                case Bam.Core.EPlatform.Linux:
                    {
                        this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        //this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(0)", new[] { this.PublishRoot }));
                        this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)/plugins", new[] { this.PublishRoot }));
                    }
                    break;

                case Bam.Core.EPlatform.OSX:
                    {
                        this.Macros.Add("macOSAppBundleContentsDir", this.CreateTokenizedString("$(0)/$(OutputName).app/Contents", new[] { this.PublishRoot }));
                        this.Macros.Add("macOSAppBundleMacOSDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/MacOS"));
                        this.Macros.Add("macOSAppBundleFrameworksDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Frameworks"));
                        this.Macros.Add("macOSAppBundlePluginsDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Plugins"));
                        this.Macros.Add("macOSAppBundleResourcesDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Resources"));
                        this.Macros.Add("macOSAppBundleSharedSupportDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/SharedSupport"));
                        this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(macOSAppBundleMacOSDir)"));
                        this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(macOSAppBundleFrameworksDir)"));
                        this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(macOSAppBundleFrameworksDir)"));
                        //this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(macOSAppBundleFrameworksDir)"));
                        this.Macros.Add("PluginDir", this.CreateTokenizedString("$(macOSAppBundlePluginsDir)"));
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported OS: '{0}'", Bam.Core.OSUtilities.CurrentOS);
            }
        }

        private void
        setLibraryDefaultMacros()
        {
            this.Macros.Add("ExecutableDir", this.CreateTokenizedString("$(0)/bin", new[] { this.PublishRoot }));
            this.Macros.Add("DynamicLibraryDir", this.CreateTokenizedString("$(0)/bin", new[] { this.PublishRoot }));
            this.Macros.Add("StaticLibraryDir", this.CreateTokenizedString("$(0)/lib", new[] { this.PublishRoot }));
            if (Bam.Core.OSUtilities.CurrentOS == Bam.Core.EPlatform.Windows)
            {
                this.Macros.Add("ImportLibraryDir", this.CreateTokenizedString("$(0)/lib", new[] { this.PublishRoot }));
            }
            this.Macros.Add("HeaderDir", this.CreateTokenizedString("$(0)/include", new[] { this.PublishRoot }));
            this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)/plugins", new[] { this.PublishRoot }));
        }

        public void
        SetDefaultMacros(
            EPublishingType type)
        {
            // TODO: can any of these paths be determined from the C package for RPATHs etc?
            // i.e. whatever layout the user wants, is honoured here as a default
            switch (type)
            {
                case EPublishingType.ConsoleApplication:
                    this.SetConsoleApplicationDefaultMacros();
                    break;

                case EPublishingType.WindowedApplication:
                    this.setWindowedApplicationDefaultMacros();
                    break;

                case EPublishingType.Library:
                    this.setLibraryDefaultMacros();
                    break;
            }
        }

        public void
        IncludeAllModulesInNamespace(
            string nameSpace,
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString publishDir)
        {
            var gen = this.GetType().GetMethod("Include", new[] { typeof(Bam.Core.PathKey), typeof(Bam.Core.TokenizedString) });
            var moduleTypes = global::System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(item =>
                item.Namespace == nameSpace && item.IsSubclassOf(typeof(Bam.Core.Module)) && item != this.GetType());
            foreach (var type in moduleTypes)
            {
                var meth = gen.MakeGenericMethod(new[] { type });
                meth.Invoke(this, new object[] { key, publishDir });
            }
        }

        public ICollatedObject
        Find<DependentModule>() where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            foreach (var dep in this.Requirements)
            {
                var obj = dep as ICollatedObject;
                if (obj.SourceModule == dependent)
                {
                    return obj;
                }
            }
            return null;
        }

        private void
        gatherAllDependencies(
            Bam.Core.Module initialModule,
            Bam.Core.PathKey key,
            ICollatedObject anchor,
            Bam.Core.TokenizedString anchorPublishRoot)
        {
            var allDependents = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();
            var toDealWith = new System.Collections.Generic.Queue<System.Tuple<Bam.Core.Module, Bam.Core.PathKey>>();
            toDealWith.Enqueue(System.Tuple.Create(initialModule, key));
            System.Func<Bam.Core.Module, bool> findPublishableDependents;
            findPublishableDependents = module =>
            {
                var any = false;
                // now look at all the dependencies and accumulate a list of child dependencies
                // TODO: need a configurable list of types, not just C.DynamicLibrary, that the user can specify to find
                foreach (var dep in module.Dependents)
                {
                    if (dep is C.Plugin)
                    {
                        if (!allDependents.ContainsKey(dep))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(dep, C.Plugin.Key));
                            any = true;
                        }
                    }
                    if (dep is C.Cxx.Plugin)
                    {
                        if (!allDependents.ContainsKey(dep))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(dep, C.Cxx.Plugin.Key));
                            any = true;
                        }
                    }
                    if (dep is C.DynamicLibrary)
                    {
                        if (!allDependents.ContainsKey(dep))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(dep, C.DynamicLibrary.Key));
                            any = true;
                        }
                    }
                    if (dep is C.Cxx.DynamicLibrary)
                    {
                        if (!allDependents.ContainsKey(dep))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(dep, C.Cxx.DynamicLibrary.Key));
                            any = true;
                        }
                    }
                    // TODO: distinguish between GUIApplication and ConsoleApplication?
                    if (dep is C.ConsoleApplication)
                    {
                        if (!allDependents.ContainsKey(dep))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(dep, C.ConsoleApplication.Key));
                            any = true;
                        }
                    }
                    if (dep is C.Cxx.ConsoleApplication)
                    {
                        if (!allDependents.ContainsKey(dep))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(dep, C.Cxx.ConsoleApplication.Key));
                            any = true;
                        }
                    }
                }
                foreach (var req in module.Requirements)
                {
                    if (req is C.Plugin)
                    {
                        if (!allDependents.ContainsKey(req))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(req, C.Plugin.Key));
                            any = true;
                        }
                    }
                    if (req is C.Cxx.Plugin)
                    {
                        if (!allDependents.ContainsKey(req))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(req, C.Cxx.Plugin.Key));
                            any = true;
                        }
                    }
                    if (req is C.DynamicLibrary)
                    {
                        if (!allDependents.ContainsKey(req))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(req, C.DynamicLibrary.Key));
                            any = true;
                        }
                    }
                    if (req is C.Cxx.DynamicLibrary)
                    {
                        if (!allDependents.ContainsKey(req))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(req, C.Cxx.DynamicLibrary.Key));
                            any = true;
                        }
                    }
                    if (req is C.ConsoleApplication)
                    {
                        if (!allDependents.ContainsKey(req))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(req, C.ConsoleApplication.Key));
                            any = true;
                        }
                    }
                    if (req is C.Cxx.ConsoleApplication)
                    {
                        if (!allDependents.ContainsKey(req))
                        {
                            toDealWith.Enqueue(System.Tuple.Create(req, C.Cxx.ConsoleApplication.Key));
                            any = true;
                        }
                    }
                }
                return any;
            };
            // iterate over each dependent, stepping into each of their dependencies
            while (toDealWith.Count > 0)
            {
                var next = toDealWith.Dequeue();
                findPublishableDependents(next.Item1);
                if (next.Item1 != initialModule)
                {
                    var notPresent = true;
                    notPresent &= !allDependents.ContainsKey(next.Item1);
                    notPresent &= !this.collatedObjects.ContainsKey(next);
                    if (anchor != null)
                    {
                        var anchorAsCollatedObject = anchor as CollatedObject;
                        notPresent &= !anchorAsCollatedObject.DependentCollations.ContainsKey(next);
                    }
                    if (notPresent)
                    {
                        allDependents.Add(next.Item1, next.Item2);
                    }
                }
            }
            // now add each as a publishable dependent
            foreach (var dep in allDependents)
            {
                var modulePublishDir = this.ModuleTypePublishDirectory(dep.Key, dep.Value);
                this.IncludeNoGather(dep.Key, dep.Value, modulePublishDir, anchor, anchorPublishRoot);
            }
        }

        public delegate Bam.Core.TokenizedString ModuleTypePublishDirectoryDelegate(Bam.Core.Module module, Bam.Core.PathKey key);

        public ModuleTypePublishDirectoryDelegate ModuleTypePublishDirectory
        {
            get;
            set;
        }

        private Bam.Core.TokenizedString
        DefaultModuleTypePublishDirectory(
            Bam.Core.Module module,
            Bam.Core.PathKey key)
        {
            if (module is C.Cxx.Plugin || module is C.Plugin)
            {
                return this.PluginDir;
            }
            else if (module is C.DynamicLibrary || module is C.Cxx.DynamicLibrary)
            {
                if (C.DynamicLibrary.Key == key)
                {
                    return this.DynamicLibraryDir;
                }
                else if (C.DynamicLibrary.ImportLibraryKey == key)
                {
                    return this.ImportLibraryDir;
                }
                else
                {
                    throw new System.NotSupportedException(System.String.Format("Dynamic library key {0}", key.ToString()));
                }
            }
            else if (module is C.ConsoleApplication || module is C.Cxx.ConsoleApplication)
            {
                // TODO: different to GUIApplication?
                return this.ExecutableDir;
            }
            else if (module is C.StaticLibrary)
            {
                return this.StaticLibraryDir;
            }
            else
            {
                throw new System.NotSupportedException(System.String.Format("Module of type {0}", module.GetType().ToString()));
            }
        }

        private ICollatedObject
        IncludeNoGather(
            Bam.Core.Module dependent,
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString modulePublishDir,
            ICollatedObject anchor,
            Bam.Core.TokenizedString anchorPublishRoot)
        {
            var collatedFile = this.CreateCollatedFile(dependent, key, modulePublishDir, anchor, anchorPublishRoot);
            var tuple = System.Tuple.Create(dependent, key);
            if (Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                // a dependency may be copied for each anchor that references it in order
                // to make that anchor fully resolved and debuggable
                if (null != anchor)
                {
                    var anchorAsCollatedObject = anchor as CollatedObject;
                    anchorAsCollatedObject.DependentCollations.Add(tuple, collatedFile);
                }
                else
                {
                    this.collatedObjects.Add(tuple, collatedFile);
                }
            }
            else
            {
                // as everything goes to a single publishdir, remember each instance of a module
                this.collatedObjects.Add(tuple, collatedFile);
            }
            return collatedFile;
        }

        private void
        Include(
            Bam.Core.Module dependent,
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString anchorPublishRoot)
        {
            var modulePublishDir = this.ModuleTypePublishDirectory(dependent, key);
            var collatedFile = this.IncludeNoGather(dependent, key, modulePublishDir, null, anchorPublishRoot);
            this.gatherAllDependencies(dependent, key, collatedFile, anchorPublishRoot);
        }

        public void
        Include<DependentModule>(
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString anchorPublishRoot = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Include(dependent, key, anchorPublishRoot);
        }

        private CollatedFile
        CreateCollatedFile(
            Bam.Core.Module dependent,
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString modulePublishDir,
            ICollatedObject anchor,
            Bam.Core.TokenizedString anchorPublishRoot)
        {
            var module = Bam.Core.Module.Create<CollatedFile>() as CollatedFile;
            if (Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                // the publishdir is different for each anchor, so dependents may be duplicated
                // if referenced by multiple anchors
                if (null != anchor)
                {
                    module.Macros.Add("publishroot", dependent.CreateTokenizedString("@dir($(0))", new[] { anchor.SourceModule.GeneratedPaths[anchor.SourcePathKey] }));
                }
                else
                {
                    module.Macros.Add("publishroot", dependent.CreateTokenizedString("@dir($(0))", new[] { dependent.GeneratedPaths[key] }));
                }
            }
            else
            {
                // publishdir is the same for all anchors, and thus all dependents are unique for all anchors
                module.Macros.Add("publishroot", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
            }

            // for PublishBesideExecutable, a custom anchor publish root won't work, as the debugger won't run any file
            // other than that built, and if copied elsewhere (as would expect from a custom anchor publish root), so
            // will it's dependencies, which won't be found by the debugged executable
            if (null != anchorPublishRoot && !Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                module.Macros.Add("publishdir", module.CreateTokenizedString("$(0)", anchorPublishRoot));
            }
            else
            {
                module.Macros.Add("publishdir", module.CreateTokenizedString("$(publishroot)"));
            }
            module.SetPublishingDirectory("$(0)", new[] { modulePublishDir });
            module.SourceModule = dependent;
            module.SourcePathKey = key;

            this.Requires(module);
            module.Requires(dependent);
            return module;
        }
    }
#else
    public abstract class Collation :
        Bam.Core.Module
    {
        static Collation()
        {
            Bam.Core.TokenizedString.registerPostUnaryFunction("readlink", argument =>
                {
#if __MonoCS__
                    var symlink = new Mono.Unix.UnixSymbolicLinkInfo(argument);
                    return symlink.ContentsPath;
#else
                    throw new System.NotSupportedException("Unable to get symbolic link target on Windows");
#endif
                });
        }

        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Publishing Root");
        private Bam.Core.Array<CollatedFile> CopiedFrameworks = new Bam.Core.Array<CollatedFile>();
        private Bam.Core.Array<ChangeNameOSX> ChangedNamedBinaries = new Bam.Core.Array<ChangeNameOSX>();
        private ICollationPolicy Policy = null;

        /// <summary>
        /// The type of application being published.
        /// </summary>
        public enum EPublishingType
        {
            /// <summary>
            /// Application in a console application.
            /// </summary>
            ConsoleApplication,

            /// <summary>
            /// Application is a GUI application.
            /// On OSX, this is an application bundle, and will automatically appear in a <name>.app/Contents/MacOS folder
            /// under the publishing root.
            /// </summary>
            WindowedApplication,

            /// <summary>
            /// Distributing a library, with headers, import/static/dynamic libraries, and potentially tools or tests.
            /// </summary>
            Library

            // TODO: macosFramework
        }

#if D_NEW_PUBLISHING
        // this is doubling up the cost of the this.Requires list, but at less runtime cost
        // for expanding each CollatedObject2 to peek as it's properties
        private System.Collections.Generic.Dictionary<System.Tuple<Bam.Core.Module, Bam.Core.PathKey>, CollatedObject2> collatedObjects = new System.Collections.Generic.Dictionary<System.Tuple<Module, PathKey>, CollatedObject2>();

        private Bam.Core.TokenizedString publishDir;
        public Bam.Core.TokenizedString PublishDir
        {
            get
            {
                return this.publishDir;
            }
        }

        public Bam.Core.TokenizedString BinDir
        {
            get
            {
                return this.Macros["BinDir"];
            }

            set
            {
                this.Macros["BinDir"] = value;
            }
        }

        public Bam.Core.TokenizedString LibDir
        {
            get
            {
                return this.Macros["LibDir"];
            }

            set
            {
                this.Macros["LibDir"] = value;
            }
        }

        public Bam.Core.TokenizedString PluginDir
        {
            get
            {
                return this.Macros["PluginDir"];
            }

            set
            {
                this.Macros["PluginDir"] = value;
            }
        }

        public Bam.Core.TokenizedString HeaderDir
        {
            get
            {
                return this.Macros["HeaderDir"];
            }

            set
            {
                this.Macros["HeaderDir"] = value;
            }
        }

        private void
        SetConsoleApplicationDefaultMacros()
        {
            this.Macros.Add("BinDir", this.CreateTokenizedString("$(0)", new [] {this.PublishDir}));
            this.Macros.Add("LibDir", this.CreateTokenizedString("$(0)", new[] { this.PublishDir }));
            this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)", new[] { this.PublishDir }));
        }

        private void
        setWindowedApplicationDefaultMacros()
        {
            switch (Bam.Core.OSUtilities.CurrentOS)
            {
                case Bam.Core.EPlatform.Windows:
                case Bam.Core.EPlatform.Linux:
                    {
                        this.Macros.Add("BinDir", this.CreateTokenizedString("$(0)", new[] { this.PublishDir }));
                        this.Macros.Add("LibDir", this.CreateTokenizedString("$(0)", new[] { this.PublishDir }));
                        this.Macros.Add("PluginDir", this.CreateTokenizedString("$(0)/plugins", new[] { this.PublishDir }));
                    }
                    break;

                case Bam.Core.EPlatform.OSX:
                    {
                        this.Macros.Add("macOSAppBundleContentsDir", this.CreateTokenizedString("$(0)/$(OutputName).app/Contents", new[] { this.PublishDir }));
                        this.Macros.Add("macOSAppBundleMacOSDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/MacOS"));
                        this.Macros.Add("macOSAppBundleFrameworksDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Frameworks"));
                        this.Macros.Add("macOSAppBundlePluginsDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Plugins"));
                        this.Macros.Add("macOSAppBundleResourcesDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/Resources"));
                        this.Macros.Add("macOSAppBundleSharedSupportDir", this.CreateTokenizedString("$(macOSAppBundleContentsDir)/SharedSupport"));
                        this.Macros.Add("BinDir", this.CreateTokenizedString("$(macOSAppBundleMacOSDir)"));
                        this.Macros.Add("LibDir", this.CreateTokenizedString("$(macOSAppBundleFrameworksDir)"));
                        this.Macros.Add("PluginDir", this.CreateTokenizedString("$(macOSAppBundlePluginsDir)"));
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported OS: '{0}'", Bam.Core.OSUtilities.CurrentOS);
            }
        }

        private void
        setLibraryDefaultMacros()
        {
            this.Macros.Add("BinDir", this.CreateTokenizedString("$(0)/bin", new[] { this.PublishDir }));
            this.Macros.Add("LibDir", this.CreateTokenizedString("$(0)/lib", new[] { this.PublishDir }));
            this.Macros.Add("HeaderDir", this.CreateTokenizedString("$(0)/include", new[] { this.PublishDir }));
        }

        public void
        SetDefaultMacros(
            EPublishingType type)
        {
            // TODO: can any of these paths be determined from the C package for RPATHs etc?
            // i.e. whatever layout the user wants, is honoured here as a default
            switch (type)
            {
                case EPublishingType.ConsoleApplication:
                    this.SetConsoleApplicationDefaultMacros();
                    break;

                case EPublishingType.WindowedApplication:
                    this.setWindowedApplicationDefaultMacros();
                    break;

                case EPublishingType.Library:
                    this.setLibraryDefaultMacros();
                    break;
            }
        }

        public void
        IncludeAllModulesInNamespace(
            string nameSpace,
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString publishDir)
        {
            var gen = this.GetType().GetMethod("Include2", new[] { typeof(Bam.Core.PathKey), typeof(Bam.Core.TokenizedString) });
            var moduleTypes = global::System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(item =>
                item.Namespace == nameSpace && item.IsSubclassOf(typeof(Bam.Core.Module)) && item != this.GetType());
            foreach (var type in moduleTypes)
            {
                var meth = gen.MakeGenericMethod(new[] { type });
                meth.Invoke(this, new object[] { key, publishDir });
            }
        }
#endif

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
            this.publishDir = Bam.Core.TokenizedString.CreateInline("$(publishdir)");
#else
            if (!Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
            }
#endif
        }

        private string
        PublishingPath(
            Bam.Core.Module module,
            EPublishingType type)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX) &&
                (EPublishingType.WindowedApplication == type))
            {
                var bundlePath = module.CreateTokenizedString("$(OutputName).app/Contents/MacOS");
                bundlePath.Parse();
                return bundlePath.ToString();
            }
            return null;
        }

        public static Bam.Core.TokenizedString
        GenerateFileCopyDestination(
            Bam.Core.Module module,
            Bam.Core.TokenizedString referenceFilePath,
            Bam.Core.TokenizedString subDirectory,
            Bam.Core.TokenizedString unReferencedRoot)
        {
            if (referenceFilePath != null)
            {
                if (null != subDirectory)
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)",
                        referenceFilePath,
                        subDirectory);
                }
                else
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/)",
                        referenceFilePath);
                }
            }
            else
            {
                if (null != subDirectory)
                {
                    return module.CreateTokenizedString("@normalize($(0)/$(1)/)",
                        unReferencedRoot,
                        subDirectory);
                }
                else
                {
                    return module.CreateTokenizedString("@normalize($(0)/)",
                        unReferencedRoot);
                }
            }
        }

        public static Bam.Core.TokenizedString
        GenerateDirectoryCopyDestination(
            Bam.Core.Module module,
            Bam.Core.TokenizedString referenceFilePath,
            Bam.Core.TokenizedString subDirectory,
            Bam.Core.TokenizedString sourcePath)
        {
            // Windows XCOPY requires the directory name to be added to the destination, while Posix cp does not
            if (null != subDirectory)
            {
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/$(1)/@ifnotempty($(CopiedFilename),$(CopiedFilename),@filename($(2)))/)",
                        referenceFilePath,
                        subDirectory,
                        sourcePath);
                }
                else
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/$(1)/@ifnotempty($(CopiedFilename),$(CopiedFilename),))",
                        referenceFilePath,
                        subDirectory);
                }
            }
            else
            {
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/@ifnotempty($(CopiedFilename),$(CopiedFilename),@filename($(1)))/)",
                        referenceFilePath,
                        sourcePath);
                }
                else
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/@ifnotempty($(CopiedFilename),$(CopiedFilename),))",
                        referenceFilePath);
                }
            }
        }

        public static Bam.Core.TokenizedString
        GenerateSymbolicLinkCopyDestination(
            Bam.Core.Module module,
            Bam.Core.TokenizedString referenceFilePath,
            Bam.Core.TokenizedString subDirectory)
        {
            if (null != subDirectory)
            {
                return module.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)",
                    referenceFilePath,
                    subDirectory);
            }
            else
            {
                return module.CreateTokenizedString("@normalize(@dir($(0))/)",
                    referenceFilePath);
            }
        }

#if D_NEW_PUBLISHING
        private CollatedFile2
        CreateCollatedFile2(
            Bam.Core.Module dependent,
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString publishDir,
            ICollatedObject2 anchor)
        {
            var module = Bam.Core.Module.Create<CollatedFile2>() as CollatedFile2;
            if (Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                // the publishdir is different for each anchor, so dependents may be duplicated
                // if referenced by a different anchor
                if (null != anchor)
                {
                    module.Macros.Add("publishdir", dependent.CreateTokenizedString("@dir($(0))", new[] { anchor.SourceModule.GeneratedPaths[anchor.SourcePathKey] }));
                }
                else
                {
                    module.Macros.Add("publishdir", dependent.CreateTokenizedString("@dir($(0))", new[] { dependent.GeneratedPaths[key] }));
                }
                module.SetPublishingDirectory("#inline(0)", new[] { publishDir });
            }
            else
            {
                // publishdir is the same for all anchors, and thus all dependents are unique for all anchors
                module.Macros.Add("publishdir", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
                module.SetPublishingDirectory("$(0)", new[] { publishDir });
            }
            module.SourceModule = dependent;
            module.SourcePathKey = key;

            this.Requires(module);
            module.Requires(dependent);
            return module;
        }
#endif

        private CollatedFile
        CreateCollatedFile(
            Bam.Core.Module sourceModule,
            Bam.Core.TokenizedString sourcePath,
            CollatedFile reference,
            Bam.Core.TokenizedString subDirectory)
        {
            var copyFileModule = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
                {
                    Bam.Core.TokenizedString referenceFilePath = null;
                    if (null != reference)
                    {
                        referenceFilePath = reference.GeneratedPaths[CollatedObject.Key];
                    }
                    else
                    {
                        if (!this.GeneratedPaths.ContainsKey(Key))
                        {
                            this.RegisterGeneratedFile(Key, module.CreateTokenizedString("@dir($(0))", sourcePath));
                        }
                    }
                    module.Macros["CopyDir"] = GenerateFileCopyDestination(
                        module,
                        referenceFilePath,
                        subDirectory,
                        this.GeneratedPaths[Key]);
                });
            this.Requires(copyFileModule);
            if (null != reference &&
                null != reference.SourceModule &&
                null != sourceModule &&
                reference.SourceModule != sourceModule) // in case a different key (e.g. import library) is published from the same module
            {
                // ensuring that non-Native builders set up order-only dependencies for additional published only modules
                reference.SourceModule.Requires(sourceModule);
            }

            copyFileModule.Collator = this;
            copyFileModule.SourceModule = sourceModule;
            copyFileModule.SourcePath = sourcePath;
            copyFileModule.Reference = reference;
            copyFileModule.SubDirectory = subDirectory;
            return copyFileModule;
        }

        private CollatedDirectory
        CreateCollatedDirectory(
            Bam.Core.Module sourceModule,
            Bam.Core.TokenizedString sourcePath,
            CollatedFile reference,
            Bam.Core.TokenizedString subDirectory)
        {
            if (null == reference)
            {
                throw new Bam.Core.Exception("Collating a directory requires a collated file as reference");
            }

            // copying a directory must not have a trailing slash on the source directory path
            // otherwise the leafname ends up being duplicated
            var fixedSourcePath = this.CreateTokenizedString("@removetrailingseparator($(0))", sourcePath);
            var copyDirectoryModule = Bam.Core.Module.Create<CollatedDirectory>(preInitCallback: module =>
                {
                    module.Macros["CopyDir"] = GenerateDirectoryCopyDestination(
                        module,
                        reference.GeneratedPaths[CollatedObject.Key],
                        subDirectory,
                        fixedSourcePath);
                });
            this.Requires(copyDirectoryModule);
            if (null != reference.SourceModule &&
                null != sourceModule &&
                reference.SourceModule != sourceModule) // in case a different key from the same module is published
            {
                // ensuring that non-Native builders set up order-only dependencies for additional published only modules
                reference.SourceModule.Requires(sourceModule);
            }

            copyDirectoryModule.SourceModule = sourceModule;
            copyDirectoryModule.SourcePath = fixedSourcePath;
            copyDirectoryModule.Reference = reference;
            copyDirectoryModule.SubDirectory = subDirectory;
            return copyDirectoryModule;
        }

        private CollatedSymbolicLink
        CreateCollatedSymbolicLink(
            Bam.Core.Module sourceModule,
            Bam.Core.TokenizedString sourcePath,
            CollatedFile reference,
            Bam.Core.TokenizedString subDirectory)
        {
            if (null == reference)
            {
                throw new Bam.Core.Exception("Collating a symbolic link requires a collated file as reference");
            }

            var copySymlinkModule = Bam.Core.Module.Create<CollatedSymbolicLink>(preInitCallback: module =>
                {
                    module.Macros["CopyDir"] = GenerateSymbolicLinkCopyDestination(
                        module,
                        reference.GeneratedPaths[CollatedObject.Key],
                        subDirectory);
                });
            this.Requires(copySymlinkModule);
            if (null != reference.SourceModule &&
                null != sourceModule &&
                reference.SourceModule != sourceModule) // in case a different key from the same module is published
            {
                // ensuring that non-Native builders set up order-only dependencies for additional published only modules
                reference.SourceModule.Requires(sourceModule);
            }

            copySymlinkModule.SourceModule = sourceModule;
            copySymlinkModule.SourcePath = sourcePath;
            copySymlinkModule.Reference = reference;
            copySymlinkModule.SubDirectory = subDirectory;
            return copySymlinkModule;
        }

        private void
        AddOSXChangeIDNameForBinary(
            CollatedFile copyFileModule)
        {
            if (!this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                return;
            }
            var changeIDName = Bam.Core.Module.Create<ChangeNameOSX>();
            changeIDName.Source = copyFileModule;
            changeIDName.Frameworks = this.CopiedFrameworks;
            this.ChangedNamedBinaries.Add(changeIDName);
            this.Requires(changeIDName);

            foreach (var framework in this.CopiedFrameworks)
            {
                changeIDName.Requires(framework);
            }
        }

        private void
        CopySONameSymlink(
            CollatedFile copyFileModule)
        {
            if (!this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                return;
            }
            var copySymlink = this.CreateCollatedSymbolicLink(
                copyFileModule.SourceModule,
                copyFileModule.SourceModule.Macros["SOName"],
                copyFileModule.Reference,
                copyFileModule.SubDirectory);
            copySymlink.AssignLinkTarget(copySymlink.CreateTokenizedString("@filename($(0))", copyFileModule.SourcePath));
        }

        private bool
        IsReferenceAWindowedApp(
            CollatedFile reference)
        {
            if (null == reference.SubDirectory)
            {
                return false;
            }
            return reference.SubDirectory.ToString().Contains(".app");
        }

        public CollatedObject InitialReference
        {
            get;
            private set;
        }

#if D_NEW_PUBLISHING
        private void
        gatherAllDependencies(
            Bam.Core.Module initialModule,
            Bam.Core.PathKey key,
            ICollatedObject2 anchor)
        {
            var allDependents = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();
            var toDealWith = new System.Collections.Generic.Queue<System.Tuple<Bam.Core.Module, Bam.Core.PathKey>>();
            toDealWith.Enqueue(System.Tuple.Create(initialModule, key));
            System.Func<Bam.Core.Module, bool> findPublishableDependents;
            findPublishableDependents = module =>
                {
                    var any = false;
                    // now look at all the dependencies and accumulate a list of child dependencies
                    // TODO: need a configurable list of types, not just C.DynamicLibrary, that the user can specify to find
                    foreach (var dep in module.Dependents)
                    {
                        if (dep is C.Plugin)
                        {
                            if (!allDependents.ContainsKey(dep))
                            {
                                toDealWith.Enqueue(System.Tuple.Create(dep, C.Plugin.Key));
                                any = true;
                            }
                        }
                        if (dep is C.Cxx.Plugin)
                        {
                            if (!allDependents.ContainsKey(dep))
                            {
                                toDealWith.Enqueue(System.Tuple.Create(dep, C.Cxx.Plugin.Key));
                                any = true;
                            }
                        }
                        if (dep is C.DynamicLibrary)
                        {
                            if (!allDependents.ContainsKey(dep))
                            {
                                toDealWith.Enqueue(System.Tuple.Create(dep, C.DynamicLibrary.Key));
                                any = true;
                            }
                        }
                        if (dep is C.Cxx.DynamicLibrary)
                        {
                            if (!allDependents.ContainsKey(dep))
                            {
                                toDealWith.Enqueue(System.Tuple.Create(dep, C.Cxx.DynamicLibrary.Key));
                                any = true;
                            }
                        }
                    }
                    foreach (var req in module.Requirements)
                    {
                        if (req is C.Plugin)
                        {
                            if (!allDependents.ContainsKey(req))
                            {
                                toDealWith.Enqueue(System.Tuple.Create(req, C.Plugin.Key));
                                any = true;
                            }
                        }
                        if (req is C.Cxx.Plugin)
                        {
                            if (!allDependents.ContainsKey(req))
                            {
                                toDealWith.Enqueue(System.Tuple.Create(req, C.Cxx.Plugin.Key));
                                any = true;
                            }
                        }
                        if (req is C.DynamicLibrary)
                        {
                            if (!allDependents.ContainsKey(req))
                            {
                                toDealWith.Enqueue(System.Tuple.Create(req, C.DynamicLibrary.Key));
                                any = true;
                            }
                        }
                        if (req is C.Cxx.DynamicLibrary)
                        {
                            if (!allDependents.ContainsKey(req))
                            {
                                toDealWith.Enqueue(System.Tuple.Create(req, C.Cxx.DynamicLibrary.Key));
                                any = true;
                            }
                        }
                    }
                    return any;
                };
            // iterate over each dependent, stepping into each of their dependencies
            while (toDealWith.Count > 0)
            {
                var next = toDealWith.Dequeue();
                findPublishableDependents(next.Item1);
                if (next.Item1 != initialModule)
                {
                    var notPresent = true;
                    notPresent &= !allDependents.ContainsKey(next.Item1);
                    notPresent &= !this.collatedObjects.ContainsKey(next);
                    if (anchor != null)
                    {
                        var anchorAsCollatedObject = anchor as CollatedObject2;
                        notPresent &= !anchorAsCollatedObject.DependentCollations.ContainsKey(next);
                    }
                    if (notPresent)
                    {
                        allDependents.Add(next.Item1, next.Item2);
                    }
                }
            }
            // now add each as a publishable dependent
            foreach (var dep in allDependents)
            {
                if (dep.Key is C.Cxx.Plugin || dep.Key is C.Plugin)
                {
                    this.Include2NoGather(dep.Key, dep.Value, this.PluginDir, anchor);
                }
                else if (dep.Key is C.DynamicLibrary || dep.Key is C.Cxx.DynamicLibrary)
                {
                    this.Include2NoGather(dep.Key, dep.Value, this.LibDir, anchor);
                }
                else
                {
                    throw new System.NotSupportedException(System.String.Format("Module of type {0}", dep.Key.GetType().ToString()));
                }
            }
        }

        private ICollatedObject2
        Include2NoGather(
            Bam.Core.Module dependent,
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString publishDir,
            ICollatedObject2 anchor)
        {
            var collatedFile = this.CreateCollatedFile2(dependent, key, publishDir, anchor);
            var tuple = System.Tuple.Create(dependent, key);
            if (Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                // a dependency may be copied for each anchor that references it in order
                // to make that anchor fully resolved and debuggable
                if (null != anchor)
                {
                    var anchorAsCollatedObject = anchor as CollatedObject2;
                    anchorAsCollatedObject.DependentCollations.Add(tuple, collatedFile);
                }
                else
                {
                    this.collatedObjects.Add(tuple, collatedFile);
                }
            }
            else
            {
                // as everything goes to a single publishdir, remember each instance of a module
                this.collatedObjects.Add(tuple, collatedFile);
            }
            return collatedFile;
        }

        private void
        Include2(
            Bam.Core.Module dependent,
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString publishDir)
        {
            var collatedFile = this.Include2NoGather(dependent, key, publishDir, null);
            this.gatherAllDependencies(dependent, key, collatedFile);
        }

        public void
        Include2<DependentModule>(
            Bam.Core.PathKey key,
            Bam.Core.TokenizedString publishDir) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Include2(dependent, key, publishDir);
        }

        public void
        Include2<DependentModule>(
            Bam.Core.PathKey key,
            string publishDir) where DependentModule : Bam.Core.Module, new()
        {
            this.Include2<DependentModule>(key, this.CreateTokenizedString(publishDir));
        }
#endif

        /// <summary>
        /// Collate the main application file in the publishing root. Use the publishing type to determine
        /// what kind of application this will be.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="type">Type.</param>
        /// <param name="subdir">Subdir.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public CollatedFile
        Include<DependentModule>(
            Bam.Core.PathKey key,
            EPublishingType type,
            string subdir = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            var path = this.PublishingPath(dependent, type);
            string destSubDir;
            if (null == path)
            {
                destSubDir = subdir;
            }
            else
            {
                if (null != subdir)
                {
                    destSubDir = System.IO.Path.Combine(path, subdir);
                }
                else
                {
                    destSubDir = path;
                }
            }

            var copyFileModule = this.CreateCollatedFile(
                dependent,
                dependent.GeneratedPaths[key],
                null,
                Bam.Core.TokenizedString.CreateVerbatim(destSubDir));

            if (EPublishingType.WindowedApplication == type)
            {
                if (C.ConsoleApplication.Key == key)
                {
                    this.AddOSXChangeIDNameForBinary(copyFileModule);
                }
            }

            this.InitialReference = copyFileModule;

            return copyFileModule;
        }

        private CollatedFile
        Include(
            Bam.Core.Module dependent,
            Bam.Core.PathKey key,
            string subdir,
            CollatedFile reference)
        {
            try
            {
                var copyFileModule = this.CreateCollatedFile(
                    dependent,
                    dependent.GeneratedPaths[key],
                    reference,
                    Bam.Core.TokenizedString.CreateVerbatim(subdir));

                if (this.IsReferenceAWindowedApp(reference))
                {
                    if (C.ConsoleApplication.Key == key)
                    {
                        this.AddOSXChangeIDNameForBinary(copyFileModule);
                    }
                }
                else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    if ((dependent is C.IDynamicLibrary) && dependent.Macros.Contains("SOName"))
                    {
                        this.CopySONameSymlink(copyFileModule);
                    }
                }

                return copyFileModule;
            }
            catch (Bam.Core.UnableToBuildModuleException exception)
            {
                Bam.Core.Log.Info("Not publishing {0} requested by {1} because {2}, but publishing will continue",
                    dependent.GetType().ToString(),
                    this.GetType().ToString(),
                    exception.Message);
                return null;
            }
        }

        /// <summary>
        /// Include a file built by Bam in a location relative to the reference file.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="subdir">Subdir.</param>
        /// <param name="reference">Reference.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public CollatedFile
        Include<DependentModule>(
            Bam.Core.PathKey key,
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            return this.Include(dependent, key, subdir, reference);
        }

        /// <summary>
        /// Include a number of files relative to the reference file, from the DependentModule.
        /// </summary>
        /// <param name="parameterizedFilePath">Parameterized file path.</param>
        /// <param name="subdir">Subdir.</param>
        /// <param name="reference">Reference.</param>
        /// <param name="isExecutable">If set to <c>true</c> is executable.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public CollatedFile
        IncludeFiles<DependentModule>(
            string parameterizedFilePath,
            string subdir,
            CollatedFile reference,
            bool isExecutable = false) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            var copyFileModule = this.CreateCollatedFile(
                dependent,
                dependent.CreateTokenizedString(parameterizedFilePath),
                reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));

            if (isExecutable)
            {
                if (this.IsReferenceAWindowedApp(reference))
                {
                    this.AddOSXChangeIDNameForBinary(copyFileModule);
                }
            }

            return copyFileModule;
        }

        /// <summary>
        /// Include a file relative to the reference file, from an arbitrary location.
        /// </summary>
        /// <param name="parameterizedFilePath">Parameterized file path.</param>
        /// <param name="subdir">Subdir.</param>
        /// <param name="reference">Reference.</param>
        /// <param name="isExecutable">If set to <c>true</c> is executable.</param>
        public CollatedFile
        IncludeFile(
            string parameterizedFilePath,
            string subdir,
            CollatedFile reference,
            bool isExecutable = false)
        {
            var tokenString = this.CreateTokenizedString(parameterizedFilePath);
            return this.IncludeFile(tokenString, subdir, reference, isExecutable);
        }

        /// <summary>
        /// Include a file relative to the reference file, from an arbitrary location.
        /// </summary>
        /// <param name="parameterizedFilePath">Parameterized file path.</param>
        /// <param name="subdir">Subdir.</param>
        /// <param name="reference">Reference.</param>
        /// <param name="isExecutable">If set to <c>true</c> is executable.</param>
        public CollatedFile
        IncludeFile(
            Bam.Core.TokenizedString parameterizedFilePath,
            string subdir,
            CollatedFile reference,
            bool isExecutable = false)
        {
            var copyFileModule = this.CreateCollatedFile(
                null,
                parameterizedFilePath,
                reference,
                Bam.Core.TokenizedString.CreateVerbatim(subdir));

            if (isExecutable)
            {
                if (this.IsReferenceAWindowedApp(reference))
                {
                    this.AddOSXChangeIDNameForBinary(copyFileModule);
                }
            }

            return copyFileModule;
        }

        /// <summary>
        /// Include a file which can act as a reference file, from an arbitrary location.
        /// </summary>
        /// <param name="parameterizedFilePath">Parameterized file path.</param>
        /// <param name="subdir">Subdir.</param>
        public CollatedFile
        IncludeFile(
            Bam.Core.TokenizedString parameterizedFilePath,
            string subdir)
        {
            var copyFileModule = this.CreateCollatedFile(
                this,
                parameterizedFilePath,
                null,
                Bam.Core.TokenizedString.CreateVerbatim(subdir));
            this.InitialReference = copyFileModule;
            return copyFileModule;
        }

        /// <summary>
        /// Include a directory relative to the reference file, from an arbitrary location.
        /// </summary>
        /// <param name="parameterizedPath">Parameterized path.</param>
        /// <param name="subdir">Subdir.</param>
        /// <param name="reference">Reference.</param>
        public CollatedDirectory
        IncludeDirectory(
            Bam.Core.TokenizedString parameterizedPath,
            string subdir,
            CollatedFile reference)
        {
            return this.CreateCollatedDirectory(null, parameterizedPath, reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
        }

        /// <summary>
        /// Include a symlink relative to the reference file.
        /// </summary>
        /// <param name="parameterizedPath">Parameterized path.</param>
        /// <param name="subdir">Subdir.</param>
        /// <param name="reference">Reference.</param>
        public CollatedSymbolicLink
        IncludeSymlink(
            Bam.Core.TokenizedString parameterizedPath,
            string subdir,
            CollatedFile reference)
        {
            return this.CreateCollatedSymbolicLink(
                null,
                parameterizedPath,
                reference,
                Bam.Core.TokenizedString.CreateVerbatim(subdir));
        }

        /// <summary>
        /// Include an OSX framework relative to the reference file, from DependentModule, and optionally
        /// update its install name to function in its new location.
        /// </summary>
        /// <param name="subdir">Subdir.</param>
        /// <param name="reference">Reference.</param>
        /// <param name="updateInstallName">If set to <c>true</c> update install name.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public Bam.Core.Array<CollatedObject>
        IncludeFramework<DependentModule>(
            string subdir,
            CollatedFile reference,
            bool updateInstallName = false) where DependentModule : C.OSXFramework, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            // TODO: confirm that reference was created in WindowedApplication mode

            var subdirTS = Bam.Core.TokenizedString.CreateVerbatim(subdir);

            var framework = dependent as C.OSXFramework;
            var frameworkPath = framework.FrameworkPath;

            var dirPublishedModules = new Bam.Core.Array<CollatedDirectory>();
            if (null != framework.DirectoriesToPublish)
            {
                foreach (var dirData in framework.DirectoriesToPublish)
                {
                    var dir = dirData.SourcePath;
                    // copying a directory must not have a trailing slash on the source directory path
                    // otherwise the leafname ends up being duplicated
                    var copyDir = this.CreateCollatedDirectory(
                        dependent,
                        this.CreateTokenizedString("$(0)/@removetrailingseparator($(1))", frameworkPath, dir),
                        reference,
                        this.CreateTokenizedString("$(0)/@dir(@removetrailingseparator($(1)))", subdirTS, dirData.DestinationPath != null ? dirData.DestinationPath : dir));
                    dirPublishedModules.AddUnique(copyDir);
                }
            }
            var filePublishedModules = new Bam.Core.Array<CollatedFile>();
            if (null != framework.FilesToPublish)
            {
                foreach (var fileData in framework.FilesToPublish)
                {
                    var file = fileData.SourcePath;
                    var copyFile = this.CreateCollatedFile(
                        dependent,
                        this.CreateTokenizedString("$(0)/$(1)", frameworkPath, file),
                        reference,
                        this.CreateTokenizedString("$(0)/@dir($(1))", subdirTS, fileData.DestinationPath != null ? fileData.DestinationPath : file));
                    foreach (var publishedDir in dirPublishedModules)
                    {
                        copyFile.Requires(publishedDir);
                    }
                    filePublishedModules.AddUnique(copyFile);

                    // the dylib in the framework
                    if (updateInstallName && (file == framework.Macros["FrameworkLibraryPath"]))
                    {
                        var updateIDName = Bam.Core.Module.Create<IdNameOSX>();
                        updateIDName.Source = copyFile;
                        this.Requires(updateIDName);
                        this.CopiedFrameworks.Add(copyFile);

                        foreach (var changedName in this.ChangedNamedBinaries)
                        {
                            changedName.Requires(updateIDName);
                        }

                        if (this.IsReferenceAWindowedApp(reference))
                        {
                            this.AddOSXChangeIDNameForBinary(copyFile);
                        }
                    }
                }
            }
            var symlinkPublishedModules = new Bam.Core.Array<CollatedSymbolicLink>();
            if (null != framework.SymlinksToPublish)
            {
                foreach (var symlinkData in framework.SymlinksToPublish)
                {
                    var symlink = symlinkData.SourcePath;
                    var copySymlink = this.CreateCollatedSymbolicLink(
                        dependent,
                        this.CreateTokenizedString("$(0)/$(1)", frameworkPath, symlink),
                        reference,
                        this.CreateTokenizedString("$(0)/@dir($(1))", subdirTS, symlink));
                    copySymlink.AssignLinkTarget(symlinkData.DestinationPath);
                    foreach (var publishedDir in dirPublishedModules)
                    {
                        copySymlink.Requires(publishedDir);
                    }
                    foreach (var publishedFile in filePublishedModules)
                    {
                        copySymlink.Requires(publishedFile);
                    }
                    symlinkPublishedModules.AddUnique(copySymlink);
                }
            }

            var frameworkComponents = new Bam.Core.Array<CollatedObject>();
            frameworkComponents.AddRangeUnique(filePublishedModules);
            frameworkComponents.AddRangeUnique(dirPublishedModules);
            frameworkComponents.AddRangeUnique(symlinkPublishedModules);
            return frameworkComponents;
        }

        /// <summary>
        /// For a collated ELF file, update it's RPATH.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="newRPath">New R path.</param>
        public ChangeRPathModule
        ChangeRPath(
            CollatedFile source,
            string newRPath)
        {
            var change = Bam.Core.Module.Create<ChangeRPathModule>();
            change.Source = source;
            change.NewRPath = newRPath;
            this.Requires(change);
            return change;
        }

        public sealed override void
        Evaluate()
        {
            // TODO
        }

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.Collate(this, context);
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "Collation";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<ICollationPolicy>.Create(className);
                    }
                    break;
            }
        }

#if D_NEW_PUBLISHING
        public ICollatedObject2
        Find<DependentModule>() where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            foreach (var dep in this.Requirements)
            {
                var obj = dep as ICollatedObject2;
                if (obj.SourceModule == dependent)
                {
                    return obj;
                }
            }
            return null;
        }
#endif
    }
#endif // D_NEW_PUBLISHING
}
