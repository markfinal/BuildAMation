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
namespace Direct3DTriangle
{
    // Define module classes here
    [Bam.Core.ModuleTargets(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
    class D3D9TriangleTest :
        C.WindowsApplication
    {
        public
        D3D9TriangleTest()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.headerFiles.Include(sourceDir, "*.h");
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.cpp");

                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_VCDefines);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(SourceFiles_EnableException);
            }

            void
            SourceFiles_EnableException(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
            }

            void
            SourceFiles_VCDefines(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                if (module.Options is VisualCCommon.ICCompilerOptions)
                {
                    var compilerOptions = module.Options as C.ICCompilerOptions;
                    compilerOptions.Defines.Add("_CRT_SECURE_NO_WARNINGS");
                }
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

#if USE_SEPARATE_DXSDK
        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(DirectXSDK.Direct3D9),
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray winVCLibraries = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "USER32.lib",
            "DxErr.lib"
        );
#else // USE_SEPARATE_DXSDK
        [Bam.Core.DependentModules]
        Bam.Core.TypeArray dependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK),
            // the following require the Windows 8.1 SDK
            typeof(WindowsSDK.Direct3D9),
            typeof(WindowsSDK.Direct3DShaderCompiler)
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray winVCLibraries = new Bam.Core.StringArray(
            "KERNEL32.lib",
            "USER32.lib"
        );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        private Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.Application.OutputFile));
#endif
#endif // USE_SEPARATE_DXSDK
    }

#if !USE_SEPARATE_DXSDK
#if D_PACKAGE_PUBLISHER_DEV
    class Publish :
        Publisher.ProductModule
    {
        [Publisher.PrimaryTarget]
        System.Type primary = typeof(D3D9TriangleTest);
    }
#endif
#endif
}
