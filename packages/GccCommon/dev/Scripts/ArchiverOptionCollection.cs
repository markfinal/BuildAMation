// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public sealed class ArchiverOutputPathFlag : C.ArchiverOutputPathFlag
    {
        private ArchiverOutputPathFlag(string name)
            : base(name)
        {
        }
    }

    public abstract partial class ArchiverOptionCollection : C.ArchiverOptionCollection, C.IArchiverOptions, IArchiverOptions
    {
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
                return this.OutputPaths[ArchiverOutputPathFlag.LibraryFile];
            }

            set
            {
                this.OutputPaths[ArchiverOutputPathFlag.LibraryFile] = value;
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