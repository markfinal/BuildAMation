// <copyright file="OptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public partial class OptionCollection : Opus.Core.BaseOptionCollection, IOptions, CommandLineProcessor.ICommandLineSupport, VisualStudioProcessor.IVisualStudioSupport
    {
        private void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            Opus.Core.Target target = node.Target;

            IOptions options = this as IOptions;
            options.Target = ETarget.Executable;
            options.NoLogo = true;
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                // this does not exist on mono
                options.Platform = EPlatform.AnyCpu;
            }
            options.Checked = true;
            options.Unsafe = false;
            options.WarningLevel = EWarningLevel.Level4;
            options.WarningsAsErrors = true;
            options.Defines = new Opus.Core.StringArray();

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                options.DebugInformation = EDebugInformation.Full;
                options.Optimize = false;
                options.Defines.Add("DEBUG");
            }
            else
            {
                if (!target.HasConfiguration(Opus.Core.EConfiguration.Profile))
                {
                    options.DebugInformation = EDebugInformation.Disabled;
                }
                else
                {
                    options.DebugInformation = EDebugInformation.Full;
                    options.Defines.Add("TRACE");
                }
                options.Optimize = true;
            }

            options.References = new Opus.Core.FileCollection();
            options.Modules = new Opus.Core.FileCollection();
        }

        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["Target"].PrivateData = new PrivateData(TargetCommandLine, TargetVisualStudio);
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLine, NoLogoVisualStudio);
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                this["Platform"].PrivateData = new PrivateData(PlatformCommandLine, PlatformVisualStudio);
            }
            this["Checked"].PrivateData = new PrivateData(CheckedCommandLine, CheckedVisualStudio);
            this["Unsafe"].PrivateData = new PrivateData(UnsafeCommandLine, UnsafeVisualStudio);
            this["WarningLevel"].PrivateData = new PrivateData(WarningLevelCommandLine, WarningLevelVisualStudio);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLine, WarningsAsErrorsVisualStudio);
            this["Optimize"].PrivateData = new PrivateData(OptimizeCommandLine, OptimizeVisualStudio);
            this["DebugInformation"].PrivateData = new PrivateData(DebugInformationCommandLine, DebugInformationVisualStudio);
            this["References"].PrivateData = new PrivateData(ReferencesCommandLine, ReferencesVisualStudio);
            this["Modules"].PrivateData = new PrivateData(ModulesCommandLine, ModulesVisualStudio);
            this["Defines"].PrivateData = new PrivateData(DefinesCL, DefinesVS);
        }

        public OptionCollection(Opus.Core.DependencyNode node)
        {
            this.SetNodeOwnership(node);
            this.InitializeDefaults(node);
            this.SetDelegates(node);
        }

        public string OutputName
        {
            get;
            set;
        }

        public string OutputDirectoryPath
        {
            get;
            set;
        }

        public string OutputFilePath
        {
            get
            {
                return this.OutputPaths[OutputFileFlags.AssemblyFile];
            }
            set
            {
                this.OutputPaths[OutputFileFlags.AssemblyFile] = value;
            }
        }

        public string ProgramDatabaseFilePath
        {
            get
            {
                return this.OutputPaths[OutputFileFlags.ProgramDatabaseFile];
            }
            set
            {
                this.OutputPaths[OutputFileFlags.ProgramDatabaseFile] = value;
            }
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory("bin");
        }

        public override void FinalizeOptions(Opus.Core.Target target)
        {
            if (null == this.OutputFilePath)
            {
                string outputSuffix;
                switch (this.Target)
                {
                    case ETarget.Executable:
                    case ETarget.WindowsExecutable:
                        outputSuffix = ".exe";
                        break;

                    case ETarget.Library:
                        outputSuffix = ".dll";
                        break;

                    case ETarget.Module:
                        outputSuffix = ".netmodule";
                        break;

                    default:
                        throw new Opus.Core.Exception("Unrecognized CSharp.ETarget value");
                }

                string outputPathName = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + outputSuffix;
                this.OutputFilePath = outputPathName;
            }

            if ((this.DebugInformation != EDebugInformation.Disabled) && (null == this.ProgramDatabaseFilePath))
            {
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    string pdbPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + ".pdb";
                    this.ProgramDatabaseFilePath = pdbPathname;
                }
            }

            base.FinalizeOptions(target);
        }

        private static void TargetCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            OptionCollection options = sender as OptionCollection;
            Opus.Core.ValueTypeOption<ETarget> enumOption = option as Opus.Core.ValueTypeOption<ETarget>;
            switch (enumOption.Value)
            {
                case ETarget.Executable:
                    commandLineBuilder.Add("/target:exe");
                    break;

                case ETarget.Library:
                    commandLineBuilder.Add("/target:library");
                    break;

                case ETarget.Module:
                    commandLineBuilder.Add("/target:module");
                    break;

                case ETarget.WindowsExecutable:
                    commandLineBuilder.Add("/target:winexe");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.ETarget value");
            }

            string outputPathName = options.OutputFilePath;
            if (outputPathName.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("/out:\"{0}\"", outputPathName));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("/out:{0}", outputPathName));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary TargetVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<ETarget> enumOption = option as Opus.Core.ValueTypeOption<ETarget>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            switch (enumOption.Value)
            {
                case ETarget.Executable:
                    dictionary.Add("OutputType", "Exe");
                    break;

                case ETarget.Library:
                    dictionary.Add("OutputType", "Library");
                    break;

                case ETarget.Module:
                    dictionary.Add("OutputType", "Module");
                    break;

                case ETarget.WindowsExecutable:
                    dictionary.Add("OutputType", "WinExe");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.ETarget value");
            }
            return dictionary;
        }

        private static void NoLogoCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/nologo");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            dictionary.Add("NoLogo", boolOption.Value ? "true" : "false");
            return dictionary;
        }

        private static void PlatformCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EPlatform> enumOption = option as Opus.Core.ValueTypeOption<EPlatform>;
            switch (enumOption.Value)
            {
                case EPlatform.X86:
                    commandLineBuilder.Add("/platform:x86");
                    break;

                case EPlatform.X64:
                    commandLineBuilder.Add("/platform:x64");
                    break;

                case EPlatform.Itanium:
                    commandLineBuilder.Add("/platform:Itanium");
                    break;

                case EPlatform.AnyCpu:
                    commandLineBuilder.Add("/platform:AnyCpu");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EPlatform value");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary PlatformVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<EPlatform> enumOption = option as Opus.Core.ValueTypeOption<EPlatform>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            switch (enumOption.Value)
            {
                case EPlatform.X86:
                    dictionary.Add("PlatformTarget", "x86");
                    break;

                case EPlatform.X64:
                    dictionary.Add("PlatformTarget", "x64");
                    break;

                case EPlatform.Itanium:
                    dictionary.Add("PlatformTarget", "Itanium");
                    break;

                case EPlatform.AnyCpu:
                    dictionary.Add("PlatformTarget", "AnyCPU");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EPlatform value");
            }
            return dictionary;
        }

        private static void CheckedCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/checked+");
            }
            else
            {
                commandLineBuilder.Add("/checked-");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary CheckedVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("CheckForOverflowUnderflow", boolOption.Value ? "true" : "false");
            return dictionary;
        }

        private static void UnsafeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/unsafe+");
            }
            else
            {
                commandLineBuilder.Add("/unsafe-");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary UnsafeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("Unsafe", boolOption.Value ? "true" : "false");
            return dictionary;
        }

        private static void WarningLevelCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EWarningLevel> enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            switch (enumOption.Value)
            {
                case EWarningLevel.Level0:
                case EWarningLevel.Level1:
                case EWarningLevel.Level2:
                case EWarningLevel.Level3:
                case EWarningLevel.Level4:
                    commandLineBuilder.Add(System.String.Format("/warn:{0}", enumOption.Value.ToString("d")));
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EWarningLevel value");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary WarningLevelVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<EWarningLevel> enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            switch (enumOption.Value)
            {
                case EWarningLevel.Level0:
                case EWarningLevel.Level1:
                case EWarningLevel.Level2:
                case EWarningLevel.Level3:
                case EWarningLevel.Level4:
                    dictionary.Add("WarningLevel", enumOption.Value.ToString("d"));
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EWarningLevel value");
            }
            return dictionary;
        }

        private static void WarningsAsErrorsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/warnaserror+");
            }
            else
            {
                commandLineBuilder.Add("/warnaserror-");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary WarningsAsErrorsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("TreatWarningsAsErrors", boolOption.Value ? "true" : "false");
            return dictionary;
        }

        private static void OptimizeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("/optimize+");
            }
            else
            {
                commandLineBuilder.Add("/optimize-");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OptimizeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("Optimize", boolOption.Value ? "true" : "false");
            return dictionary;
        }

        private static void DebugInformationCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            OptionCollection options = sender as OptionCollection;
            Opus.Core.ValueTypeOption<EDebugInformation> enumOption = option as Opus.Core.ValueTypeOption<EDebugInformation>;
            switch (enumOption.Value)
            {
                case EDebugInformation.Disabled:
                    commandLineBuilder.Add("/debug-");
                    break;

                case EDebugInformation.ProgramDatabaseOnly:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            commandLineBuilder.Add("/debug+");
                            commandLineBuilder.Add("/debug:pdbinfo");
                            string pdbPathName = options.ProgramDatabaseFilePath;
                            if (pdbPathName.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", pdbPathName));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("/pdb:{0}", pdbPathName));
                            }
                        }
                        else
                        {
                            commandLineBuilder.Add("/debug+");
                        }
                    }
                    break;

                case EDebugInformation.Full:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            commandLineBuilder.Add("/debug+");
                            commandLineBuilder.Add("/debug:full");
                            string pdbPathName = options.ProgramDatabaseFilePath;
                            if (pdbPathName.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", pdbPathName));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("/pdb:{0}", pdbPathName));
                            }
                        }
                        else
                        {
                            commandLineBuilder.Add("/debug+");
                        }
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EDebugInformation value");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DebugInformationVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            Opus.Core.ValueTypeOption<EDebugInformation> enumOption = option as Opus.Core.ValueTypeOption<EDebugInformation>;
            switch (enumOption.Value)
            {
                case EDebugInformation.Disabled:
                    dictionary.Add("DebugSymbols", "false");
                    break;

                case EDebugInformation.ProgramDatabaseOnly:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            dictionary.Add("DebugSymbols", "true");
                            dictionary.Add("DebugType", "pdbinfo");
                            // TODO
                            //commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", options.ProgramDatabaseFilePath));
                        }
                        else
                        {
                            dictionary.Add("DebugSymbols", "true");
                        }
                    }
                    break;

                case EDebugInformation.Full:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            dictionary.Add("DebugSymbols", "true");
                            dictionary.Add("DebugType", "Full");
                            // TODO
                            //commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", options.ProgramDatabaseFilePath));
                        }
                        else
                        {
                            dictionary.Add("DebugSymbols", "true");
                        }
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EDebugInformation value");
            }
            return dictionary;
        }

        private static void ReferencesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> fileCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                System.Text.StringBuilder fileList = new System.Text.StringBuilder();
                foreach (string file in fileCollectionOption.Value)
                {
                    // TODO: need to check whether ; is appropriate here for *nix platforms
                    // It hasn't failed before, but if you are to copy-n-paste an Opus command line
                    // to a shell, would it think that a ; is a continuation character?
                    if (file.Contains(" "))
                    {
                        fileList.AppendFormat("\"{0}\";", file);
                    }
                    else
                    {
                        fileList.AppendFormat("{0};", file);
                    }
                }
                commandLineBuilder.Add(System.String.Format("/reference:{0}", fileList.ToString()));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ReferencesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            // this is handled elsewhere
            return null;
        }

        private static void ModulesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> fileCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                System.Text.StringBuilder fileList = new System.Text.StringBuilder();
                foreach (string file in fileCollectionOption.Value)
                {
                    // TODO: need to check whether ; is appropriate here for *nix platforms
                    // It hasn't failed before, but if you are to copy-n-paste an Opus command line
                    // to a shell, would it think that a ; is a continuation character?
                    if (file.Contains(" "))
                    {
                        fileList.AppendFormat("\"{0}\";", file);
                    }
                    else
                    {
                        fileList.AppendFormat("{0};", file);
                    }
                }
                commandLineBuilder.Add(System.String.Format("/addmodule:{0}", fileList.ToString()));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ModulesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            // TODO
            return dictionary;
        }

        private static void DefinesCL(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> stringArrayOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            if (stringArrayOption.Value.Count == 0)
            {
                return;
            }
            System.Text.StringBuilder definesList = new System.Text.StringBuilder();
            definesList.Append("/define:");
            foreach (string define in stringArrayOption.Value)
            {
                definesList.AppendFormat("{0};", define);
            }
            commandLineBuilder.Add(definesList.ToString().TrimEnd(new char [] { ';' }));
        }

        private static VisualStudioProcessor.ToolAttributeDictionary DefinesVS(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> stringArrayOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            if (stringArrayOption.Value.Count == 0)
            {
                return dictionary;
            }
            System.Text.StringBuilder definesList = new System.Text.StringBuilder();
            foreach (string define in stringArrayOption.Value)
            {
                definesList.AppendFormat("{0};", define);
            }
            dictionary.Add("DefineConstants", definesList.ToString().TrimEnd(new char[] { ';' }));
            return dictionary;
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            string outputPathName = this.OutputFilePath;
            if (null != outputPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(outputPathName), false);
            }

            string pdbPathName = this.ProgramDatabaseFilePath;
            if (null != pdbPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(pdbPathName), false);
            }

            return directoriesToCreate;
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, VisualStudioProcessor.EVisualStudioTarget.MSBUILD);
            return dictionary;
        }
    }
}