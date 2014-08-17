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
#endregion
namespace Test15
{
    public class StaticLibrary1 :
        C.StaticLibrary
    {
        public
        StaticLibrary1()
        {
            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headers.Include(includeDir, "*.h");
        }

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.c");
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            [C.ExportCompilerOptionsDelegate]
            void
            SourceFiles_UpdateOptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var cOptions = module.Options as C.ICCompilerOptions;
                if (null != cOptions)
                {
                    cOptions.IncludePaths.Include(this.PackageLocation, "include");
                }
            }
        }

        [C.HeaderFiles]
        Bam.Core.FileCollection headers = new Bam.Core.FileCollection();

        [Bam.Core.SourceFiles]
        SourceFiles source = new SourceFiles();
    }
}
