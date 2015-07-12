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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace Test17
{
    public sealed class ApplicationV2 :
        C.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/main.c");

            // TODO: this is missing the automatic link dependency on StaticLibrary1V2
            var lib = this.LinkAgainst<Test16.StaticLibrary2V2>();
            source.UsePublicPatches(lib);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                var windowsSDK = Bam.Core.V2.Graph.Instance.FindReferencedModule<WindowsSDK.WindowsSDKV2>();
                this.Requires(windowsSDK);
                this.UsePublicPatches(windowsSDK); // linking
            }
        }
    }

    public class Application :
        C.Application
    {
        public class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.c");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(Test16.StaticLibrary2)
            );

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows)]
        Bam.Core.TypeArray winDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
            );
    }
}
