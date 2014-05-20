// Automatically generated file from OpusOptionCodeGenerator.
// Command line:
// -i=../../../C/dev/Scripts/ILinkerOptions.cs;ILinkerOptions.cs -n=VisualCCommon -c=LinkerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs;../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs -pv=PrivateData

namespace VisualCCommon
{
    public partial class LinkerOptionCollection
    {
        #region C.ILinkerOptions Option delegates
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            var options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    var outputFileLocation = options.OwningNode.Module.Locations[C.Application.OutputFile];
                    commandLineBuilder.Add(System.String.Format("-OUT:{0}", outputFileLocation.GetSinglePath()));
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
#if true
            var enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            var options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    {
                        var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                        var outputFileLocation = options.OwningNode.Module.Locations[C.Application.OutputFile];
                        returnVal.Add("OutputFile", outputFileLocation.GetSinglePath());
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
#else
            var enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            var options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    {
                        var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                        returnVal.Add("OutputFile", options.OutputFilePath);
                        return returnVal;
                    }
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
#endif
        }
        private static void DoNotAutoIncludeStandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-NODEFAULTLIB");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DoNotAutoIncludeStandardLibrariesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("IgnoreAllDefaultLibraries", ignoreStandardLibrariesOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void DebugSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-DEBUG");
#if true
                var pdbFile = (sender as Opus.Core.BaseOptionCollection).OwningNode.Module.Locations[Linker.PDBFile];
                var pdbPath = pdbFile.GetSinglePath();
                commandLineBuilder.Add(System.String.Format("/PDB:{0}", pdbPath));
#else
                var options = sender as LinkerOptionCollection;
                var pdbPathName = options.ProgramDatabaseFilePath;
                if (pdbPathName.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("/PDB:\"{0}\"", pdbPathName));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("/PDB:{0}", pdbPathName));
                }
#endif
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DebugSymbolsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
#if true
            var debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("GenerateDebugInformation", debugSymbolsOption.Value.ToString().ToLower());
#if true
            if (debugSymbolsOption.Value)
            {
                var pdbFile = (sender as Opus.Core.BaseOptionCollection).OwningNode.Module.Locations[Linker.PDBFile];
                var pdbPath = pdbFile.GetSinglePath();
                returnVal.Add("ProgramDatabaseFile", pdbPath);
            }
#else
            if (debugSymbolsOption.Value)
            {
                var options = sender as LinkerOptionCollection;
                returnVal.Add("ProgramDatabaseFile", options.ProgramDatabaseFilePath);
            }
#endif
            return returnVal;
#else
            var debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("GenerateDebugInformation", debugSymbolsOption.Value.ToString().ToLower());
            if (debugSymbolsOption.Value)
            {
                var options = sender as LinkerOptionCollection;
                returnVal.Add("ProgramDatabaseFile", options.ProgramDatabaseFilePath);
            }
            return returnVal;
#endif
        }
        private static void SubSystemCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                    // do nothing
                    break;
                case C.ESubsystem.Console:
                    commandLineBuilder.Add("-SUBSYSTEM:CONSOLE");
                    break;
                case C.ESubsystem.Windows:
                    commandLineBuilder.Add("-SUBSYSTEM:WINDOWS");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary SubSystemVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                case C.ESubsystem.Console:
                case C.ESubsystem.Windows:
                    {
                        var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
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
            var dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                commandLineBuilder.Add("-DLL");
                var options = sender as LinkerOptionCollection;
                var staticImportLibraryLoc = options.OwningNode.Module.Locations[C.DynamicLibrary.ImportLibraryFile];
                commandLineBuilder.Add(System.String.Format("-IMPLIB:{0}", staticImportLibraryLoc.GetSinglePath()));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary DynamicLibraryVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
#if true
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                var options = sender as LinkerOptionCollection;
                var staticImportLibraryLoc = options.OwningNode.Module.Locations[C.DynamicLibrary.ImportLibraryFile];
                returnVal.Add("ImportLibrary", staticImportLibraryLoc.GetSinglePath());
            }
            return returnVal;
#else
            var options = sender as LinkerOptionCollection;
            if (options.OutputPaths.Has(C.OutputFileFlags.StaticImportLibrary))
            {
                var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
                returnVal.Add("ImportLibrary", options.StaticImportLibraryFilePath);
                return returnVal;
            }
            else
            {
                return null;
            }
#endif
        }
        private static void LibraryPathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            // TODO: change to var, and returning Locations
            foreach (string includePath in includePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-LIBPATH:\"{0}\"", includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-LIBPATH:{0}", includePath));
                }
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary LibraryPathsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            var libraryPaths = new System.Text.StringBuilder();
            // TODO: change to var, returning Locations
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
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalLibraryDirectories", libraryPaths.ToString());
            return returnVal;
        }
        private static void StandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static VisualStudioProcessor.ToolAttributeDictionary StandardLibrariesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var options = sender as C.ILinkerOptions;
            if (options.DoNotAutoIncludeStandardLibraries)
            {
                var standardLibraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
                var standardLibraryPaths = new System.Text.StringBuilder();
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                {
                    // this stops any other libraries from being inherited
                    standardLibraryPaths.Append("$(NOINHERIT) ");
                    // TODO: change to var, returning Locations
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
                    // TODO: change to var, returning Locations
                    foreach (string standardLibraryPath in standardLibraryPathsOption.Value)
                    {
                        standardLibraryPaths.Append(System.String.Format("{0};", standardLibraryPath));
                    }
                }
                var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
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
            var libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            var libraryPaths = new System.Text.StringBuilder();
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                // this stops any other libraries from being inherited
                libraryPaths.Append("$(NOINHERIT) ");
                // TODO: change to var, returning Locations
                foreach (Opus.Core.Location location in libraryPathsOption.Value)
                {
                    var standardLibraryPath = location.GetSinglePath();
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
                // TODO: change to var, returning Locations
                foreach (Opus.Core.Location location in libraryPathsOption.Value)
                {
                    var standardLibraryPath = location.GetSinglePath();
                    libraryPaths.Append(System.String.Format("{0};", standardLibraryPath));
                }
            }
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalDependencies", libraryPaths.ToString());
            return returnVal;
        }
        private static void GenerateMapFileCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
#if true
            // TODO: not handling map files yet
#else
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                var options = sender as LinkerOptionCollection;
                if (options.MapFilePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-MAP:\"{0}\"", options.MapFilePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-MAP:{0}", options.MapFilePath));
                }
            }
#endif
        }
        private static VisualStudioProcessor.ToolAttributeDictionary GenerateMapFileVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
#if true
            // TODO: not handling map files yet
            return null;
#else
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("GenerateMapFile", boolOption.Value.ToString().ToLower());
            if (boolOption.Value)
            {
                var options = sender as LinkerOptionCollection;
                returnVal.Add("MapFileName", options.MapFilePath);
            }
            return returnVal;
#endif
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            var arguments = stringOption.Value.Split(' ');
            foreach (var argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary AdditionalOptionsVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("AdditionalOptions", stringOption.Value);
            return returnVal;
        }
        #endregion
        #region ILinkerOptions Option delegates
        private static void NoLogoCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("-NOLOGO");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            returnVal.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return returnVal;
        }
        private static void StackReserveAndCommitCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var stackSizeOption = option as Opus.Core.ReferenceTypeOption<string>;
            var stackSize = stackSizeOption.Value;
            if (stackSize != null)
            {
                // this will be formatted as "reserve[,commit]"
                commandLineBuilder.Add(System.String.Format("-STACK:{0}", stackSizeOption.Value));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary StackReserveAndCommitVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var stackSizeOption = option as Opus.Core.ReferenceTypeOption<string>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var stackSize = stackSizeOption.Value;
            if (stackSize != null)
            {
                // this will be formatted as "reserve[,commit]"
                var split = stackSize.Split(',');
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
            var ignoredLibrariesOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            // TODO: change to var
            foreach (string library in ignoredLibrariesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-NODEFAULTLIB:{0}", library));
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary IgnoredLibrariesVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var ignoredLibrariesOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var value = ignoredLibrariesOption.Value.ToString(';');
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
        private static void IncrementalLinkCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-INCREMENTAL");
            }
            else
            {
                commandLineBuilder.Add("-INCREMENTAL:NO");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary IncrementalLinkVisualStudioProcessor(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (vsTarget == VisualStudioProcessor.EVisualStudioTarget.VCPROJ)
            {
                if (boolOption.Value)
                {
                    returnVal.Add("LinkIncremental", "2");
                }
                else
                {
                    returnVal.Add("LinkIncremental", "0");
                }
            }
            else
            {
                // TODO: this is wrong - it needs to be on the PropertyGroup, not the ItemDefinitionGroup
                returnVal.Add("LinkIncremental", boolOption.Value.ToString().ToLower());
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
            this["IncrementalLink"].PrivateData = new PrivateData(IncrementalLinkCommandLineProcessor,IncrementalLinkVisualStudioProcessor);
        }
    }
}
