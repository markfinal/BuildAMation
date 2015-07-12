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
namespace Test7
{
    sealed class ExplicitDynamicLibraryV2 :
        C.V2.DynamicLibrary
    {
        private Bam.Core.V2.Module.PublicPatchDelegate includePaths = (settings, appliedTo) =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                if (null == compiler)
                {
                    return;
                }
                compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgroot)/include", appliedTo));
            };

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = Bam.Core.V2.TokenizedString.Create("ExplicitDynamicLibrary", null);

            this.PublicPatch((settings, appliedTo) => this.includePaths(settings, this));

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/dynamiclibrary.c");
            source.PrivatePatch(settings => this.includePaths(settings, this));

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    // Define module classes here
    class ExplicitDynamicLibrary :
        C.DynamicLibrary
    {
        public
        ExplicitDynamicLibrary()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.sourceFile.Include(sourceDir, "dynamiclibrary.c");
            this.sourceFile.UpdateOptions += SetIncludePaths;

            var includeDir = this.PackageLocation.SubDirectory("include");
            this.headerFiles.Include(includeDir, "*.h");
        }

        [C.ExportCompilerOptionsDelegate]
        private void
        SetIncludePaths(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.PackageLocation, "include");
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [C.HeaderFiles]
        Bam.Core.FileCollection headerFiles = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows, ToolsetTypes=new[]{typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray(
            "KERNEL32.lib"
        );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }
}
