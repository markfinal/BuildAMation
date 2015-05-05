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
namespace Test2
{
    sealed class LibraryV2 :
        C.V2.StaticLibrary
    {
        public LibraryV2()
        {
            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/library.c");
        }
    }

    sealed class ApplicationV2 :
        C.V2.ConsoleApplication
    {
        public ApplicationV2()
        {
            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/application.c");
            source.PatchSettings(settings =>
                {
                    var cOnly = settings as C.V2.ICOnlyCompilerOptions;
                    cOnly.C99Specific = true;
                });
            this.LinkAgainst<LibraryV2>();
        }
    }

    static class BuildOutputDirHelper
    {
        public static void
        Change(
            Bam.Core.BaseModule module,
            Bam.Core.LocationKey key)
        {
            var output = module.Locations[key] as Bam.Core.ScaffoldLocation;
            var banana = module.Locations[Bam.Core.State.ModuleBuildDirLocationKey].SubDirectory("banana");
            output.SetReference(banana);
            //output.SpecifyStub(module.Locations[Bam.Core.State.BuildRootLocationKey], "banana", Bam.Core.Location.EExists.WillExist);
        }
    }

    // Define module classes here
    sealed class Library :
        C.StaticLibrary
    {
        public
        Library()
        {
            // TODO: want to share the LocationMap between all related modules
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headerFiles.Include(includeDir, "*.h");

            BuildOutputDirHelper.Change(this, C.StaticLibrary.OutputDirLocKey);
        }

        sealed class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library.c");
                this.UpdateOptions += SetIncludePaths;
            }

            [C.ExportCompilerOptionsDelegate]
            public void
            SetIncludePaths(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation.SubDirectory("include"));
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();
    }

    sealed class Application :
        C.Application
    {
        public
        Application()
        {
            BuildOutputDirHelper.Change(this, C.Application.OutputDir);
        }

        sealed class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "application.c");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(Library),
            typeof(Test3.Library2)
        );

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }
}
