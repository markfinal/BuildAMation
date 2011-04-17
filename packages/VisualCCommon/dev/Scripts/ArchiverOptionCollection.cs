// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class ArchiverOptionCollection : C.ArchiverOptionCollection, C.IArchiverOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        private void SetDelegates(Opus.Core.Target target)
        {
            // common archiver options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(null, null);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine, OutputTypeVisualStudio);

            // archiver specific options
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLine, NoLogoVisualStudio);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            this.NoLogo = true;

            Opus.Core.Target target = node.Target;

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
                return this.OutputPaths[C.OutputFileFlags.StaticLibrary];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.StaticLibrary] = value;
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
                        string libraryPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName + ".lib");
                        options.LibraryFilePath = libraryPathname;
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }

        private static void OutputTypeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EArchiverOutput> enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    ArchiverOptionCollection options = sender as ArchiverOptionCollection;
                    commandLineBuilder.Add(System.String.Format("/OUT:\"{0}\"", options.LibraryFilePath));
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EArchiverOutput> enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    {
                        VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
                        ArchiverOptionCollection options = sender as ArchiverOptionCollection;
                        dictionary.Add("OutputFile", options.LibraryFilePath);
                        return dictionary;
                    }

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }

        private static void NoLogoCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            if (noLogoOption.Value)
            {
                commandLineBuilder.Add("/NOLOGO");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return dictionary;
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.LibraryFilePath)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(this.LibraryFilePath), false);
            }

            return directoriesToCreate;
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target);
            return dictionary;
        }
    }
}