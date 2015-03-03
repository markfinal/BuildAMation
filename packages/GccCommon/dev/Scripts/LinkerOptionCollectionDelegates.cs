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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ILinkerOptions.cs&../../../C/dev/Scripts/ILinkerOptionsOSX.cs&ILinkerOptions.cs
//     -n=GccCommon
//     -c=LinkerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace GccCommon
{
    public partial class LinkerOptionCollection
    {
        static bool EnableXcodeBundleGeneration = false;

        #region C.ILinkerOptions Option delegates
        private static void
        OutputTypeCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<C.ELinkerOutput>;
            var options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                    {
                        var outputPath = options.GetModuleLocation(C.Application.OutputFile).GetSinglePath();
                        // TODO: isn't there an option for this on the tool?
                        commandLineBuilder.Add(System.String.Format("-o {0}", outputPath));
                    }
                    break;
                case C.ELinkerOutput.DynamicLibrary:
                    {
                        var outputPath = options.GetModuleLocation(C.Application.OutputFile).GetSinglePath();
                        // TODO: isn't there an option for this on the tool?
                        commandLineBuilder.Add(System.String.Format("-o {0}", outputPath));
                        // TODO: this option needs to be pulled out of the common output type option
                        // TODO: this needs more work, re: revisions
                        // see http://tldp.org/HOWTO/Program-Library-HOWTO/shared-libraries.html
                        // see http://www.adp-gmbh.ch/cpp/gcc/create_lib.html
                        // see http://lists.apple.com/archives/unix-porting/2003/Oct/msg00032.html
                        if (Bam.Core.OSUtilities.IsUnixHosting)
                        {
                            commandLineBuilder.Add("-shared");
                            var leafname = System.IO.Path.GetFileName(outputPath);
                            var splitLeafName = leafname.Split('.');
                            // index 0: filename without extension
                            // index 1: 'so'
                            // index 2: major version number
                            var soName = System.String.Format("{0}.{1}.{2}", splitLeafName[0], splitLeafName[1], splitLeafName[2]);
                            commandLineBuilder.Add(System.String.Format("-Wl,-soname,{0}", soName));
                        }
                        else if (Bam.Core.OSUtilities.IsOSXHosting)
                        {
                            var isPlugin = (sender as Bam.Core.BaseOptionCollection).IsFromModuleType<C.Plugin>();
                            // TODO: revisit Plugins
                            if (isPlugin && EnableXcodeBundleGeneration)
                            {
                                commandLineBuilder.Add("-bundle");
                            }
                            else
                            {
                                commandLineBuilder.Add("-dynamiclib");
                                var filename = System.IO.Path.GetFileName(outputPath);
                                commandLineBuilder.Add(System.String.Format("-Wl,-dylib_install_name,@executable_path/{0}", filename));
                                var linkerOptions = sender as C.ILinkerOptions;
                                commandLineBuilder.Add(System.String.Format("-Wl,-current_version,{0}.{1}.{2}", linkerOptions.MajorVersion, linkerOptions.MinorVersion, linkerOptions.PatchVersion));
                                // TODO: this needs to have a proper option
                                commandLineBuilder.Add(System.String.Format("-Wl,-compatibility_version,{0}.{1}.{2}", linkerOptions.MajorVersion, linkerOptions.MinorVersion, linkerOptions.PatchVersion));
                            }
                        }
                    }
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }
        private static void
        OutputTypeXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var enumOption = option as Bam.Core.ValueTypeOption<C.ELinkerOutput>;
            if (enumOption.Value != C.ELinkerOutput.DynamicLibrary)
            {
                return;
            }
            var machoTypeOption = configuration.Options["MACH_O_TYPE"];
            var isPlugin = (sender as Bam.Core.BaseOptionCollection).IsFromModuleType<C.Plugin>();
            if (isPlugin && EnableXcodeBundleGeneration)
            {
                machoTypeOption.AddUnique("mh_bundle");
                return;
            }
            machoTypeOption.AddUnique("mh_dylib");
            var options = sender as LinkerOptionCollection;
            {
                var installNameOption = configuration.Options["LD_DYLIB_INSTALL_NAME"];
                var outputPath = options.GetModuleLocation(C.Application.OutputFile).GetSinglePath();
                var filename = System.IO.Path.GetFileName(outputPath);
                var installName = System.String.Format("@executable_path/{0}", filename);
                installNameOption.AddUnique(installName);
            }
            var linkerOptions = sender as C.ILinkerOptions;
            {
                var currentVersionOption = configuration.Options["DYLIB_CURRENT_VERSION"];
                var version = System.String.Format("{0}.{1}.{2}", linkerOptions.MajorVersion, linkerOptions.MinorVersion, linkerOptions.PatchVersion);
                currentVersionOption.AddUnique(version);
            }
            {
                var compatibilityVersionOption = configuration.Options["DYLIB_COMPATIBILITY_VERSION"];
                var version = System.String.Format("{0}.{1}.{2}", linkerOptions.MajorVersion, linkerOptions.MinorVersion, linkerOptions.PatchVersion);
                compatibilityVersionOption.AddUnique(version);
            }
        }
        private static void
        DoNotAutoIncludeStandardLibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var ignoreStandardLibrariesOption = option as Bam.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-nostdlib");
            }
        }
        private static void
        DoNotAutoIncludeStandardLibrariesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var ignoreStandardLibs = option as Bam.Core.ValueTypeOption<bool>;
            var linkWithStandardLibsOption = configuration.Options["LINK_WITH_STANDARD_LIBRARIES"];
            if (ignoreStandardLibs.Value)
            {
                linkWithStandardLibsOption.AddUnique("NO");
            }
            else
            {
                linkWithStandardLibsOption.AddUnique("YES");
            }
            if (linkWithStandardLibsOption.Count != 1)
            {
                throw new Bam.Core.Exception("More than one ignore standard libraries option has been set");
            }
        }
        private static void
        DebugSymbolsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var debugSymbolsOption = option as Bam.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
        }
        private static void
        DebugSymbolsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var debugSymbols = option as Bam.Core.ValueTypeOption<bool>;
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            if (debugSymbols.Value)
            {
                otherLDOptions.AddUnique("-g");
            }
        }
        private static void
        SubSystemCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // empty
        }
        private static void
        SubSystemXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // empty
        }
        private static void
        LibraryPathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var libraryPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to var
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                if (libraryPath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-L\"{0}\"", libraryPath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-L{0}", libraryPath));
                }
            }
        }
        private static void
        LibraryPathsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var libraryPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            var librarySearchPathsOption = configuration.Options["LIBRARY_SEARCH_PATHS"];
            librarySearchPathsOption.AddRangeUnique(libraryPathsOption.Value.ToStringArray());
        }
        private static void
        StandardLibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var librariesOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            // TODO: change to var, and returning Locations
            foreach (Bam.Core.Location libraryPath in librariesOption.Value)
            {
                commandLineBuilder.Add(libraryPath.GetSinglePath());
            }
        }
        private static void
        StandardLibrariesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // empty
        }
        private static void
        LibrariesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var librariesOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.FileCollection>;
            // TODO: change to var, and returning Locations
            foreach (Bam.Core.Location libraryPath in librariesOption.Value)
            {
                commandLineBuilder.Add(libraryPath.GetSinglePath());
            }
        }
        private static void
        LibrariesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // empty
        }
        private static void
        GenerateMapFileCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                //var options = sender as LinkerOptionCollection;
                var mapFileLoc = (sender as LinkerOptionCollection).GetModuleLocation(C.Application.MapFile);
                if (Bam.Core.OSUtilities.IsUnixHosting)
                {
                    commandLineBuilder.Add(System.String.Format("-Wl,-Map,{0}", mapFileLoc.GetSinglePath()));
                }
                else if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    commandLineBuilder.Add(System.String.Format("-Wl,-map,{0}", mapFileLoc.GetSinglePath()));
                }
            }
        }
        private static void
        GenerateMapFileXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var generateMapfile = option as Bam.Core.ValueTypeOption<bool>;
            if (generateMapfile.Value)
            {
                var mapFileLoc = (sender as LinkerOptionCollection).GetModuleLocation(C.Application.MapFile);
                var generateMapfileOption = configuration.Options["LD_MAP_FILE_PATH"];
                generateMapfileOption.AddUnique(mapFileLoc.GetSinglePath());
                if (generateMapfileOption.Count != 1)
                {
                    var message = new System.Text.StringBuilder();
                    message.AppendLine("More than one map file location option has been set");
                    foreach (var mapFile in generateMapfileOption)
                    {
                        message.AppendFormat("\t{0}\n", mapFile);
                    }
                    throw new Bam.Core.Exception(message.ToString());
                }
            }
        }
        private static void
        AdditionalOptionsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            foreach (var argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static void
        AdditionalOptionsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            foreach (var argument in arguments)
            {
                otherLDOptions.AddUnique(argument);
            }
        }
        #endregion
        #region C.ILinkerOptionsOSX Option delegates
        private static void
        FrameworksCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            if (!Bam.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }
            var stringArrayOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
            foreach (var framework in stringArrayOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-framework {0}", framework));
            }
        }
        private static void
        FrameworksXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var frameworks = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
            foreach (var framework in frameworks.Value)
            {
                var fileReference = project.FileReferences.Get(framework, XcodeBuilder.PBXFileReference.EType.Framework, framework, null);
                var frameworksBuildPhase = project.FrameworksBuildPhases.Get("Frameworks", currentObject.Name);
                var buildFile = project.BuildFiles.Get(framework, fileReference, frameworksBuildPhase);
                if (null == buildFile)
                {
                    throw new Bam.Core.Exception("Build file not available");
                }
            }
        }
        private static void
        FrameworkSearchDirectoriesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var switchPrefix = "-F";
            var frameworkIncludePathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to 'var'
            foreach (string includePath in frameworkIncludePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", switchPrefix, includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("{0}{1}", switchPrefix, includePath));
                }
            }
        }
        private static void
        FrameworkSearchDirectoriesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var frameworkPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            configuration.Options["FRAMEWORK_SEARCH_PATHS"].AddRangeUnique(frameworkPathsOption.Value.ToStringArray());
        }
        private static void
        SuppressReadOnlyRelocationsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            if (!target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                return;
            }
            var readOnlyRelocations = option as Bam.Core.ValueTypeOption<bool>;
            if (readOnlyRelocations.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wl,-read_only_relocs,suppress"));
            }
        }
        private static void
        SuppressReadOnlyRelocationsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var readOnlyRelocations = option as Bam.Core.ValueTypeOption<bool>;
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            if (readOnlyRelocations.Value)
            {
                otherLDOptions.AddUnique("-Wl,-read_only_relocs,suppress");
            }
        }
        #endregion
        #region ILinkerOptions Option delegates
        private static void
        CanUseOriginCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // $ORIGIN not supported on OSX linkers - use install name, etc
            if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                return;
            }
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wl,-z,origin");
            }
        }
        private static void
        CanUseOriginXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // $ORIGIN not supported on OSX linkers - use install name, etc
            if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                return;
            }
            var useOrigin = option as Bam.Core.ValueTypeOption<bool>;
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            if (useOrigin.Value)
            {
                otherLDOptions.AddUnique("-Wl,-z,origin");
            }
        }
        private static void
        AllowUndefinedSymbolsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    // TODO: I did originally think suppress here, but there is an issue with that and 'two level namespaces'
                    commandLineBuilder.Add("-Wl,-undefined,dynamic_lookup");
                }
                else
                {
                    commandLineBuilder.Add("-Wl,-z,nodefs");
                }
            }
            else
            {
                if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    commandLineBuilder.Add("-Wl,-undefined,error");
                }
                else
                {
                    commandLineBuilder.Add("-Wl,-z,defs");
                }
            }
        }
        private static void
        AllowUndefinedSymbolsXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var allowUndefined = option as Bam.Core.ValueTypeOption<bool>;
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            if (allowUndefined.Value)
            {
                // TODO: I did originally think suppress here, but there is an issue with that and 'two level namespaces'
                otherLDOptions.AddUnique("-Wl,-undefined,dynamic_lookup");
            }
            else
            {
                otherLDOptions.AddUnique("-Wl,-undefined,error");
            }
        }
        private static void
        RPathCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
            foreach (string rpath in stringsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wl,-rpath,{0}", rpath));
            }
        }
        private static void
        RPathXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var rpathOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.StringArray>;
            configuration.Options["LD_RUNPATH_SEARCH_PATHS"].AddRangeUnique(rpathOption.Value);
        }
        private static void
        SixtyFourBitCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var sixtyFourBitOption = option as Bam.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Add("-m64");
            }
            else
            {
                commandLineBuilder.Add("-m32");
            }
        }
        private static void
        SixtyFourBitXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeXcodeProjectProcessor);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLineProcessor,DoNotAutoIncludeStandardLibrariesXcodeProjectProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor,DebugSymbolsXcodeProjectProcessor);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLineProcessor,SubSystemXcodeProjectProcessor);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLineProcessor,LibraryPathsXcodeProjectProcessor);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLineProcessor,StandardLibrariesXcodeProjectProcessor);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLineProcessor,LibrariesXcodeProjectProcessor);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLineProcessor,GenerateMapFileXcodeProjectProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsXcodeProjectProcessor);
            // Property 'MajorVersion' is value only - no delegates
            // Property 'MinorVersion' is value only - no delegates
            // Property 'PatchVersion' is value only - no delegates
            this["Frameworks"].PrivateData = new PrivateData(FrameworksCommandLineProcessor,FrameworksXcodeProjectProcessor);
            this["FrameworkSearchDirectories"].PrivateData = new PrivateData(FrameworkSearchDirectoriesCommandLineProcessor,FrameworkSearchDirectoriesXcodeProjectProcessor);
            this["SuppressReadOnlyRelocations"].PrivateData = new PrivateData(SuppressReadOnlyRelocationsCommandLineProcessor,SuppressReadOnlyRelocationsXcodeProjectProcessor);
            this["CanUseOrigin"].PrivateData = new PrivateData(CanUseOriginCommandLineProcessor,CanUseOriginXcodeProjectProcessor);
            this["AllowUndefinedSymbols"].PrivateData = new PrivateData(AllowUndefinedSymbolsCommandLineProcessor,AllowUndefinedSymbolsXcodeProjectProcessor);
            this["RPath"].PrivateData = new PrivateData(RPathCommandLineProcessor,RPathXcodeProjectProcessor);
            this["SixtyFourBit"].PrivateData = new PrivateData(SixtyFourBitCommandLineProcessor,SixtyFourBitXcodeProjectProcessor);
        }
    }
}
