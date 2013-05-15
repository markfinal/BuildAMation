// <copyright file="SymlinkOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public sealed partial class SymlinkOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, ISymlinkOptions
    {
        public SymlinkOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        #region implemented abstract members of BaseOptionCollection
        protected override void InitializeDefaults(Opus.Core.DependencyNode owningNode)
        {
            var options = this as ISymlinkOptions;
            options.TargetName = null;
            options.DestinationModuleType = null;
            options.DestinationModuleOutputEnum = null;
            options.SourceModuleType = null;
            options.SourceModuleOutputEnum = null;
        }
        #endregion

        private Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            var options = node.Module.Options as ISymlinkOptions;
            Opus.Core.BaseTarget baseTarget = (Opus.Core.BaseTarget)node.Target;

            if (options.SourceModuleType != null)
            {
                var sourceModule = Opus.Core.ModuleUtilities.GetModule(options.SourceModuleType, baseTarget);
                if (null == sourceModule)
                {
                    throw new Opus.Core.Exception("Source module to symlink from '{0}' has not been created", options.SourceModuleType.ToString());
                }
                string sourceModuleOutputPath = sourceModule.Options.OutputPaths[options.SourceModuleOutputEnum];
                (node.Module as SymlinkFile).SetGuaranteedAbsolutePath(sourceModuleOutputPath);
            }

            if (null == this.OutputPaths[OutputFileFlags.Symlink])
            {
                string destinationDirectory;
                if (null != options.DestinationModuleType)
                {
                    var destinationModule = Opus.Core.ModuleUtilities.GetModule(options.DestinationModuleType, baseTarget);
                    if (null == destinationModule)
                    {
                        throw new Opus.Core.Exception("Module to symlink next to '{0}' has not been created", options.DestinationModuleType.ToString());
                    }
                    destinationDirectory = System.IO.Path.GetDirectoryName(destinationModule.Options.OutputPaths[options.DestinationModuleOutputEnum]);
                }
                else
                {
                    destinationDirectory = node.GetModuleBuildDirectory();
                }
                this.directoriesToCreate.AddAbsoluteDirectory(destinationDirectory, false);

                string targetName;
                if (null != options.TargetName)
                {
                    targetName = options.TargetName;
                }
                else
                {
                    var module = node.Module as SymlinkBase;
                    string filename = System.IO.Path.GetFileName(module.SourceFile.AbsolutePath);
                    targetName = filename;
                }

                this.OutputPaths[OutputFileFlags.Symlink] = System.IO.Path.Combine(destinationDirectory, targetName);
            }

            base.FinalizeOptions (node);
        }

        #region ICommandLineSupport implementation

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                // Windows requires additional options passed to the cmd executable
                commandLineBuilder.Add("/c");
                commandLineBuilder.Add("MKLINK");
            }
            else
            {
                // soft link
                commandLineBuilder.Add("-s");
            }

            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            return this.directoriesToCreate;
        }

        #endregion
    }
}
