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
        {
            this.InitializeDefaults(node);
            this.SetDelegates(node);
        }

        private void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            this.Type = EType.File;
            this.LinkName = node.ModuleName;

            string linkDirectory = node.GetTargettedModuleBuildDirectory("");
            this.LinkDirectory = linkDirectory;
        }

        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["LinkName"].PrivateData = new SymLinkPrivateData(null);
            this["LinkDirectory"].PrivateData = new SymLinkPrivateData(null);
            this["Type"].PrivateData = new SymLinkPrivateData(TypeCL);
        }

        public override void Finalize(Opus.Core.Target target)
        {
            if (null == this.OutputPaths[SymLinkOutputFileFlags.Link])
            {
                this.OutputPaths[SymLinkOutputFileFlags.Link] = System.IO.Path.Combine(this.LinkDirectory, this.LinkName);
            }

            base.Finalize(target);
        }

        private static void TypeCL(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                Opus.Core.ValueTypeOption<EType> enumOption = option as Opus.Core.ValueTypeOption<EType>;
                if (enumOption.Value == EType.Directory)
                {
                    commandLineBuilder.Add("/D");
                }
            }
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
            directoriesToCreate.AddAbsoluteDirectory(this.LinkDirectory, false);
            return directoriesToCreate;
        }
    }
}