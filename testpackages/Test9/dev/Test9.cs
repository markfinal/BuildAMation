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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace Test9
{
    sealed class CFileV2 :
        C.V2.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.InputPath = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/main_c.c", this);
        }
    }

    sealed class CFileCollectionV2 :
        C.V2.CObjectFileCollection
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.AddFile("$(pkgroot)/source/main_c.c");
        }
    }

    sealed class CppFileV2 :
        C.Cxx.V2.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.InputPath = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/main_cpp.c", this);
            this.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICxxOnlyCompilerOptions;
                    compiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });
        }
    }

    // Note: Uses the C++ application module, in order to use the C++ linker, in order to link in C++ runtimes
    sealed class MixedLanguageApplicationV2 :
        C.Cxx.V2.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/include/*.h");

            var cSource = this.CreateCSourceContainer("$(pkgroot)/source/library_c.c");
            cSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));
                });

            var cxxSource = this.CreateCxxSourceContainer();
            cxxSource.AddFile("$(pkgroot)/source/library_cpp.c");
            cxxSource.AddFile("$(pkgroot)/source/appmain_cpp.c");
            cxxSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));
                    var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    sealed class CStaticLibraryFromCollectionV2 :
        C.V2.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/include/library_c.h");

            var source = this.CreateCSourceContainer("$(pkgroot)/source/library_c.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));
                });
        }
    }

    sealed class CppStaticLibaryFromCollectionV2 :
        C.V2.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/include/library_cpp.h");

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/library_cpp.c");
            source.PrivatePatch(settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));

                var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            });
        }
    }

    sealed class CDynamicLibraryFromCollectionV2 :
        C.V2.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/include/library_c.h");

            var source = this.CreateCSourceContainer("$(pkgroot)/source/library_c.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    sealed class CppDynamicLibaryFromCollectionV2 :
        C.V2.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(pkgroot)/include/library_cpp.h");

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/library_cpp.c");
            source.PrivatePatch(settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", this));

                var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    // Define module classes here
    class CFile :
        C.ObjectFile
    {
        public
        CFile()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "main_c.c");
        }
    }

    class CFileCollection :
        C.ObjectFileCollection
    {
        public
        CFileCollection()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "main_c.c");
        }
    }

    class CppFile :
        C.Cxx.ObjectFile
    {
        public
        CppFile()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "main_cpp.c");
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(CppFile_UpdateOptions);
        }

        void
        CppFile_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
        }
    }

    class CppFileCollection :
        C.Cxx.ObjectFileCollection
    {
        public
        CppFileCollection()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.Include(sourceDir, "main_cpp.c");

            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(CppFileCollection_UpdateOptions);
        }

        void
        CppFileCollection_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
        }
    }

    class MixedLanguageApplication :
        C.Application
    {
        public
        MixedLanguageApplication()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SetSystemLibraries);
        }

        static void
        SetSystemLibraries(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                if (linkerOptions is VisualC.LinkerOptionCollection)
                {
                    linkerOptions.Libraries.Add("KERNEL32.lib");
                }
            }
        }

        class CSourceFiles :
            C.ObjectFileCollection
        {
            public
            CSourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library_c.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(IncludePaths);
            }

            void
            IncludePaths(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
            }
        }

        class CxxSourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            CxxSourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library_cpp.c");
                this.Include(sourceDir, "appmain_cpp.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(CxxSourceFiles_UpdateOptions);
                this.UpdateOptions += IncludePaths;
            }

            void
            IncludePaths(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
            }

            void
            CxxSourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            }
        }

        [Bam.Core.SourceFiles]
        CSourceFiles cSourceFiles = new CSourceFiles();
        [Bam.Core.SourceFiles]
        CxxSourceFiles cppSourceFiles = new CxxSourceFiles();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }

    class CStaticLibraryFromFile :
        C.StaticLibrary
    {
        public
        CStaticLibraryFromFile()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "library_c.c");
            this.sourceFile.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
        }

        void
        sourceFile_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();
    }

    class CStaticLibraryFromCollection :
        C.StaticLibrary
    {
        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library_c.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();
    }

    class CppStaticLibraryFromFile :
        C.StaticLibrary
    {
        public
        CppStaticLibraryFromFile()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "library_cpp.c");
            this.sourceFile.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
            this.sourceFile.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(sourceFile_ExceptionHandling);
        }

        void
        sourceFile_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        void
        sourceFile_ExceptionHandling(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
        }

        [Bam.Core.SourceFiles]
        C.Cxx.ObjectFile sourceFile = new C.Cxx.ObjectFile();
    }

    class CppStaticLibaryFromCollection :
        C.StaticLibrary
    {
        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library_cpp.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_ExceptionHandling);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
            }

            void
            SourceFiles_ExceptionHandling(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();
    }

    class CDynamicLibraryFromFile :
        C.DynamicLibrary
    {
        public
        CDynamicLibraryFromFile()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "library_c.c");
            this.sourceFile.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
        }

        void
        sourceFile_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }

    class CDynamicLibraryFromCollection :
        C.DynamicLibrary
    {
        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library_c.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }

    class CppDynamicLibraryFromFile :
        C.DynamicLibrary
    {
        public
        CppDynamicLibraryFromFile()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "library_cpp.c");
            this.sourceFile.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
            this.sourceFile.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(sourceFile_ExceptionHandling);
        }

        void
        sourceFile_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        void
        sourceFile_ExceptionHandling(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
        }

        [Bam.Core.SourceFiles]
        C.Cxx.ObjectFile sourceFile = new C.Cxx.ObjectFile();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }

    class CppDynamicLibaryFromCollection :
        C.DynamicLibrary
    {
        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library_cpp.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_ExceptionHandling);
            }

            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
            }

            void
            SourceFiles_ExceptionHandling(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }
}
