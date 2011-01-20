namespace Qt
{
    public sealed partial class MocOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, Opus.Core.IOutputPaths
    {
        private void SetDefaults(Opus.Core.DependencyNode node)
        {
            this.MocOutputPath = null;
            this.IncludePaths = new Opus.Core.DirectoryCollection();
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            MocFile mocFile = node.Module as MocFile;
            string sourceFilePath = mocFile.SourceFile.AbsolutePath;
            string filename = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
            string mocDir = node.GetTargettedModuleBuildDirectory("src");
            string mocPath = System.IO.Path.Combine(mocDir, System.String.Format("moc_{0}.cpp", filename));

            this.MocOutputPath = mocPath;
        }

        private void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["MocOutputPath"].PrivateData = new MocPrivateData(MocOutputPathCommandLine);
            this["IncludePaths"].PrivateData = new MocPrivateData(IncludePathsCommandLine);
        }

        public MocOptionCollection(Opus.Core.DependencyNode node)
        {
            this.SetDefaults(node);
            this.SetNodeOwnership(node);
            this.SetDelegates(node);
        }

        private static void MocOutputPathCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            commandLineBuilder.AppendFormat("-o\"{0}\" ", stringOption.Value);
        }

        private static void IncludePathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> directoryCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string directory in directoryCollectionOption.Value)
            {
                commandLineBuilder.AppendFormat("-I\"{0}\" ", directory);
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(System.Text.StringBuilder commandLineStringBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection dirsToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.MocOutputPath)
            {
                string mocDir = System.IO.Path.GetDirectoryName(this.MocOutputPath);
                dirsToCreate.Add(null, mocDir);
            }

            return dirsToCreate;
        }

        System.Collections.Generic.Dictionary<string, string> Opus.Core.IOutputPaths.GetOutputPaths()
        {
            System.Collections.Generic.Dictionary<string, string> map = new System.Collections.Generic.Dictionary<string, string>();
            if (null != this.MocOutputPath)
            {
                map.Add("OutputMocSourceFile", this.MocOutputPath);
            }
            return map;
        }
    }
}