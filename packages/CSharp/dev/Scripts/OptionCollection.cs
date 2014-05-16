// <copyright file="OptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public partial class OptionCollection : Opus.Core.BaseOptionCollection, IOptions, CommandLineProcessor.ICommandLineSupport, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
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

#if true
#else
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
#endif

        protected override void SetNodeSpecificData(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;
            (node.Module.Locations[Assembly.OutputDirectory] as Opus.Core.ScaffoldLocation).SpecifyStub(node.Module.Locations[Opus.Core.State.ModuleBuildDirLocationKey], "bin", Opus.Core.Location.EExists.WillExist);
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            var options = this as IOptions;

#if true
            if (!node.Module.Locations[Assembly.OutputFile].IsValid)
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

                (node.Module.Locations[Assembly.OutputFile] as Opus.Core.ScaffoldLocation).SpecifyStub(node.Module.Locations[Assembly.OutputDirectory], this.OutputName + outputSuffix, Opus.Core.Location.EExists.WillExist);
            }
#else
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
#endif

#if true
            // TODO pdbs
#else
            if ((options.DebugInformation != EDebugInformation.Disabled) && !this.OutputPaths.Has(OutputFileFlags.ProgramDatabaseFile))
            {
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    string pdbPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + ".pdb";
                    this.ProgramDatabaseFilePath = pdbPathname;
                }
            }
#endif

            base.FinalizeOptions(node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, VisualStudioProcessor.EVisualStudioTarget.MSBUILD);
            return dictionary;
        }
    }
}