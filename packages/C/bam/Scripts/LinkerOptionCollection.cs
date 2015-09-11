#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace C
{
    public abstract class LinkerOptionCollection :
        Bam.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;

            var target = node.Target;

            var linkerOptions = this as ILinkerOptions;
            linkerOptions.OutputType = ELinkerOutput.Executable;
            linkerOptions.SubSystem = ESubsystem.NotSet;
            linkerOptions.DoNotAutoIncludeStandardLibraries = false;
            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                linkerOptions.DebugSymbols = true;
            }
            else
            {
                if (!target.HasConfiguration(Bam.Core.EConfiguration.Profile))
                {
                    linkerOptions.DebugSymbols = false;
                }
                else
                {
                    linkerOptions.DebugSymbols = true;
                }
            }
            linkerOptions.LibraryPaths = new Bam.Core.DirectoryCollection();
            linkerOptions.GenerateMapFile = true;
            linkerOptions.Libraries = new Bam.Core.FileCollection();
            linkerOptions.StandardLibraries = new Bam.Core.FileCollection();

            var osxLinkerOptions = this as ILinkerOptionsOSX;
            if (osxLinkerOptions != null)
            {
                osxLinkerOptions.Frameworks = new Bam.Core.StringArray();
                osxLinkerOptions.FrameworkSearchDirectories = new Bam.Core.DirectoryCollection();
                osxLinkerOptions.SuppressReadOnlyRelocations = false;
            }

            linkerOptions.MajorVersion = 1;
            linkerOptions.MinorVersion = 0;
            linkerOptions.PatchVersion = 0;
            linkerOptions.AdditionalOptions = "";
        }

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            var moduleBuildDir = this.GetModuleLocation(Bam.Core.State.ModuleBuildDirLocationKey);
            var linkerTool = node.Target.Toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;

            var outputDir = this.GetModuleLocation(C.Application.OutputDir);
            if (!outputDir.IsValid)
            {
                var linkerOutputDir = moduleBuildDir.SubDirectory(linkerTool.BinaryOutputSubDirectory);
                (outputDir as Bam.Core.ScaffoldLocation).SetReference(linkerOutputDir);
            }

            // special case here of the QMakeBuilder
            // QMake does not support writing import libraries to a separate location to the dll
            var importLibraryDir = this.GetModuleLocationSafe(C.DynamicLibrary.ImportLibraryDir);
            if ((null != importLibraryDir) && !importLibraryDir.IsValid)
            {
                if (linkerTool is IWinImportLibrary && (Bam.Core.State.BuildMode != "QMake"))
                {
                    var moduleDir = moduleBuildDir.SubDirectory((linkerTool as IWinImportLibrary).ImportLibrarySubDirectory);
                    (importLibraryDir as Bam.Core.ScaffoldLocation).SetReference(moduleDir);
                }
                else
                {
                    (importLibraryDir as Bam.Core.ScaffoldLocation).SetReference(outputDir);
                }
            }

            base.SetNodeSpecificData(node);
        }

        public
        LinkerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        public string OutputName
        {
            get;
            set;
        }

        private static void
        GetBinaryPrefixAndSuffix(
            ILinkerOptions options,
            ILinkerTool tool,
            out string prefix,
            out string suffix)
        {
            switch (options.OutputType)
            {
                case ELinkerOutput.Executable:
                    prefix = string.Empty;
                    suffix = tool.ExecutableSuffix;
                    break;

                case ELinkerOutput.DynamicLibrary:
                    prefix = tool.DynamicLibraryPrefix;
                    suffix = tool.DynamicLibrarySuffix;
                    break;

                default:
                    throw new Bam.Core.Exception("Unknown output type");
            }
        }

        private string
        GetExecutableFilename(
            Bam.Core.Target target,
            ILinkerTool linkerTool,
            ILinkerOptions linkerOptions)
        {
            string prefix;
            string suffix;
            GetBinaryPrefixAndSuffix(linkerOptions, linkerTool, out prefix, out suffix);

            string filename = null;
            if (linkerOptions.OutputType == ELinkerOutput.DynamicLibrary)
            {
                if (target.HasPlatform(Bam.Core.EPlatform.Unix))
                {
                    // version number postfixes the filename
                    var versionNumber = new System.Text.StringBuilder();
                    versionNumber.AppendFormat(".{0}.{1}.{2}", linkerOptions.MajorVersion, linkerOptions.MinorVersion, linkerOptions.PatchVersion);
                    filename = prefix + this.OutputName + suffix + versionNumber.ToString();
                }
                else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    // major version number is prior to the extension
                    filename = prefix + this.OutputName + "." + linkerOptions.MajorVersion.ToString() + suffix;
                }
            }
            if (null == filename)
            {
                filename = prefix + this.OutputName + suffix;
            }
            return filename;
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var target = node.Target;
            var linkerTool = target.Toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;
            var options = this as ILinkerOptions;

            var locationMap = node.Module.Locations;
            var outputFile = locationMap[C.Application.OutputFile];
            if (!outputFile.IsValid)
            {
                var filename = this.GetExecutableFilename(target, linkerTool, options);
                (outputFile as Bam.Core.ScaffoldLocation).SpecifyStub(locationMap[C.Application.OutputDir], filename, Bam.Core.Location.EExists.WillExist);
            }

            if (node.Module is C.DynamicLibrary && linkerTool is IWinImportLibrary)
            {
                var importLibraryFile = locationMap[C.DynamicLibrary.ImportLibraryFile] as Bam.Core.ScaffoldLocation;
                if (!importLibraryFile.IsValid)
                {
                    // explicit import library
                    var importLibrary = linkerTool as IWinImportLibrary;
                    var filename = importLibrary.ImportLibraryPrefix + this.OutputName + importLibrary.ImportLibrarySuffix;
                    importLibraryFile.SpecifyStub(locationMap[C.DynamicLibrary.ImportLibraryDir], filename, Bam.Core.Location.EExists.WillExist);
                }
            }

            if (options.GenerateMapFile)
            {
                if (!locationMap[C.Application.MapFileDir].IsValid)
                {
                    (locationMap[C.Application.MapFileDir] as Bam.Core.ScaffoldLocation).SetReference(locationMap[C.Application.OutputDir]);
                }

                if (!locationMap[C.Application.MapFile].IsValid)
                {
                    (locationMap[C.Application.MapFile] as Bam.Core.ScaffoldLocation).SpecifyStub(locationMap[C.Application.MapFileDir], this.OutputName + linkerTool.MapFileSuffix, Bam.Core.Location.EExists.WillExist);
                }
            }
        }

        void
        CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(
            Bam.Core.StringArray commandLineBuilder,
            Bam.Core.Target target,
            Bam.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}
