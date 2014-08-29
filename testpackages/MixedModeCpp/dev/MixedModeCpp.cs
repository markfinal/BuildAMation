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
namespace MixedModeCpp
{
    // Define module classes here
    [Bam.Core.ModuleTargets(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
    class TestApplication :
        C.Application
    {
        public
        TestApplication()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(TestApplication_UpdateOptions);
        }

        void
        TestApplication_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            options.DoNotAutoIncludeStandardLibraries = false;
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "native.cpp");
            }
        }

        class ManagedSourceFiles :
            VisualCCommon.ManagedCxxObjectFileCollection
        {
            public
            ManagedSourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "managed.cpp");
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles nativeSourceFiles = new SourceFiles();

        [Bam.Core.SourceFiles]
        ManagedSourceFiles managedSourceFiles = new ManagedSourceFiles();

        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependentModules = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "mscoree.lib"
        );
    }
}
