// <copyright file="CopyFileOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public partial class CopyFileOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, ICopyFileOptions
    {
        public CopyFileOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        #region implemented abstract members of BaseOptionCollection
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode owningNode)
        {
            ICopyFileOptions options = this as ICopyFileOptions;
            options.DestinationDirectory = null;
            if (typeof(CopyDirectory).IsInstanceOfType(owningNode.Module))
            {
                var copyDirectory = owningNode.Module as CopyDirectory;
                if (copyDirectory.CommonBaseDirectory != null)
                {
                    options.CommonBaseDirectory = copyDirectory.CommonBaseDirectory.GetSinglePath();
                }
                else
                {
                    options.CommonBaseDirectory = null;
                }
            }
            else
            {
                options.CommonBaseDirectory = null;
            }
            options.DestinationModuleType = null;
            options.DestinationModuleOutputLocation = null;
            options.DestinationRelativePath = null;
            options.SourceModuleType = null;
            options.SourceModuleOutputLocation = null;
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
#if true
                    var sourceLocation = (sourceModule as Opus.Core.BaseModule).Locations[options.SourceModuleOutputLocation];
                    if (!sourceLocation.IsValid)
                    {
                        throw new Opus.Core.Exception("Source module '{0}' has no output path of type '{1}'",
                                                      options.SourceModuleType.ToString(),
                                                      options.SourceModuleOutputLocation.ToString());
                    }
                    (node.Module as CopyFile).SourceFileLocation = sourceLocation;
#else
                    string sourceModuleOutputPath = sourceModule.Options.OutputPaths[options.SourceModuleOutputEnum];
                    if (null == sourceModuleOutputPath)
                    {
                        throw new Opus.Core.Exception("Source module '{0}' has no output path of type '{1}'",
                                                      options.SourceModuleType.ToString(),
                                                      options.SourceModuleOutputEnum.ToString());
                    }
                    (node.Module as CopyFile).SourceFileLocation = Opus.Core.FileLocation.Get(sourceModuleOutputPath, Opus.Core.Location.EExists.WillExist);
#endif
                }

                var sourcePath = (node.Module as CopyFile).SourceFileLocation.GetSinglePath();

#if true
                Opus.Core.Location destinationDir;
                if (options.DestinationDirectory != null)
                {
                    destinationDir = Opus.Core.DirectoryLocation.Get(options.DestinationDirectory);
                }
                else if (options.DestinationModuleType != null)
                {
                    var destinationModule = Opus.Core.ModuleUtilities.GetModule(options.DestinationModuleType, (Opus.Core.BaseTarget)node.Target);
                    if (null == destinationModule)
                    {
                        throw new Opus.Core.Exception("Module to copy next to '{0}' has not been created", options.DestinationModuleType.ToString());
                    }
                    destinationDir = (destinationModule as Opus.Core.BaseModule).Locations[options.DestinationModuleOutputLocation];
                    if (null != options.DestinationRelativePath)
                    {
                        destinationDir = destinationDir.SubDirectory(options.DestinationRelativePath);
                    }
                }
                else
                {
                    destinationDir = node.Module.Locations[Opus.Core.State.ModuleBuildDirLocationKey];
                }

                if (null != options.CommonBaseDirectory)
                {
                    var relPath = Opus.Core.RelativePathUtilities.GetPath(sourcePath, options.CommonBaseDirectory);
                    (node.Module.Locations[CopyFile.OutputFile] as Opus.Core.ScaffoldLocation).SpecifyStub(destinationDir, relPath, Opus.Core.Location.EExists.WillExist);
                }
                else
                {
                    var sourceFileName = System.IO.Path.GetFileName(sourcePath);
                    (node.Module.Locations[CopyFile.OutputFile] as Opus.Core.ScaffoldLocation).SpecifyStub(destinationDir, sourceFileName, Opus.Core.Location.EExists.WillExist);
                }
#else
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
                    if (null != options.DestinationRelativePath)
                    {
                        destinationDirectory = System.IO.Path.Combine(destinationDirectory, options.DestinationRelativePath);
                        destinationDirectory = System.IO.Path.GetFullPath(destinationDirectory);
                    }
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
                directoriesToCreate.Add(parentDir);
#endif
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

        #endregion
    }
}
