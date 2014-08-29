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
namespace Test6
{
    // Define module classes here
    class ConditionalApplication :
        C.Application
    {
        // TODO: derive C.SourceFiles from this attribute?
        [Bam.Core.SourceFiles]
        private SourceFiles sourceFiles;

        public
        ConditionalApplication(
            Bam.Core.Target target)
        {
            this.sourceFiles = new SourceFiles(target);
            this.UpdateOptions += this.OverrideOptionCollection;
        }

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles(
                Bam.Core.Target target)
            {
                this.UpdateOptions += this.OverrideOptionCollection;

                var sourceDir = this.PackageLocation.SubDirectory("source");
                var debugSourceDir = sourceDir.SubDirectory("debug");
                var optSourceDir = sourceDir.SubDirectory("optimized");

                var mainObjectFile = new C.ObjectFile();
                mainObjectFile.Include(sourceDir, "main.c");
                mainObjectFile.UpdateOptions += MainUpdateOptionCollection;
                this.Add(mainObjectFile);

                if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                {
                    this.Include(debugSourceDir, "debug.c");
                }
                else
                {
                    this.Include(optSourceDir, "optimized.c");
                }
            }

            private void
            OverrideOptionCollection(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.PackageLocation.SubDirectory("include"));
            }

            private void
            MainUpdateOptionCollection(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.Defines.Add("MAIN_C");
                var includeDir = this.PackageLocation.SubDirectory("include");
                compilerOptions.IncludePaths.Include(includeDir, "platform");
            }
        }

        private void
        OverrideOptionCollection(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            //var options = module.Options as C.ILinkerOptions;
            //options.DebugSymbols = false;
        }

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray libraries = new Bam.Core.StringArray("KERNEL32.lib");
    }
}
