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
[assembly:Bam.Core.GlobalOptionCollectionOverride(typeof(Test.OptionOverride))]

namespace Test
{
    class OptionOverride :
        Bam.Core.IGlobalOptionCollectionOverride
    {
        public void
        OverrideOptions(
            Bam.Core.BaseOptionCollection optionCollection,
            Bam.Core.Target target)
        {
            if (optionCollection is C.ICCompilerOptions)
            {
                var compilerOptions = optionCollection as C.ICCompilerOptions;
                compilerOptions.Defines.Add("GLOBALOVERRIDE");
            }

            if (optionCollection is C.ICxxCompilerOptions)
            {
                var compilerOptions = optionCollection as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
            }

            // TODO: pdb support
#if false
            if (optionCollection is VisualCCommon.LinkerOptionCollection)
            {
                var linkerOptions = optionCollection as VisualCCommon.LinkerOptionCollection;
                linkerOptions.ProgamDatabaseDirectoryPath = System.IO.Path.Combine(Bam.Core.State.BuildRoot, "symbols");
            }
#endif
        }
    }

    sealed class CompileSingleCFileV2 :
        C.V2.ObjectFile
    {
        public CompileSingleCFileV2()
        {
            // TODO: can override the generated paths
            //this.RegisterGeneratedFile(C.V2.ObjectFile.Key, new Bam.Core.V2.TokenizedString("$(buildroot)/hello.obj", null));
            this.InputPath = new Bam.Core.V2.TokenizedString("$(pkgroot)/source/main.c", null);
        }
    }

    sealed class CompileSingleCFile :
        C.ObjectFile
    {
        public
        CompileSingleCFile()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "main.c");
        }
    }

    sealed class CompileSingleCFileWithCustomOptions :
        C.ObjectFile
    {
        public
        CompileSingleCFileWithCustomOptions()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "main.c");
            this.UpdateOptions += UpdateCompilerOptions;
        }

        private static void
        UpdateCompilerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;

            compilerOptions.ShowIncludes = true;

            if (target.HasToolsetType(typeof(Mingw.Toolset)))
            {
                Bam.Core.Log.MessageAll("Toolset for mingw in use");

                if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                {
                    compilerOptions.Optimization = C.EOptimization.Custom;
                }

                compilerOptions.AdditionalOptions = "-Wall";

                var mingwCompilerOptions = compilerOptions as MingwCommon.ICCompilerOptions;
                mingwCompilerOptions.InlineFunctions = true;
            }
            else if (target.HasToolsetType(typeof(VisualC.Toolset)))
            {
                Bam.Core.Log.MessageAll("Toolset for visualc in use");

                compilerOptions.Optimization = C.EOptimization.Custom;
                compilerOptions.CustomOptimization = "-Ox";
                compilerOptions.AdditionalOptions = "-openmp";

                compilerOptions.DebugSymbols = true;
                var vcCompilerOptions = compilerOptions as VisualCCommon.ICCompilerOptions;
                vcCompilerOptions.DebugType = VisualCCommon.EDebugType.Embedded;
                vcCompilerOptions.BasicRuntimeChecks = VisualCCommon.EBasicRuntimeChecks.None;
                vcCompilerOptions.SmallerTypeConversionRuntimeCheck = false;
            }
            else if (target.HasToolsetType(typeof(Gcc.Toolset)))
            {
                Bam.Core.Log.MessageAll("Toolset for gcc in use");

                compilerOptions.AdditionalOptions = "-Wall";

                var gccCompilerOptions = compilerOptions as GccCommon.ICCompilerOptions;
                gccCompilerOptions.PositionIndependentCode = true;
            }
            else if (target.HasToolsetType(typeof(LLVMGcc.Toolset)))
            {
                Bam.Core.Log.MessageAll("Toolset for llvm-gcc in use");

                compilerOptions.AdditionalOptions = "-Wall";

                GccCommon.ICCompilerOptions gccCompilerOptions = compilerOptions as GccCommon.ICCompilerOptions;
                gccCompilerOptions.PositionIndependentCode = true;
            }
            else if (target.HasToolsetType(typeof(Clang.Toolset)))
            {
                Bam.Core.Log.MessageAll("Toolset for clang in use");

                compilerOptions.AdditionalOptions = "-Wall";

                var clangCompilerOptions = compilerOptions as ClangCommon.ICCompilerOptions;
                clangCompilerOptions.PositionIndependentCode = true;
            }
            else
            {
                Bam.Core.Log.MessageAll("Unrecognized toolset, '{0}'", target.ToolsetName('='));
            }
        }
    }

    sealed class CompileCSourceCollection :
        C.ObjectFileCollection
    {
        public
        CompileCSourceCollection()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "*.c");
        }
    }

    sealed class CompileSingleCppFile :
        C.Cxx.ObjectFile
    {
        public
        CompileSingleCppFile()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "main.c");
        }
    }

    sealed class CompileCppSourceCollection :
        C.Cxx.ObjectFileCollection
    {
        public
        CompileCppSourceCollection()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "*.c");
        }
    }

    sealed class CompileCSourceCollectionWithCustomOptions :
        C.ObjectFileCollection
    {
        public
        CompileCSourceCollectionWithCustomOptions()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "*.c");

            this.UpdateOptions += OverrideOptionCollection;

            // override the options on one specific file
            this.RegisterUpdateOptions(new Bam.Core.UpdateOptionCollectionDelegateArray(mainObjFile_UpdateOptions),
                                       sourceDir, "main.c");
        }

        void
        mainObjFile_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.Defines.Add("DEFINE_FOR_MAIN_ONLY");
        }

        public void
        OverrideOptionCollection(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.ShowIncludes = true;
            compilerOptions.Defines.Add("DEFINE_FOR_ALL_SOURCE");
        }
    }

    sealed class BuildTerminalApplicationFromC :
        C.Application
    {
        sealed class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "main.c");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows, ToolsetTypes=new [] {typeof(VisualC.Toolset)})]
        Bam.Core.Array<System.Type> dependents = new Bam.Core.Array<System.Type>(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes=new[] { typeof(VisualC.Toolset)})]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }

    sealed class BuildTerminalApplicationFromCxx :
        C.Application
    {
        sealed class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "main.c");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows, ToolsetTypes=new[] {typeof(VisualC.Toolset)})]
        Bam.Core.Array<System.Type> dependents = new Bam.Core.Array<System.Type>(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }

    sealed class BuildTerminalApplicationWithUpdatedOptions :
        C.Application
    {
        sealed class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "main.c");

                this.UpdateOptions += OverrideOptionCollection;
            }

            private static void
            OverrideOptionCollection(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.ShowIncludes = true;
                compilerOptions.CharacterSet = C.ECharacterSet.NotSet;

                var vcOptions = compilerOptions as VisualC.CCompilerOptionCollection;
                if (null != vcOptions)
                {
                    //vcOptions.DebugType = VisualC.EDebugType.Embedded;
                }
                var mingwOptions = compilerOptions as Mingw.CCompilerOptionCollection;
                if (null != mingwOptions)
                {
                }
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.Array<System.Type> dependents = new Bam.Core.Array<System.Type>(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }

    [Bam.Core.ModuleTargets(Platform=Bam.Core.EPlatform.Windows)]
    sealed class BuildWindowsApplication :
        C.WindowsApplication
    {
        sealed class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "main.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Clang_CompilerOptions);
            }

            void
            Clang_CompilerOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                if (target.HasToolsetType(typeof(Clang.Toolset)))
                {
                    var cOptions = module.Options as C.ICCompilerOptions;
                    // Microsoft headers do not compile warning free with Clang
                    cOptions.WarningsAsErrors = false;
                }
            }
        }

        sealed class Win32ResourceFile :
            C.Win32Resource
        {
            public
            Win32ResourceFile()
            {
                var resourcesDir = this.PackageLocation.SubDirectory("resources");
                this.Include(resourcesDir, "win32.rc");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules(ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray vcDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
            );

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray mingwDependents = new Bam.Core.TypeArray(
            typeof(Win32ResourceFile)
            );

        [C.RequiredLibraries(ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }
}
