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
namespace CodeGenTest2
{
    // Define module classes here
    class TestAppGeneratedSource : CodeGenModule
    {
        public TestAppGeneratedSource()
        {
            //this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(TestAppGeneratedSource_UpdateOptions);
        }

#if false
        void TestAppGeneratedSource_UpdateOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            CodeGenOptions options = module.Options as CodeGenOptions;
        }
#endif
    }

    class TestApp : C.Application
    {
        public TestApp()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(TestApp_UpdateOptions);
        }

        void TestApp_UpdateOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            C.ILinkerOptions options = module.Options as C.ILinkerOptions;
            options.DoNotAutoIncludeStandardLibraries = false;
        }

        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var testAppDir = sourceDir.SubDirectory("testapp");
                this.Include(testAppDir, "main.c");
            }

            [Bam.Core.DependentModules]
            Bam.Core.TypeArray vcDependencies = new Bam.Core.TypeArray(typeof(TestAppGeneratedSource));
        }

        [Bam.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray vcDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
