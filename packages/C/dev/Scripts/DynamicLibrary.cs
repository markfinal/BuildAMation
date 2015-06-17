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
    public class DynamicLibrary :
        ConsoleApplication
    {
        static public Bam.Core.V2.FileKey ImportLibraryKey = Bam.Core.V2.FileKey.Generate("Import Library File");

        public DynamicLibrary()
        {
            this.PrivatePatch(setting =>
                {
                    var linker = setting as C.V2.ICommonLinkerOptions;
                    if (null != linker)
                    {
                        linker.OutputType = ELinkerOutput.DynamicLibrary;
                    }
                });
        }

        protected override void
        Init()
        {
            base.Init();
            this.GeneratedPaths[Key] = Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(moduleoutputdir)/$(modulename)$(dynamicext)", this);
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                this.RegisterGeneratedFile(ImportLibraryKey, Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(moduleoutputdir)/$(libprefix)$(modulename)$(libext)", this));
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> Source
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.sourceModules);
            }
        }

        public override CObjectFileCollection
        CreateCSourceContainer()
        {
            var collection = base.CreateCSourceContainer();
            collection.PrivatePatch(setting =>
            {
                var compiler = setting as C.V2.ICommonCompilerOptions;
                compiler.PreprocessorDefines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
            });
            return collection;
        }

        public override Cxx.V2.ObjectFileCollection
        CreateCxxSourceContainer(string wildcardPath)
        {
            var collection = base.CreateCxxSourceContainer(wildcardPath);
            collection.PrivatePatch(setting =>
            {
                var compiler = setting as C.V2.ICommonCompilerOptions;
                compiler.PreprocessorDefines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
            });
            return collection;
        }
    }
}
    /// <summary>
    /// C/C++ dynamic library
    /// </summary>
    public partial class DynamicLibrary :
        Application,
        Bam.Core.IPostActionModules
    {
        public static readonly Bam.Core.LocationKey ImportLibraryFile = new Bam.Core.LocationKey("ImportLibraryFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey ImportLibraryDir = new Bam.Core.LocationKey("ImportLibraryDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        protected
        DynamicLibrary()
        {
            this.PostActionModuleTypes = new Bam.Core.TypeArray();
            if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                this.PostActionModuleTypes.Add(typeof(PosixSharedLibrarySymlinks));
            }
        }

        [LocalCompilerOptionsDelegate]
        protected static void
        DynamicLibrarySetDLLBuildPreprocessorDefine(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as ICCompilerOptions;
            compilerOptions.Defines.Add("D_BAM_DYNAMICLIBRARY_BUILD");
        }

        [LocalLinkerOptionsDelegate]
        protected static void
        DynamicLibraryEnableDLL(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.DynamicLibrary;

            if (module.Options is ILinkerOptionsOSX && target.HasPlatform(Bam.Core.EPlatform.OSX32))
            {
                // only required for 32-bit builds
                (module.Options as ILinkerOptionsOSX).SuppressReadOnlyRelocations = true;
            }
        }

        public Bam.Core.TypeArray PostActionModuleTypes
        {
            get;
            private set;
        }

        #region IPostActionModules Members

        Bam.Core.TypeArray
        Bam.Core.IPostActionModules.GetPostActionModuleTypes(
            Bam.Core.BaseTarget target)
        {
            return this.PostActionModuleTypes;
        }

        #endregion
    }
}
