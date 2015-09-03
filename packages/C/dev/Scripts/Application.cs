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
namespace C
{
namespace V2
{
    public abstract class CModule :
        Bam.Core.V2.Module
    {
        protected Bam.Core.Array<Bam.Core.V2.Module> headerModules = new Bam.Core.Array<Bam.Core.V2.Module>();

        public CModule()
        {
            this.Macros.Add("OutputName", this.Macros["modulename"]);
            // default bit depth
            // TODO: override on command line
            this.BitDepth = EBit.SixtyFour;
        }

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            // if there is a parent from which this module is created, inherit bitdepth
            if (null != parent)
            {
                this.BitDepth = (parent as CModule).BitDepth;
            }
        }

        public EBit BitDepth
        {
            get;
            set;
        }

        protected static Bam.Core.Array<Bam.Core.V2.Module>
        FlattenHierarchicalFileList(
            Bam.Core.Array<Bam.Core.V2.Module> files)
        {
            var list = new Bam.Core.Array<Bam.Core.V2.Module>();
            foreach (var input in files)
            {
                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        list.Add(child);
                    }
                }
                else
                {
                    list.Add(input);
                }
            }
            return list;
        }

        protected T
        CreateContainer<T>(
            bool requires,
            string wildcardPath = null,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null,
            Bam.Core.V2.Module.PrivatePatchDelegate privatePatch = null)
            where T : CModule, new()
        {
            var source = Bam.Core.V2.Module.Create<T>(this);
            if (null != privatePatch)
            {
                source.PrivatePatch(privatePatch);
            }

            if (requires)
            {
                this.Requires(source);
            }
            else
            {
                this.DependsOn(source);
            }

            if (null != wildcardPath)
            {
                (source as IAddFiles).AddFiles(wildcardPath, macroModuleOverride: macroModuleOverride, filter: filter);
            }

            return source;
        }

        public HeaderFileCollection
        CreateHeaderContainer(
            string wildcardPath = null,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var headers = this.CreateContainer<HeaderFileCollection>(true, wildcardPath, macroModuleOverride, filter);
            this.headerModules.Add(headers);
            return headers;
        }
    }

    interface IAddFiles
    {
        Bam.Core.Array<Bam.Core.V2.Module>
        AddFiles(
            string path,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null);
    }

    public abstract class CModuleContainer<ChildModuleType> :
        CModule,
        Bam.Core.V2.IModuleGroup,
        IAddFiles
        where ChildModuleType : Bam.Core.V2.Module, Bam.Core.V2.IInputPath, Bam.Core.V2.IChildModule, new()
    {
        private System.Collections.Generic.List<ChildModuleType> children = new System.Collections.Generic.List<ChildModuleType>();

        public ChildModuleType
        AddFile(
            string path,
            Bam.Core.V2.Module macroModuleOverride = null,
            bool verbatim = false)
        {
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.V2.Module.Create<ChildModuleType>(this);
            var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
            child.InputPath = Bam.Core.V2.TokenizedString.Create(path, macroModule, verbatim);
            (child as Bam.Core.V2.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        public Bam.Core.Array<Bam.Core.V2.Module>
        AddFiles(
            string path,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
            var wildcardPath = Bam.Core.V2.TokenizedString.Create(path, macroModule).Parse();

            var dir = System.IO.Path.GetDirectoryName(wildcardPath);
            var leafname = System.IO.Path.GetFileName(wildcardPath);
            var files = System.IO.Directory.GetFiles(dir, leafname, System.IO.SearchOption.TopDirectoryOnly);
            if (filter != null)
            {
                files = files.Where(pathname => filter.IsMatch(pathname)).ToArray();
            }
            if (0 == files.Length)
            {
                throw new Bam.Core.Exception("No files were found that matched the pattern '{0}'", wildcardPath);
            }
            var modulesCreated = new Bam.Core.Array<Bam.Core.V2.Module>();
            foreach (var filepath in files)
            {
                var fp = filepath;
                modulesCreated.Add(this.AddFile(fp, verbatim: true));
            }
            return modulesCreated;
        }

        public ChildModuleType
        AddFile(
            Bam.Core.V2.FileKey generatedFileKey,
            Bam.Core.V2.Module module,
            Bam.Core.V2.Module.ModulePreInitDelegate preInitDlg = null)
        {
            if (!module.GeneratedPaths.ContainsKey(generatedFileKey))
            {
                throw new System.Exception(System.String.Format("No generated path found with key '{0}'", generatedFileKey.Id));
            }
            var child = Bam.Core.V2.Module.Create<ChildModuleType>(this, preInitCallback: preInitDlg);
            child.InputPath = module.GeneratedPaths[generatedFileKey];
            (child as Bam.Core.V2.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            child.DependsOn(module);
            return child;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            // do nothing
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // do nothing
            // TODO: might have to get the policy, for the sharing settings
        }

        public override void
        Evaluate()
        {
            foreach (var child in this.children)
            {
                if (!child.IsUpToDate)
                {
                    return;
                }
            }
            this.IsUpToDate = true;
        }
    }

    public abstract class CSDKModule :
        CModule
    {}

    public abstract class ExternalFramework :
        CModule
    {}

    public class ConsoleApplication :
        CModule
    {
        protected Bam.Core.Array<Bam.Core.V2.Module> sourceModules = new Bam.Core.Array<Bam.Core.V2.Module>();
        private Bam.Core.Array<Bam.Core.V2.Module> linkedModules = new Bam.Core.Array<Bam.Core.V2.Module>();
        private ILinkerPolicy Policy = null;

        static public Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("ExecutableFile");

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(moduleoutputdir)/$(OutputName)$(exeext)", this));
            this.Linker = DefaultToolchain.C_Linker(this.BitDepth);
            this.PrivatePatch(settings =>
            {
                var linker = settings as C.V2.ICommonLinkerOptions;
                linker.OutputType = ELinkerOutput.Executable;
            });
        }

        private Bam.Core.V2.Module.PrivatePatchDelegate ConsolePreprocessor = settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.PreprocessorDefines.Add("_CONSOLE");
            };

        public virtual CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.CreateContainer<CObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, this.ConsolePreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        public virtual Cxx.V2.ObjectFileCollection
        CreateCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.CreateContainer<Cxx.V2.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, this.ConsolePreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        public virtual C.ObjC.V2.ObjectFileCollection
        CreateObjectiveCSourceContainer(
            string wildcardPath = null,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.CreateContainer<C.ObjC.V2.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, this.ConsolePreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        public virtual C.ObjCxx.V2.ObjectFileCollection
        CreateObjectiveCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.CreateContainer<C.ObjCxx.V2.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, this.ConsolePreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        public void
        CompileAgainst<DependentModule>(
            params CModule[] affectedSources) where DependentModule : HeaderLibrary, new()
        {
            if (0 == affectedSources.Length)
            {
                throw new Bam.Core.Exception("At least one source module argument must be passed to {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, this.ToString());
            }

            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
        }

        public void LinkAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.linkedModules.Add(dependent);
            this.UsePublicPatches(dependent);
        }

        public void RequiredToExist<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Requires(dependent);
        }

        public void
        CompileAndLinkAgainst<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            if (0 == affectedSources.Length)
            {
                throw new Bam.Core.Exception("At least one source must be provided");
            }

            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.linkedModules.Add(dependent);
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
            this.LinkAllForwardedDependenciesFromLibraries(dependent);
        }

        public void
        CompilePubliclyAndLinkAgainst<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            this.CompileAndLinkAgainst<DependentModule>(affectedSources);
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.UsePublicPatches(dependent);
        }

        private void
        LinkAllForwardedDependenciesFromLibraries(
            Bam.Core.V2.Module module)
        {
            var withForwarded = module as IForwardedLibraries;
            if (null == withForwarded)
            {
                return;
            }

            // recursive
            foreach (var forwarded in withForwarded.ForwardedLibraries)
            {
                this.DependsOn(forwarded);
                // some linkers require a specific order of libraries in order to resolve symbols
                // so that if an existing library is later referenced, it needs to be moved later
                if (this.linkedModules.Contains(forwarded))
                {
                    this.linkedModules.Remove(forwarded);
                }
                this.linkedModules.Add(forwarded);
                this.LinkAllForwardedDependenciesFromLibraries(forwarded);
            }
        }

        public LinkerTool Linker
        {
            get
            {
                return this.Tool as LinkerTool;
            }
            set
            {
                this.Tool = value;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            var source = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(FlattenHierarchicalFileList(this.sourceModules).ToArray());
            var headers = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(FlattenHierarchicalFileList(this.headerModules).ToArray());
            var linked = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.linkedModules.ToArray());
            var executable = this.GeneratedPaths[Key];
            this.Policy.Link(this, context, executable, source, headers, linked, null);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            var className = "C.V2." + mode + "Linker";
            this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<ILinkerPolicy>.Create(className);
        }

        public override void Evaluate()
        {
            var exists = System.IO.File.Exists(this.GeneratedPaths[Key].ToString());
            if (!exists)
            {
                return;
            }
            foreach (var source in this.sourceModules)
            {
                if (!source.IsUpToDate)
                {
                    return;
                }
            }
            foreach (var source in this.linkedModules)
            {
                if (!source.IsUpToDate)
                {
                    return;
                }
            }
            this.IsUpToDate = true;
        }
    }
}
namespace Cxx
{
namespace V2
{
    public class ConsoleApplication :
        C.V2.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.Linker = C.V2.DefaultToolchain.Cxx_Linker(this.BitDepth);
        }
    }
}
}
    /// <summary>
    /// C/C++ console application
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(ILinkerTool))]
    public class Application :
        Bam.Core.BaseModule,
        Bam.Core.INestedDependents,
        Bam.Core.ICommonOptionCollection,
        Bam.Core.IPostActionModules
    {
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("ExecutableBinaryFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("ExecutableBinaryDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        public static readonly Bam.Core.LocationKey MapFile = new Bam.Core.LocationKey("MapFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey MapFileDir = new Bam.Core.LocationKey("MapFileDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        Bam.Core.ModuleCollection
        Bam.Core.INestedDependents.GetNestedDependents(
            Bam.Core.Target target)
        {
            var collection = new Bam.Core.ModuleCollection();

            var type = this.GetType();
            var fieldInfoArray = type.GetFields(System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfoArray)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(Bam.Core.SourceFilesAttribute), false);
                if (attributes.Length > 0)
                {
                    var targetFilters = attributes[0] as Bam.Core.ITargetFilters;
                    if (!Bam.Core.TargetUtilities.MatchFilters(target, targetFilters))
                    {
                        Bam.Core.Log.DebugMessage("Source file field '{0}' of module '{1}' with filters '{2}' does not match target '{3}'", fieldInfo.Name, type.ToString(), targetFilters.ToString(), target.ToString());
                        continue;
                    }

                    var module = fieldInfo.GetValue(this) as Bam.Core.IModule;
                    if (null == module)
                    {
                        throw new Bam.Core.Exception("Field '{0}', marked with Bam.Core.SourceFiles attribute, must be derived from type Core.IModule", fieldInfo.Name);
                    }
                    collection.Add(module);
                }
            }

            return collection;
        }

        [LocalCompilerOptionsDelegate]
        private static void
        ApplicationSetConsolePreprocessor(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var compilerOptions = module.Options as ICCompilerOptions;
                compilerOptions.Defines.Add("_CONSOLE");
            }
        }

        [LocalLinkerOptionsDelegate]
        private static void
        ApplicationSetConsoleSubSystem(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var linkerOptions = module.Options as ILinkerOptions;
                linkerOptions.SubSystem = C.ESubsystem.Console;
            }
        }

        Bam.Core.BaseOptionCollection Bam.Core.ICommonOptionCollection.CommonOptionCollection
        {
            get;
            set;
        }

        #region IPostActionModules Members

        Bam.Core.TypeArray Bam.Core.IPostActionModules.GetPostActionModuleTypes(Bam.Core.BaseTarget target)
        {
            // TODO: currently disabled - only really needs to be in versions earlier than VS2010
            // not sure if it's needed for mingw
#if true
            return null;
#else
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                var postActionModules = new Bam.Core.TypeArray(
                    typeof(Win32Manifest));
                return postActionModules;
            }
            return null;
#endif
        }

        #endregion
    }
}