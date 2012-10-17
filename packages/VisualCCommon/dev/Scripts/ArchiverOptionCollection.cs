// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class ArchiverOptionCollection : C.ArchiverOptionCollection, C.IArchiverOptions, IArchiverOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // common archiver options
#if false
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(null, null);
#endif
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine, OutputTypeVisualStudio);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLine, AdditionalOptionsVisualStudio);

            // archiver specific options
            this["NoLogo"].PrivateData = new PrivateData(NoLogoCommandLine, NoLogoVisualStudio);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            (this as IArchiverOptions).NoLogo = true;

            (this as C.IArchiverOptions).OutputType = C.EArchiverOutput.StaticLibrary;
        }

        public ArchiverOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        private static void OutputTypeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.EArchiverOutput> enumOption = option as Opus.Core.ValueTypeOption<C.EArchiverOutput>;
            switch (enumOption.Value)
            {
                case C.EArchiverOutput.StaticLibrary:
                    ArchiverOptionCollection options = sender as ArchiverOptionCollection;
                    if (options.LibraryFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("/OUT:\"{0}\"", options.LibraryFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("/OUT:{0}", options.LibraryFilePath));
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.EArchiverOutput");
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary OutputTypeVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
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

        private static VisualStudioProcessor.ToolAttributeDictionary NoLogoVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ValueTypeOption<bool> noLogoOption = option as Opus.Core.ValueTypeOption<bool>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("SuppressStartupBanner", noLogoOption.Value.ToString().ToLower());
            return dictionary;
        }

        private static void AdditionalOptionsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }

        private static VisualStudioProcessor.ToolAttributeDictionary AdditionalOptionsVisualStudio(object sender, Opus.Core.Option option, Opus.Core.Target target, VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            VisualStudioProcessor.ToolAttributeDictionary dictionary = new VisualStudioProcessor.ToolAttributeDictionary();
            dictionary.Add("AdditionalOptions", stringOption.Value);
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
            // NEW STYLE
#if true
            Opus.Core.IToolsetInfo info = Opus.Core.ToolsetInfoFactory.CreateToolsetInfo(typeof(VisualC.ToolsetInfo));
            VisualStudioProcessor.IVisualStudioTargetInfo vsInfo = info as VisualStudioProcessor.IVisualStudioTargetInfo;
            VisualStudioProcessor.EVisualStudioTarget vsTarget = vsInfo.VisualStudioTarget;
#else
            VisualCCommon.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target) as VisualCCommon.Toolchain;
            VisualStudioProcessor.EVisualStudioTarget vsTarget = toolchain.VisualStudioTarget;
#endif
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Opus.Core.Exception(System.String.Format("Unsupported VisualStudio target, '{0}'", vsTarget));
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}