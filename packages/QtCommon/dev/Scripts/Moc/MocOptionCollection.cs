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
                mocPath = System.IO.Path.Combine(mocDir, System.String.Format("{0}{1}.cpp", MocFile.Prefix, filename));
            }
            else
            {
                // TODO: would like to have a null output path for a collection, but it doesn't work for cloning reference types
                string filename = node.ModuleName;
                mocPath = System.IO.Path.Combine(mocDir, System.String.Format("{0}{1}.cpp", MocFile.Prefix, filename));
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
            options.OutputPaths[OutputFileFlags.MocGeneratedSourceFile] = stringOption.Value;
        }

        private static void MocOutputPathCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            if (stringOption.Value.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("-o\"{0}\"", stringOption.Value));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("-o{0}", stringOption.Value));
            }
        }

        private static void IncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> directoryCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string directory in directoryCollectionOption.Value)
            {
                if (directory.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-I\"{0}\"", directory));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-I{0}", directory));
                }
            }
        }

        private static void DefinesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesCollectionOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string directory in definesCollectionOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", directory));
            }
        }

        private static void DoNotGenerateIncludeStatementCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-i");
            }
        }

        private static void DoNotDisplayWarningsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-nw");
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
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