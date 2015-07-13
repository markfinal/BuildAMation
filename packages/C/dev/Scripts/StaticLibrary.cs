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
    public class StaticLibrary :
        CModule
    {
        private Bam.Core.Array<Bam.Core.V2.Module> source = new Bam.Core.Array<Bam.Core.V2.Module>();
        private Bam.Core.Array<Bam.Core.V2.Module> forwardedDeps = new Bam.Core.Array<Bam.Core.V2.Module>();
        private ILibrarianPolicy Policy = null;

        static public Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("Static Library File");

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.Librarian = DefaultToolchain.Librarian(this.BitDepth);
            this.RegisterGeneratedFile(Key, Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(moduleoutputdir)/$(libprefix)$(OutputName)$(libext)", this));
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> Source
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.source.ToArray());
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> ForwardedStaticLibraries
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.forwardedDeps.ToArray());
            }
        }

        public CObjectFileCollection CreateCSourceContainer()
        {
            var source = Bam.Core.V2.Module.Create<CObjectFileCollection>();
            this.source.Add(source);
            this.DependsOn(source);
            return source;
        }

        public Cxx.V2.ObjectFileCollection CreateCxxSourceContainer(string wildcardPath = null)
        {
            var source = Bam.Core.V2.Module.Create<Cxx.V2.ObjectFileCollection>();
            this.source.Add(source);
            this.DependsOn(source);
            return source;
        }

        public void
        CompileAgainst<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            if (0 == affectedSources.Length)
            {
                throw new Bam.Core.Exception("At least one source must be provided");
            }
            // no graph dependency, as it's just using patches
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.forwardedDeps.AddUnique(dependent);
            foreach (var source in affectedSources)
            {
                source.UsePublicPatches(dependent);
            }
        }

        public LibrarianTool Librarian
        {
            get
            {
                return this.Tool as LibrarianTool;
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
            var source = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.source.ToArray());
            var libraryFile = this.GeneratedPaths[Key];
            this.Policy.Archive(this, context, libraryFile, source);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            var className = "C.V2." + mode + "Librarian";
            this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<ILibrarianPolicy>.Create(className);
        }

        public override void Evaluate()
        {
            foreach (var source in this.source)
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
    /// C/C++ static library
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(IArchiverTool))]
    public class StaticLibrary :
        Bam.Core.BaseModule,
        Bam.Core.INestedDependents,
        Bam.Core.IForwardDependenciesOn,
        Bam.Core.ICommonOptionCollection
    {
        public static readonly Bam.Core.LocationKey OutputFileLocKey = new Bam.Core.LocationKey("StaticLibraryFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDirLocKey = new Bam.Core.LocationKey("StaticLibraryOutputDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        private string PreprocessorDefine
        {
            get;
            set;
        }

        [ExportCompilerOptionsDelegate]
        protected void
        StaticLibrarySetPreprocessorDefine(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (null == this.PreprocessorDefine)
            {
                var packageName = this.OwningNode.Package.Name.ToUpper();
                var moduleName = this.OwningNode.ModuleName.ToUpper();
                var preprocessorName = new System.Text.StringBuilder();
                preprocessorName.AppendFormat("D_{0}_{1}_STATICAPI", packageName, moduleName);
                this.PreprocessorDefine = preprocessorName.ToString();
            }

            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.Defines.Add(this.PreprocessorDefine);
        }

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

        Bam.Core.BaseOptionCollection Bam.Core.ICommonOptionCollection.CommonOptionCollection
        {
            get;
            set;
        }
    }
}
