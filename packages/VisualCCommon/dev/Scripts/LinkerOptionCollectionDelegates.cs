// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=..\..\..\C\dev\Scripts\ILinkerOptions.cs;ILinkerOptions.cs -n=VisualCCommon -c=LinkerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs;..\..\..\VisualStudioProcessor\dev\Scripts\VisualStudioDelegate.cs -pv=PrivateData

namespace VisualCCommon
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
                case C.ELinkerOutput.DynamicLibrary:
                    string outputPathName = options.OutputFilePath;
                    if (outputPathName.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("/OUT:\"{0}\"", outputPathName));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("/OUT:{0}", outputPathName));
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                        returnVal.Add("OutputFile", options.OutputFilePath);
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }
        private static void DoNotAutoIncludeStandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("/NODEFAULTLIB");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DoNotAutoIncludeStandardLibrariesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("IgnoreAllDefaultLibraries", ignoreStandardLibrariesOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void DebugSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("/DEBUG");
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                string pdbPathName = options.ProgramDatabaseFilePath;
                if (pdbPathName.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("/PDB:\"{0}\"", pdbPathName));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("/PDB:{0}", pdbPathName));
                }
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DebugSymbolsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("GenerateDebugInformation", debugSymbolsOption.Value.ToString().ToLower());
            if (debugSymbolsOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                returnVal.Add("ProgramDatabaseFile", options.ProgramDatabaseFilePath);
            }
            return returnVal;
        }
        private static void SubSystemCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ESubsystem> subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                    // do nothing
                    break;
                case C.ESubsystem.Console:
                    commandLineBuilder.Add("/SUBSYSTEM:CONSOLE");
                    break;
                case C.ESubsystem.Windows:
                    commandLineBuilder.Add("/SUBSYSTEM:WINDOWS");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary SubSystemVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<C.ESubsystem> subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                case C.ESubsystem.Console:
                case C.ESubsystem.Windows:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                        if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                        {
                            returnVal.Add("SubSystem", subSystemOption.Value.ToString("D"));
                        }
                        else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                        {
                            returnVal.Add("SubSystem", subSystemOption.Value.ToString());
                        }
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
            }
        }
        private static void DynamicLibraryCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                commandLineBuilder.Add("/DLL");
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (null != options.StaticImportLibraryFilePath)
                {
                    if (options.StaticImportLibraryFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("/IMPLIB:\"{0}\"", options.StaticImportLibraryFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("/IMPLIB:{0}", options.StaticImportLibraryFilePath));
                    }
                }
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DynamicLibraryVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            if (null != options.StaticImportLibraryFilePath)
            {
                VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                returnVal.Add("ImportLibrary", options.StaticImportLibraryFilePath);
                return returnVal;
            }
            else
            {
                return null;
            }
        }
        private static void LibraryPathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("/LIBPATH:\"{0}\"", includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("/LIBPATH:{0}", includePath));
                }
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary LibraryPathsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            System.Text.StringBuilder libraryPaths = new System.Text.StringBuilder();
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                if (libraryPath.Contains(" "))
                {
                    libraryPaths.Append(System.String.Format("\"{0}\";", libraryPath));
                }
                else
                {
                    libraryPaths.Append(System.String.Format("{0};", libraryPath));
                }
            }
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalLibraryDirectories", libraryPaths.ToString());
            return returnVal;
        }
        private static void StandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static VisualStudioProcessor.ToolAttributeDictionary StandardLibrariesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            C.ILinkerOptions options = sender as C.ILinkerOptions;
            if (options.DoNotAutoIncludeStandardLibraries)
            {
                Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> standardLibraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
                System.Text.StringBuilder standardLibraryPaths = new System.Text.StringBuilder();
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                {
                    // this stops any other libraries from being inherited
                    standardLibraryPaths.Append("$(NOINHERIT) ");
                    foreach (string standardLibraryPath in standardLibraryPathsOption.Value)
                    {
                        if (standardLibraryPath.Contains(" "))
                        {
                            standardLibraryPaths.Append(System.String.Format("\"{0}\" ", standardLibraryPath));
                        }
                        else
                        {
                            standardLibraryPaths.Append(System.String.Format("{0} ", standardLibraryPath));
                        }
                    }
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                {
                    foreach (string standardLibraryPath in standardLibraryPathsOption.Value)
                    {
                        standardLibraryPaths.Append(System.String.Format("{0};", standardLibraryPath));
                    }
                }
                VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                returnVal.Add("AdditionalDependencies", standardLibraryPaths.ToString());
                return returnVal;
            }
            else
            {
                return null;
            }
        }
        private static void LibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static VisualStudioProcessor.ToolAttributeDictionary LibrariesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            System.Text.StringBuilder libraryPaths = new System.Text.StringBuilder();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                // this stops any other libraries from being inherited
                libraryPaths.Append("$(NOINHERIT) ");
                foreach (string standardLibraryPath in libraryPathsOption.Value)
                {
                    if (standardLibraryPath.Contains(" "))
                    {
                        libraryPaths.Append(System.String.Format("\"{0}\" ", standardLibraryPath));
                    }
                    else
                    {
                        libraryPaths.Append(System.String.Format("{0} ", standardLibraryPath));
                    }
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                foreach (string standardLibraryPath in libraryPathsOption.Value)
                {
                    libraryPaths.Append(System.String.Format("{0};", standardLibraryPath));
                }
            }
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalDependencies", libraryPaths.ToString());
            return returnVal;
        }
        private static void GenerateMapFileCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (options.MapFilePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("/MAP:\"{0}\"", options.MapFilePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("/MAP:{0}", options.MapFilePath));
                }
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary GenerateMapFileVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("GenerateMapFile", boolOption.Value.ToString().ToLower());
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                returnVal.Add("MapFileName", options.MapFilePath);
            }
            return returnVal;
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
        private static VisualStudioProcessor.ToolAttributeDictionary AdditionalOptionsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalOptions", stringOption.Value);
            return returnVal;
        }
        #endregion
        #region ILinkerOptions Option delegates
        private static void NoLogoCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("/NOLOGO");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void StackReserveAndCommitCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stackSizeOption = option as Opus.Core.ReferenceTypeOption<string>;
            string stackSize = stackSizeOption.Value;
            if (stackSize != null)
            {
                // this will be formatted as "reserve[,commit]"
                commandLineBuilder.Add(System.String.Format("/STACK:{0}", stackSizeOption.Value));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary StackReserveAndCommitVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<string> stackSizeOption = option as Opus.Core.ReferenceTypeOption<string>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            string stackSize = stackSizeOption.Value;
            if (stackSize != null)
            {
                // this will be formatted as "reserve[,commit]"
                string[] split = stackSize.Split(',');
                returnVal.Add("StackReserveSize", split[0]);
                if (split.Length > 1)
                {
                    returnVal.Add("StackCommitSize", split[1]);
                }
            }
            return returnVal;
        }
        private static void IgnoredLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> ignoredLibrariesOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string library in ignoredLibrariesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("/NODEFAULTLIB:{0}", library));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary IgnoredLibrariesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> ignoredLibrariesOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            VisualStudioProcessor.ToolAttributeDictionary returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            string value = ignoredLibrariesOption.Value.ToString(';');
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                returnVal.Add("IgnoreDefaultLibraryNames", value);
            }
            else
            {
                returnVal.Add("IgnoreSpecificDefaultLibraries", value);
            }
            return returnVal;
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor,OutputTypeVisualStudioProcessor);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLineProcessor,DoNotAutoIncludeStandardLibrariesVisualStudioProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor,DebugSymbolsVisualStudioProcessor);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLineProcessor,SubSystemVisualStudioProcessor);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLineProcessor,DynamicLibraryVisualStudioProcessor);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLineProcessor,LibraryPathsVisualStudioProcessor);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLineProcessor,StandardLibrariesVisualStudioProcessor);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLineProcessor,LibrariesVisualStudioProcessor);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLineProcessor,GenerateMapFileVisualStudioProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor,AdditionalOptionsVisualStudioProcessor);
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLineProcessor,NoLogoVisualStudioProcessor);
            this["StackReserveAndCommit"].PrivateData = new PrivateData(StackReserveAndCommitCommandLineProcessor,StackReserveAndCommitVisualStudioProcessor);
            this["IgnoredLibraries"].PrivateData = new PrivateData(IgnoredLibrariesCommandLineProcessor,IgnoredLibrariesVisualStudioProcessor);
        }
    }
}
