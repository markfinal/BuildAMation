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
            linkerOptions.DynamicLibrary = false;
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
            var locationMap = this.OwningNode.Module.Locations;
            var moduleBuildDir = locationMap[Bam.Core.State.ModuleBuildDirLocationKey];

            var linkerTool = node.Target.Toolset.Tool(typeof(ILinkerTool)) as ILinkerTool;

            var outputDir = locationMap[C.Application.OutputDir];
            if (!outputDir.IsValid)
            {
                var linkerOutputDir = moduleBuildDir.SubDirectory(linkerTool.BinaryOutputSubDirectory);
                (outputDir as Bam.Core.ScaffoldLocation).SetReference(linkerOutputDir);
            }

            // special case here of the QMakeBuilder
            // QMake does not support writing import libraries to a separate location to the dll
            if (node.Module.Locations.Contains(C.DynamicLibrary.ImportLibraryDir))
            {
                var importLibDir = node.Module.Locations[C.DynamicLibrary.ImportLibraryDir];
                if (!importLibDir.IsValid)
                {
                    if (linkerTool is IWinImportLibrary && (Bam.Core.State.BuilderName != "QMake"))
                    {
                        var moduleDir = moduleBuildDir.SubDirectory((linkerTool as IWinImportLibrary).ImportLibrarySubDirectory);
                        (importLibDir as Bam.Core.ScaffoldLocation).SetReference(moduleDir);
                    }
                    else
                    {
                        (importLibDir as Bam.Core.ScaffoldLocation).SetReference(outputDir);
                    }
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
