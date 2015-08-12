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
namespace Test16
{
    public sealed class StaticLibrary2V2 :
        C.V2.StaticLibrary
    {
        private Bam.Core.V2.Module.PublicPatchDelegate includePath = (settings, appliedTo) =>
        {
            var compiler = settings as C.V2.ICommonCompilerOptions;
            if (null != compiler)
            {
                compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", appliedTo));
            }
        };

        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var headers = this.CreateHeaderContainer();
            headers.AddFile("$(pkgroot)/include/staticlibrary2.h");

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/staticlibrary2.c");
            source.PrivatePatch(settings => this.includePath(settings, this));

            this.PublicPatch((settings, appliedTo) => this.includePath(settings, this));

            this.CompileAgainst<Test15.StaticLibrary1V2>(source);
        }
    }

    public class StaticLibrary2 :
        C.StaticLibrary
    {
        public
        StaticLibrary2()
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

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(Test15.StaticLibrary1)
            );
    }
}
