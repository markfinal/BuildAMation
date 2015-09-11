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
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.InputPath = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/main.c", this);
            this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<Mingw.V2.Compiler32>();
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

    sealed class CompileSingleCFileWithCustomOptionsV2 :
        C.V2.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.InputPath = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/main.c", this);
            this.PrivatePatch(settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.DebugSymbols = false;
            });
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
