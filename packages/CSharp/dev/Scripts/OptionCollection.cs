// <copyright file="OptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public partial class OptionCollection : Opus.Core.BaseOptionCollection, IOptions, CommandLineProcessor.ICommandLineSupport, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            Opus.Core.Target target = node.Target;

            IOptions options = this as IOptions;
            options.Target = ETarget.Executable;
            options.NoLogo = true;
            options.Platform = EPlatform.AnyCpu;
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

        public OptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
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

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            var options = this as IOptions;

            if (!this.OutputPaths.Has(OutputFileFlags.AssemblyFile))
            {
                string outputSuffix;
                switch (options.Target)
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

            if ((options.DebugInformation != EDebugInformation.Disabled) && !this.OutputPaths.Has(OutputFileFlags.ProgramDatabaseFile))
            {
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    string pdbPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + ".pdb";
                    this.ProgramDatabaseFilePath = pdbPathname;
                }
            }

            base.FinalizeOptions(node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
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