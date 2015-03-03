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
namespace Test9
{
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
