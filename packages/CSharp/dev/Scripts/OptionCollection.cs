// <copyright file="OptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public partial class OptionCollection : Opus.Core.BaseOptionCollection, IOptions, CommandLineProcessor.ICommandLineSupport
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
            this["Target"].PrivateData = new PrivateData(TargetCommandLine);
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLine);
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                this["Platform"].PrivateData = new PrivateData(PlatformCommandLine);
            }
            this["Checked"].PrivateData = new PrivateData(CheckedCommandLine);
            this["Unsafe"].PrivateData = new PrivateData(UnsafeCommandLine);
            this["WarningLevel"].PrivateData = new PrivateData(WarningLevelCommandLine);
            this["WarningsAsErrors"].PrivateData = new PrivateData(WarningsAsErrorsCommandLine);
            this["Optimize"].PrivateData = new PrivateData(OptimizeCommandLine);
            this["DebugInformation"].PrivateData = new PrivateData(DebugInformationCommandLine);
            this["References"].PrivateData = new PrivateData(ReferencesCommandLine);
            this["Modules"].PrivateData = new PrivateData(ModulesCommandLine);
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

        private static void TargetCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            OptionCollection options = sender as OptionCollection;
            Opus.Core.ValueTypeOption<ETarget> enumOption = option as Opus.Core.ValueTypeOption<ETarget>;
            switch (enumOption.Value)
            {
                case ETarget.Executable:
                    commandLineBuilder.Append("/target:exe ");
                    break;

                case ETarget.Library:
                    commandLineBuilder.Append("/target:library ");
                    break;

                case ETarget.Module:
                    commandLineBuilder.Append("/target:module ");
                    break;

                case ETarget.WindowsExecutable:
                    commandLineBuilder.Append("/target:winexe ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.ETarget value");
            }
            commandLineBuilder.AppendFormat("/out:\"{0}\" ", options.OutputFilePath);
        }

        private static void NoLogoCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/nologo ");
            }
        }

        private static void PlatformCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EPlatform> enumOption = option as Opus.Core.ValueTypeOption<EPlatform>;
            switch (enumOption.Value)
            {
                case EPlatform.X86:
                    commandLineBuilder.Append("/platform:x86 ");
                    break;

                case EPlatform.X64:
                    commandLineBuilder.Append("/platform:x64 ");
                    break;

                case EPlatform.Itanium:
                    commandLineBuilder.Append("/platform:Itanium ");
                    break;

                case EPlatform.AnyCpu:
                    commandLineBuilder.Append("/platform:AnyCpu ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EPlatform value");
            }
        }

        private static void CheckedCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/checked+ ");
            }
            else
            {
                commandLineBuilder.Append("/checked- ");
            }
        }

        private static void UnsafeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/unsafe+ ");
            }
            else
            {
                commandLineBuilder.Append("/unsafe- ");
            }
        }

        private static void WarningLevelCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EWarningLevel> enumOption = option as Opus.Core.ValueTypeOption<EWarningLevel>;
            switch (enumOption.Value)
            {
                case EWarningLevel.Level0:
                case EWarningLevel.Level1:
                case EWarningLevel.Level2:
                case EWarningLevel.Level3:
                case EWarningLevel.Level4:
                    commandLineBuilder.AppendFormat("/warn:{0} ", enumOption.Value.ToString("d"));
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EWarningLevel value");
            }
        }

        private static void WarningsAsErrorsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/warnaserror+ ");
            }
            else
            {
                commandLineBuilder.Append("/warnaserror- ");
            }
        }

        private static void OptimizeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("/optimize+ ");
            }
            else
            {
                commandLineBuilder.Append("/optimize- ");
            }
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

        private static void DebugInformationCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            OptionCollection options = sender as OptionCollection;
            Opus.Core.ValueTypeOption<EDebugInformation> enumOption = option as Opus.Core.ValueTypeOption<EDebugInformation>;
            switch (enumOption.Value)
            {
                case EDebugInformation.Disabled:
                    commandLineBuilder.Append("/debug- ");
                    break;

                case EDebugInformation.ProgramDatabaseOnly:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            commandLineBuilder.AppendFormat("/debug+ /debug:pdbinfo /pdb:\"{0}\" ", options.ProgramDatabaseFilePath);
                        }
                        else
                        {
                            commandLineBuilder.Append("/debug+ ");
                        }
                    }
                    break;

                case EDebugInformation.Full:
                    {
                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            commandLineBuilder.AppendFormat("/debug+ /debug:full /pdb:\"{0}\" ", options.ProgramDatabaseFilePath);
                        }
                        else
                        {
                            commandLineBuilder.Append("/debug+ ");
                        }
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized CSharp.EDebugInformation value");
            }
        }

        private static void ReferencesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> fileCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                System.Text.StringBuilder fileList = new System.Text.StringBuilder();
                foreach (string file in fileCollectionOption.Value)
                {
                    fileList.AppendFormat("\"{0}\";", file);
                }
                commandLineBuilder.AppendFormat("/reference:{0} ", fileList.ToString());
            }
        }

        private static void ModulesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> fileCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            if (fileCollectionOption.Value.Count > 0)
            {
                System.Text.StringBuilder fileList = new System.Text.StringBuilder();
                foreach (string file in fileCollectionOption.Value)
                {
                    fileList.AppendFormat("\"{0}\";", file);
                }
                commandLineBuilder.AppendFormat("/addmodule:{0} ", fileList.ToString());
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(System.Text.StringBuilder commandLineStringBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.OutputFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.OutputFilePath), false);
            }
            if (null != this.ProgramDatabaseFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.ProgramDatabaseFilePath), false);
            }

            return directoriesToCreate;
        }
    }
}