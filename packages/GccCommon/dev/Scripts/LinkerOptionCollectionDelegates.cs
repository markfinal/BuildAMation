// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=..\..\..\C\dev\Scripts\ILinkerOptions.cs;ILinkerOptions.cs -n=GccCommon -c=LinkerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=PrivateData

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
        private static void DoNotAutoIncludeStandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-nostdlib");
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
        private static void SubSystemCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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
        private static void StandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void LibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
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
        private static void RPathCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> stringsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string rpath in stringsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wl,-rpath,{0}", rpath));
            }
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
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLineProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLineProcessor);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLineProcessor);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLineProcessor);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLineProcessor);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLineProcessor);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLineProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor);
            this["CanUseOrigin"].PrivateData = new PrivateData(CanUseOriginCommandLineProcessor);
            this["AllowUndefinedSymbols"].PrivateData = new PrivateData(AllowUndefinedSymbolsCommandLineProcessor);
            this["RPath"].PrivateData = new PrivateData(RPathCommandLineProcessor);
            this["SixtyFourBit"].PrivateData = new PrivateData(SixtyFourBitCommandLineProcessor);
        }
    }
}
