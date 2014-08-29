#region License
// Copyright 2010-2014 Mark Final
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
namespace HeaderLibraryTest
{
    class HeaderLibrary :
        C.HeaderLibrary
    {
        public
        HeaderLibrary()
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headers.Include(includeDir, "*.h");

            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(HeaderLibrary_IncludePaths);
        }

        [C.ExportCompilerOptionsDelegate]
        void
        HeaderLibrary_IncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ICCompilerOptions;
            options.IncludePaths.Include(this.PackageLocation, "include");
        }

        [C.HeaderFiles]
        Bam.Core.FileCollection headers = new Bam.Core.FileCollection();
    }

    class Application :
        C.Application
    {
        public
        Application()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "main.c");
        }

        [Bam.Core.SourceFiles]
        C.Cxx.ObjectFile sourceFile = new C.Cxx.ObjectFile();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(HeaderLibrary)
            );

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
            );
    }
}
