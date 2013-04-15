// <copyright file="SymLinkOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public sealed partial class SymLinkOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, ISymLinkOptions
    {
        public SymLinkOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            ISymLinkOptions options = this as ISymLinkOptions;

            options.Type = EType.File;
            options.LinkName = node.ModuleName;

            string linkDirectory = node.GetTargettedModuleBuildDirectory("");
            options.LinkDirectory = linkDirectory;
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            if (null == this.OutputPaths[SymLinkOutputFileFlags.Link])
            {
                ISymLinkOptions options = this as ISymLinkOptions;
                this.OutputPaths[SymLinkOutputFileFlags.Link] = System.IO.Path.Combine(options.LinkDirectory, options.LinkName);
            }

            base.FinalizeOptions(node);
        }

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
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();
            directoriesToCreate.AddAbsoluteDirectory((this as ISymLinkOptions).LinkDirectory, false);
            return directoriesToCreate;
        }
    }
}