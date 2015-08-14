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
namespace C
{
namespace V2
{
    public abstract class CModule :
        Bam.Core.V2.Module
    {
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
        protected Bam.Core.Array<Bam.Core.V2.Module> headerModules = new Bam.Core.Array<Bam.Core.V2.Module>();
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

        private Bam.Core.V2.Module.PublicPatchDelegate ConsolePreprocessor = (settings, appliedTo) =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.PreprocessorDefines.Add("_CONSOLE");
            };

        public HeaderFileCollection
        CreateHeaderContainer()
        {
            var headers = Bam.Core.V2.Module.Create<HeaderFileCollection>();
            this.headerModules.Add(headers);
            this.Requires(headers);
            return headers;
        }

        private T CreateContainer<T>() where T : CModule, new()
        {
            var source = Bam.Core.V2.Module.Create<T>(this);
            source.PrivatePatch(settings => this.ConsolePreprocessor(settings, this));

            this.sourceModules.Add(source);
            this.DependsOn(source);
            return source;
        }

        public virtual CObjectFileCollection CreateCSourceContainer()
        {
            return this.CreateContainer<CObjectFileCollection>();
        }

        public virtual Cxx.V2.ObjectFileCollection CreateCxxSourceContainer(string wildcardPath = null)
        {
            return this.CreateContainer<Cxx.V2.ObjectFileCollection>();
        }

        public virtual C.ObjC.V2.ObjectFileCollection CreateObjectiveCSourceContainer()
        {
            return this.CreateContainer<C.ObjC.V2.ObjectFileCollection>();
        }

        public virtual C.ObjCxx.V2.ObjectFileCollection CreateObjectiveCxxSourceContainer()
        {
            return this.CreateContainer<C.ObjCxx.V2.ObjectFileCollection>();
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
            this.UsePublicPatches(dependent);
            foreach (var source in affectedSources)
            {
                source.UsePublicPatches(dependent);
            }
            if (dependent is StaticLibrary)
            {
                foreach (var forwarded in (dependent as StaticLibrary).ForwardedStaticLibraries)
                {
                    this.DependsOn(forwarded);
                    this.linkedModules.Add(forwarded);
                }
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
            var source = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.sourceModules.ToArray());
            var headers = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.headerModules.ToArray());
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