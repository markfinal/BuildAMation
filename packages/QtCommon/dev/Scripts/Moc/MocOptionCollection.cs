// <copyright file="MocOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public sealed partial class MocOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, IMocOptions
    {
        public MocOptionCollection()
            : base()
        {
        }

        public MocOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            IMocOptions options = this as IMocOptions;
            options.MocOutputPath = null;
            options.IncludePaths = new Opus.Core.DirectoryCollection();
            options.Defines = new C.DefineCollection();
            options.DoNotGenerateIncludeStatement = false;
            options.DoNotDisplayWarnings = false;
            options.PathPrefix = null;

            // version number of the current Qt package
            string QtVersion = Opus.Core.State.PackageInfo["Qt"].Version;
            string QtVersionFormatted = QtVersion.Replace(".", "0");
            string VersionDefine = "QT_VERSION=0x0" + QtVersionFormatted;
            options.Defines.Add(VersionDefine);
        }

        public string OutputDirectoryPath
        {
            get;
            set;
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            string mocDir = node.GetTargettedModuleBuildDirectory("src");
            this.OutputDirectoryPath = mocDir;
            MocFile mocFile = node.Module as MocFile;
            string mocPath;
            if (null != mocFile)
            {
                string sourceFilePath = mocFile.SourceFileLocation.GetSinglePath();
                string filename = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
                mocPath = System.IO.Path.Combine(mocDir, System.String.Format("{0}{1}.cpp", MocFile.Prefix, filename));
            }
            else
            {
                // TODO: would like to have a null output path for a collection, but it doesn't work for cloning reference types
                string filename = node.ModuleName;
                mocPath = System.IO.Path.Combine(mocDir, System.String.Format("{0}{1}.cpp", MocFile.Prefix, filename));
            }

            (this as IMocOptions).MocOutputPath = mocPath;
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            if (!this.OutputPaths.Has(OutputFileFlags.MocGeneratedSourceFile))
            {
                this.OutputPaths[OutputFileFlags.MocGeneratedSourceFile] = (this as IMocOptions).MocOutputPath;
            }

            base.FinalizeOptions(node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection dirsToCreate = new Opus.Core.DirectoryCollection();

            if (this.OutputPaths.Has(OutputFileFlags.MocGeneratedSourceFile))
            {
                var options = this as IMocOptions;
                string mocDir = System.IO.Path.GetDirectoryName(options.MocOutputPath);
                dirsToCreate.Add(mocDir);
            }

            return dirsToCreate;
        }
    }
}