// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/ILinkerOptions.cs:ILinkerOptions.cs -n=GccCommon -c=LinkerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=PrivateData

namespace GccCommon
{
    public partial class LinkerOptionCollection
    {
        #region C.ILinkerOptions Option delegates
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                    {
                        string outputPathName = options.OutputFilePath;
                        if (outputPathName.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-o \"{0}\"", outputPathName));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-o {0}", outputPathName));
                        }
                    }
                    break;
                case C.ELinkerOutput.DynamicLibrary:
                    {
                        string outputPathName = options.OutputFilePath;
                        if (outputPathName.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-o \"{0}\"", outputPathName));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-o {0}", outputPathName));
                        }
                        // TODO: this needs more work, re: revisions
                        // see http://tldp.org/HOWTO/Program-Library-HOWTO/shared-libraries.html
                        // see http://www.adp-gmbh.ch/cpp/gcc/create_lib.html
                        // see http://lists.apple.com/archives/unix-porting/2003/Oct/msg00032.html
                        if (Opus.Core.OSUtilities.IsUnixHosting)
                        {
                            if (outputPathName.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("-Wl,-soname,\"{0}\"", outputPathName));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("-Wl,-soname,{0}", outputPathName));
                            }
                        }
                        else if (Opus.Core.OSUtilities.IsOSXHosting)
                        {
                            if (outputPathName.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("-Wl,-dylib_install_name,\"{0}\"", outputPathName));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("-Wl,-dylib_install_name,{0}", outputPathName));
                            }
                        }
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }
        private static void OutputTypeXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void DoNotAutoIncludeStandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-nostdlib");
            }
        }
        private static void DoNotAutoIncludeStandardLibrariesXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var ignoreStandardLibs = option as Opus.Core.ValueTypeOption<bool>;
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
                throw new Opus.Core.Exception("More than one ignore standard libraries option has been set");
            }
        }
        private static void DebugSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
        }
        private static void DebugSymbolsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var debugSymbols = option as Opus.Core.ValueTypeOption<bool>;
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            if (debugSymbols.Value)
            {
                otherLDOptions.AddUnique("-g");
            }
        }
        private static void SubSystemCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void SubSystemXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void DynamicLibraryCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    commandLineBuilder.Add("-shared");
                }
                else if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    commandLineBuilder.Add("-dynamiclib");
                }
            }
        }
        private static void DynamicLibraryXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            // TODO: this looks like it might actually be MACH_O_TYPE=mh_dylib or mh_execute
            var dynamicLibrary = option as Opus.Core.ValueTypeOption<bool>;
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            if (dynamicLibrary.Value)
            {
                otherLDOptions.AddUnique("-dynamiclib");
            }
        }
        private static void LibraryPathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
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
        private static void LibraryPathsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                if (libraryPath.Contains(" "))
                {
                    otherLDOptions.AddUnique(System.String.Format("-L\"{0}\"", libraryPath));
                }
                else
                {
                    otherLDOptions.AddUnique(System.String.Format("-L{0}", libraryPath));
                }
            }
        }
        private static void StandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void StandardLibrariesXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void LibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void LibrariesXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void GenerateMapFileCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    if (options.MapFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,-Map,\"{0}\"", options.MapFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,-Map,{0}", options.MapFilePath));
                    }
                }
                else if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    if (options.MapFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,-map,\"{0}\"", options.MapFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,-map,{0}", options.MapFilePath));
                    }
                }
            }
        }
        private static void GenerateMapFileXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var generateMapfile = option as Opus.Core.ValueTypeOption<bool>;
            if (generateMapfile.Value)
            {
                var generateMapfileOption = configuration.Options["LD_MAP_FILE_PATH"];
                var options = sender as LinkerOptionCollection;
                generateMapfileOption.AddUnique(options.MapFilePath);

                if (generateMapfileOption.Count != 1)
                {
                    throw new Opus.Core.Exception("More than one map file location option has been set");
                }
            }
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static void AdditionalOptionsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            foreach (var argument in arguments)
            {
                otherLDOptions.AddUnique(argument);
            }
        }
        private static void OSXFrameworksCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> stringArrayOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string framework in stringArrayOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-framework {0}", framework));
            }
        }
        private static void OSXFrameworksXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var frameworks = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (var framework in frameworks.Value)
            {
                var fileReference = project.FileReferences.Get(framework, XcodeBuilder.PBXFileReference.EType.Framework, framework, null);

                var buildFile = project.BuildFiles.Get(framework, fileReference);
                buildFile.BuildPhase = project.FrameworksBuildPhases.Get("Frameworks", currentObject.Name);

                buildFile.BuildPhase.Files.AddUnique(buildFile);
            }
        }
        #endregion
        #region ILinkerOptions Option delegates
        private static void CanUseOriginCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wl,-z,origin");
            }
        }
        private static void CanUseOriginXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var useOrigin = option as Opus.Core.ValueTypeOption<bool>;
            var otherLDOptions = configuration.Options["OTHER_LDFLAGS"];
            if (useOrigin.Value)
            {
                otherLDOptions.AddUnique("-Wl,-z,origin");
            }
        }
        private static void AllowUndefinedSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                if (Opus.Core.OSUtilities.IsOSXHosting)
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
                if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    commandLineBuilder.Add("-Wl,-undefined,error");
                }
                else
                {
                    commandLineBuilder.Add("-Wl,-z,defs");
                }
            }
        }
        private static void AllowUndefinedSymbolsXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
            var allowUndefined = option as Opus.Core.ValueTypeOption<bool>;
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
        private static void RPathCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> stringsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string rpath in stringsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wl,-rpath,{0}", rpath));
            }
        }
        private static void RPathXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        private static void SixtyFourBitCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> sixtyFourBitOption = option as Opus.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Add("-m64");
            }
            else
            {
                commandLineBuilder.Add("-m32");
            }
        }
        private static void SixtyFourBitXcodeProjectProcessor(object sender, XcodeBuilder.PBXProject project, XcodeBuilder.XCodeNodeData currentObject, XcodeBuilder.XCBuildConfiguration configuration, Opus.Core.Option option, Opus.Core.Target target)
        {
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeXcodeProjectProcessor);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLineProcessor,DoNotAutoIncludeStandardLibrariesXcodeProjectProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor,DebugSymbolsXcodeProjectProcessor);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLineProcessor,SubSystemXcodeProjectProcessor);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLineProcessor,DynamicLibraryXcodeProjectProcessor);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLineProcessor,LibraryPathsXcodeProjectProcessor);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLineProcessor,StandardLibrariesXcodeProjectProcessor);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLineProcessor,LibrariesXcodeProjectProcessor);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLineProcessor,GenerateMapFileXcodeProjectProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsXcodeProjectProcessor);
            // Property 'OSXApplicationBundle' is state only
            this["OSXFrameworks"].PrivateData = new PrivateData(OSXFrameworksCommandLineProcessor,OSXFrameworksXcodeProjectProcessor);
            this["CanUseOrigin"].PrivateData = new PrivateData(CanUseOriginCommandLineProcessor,CanUseOriginXcodeProjectProcessor);
            this["AllowUndefinedSymbols"].PrivateData = new PrivateData(AllowUndefinedSymbolsCommandLineProcessor,AllowUndefinedSymbolsXcodeProjectProcessor);
            this["RPath"].PrivateData = new PrivateData(RPathCommandLineProcessor,RPathXcodeProjectProcessor);
            this["SixtyFourBit"].PrivateData = new PrivateData(SixtyFourBitCommandLineProcessor,SixtyFourBitXcodeProjectProcessor);
        }
    }
}
