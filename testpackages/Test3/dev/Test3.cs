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
namespace Test3
{
    // Define module classes here
    sealed class Library2 :
        C.StaticLibrary
    {
        public
        Library2()
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headerFiles.Include(includeDir, "*.h");
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target)
            {
                var options = module.Options as C.ArchiverOptionCollection;
                if (null != options)
                {
                    options.OutputName = "FooBar";
                }
            };
        }

        sealed class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "library2.c");
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
}
