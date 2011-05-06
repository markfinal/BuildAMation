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

            if (target.Configuration == Opus.Core.EConfiguration.Debug)
            {
                options.DebugInformation = EDebugInformation.Full;
                options.Optimize = false;
            }
            else
            {
                options.DebugInformation = EDebugInformation.Disabled;
                options.Optimize = true;
            }

            options.References = new Opus.Core.FileCollection();
            options.Modules = new Opus.Core.FileCollection();
        }

        private void SetDelegates()
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
        }

        public OptionCollection(Opus.Core.DependencyNode node)
        {
            this.SetNodeOwnership(node);

            this.InitializeDefaults(node);

            this.SetDelegates();
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

        protected static void TargetSetHandler(object sender, Opus.Core.Option option)
        {
            OptionCollection options = sender as OptionCollection;
            Opus.Core.ValueTypeOption<ETarget> enumOption = option as Opus.Core.ValueTypeOption<ETarget>;
            switch (enumOption.Value)
            {
                case ETarget.Executable:
                    {
                        string executablePathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".exe";
                        options.OutputFilePath = executablePathname;
                    }
                    break;

                case ETarget.Library:
                    {
                        string libraryPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".dll";
                        options.OutputFilePath = libraryPathname;
                    }
                    break;

                case ETarget.Module:
                    {
                        string libraryPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".netmodule";
                        options.OutputFilePath = libraryPathname;
                    }
                    break;

                case ETarget.WindowsExecutable:
                    {
                        string executablePathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".exe";
                        options.OutputFilePath = executablePathname;
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.ETarget value");
            }
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
            commandLineBuilder.Add(System.String.Format("/out:\"{0}\"", options.OutputFilePath));
        }

        private static VisualStudioProcessor.ToolAttributeDictionary TargetVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            OptionCollection options = sender as OptionCollection;
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
            dictionary.Add("Checked", boolOption.Value ? "true" : "false");
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
            dictionary.Add("WarnAsError", boolOption.Value ? "true" : "false");
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

        protected static void DebugInformationSetHandler(object sender, Opus.Core.Option option)
        {
            OptionCollection options = sender as OptionCollection;
            Opus.Core.ValueTypeOption<EDebugInformation> enumOption = option as Opus.Core.ValueTypeOption<EDebugInformation>;
            switch (enumOption.Value)
            {
                case EDebugInformation.Disabled:
                    options.ProgramDatabaseFilePath = null;
                    break;

                case EDebugInformation.ProgramDatabaseOnly:
                case EDebugInformation.Full:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            string pdbPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".pdb";
                            options.ProgramDatabaseFilePath = pdbPathname;
                        }
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EDebugInformation value");
            }
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
                            commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", options.ProgramDatabaseFilePath));
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
                            commandLineBuilder.Add(System.String.Format("/pdb:\"{0}\"", options.ProgramDatabaseFilePath));
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
            OptionCollection options = sender as OptionCollection;
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
                    fileList.AppendFormat("\"{0}\";", file);
                }
                commandLineBuilder.Add(System.String.Format("/reference:{0}", fileList.ToString()));
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary ReferencesVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            // TODO
            return dictionary;
        }

        private static void ModulesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> fileCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                System.Text.StringBuilder fileList = new System.Text.StringBuilder();
                foreach (string file in fileCollectionOption.Value)
                {
                    fileList.AppendFormat("\"{0}\";", file);
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

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.OutputFilePath)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(this.OutputFilePath), false);
            }
            if (null != this.ProgramDatabaseFilePath)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(this.ProgramDatabaseFilePath), false);
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