// <copyright file="CopyFileOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public partial class CopyFileOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, ICopyFileOptions
    {
        public CopyFileOptionCollection()
            : base()
        {
        }

        public CopyFileOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        #region implemented abstract members of BaseOptionCollection
        protected override void InitializeDefaults(Opus.Core.DependencyNode owningNode)
        {
            ICopyFileOptions options = this as ICopyFileOptions;
            options.DestinationDirectory = null;
            if (typeof(CopyDirectory).IsInstanceOfType(owningNode.Module))
            {
                options.CommonBaseDirectory = (owningNode.Module as CopyDirectory).CommonBaseDirectory;
            }
            else
            {
                options.CommonBaseDirectory = null;
            }
            options.DestinationModuleType = null;
            options.DestinationModuleOutputEnum = null;
            options.SourceModuleType = null;
            options.SourceModuleOutputEnum = null;
        }
        #endregion

        private Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            if (typeof(CopyFile).IsInstanceOfType(node.Module))
            {
                ICopyFileOptions options = this as ICopyFileOptions;

                if (options.SourceModuleType != null)
                {
                    var sourceModule = Opus.Core.ModuleUtilities.GetModule(options.SourceModuleType, (Opus.Core.BaseTarget)node.Target);
                    if (null == sourceModule)
                    {
                        throw new Opus.Core.Exception("Source module to copy from '{0}' has not been created", options.SourceModuleType.ToString());
                    }
                    string sourceModuleOutputPath = sourceModule.Options.OutputPaths[options.SourceModuleOutputEnum];
                    if (null == sourceModuleOutputPath)
                    {
                        throw new Opus.Core.Exception("Source module '{0}' has no output path of type '{1}'",
                                                      options.SourceModuleType.ToString(),
                                                      options.SourceModuleOutputEnum.ToString());
                    }
                    (node.Module as CopyFile).SetGuaranteedAbsolutePath(sourceModuleOutputPath);
                }

                string sourcePath = (node.Module as CopyFile).SourceFile.AbsolutePath;

                string destinationDirectory;
                if (options.DestinationDirectory != null)
                {
                    destinationDirectory = options.DestinationDirectory;
                }
                else if (options.DestinationModuleType != null)
                {
                    var destinationModule = Opus.Core.ModuleUtilities.GetModule(options.DestinationModuleType, (Opus.Core.BaseTarget)node.Target);
                    if (null == destinationModule)
                    {
                        throw new Opus.Core.Exception("Module to copy next to '{0}' has not been created", options.DestinationModuleType.ToString());
                    }
                    destinationDirectory = System.IO.Path.GetDirectoryName(destinationModule.Options.OutputPaths[options.DestinationModuleOutputEnum]);
                }
                else
                {
                    destinationDirectory = node.GetModuleBuildDirectory();
                }

                if (null != options.CommonBaseDirectory)
                {
                    string relPath = Opus.Core.RelativePathUtilities.GetPath(sourcePath, options.CommonBaseDirectory);
                    this.OutputPaths[OutputFileFlags.CopiedFile] = System.IO.Path.Combine(destinationDirectory, relPath);
                }
                else
                {
                    string sourceFileName = System.IO.Path.GetFileName(sourcePath);
                    this.OutputPaths[OutputFileFlags.CopiedFile] = System.IO.Path.Combine(destinationDirectory, sourceFileName);
                }

                string parentDir = System.IO.Path.GetDirectoryName(this.OutputPaths[OutputFileFlags.CopiedFile]);
                directoriesToCreate.AddAbsoluteDirectory(parentDir, false);
            }
            base.FinalizeOptions (node);
        }

        #region ICommandLineSupport implementation

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                commandLineBuilder.Add("/c");
                commandLineBuilder.Add("COPY");
            }
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            return this.directoriesToCreate;
        }

        #endregion
    }
}
