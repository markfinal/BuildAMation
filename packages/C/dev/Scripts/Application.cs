#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace C
{
namespace V2
{
    public class ConsoleApplication :
        Bam.Core.V2.Module
    {
        private System.Collections.Generic.List<Bam.Core.V2.Module> sourceModules = new System.Collections.Generic.List<Bam.Core.V2.Module>();
        private System.Collections.Generic.List<Bam.Core.V2.Module> linkedModules = new System.Collections.Generic.List<Bam.Core.V2.Module>();
        private ILinkerPolicy Policy = null;

        static public Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("ExecutableFile");

        public ConsoleApplication()
        {
            this.Linker = DefaultToolchain.Linker;
        }

        protected override void Init()
        {
            base.Init();
            this.RegisterGeneratedFile(Key, Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename)/$(config)/$(modulename)$(exeext)", this));
        }

        public CObjectFileCollection CreateCSourceContainer()
        {
            var source = Bam.Core.V2.Module.Create<CObjectFileCollection>();
            this.sourceModules.Add(source);
            this.DependsOn(source);
            return source;
        }

        public Cxx.V2.ObjectFileCollection CreateCxxSourceContainer(string wildcardPath)
        {
            var source = Bam.Core.V2.Module.Create<Cxx.V2.ObjectFileCollection>();
            this.sourceModules.Add(source);
            this.DependsOn(source);
            return source;
        }

        public DependentModule LinkAgainst<DependentModule>() where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.linkedModules.Add(dependent);
            return dependent as DependentModule;
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

        protected override void ExecuteInternal()
        {
            var source = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.sourceModules);
            var linked = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.linkedModules);
            var executable = this.GeneratedPaths[Key].ToString();
            this.Policy.Link(this, executable, source, linked, null);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            var className = "C.V2." + mode + "Linker";
            this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<ILinkerPolicy>.Create(className);
        }

        public override void Evaluate()
        {
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