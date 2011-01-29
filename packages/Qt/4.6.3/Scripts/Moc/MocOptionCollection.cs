// <copyright file="MocOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
namespace Qt
{
    public sealed class MocOutputPathFlag : Opus.Core.FlagsBase
    {
        public static MocOutputPathFlag MocGeneratedSourceFile = new MocOutputPathFlag("MocGeneratedSourceFile");

        private MocOutputPathFlag(string name)
            : base(name)
        {
        }
    }

    public sealed partial class MocOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        public MocOptionCollection()
            : base()
        {
        }

        public MocOptionCollection(Opus.Core.DependencyNode node)
        {
            this.SetDefaults(node);
            this.SetNodeOwnership(node);
            this.SetDelegates(node);
        }

        private void SetDefaults(Opus.Core.DependencyNode node)
        {
            this.MocOutputPath = null;
            this.IncludePaths = new Opus.Core.DirectoryCollection();
            this.Defines = new C.DefineCollection();
            this.DoNotGenerateIncludeStatement = false;
            this.DoNotDisplayWarnings = false;
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            string mocDir = node.GetTargettedModuleBuildDirectory("src");
            MocFile mocFile = node.Module as MocFile;
            string mocPath;
            if (null != mocFile)
            {
                string sourceFilePath = mocFile.SourceFile.AbsolutePath;
                string filename = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
                mocPath = System.IO.Path.Combine(mocDir, System.String.Format("moc_{0}.cpp", filename));
            }
            else
            {
                // TODO: would like to have a null output path for a collection, but it doesn't work for cloning reference types
                string filename = node.ModuleName;
                mocPath = System.IO.Path.Combine(mocDir, System.String.Format("moc_{0}.cpp", filename));
            }

            this.MocOutputPath = mocPath;
        }

        private void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["MocOutputPath"].PrivateData = new MocPrivateData(MocOutputPathCommandLine);
            this["IncludePaths"].PrivateData = new MocPrivateData(IncludePathsCommandLine);
            this["Defines"].PrivateData = new MocPrivateData(DefinesCommandLine);
            this["DoNotGenerateIncludeStatement"].PrivateData = new MocPrivateData(DoNotGenerateIncludeStatementCommandLine);
            this["DoNotDisplayWarnings"].PrivateData = new MocPrivateData(DoNotDisplayWarningsCommandLine);
        }

        private static void MocOutputPathSetHandler(object sender, Opus.Core.Option option)
        {
            MocOptionCollection options = sender as MocOptionCollection;
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            options.OutputPaths[MocOutputPathFlag.MocGeneratedSourceFile] = stringOption.Value;
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

        private static void DefinesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesCollectionOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string directory in definesCollectionOption.Value)
            {
                commandLineBuilder.AppendFormat("-D{0} ", directory);
            }
        }

        private static void DoNotGenerateIncludeStatementCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("-i ");
            }
        }

        private static void DoNotDisplayWarningsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("-nw ");
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
    }
}