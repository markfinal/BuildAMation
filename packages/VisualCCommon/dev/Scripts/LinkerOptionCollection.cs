// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions, ILinkerOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // common linker options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(ToolchainOptionCollectionCommandLine, ToolchainOptionCollectionVisualStudio);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine, OutputTypeVisualStudio);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLine, DebugSymbolsVisualStudio);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLine, SubSystemVisualStudio);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLine, DoNotAutoIncludeStandardLibrariesVisualStudio);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLine, DynamicLibraryVisualStudio);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLine, LibraryPathsVisualStudio);
            this["StandardLibraries"].PrivateData = new PrivateData(null, StandardLibrariesVisualStudio);
            this["Libraries"].PrivateData = new PrivateData(null, LibrariesVisualStudio);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLine, GenerateMapFileVisualStudio);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLine, AdditionalOptionsVisualStudio);

            // linker specific options
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLine, NoLogoVisualStudio);
            this["StackReserveAndCommit"].PrivateData = new PrivateData(StackReserveAndCommitCommandLine, StackReserveAndCommitVisualStudio);
            this["IgnoredLibraries"].PrivateData = new PrivateData(IgnoredLibrariesCommandLine, IgnoredLibrariesVisualStudio);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            ILinkerOptions linkerInterface = this as ILinkerOptions;

            linkerInterface.NoLogo = true;
            linkerInterface.StackReserveAndCommit = null;
            linkerInterface.IgnoredLibraries = new Opus.Core.StringArray();
            this.ProgamDatabaseDirectoryPath = this.OutputDirectoryPath.Clone() as string;

            Opus.Core.Target target = node.Target;

            // NEW STYLE
#if true
            Opus.Core.IToolsetInfo info = Opus.Core.ToolsetInfoFactory.CreateToolsetInfo(typeof(VisualC.ToolsetInfo));
            C.ILinkerInfo linkerInfo = info as C.ILinkerInfo;

            foreach (string libPath in linkerInfo.LibPaths(target))
            {
                (this as C.ILinkerOptions).LibraryPaths.AddAbsoluteDirectory(libPath, true);
            }
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            (this as C.ILinkerOptions).LibraryPaths.AddAbsoluteDirectory(toolChainInstance.LibPath(target), true);
#endif
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public string ProgamDatabaseDirectoryPath
        {
            get;
            set;
        }

        public string ProgramDatabaseFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.LinkerProgramDatabase];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.LinkerProgramDatabase] = value;
            }
        }

        public override void FinalizeOptions(Opus.Core.Target target)
        {
            C.ILinkerOptions options = this as C.ILinkerOptions;

            if (options.DebugSymbols && (null == this.ProgramDatabaseFilePath))
            {
                string pdbPathName = System.IO.Path.Combine(this.ProgamDatabaseDirectoryPath, this.OutputName) + ".pdb";
                this.ProgramDatabaseFilePath = pdbPathName;
            }

            base.FinalizeOptions(target);
        }

        private static void ToolchainOptionCollectionCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            RuntimeLibraryCommandLine(sender, commandLineBuilder, toolchainOptions.Value["RuntimeLibrary"], target);
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ToolchainOptionCollectionVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection> toolchainOptions = option as Opus.Core.ReferenceTypeOption<C.ToolchainOptionCollection>;
            return RuntimeLibraryVisualStudio(sender, toolchainOptions.Value["RuntimeLibrary"], target, vsTarget);
        }

        private static void OutputTypeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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

        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                        dictionary.Add("OutputFile", options.OutputFilePath);
                        return dictionary;
                    }

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        private static void DebugSymbolsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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

        private static VisualStudioProcessor.ToolAttributeDictionary DebugSymbolsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("GenerateDebugInformation", debugSymbolsOption.Value.ToString().ToLower());
            if (debugSymbolsOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                dictionary.Add("ProgramDatabaseFile", options.ProgramDatabaseFilePath);
            }
            return dictionary;
        }

        private static void NoLogoCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("/NOLOGO");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void StackReserveAndCommitCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stackSizeOption = option as Opus.Core.ReferenceTypeOption<string>;
            string stackSize = stackSizeOption.Value;
            if (stackSize != null)
            {
                // this will be formatted as "reserve[,commit]"
                commandLineBuilder.Add(System.String.Format("/STACK:{0}", stackSizeOption.Value));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary StackReserveAndCommitVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<string> stackSizeOption = option as Opus.Core.ReferenceTypeOption<string>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            string stackSize = stackSizeOption.Value;
            if (stackSize != null)
            {
                // this will be formatted as "reserve[,commit]"
                string[] split = stackSize.Split(',');
                dictionary.Add("StackReserveSize", split[0]);
                if (split.Length > 1)
                {
                    dictionary.Add("StackCommitSize", split[1]);
                }
            }
            return dictionary;
        }

        private static void IgnoredLibrariesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> ignoredLibrariesOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string library in ignoredLibrariesOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("/NODEFAULTLIB:{0}", library));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary IgnoredLibrariesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> ignoredLibrariesOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            string value = ignoredLibrariesOption.Value.ToString(';');
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                dictionary.Add("IgnoreDefaultLibraryNames", value);
            }
            else
            {
                dictionary.Add("IgnoreSpecificDefaultLibraries", value);
            }
            return dictionary;
        }

        private static void SubSystemCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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

        private static VisualStudioProcessor.ToolAttributeDictionary SubSystemVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<C.ESubsystem> subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                case C.ESubsystem.Console:
                case C.ESubsystem.Windows:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                        if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
                        {
                            dictionary.Add("SubSystem", subSystemOption.Value.ToString("D"));
                        }
                        else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
                        {
                            dictionary.Add("SubSystem", subSystemOption.Value.ToString());
                        }
                        return dictionary;
                    }

                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
            }
        }

        private static void DoNotAutoIncludeStandardLibrariesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("/NODEFAULTLIB");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DoNotAutoIncludeStandardLibrariesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("IgnoreAllDefaultLibraries", ignoreStandardLibrariesOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void RuntimeLibraryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            C.ILinkerOptions options = sender as C.ILinkerOptions;
            C.IToolchainOptions toolchainOptions = options.ToolchainOptionCollection as C.IToolchainOptions;
            bool isCPlusPlus = toolchainOptions.IsCPlusPlus;
            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                    options.StandardLibraries.Add("LIBCMT.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("LIBCPMT.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDebug:
                    options.StandardLibraries.Add("LIBCMTD.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("LIBCPMTD.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDLL:
                    options.StandardLibraries.Add("MSVCRT.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("MSVCPRT.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDebugDLL:
                    options.StandardLibraries.Add("MSVCRTD.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("MSVCPRTD.lib");
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized ERuntimeLibrary option");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary RuntimeLibraryVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            C.ILinkerOptions options = sender as C.ILinkerOptions;
            C.IToolchainOptions toolchainOptions = options.ToolchainOptionCollection as C.IToolchainOptions;
            bool isCPlusPlus = toolchainOptions.IsCPlusPlus;
            Opus.Core.ValueTypeOption<ERuntimeLibrary> runtimeLibraryOption = option as Opus.Core.ValueTypeOption<ERuntimeLibrary>;
            switch (runtimeLibraryOption.Value)
            {
                case ERuntimeLibrary.MultiThreaded:
                    options.StandardLibraries.Add("LIBCMT.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("LIBCPMT.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDebug:
                    options.StandardLibraries.Add("LIBCMTD.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("LIBCPMTD.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDLL:
                    options.StandardLibraries.Add("MSVCRT.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("MSVCPRT.lib");
                    }
                    break;

                case ERuntimeLibrary.MultiThreadedDebugDLL:
                    options.StandardLibraries.Add("MSVCRTD.lib");
                    if (isCPlusPlus)
                    {
                        options.StandardLibraries.Add("MSVCPRTD.lib");
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized ERuntimeLibrary option");
            }
            return null;
        }

        private static void DynamicLibraryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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

        private static VisualStudioProcessor.ToolAttributeDictionary DynamicLibraryVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            if (null != options.StaticImportLibraryFilePath)
            {
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                dictionary.Add("ImportLibrary", options.StaticImportLibraryFilePath);
                return dictionary;
            }
            else
            {
                return null;
            }
        }

        private static void LibraryPathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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

        private static VisualStudioProcessor.ToolAttributeDictionary LibraryPathsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
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
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("AdditionalLibraryDirectories", libraryPaths.ToString());
            return dictionary;
        }

        private static VisualStudioProcessor.ToolAttributeDictionary StandardLibrariesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
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
                VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                dictionary.Add("AdditionalDependencies", standardLibraryPaths.ToString());
                return dictionary;
            }
            else
            {
                return null;
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary LibrariesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
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
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("AdditionalDependencies", libraryPaths.ToString());
            return dictionary;
        }

        private static void GenerateMapFileCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
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

        private static VisualStudioProcessor.ToolAttributeDictionary GenerateMapFileVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("GenerateMapFile", boolOption.Value.ToString().ToLower());
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                dictionary.Add("MapFileName", options.MapFilePath);
            }
            return dictionary;
        }

        private static void AdditionalOptionsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary AdditionalOptionsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("AdditionalOptions", stringOption.Value);
            return dictionary;
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            string outputPathName = this.OutputFilePath;
            if (null != outputPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(outputPathName), false);
            }

            string libraryPathName = this.StaticImportLibraryFilePath;
            if (null != libraryPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(libraryPathName), false);
            }

            string programDatabasePathName = this.ProgramDatabaseFilePath;
            if (null != programDatabasePathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(programDatabasePathName), false);
            }

            return directoriesToCreate;
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualCCommon.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target) as VisualCCommon.Toolchain;
            VisualStudioProcessor.EVisualStudioTarget vsTarget = toolchain.VisualStudioTarget;
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Opus.Core.Exception(System.String.Format("Unsupported VisualStudio target, '{0}'", vsTarget));
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}