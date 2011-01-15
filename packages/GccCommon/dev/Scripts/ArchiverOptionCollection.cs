// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract partial class ArchiverOptionCollection : C.ArchiverOptionCollection, C.IArchiverOptions, IArchiverOptions, Opus.Core.IOutputPaths
    {
        private enum EOutputFile
        {
            LibraryFile = 0
        }

        private System.Collections.Generic.Dictionary<EOutputFile, string> outputFileMap = new System.Collections.Generic.Dictionary<EOutputFile, string>();
        System.Collections.Generic.Dictionary<string, string> Opus.Core.IOutputPaths.GetOutputPaths()
        {
            System.Collections.Generic.Dictionary<string, string> pathMap = new System.Collections.Generic.Dictionary<string, string>();
            foreach (System.Collections.Generic.KeyValuePair<EOutputFile, string> file in this.outputFileMap)
            {
                pathMap.Add(file.Key.ToString(), file.Value);
            }
            return pathMap;
        }

        private void SetDelegates(Opus.Core.Target target)
        {
            // common archiver options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(null);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine);

            // archiver specific options
            this["Command"].PrivateData = new PrivateData(CommandCommandLine);
            this["DoNotWarnIfLibraryCreated"].PrivateData = new PrivateData(DoNotWarnIfLibraryCreatedCommandLine);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            this.Command = EArchiverCommand.Replace;
            this.DoNotWarnIfLibraryCreated = true;

            Opus.Core.Target target = node.Target;

            // this must be set last, as it appears last on the command line
            this.OutputType = C.EArchiverOutput.StaticLibrary;

            this.SetDelegates(target);
        }

        public ArchiverOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override string LibraryFilePath
        {
            get
            {
                if (this.outputFileMap.ContainsKey(EOutputFile.LibraryFile))
                {
                    return this.outputFileMap[EOutputFile.LibraryFile];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    this.outputFileMap[EOutputFile.LibraryFile] = value;
                }
                else if (this.outputFileMap.ContainsKey(EOutputFile.LibraryFile))
                {
                    this.outputFileMap.Remove(EOutputFile.LibraryFile);
                }
            }
        }

        protected static void OutputTypeSetHandler(object sender, Opus.Core.Option option)
        {
            ArchiverOptionCollection options = sender as ArchiverOptionCollection;
            Opus.Core.ValueTypeOption<C.EArchiverOutput> enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    {
                        string libraryPathname = System.IO.Path.Combine(options.OutputDirectoryPath, "lib" + options.OutputName + ".a");
                        options.LibraryFilePath = libraryPathname;
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }

        private static void OutputTypeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EArchiverOutput> enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    ArchiverOptionCollection options = sender as ArchiverOptionCollection;
                    commandLineBuilder.AppendFormat("\"{0}\" ", options.LibraryFilePath);
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }

        private static void CommandCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<EArchiverCommand> commandOption = option as Opus.Core.ValueTypeOption<EArchiverCommand>;
            switch (commandOption.Value)
            {
                case EArchiverCommand.Replace:
                    commandLineBuilder.Append("-r ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized command option");
            }
        }

        private static void DoNotWarnIfLibraryCreatedCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Append("-c ");
            }
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.LibraryFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.LibraryFilePath), false);
            }

            return directoriesToCreate;
        }
    }
}