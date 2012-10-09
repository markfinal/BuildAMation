// <copyright file="CompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class CompilerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        // TODO:  no reason why this can't be a static utility function
        protected virtual void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            ICCompilerOptions compilerOptions = this as ICCompilerOptions;

            // TODO: I'm sure this will break lots of things, but we'll see
            //compilerOptions.ToolchainOptionCollection = ToolchainOptionCollection.GetSharedFromNode(node);

            Opus.Core.Target target = node.Target;

            compilerOptions.OutputType = ECompilerOutput.CompileOnly;
            compilerOptions.WarningsAsErrors = true;
            compilerOptions.IgnoreStandardIncludePaths = true;
            compilerOptions.TargetLanguage = ETargetLanguage.Default;

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                compilerOptions.DebugSymbols = true;
                compilerOptions.Optimization = EOptimization.Off;
            }
            else
            {
                if (!target.HasConfiguration(Opus.Core.EConfiguration.Profile))
                {
                    compilerOptions.DebugSymbols = false;
                }
                else
                {
                    compilerOptions.DebugSymbols = true;
                }
                compilerOptions.Optimization = EOptimization.Speed;
            }
            compilerOptions.CustomOptimization = "";
            compilerOptions.ShowIncludes = false;

            compilerOptions.Defines = new DefineCollection();
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_{0}", ((Opus.Core.BaseTarget)target).PlatformName('u')));
            {
                bool is64bit = Opus.Core.OSUtilities.Is64Bit(target);
                int bits = (is64bit) ? 64 : 32;
                compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_BITS={0}", bits.ToString()));
            }
            {
                bool isLittleEndian = Opus.Core.State.IsLittleEndian;
                if (isLittleEndian)
                {
                    compilerOptions.Defines.Add("D_OPUS_PLATFORM_LITTLEENDIAN");
                }
                else
                {
                    compilerOptions.Defines.Add("D_OPUS_PLATFORM_BIGENDIAN");
                }
            }
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_CONFIGURATION_{0}", ((Opus.Core.BaseTarget)target).ConfigurationName('u')));
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_TOOLCHAIN_{0}", target.Toolchain.ToUpper()));

            compilerOptions.IncludePaths = new Opus.Core.DirectoryCollection();
            compilerOptions.IncludePaths.AddAbsoluteDirectory(".", true); // explicitly add the one that is assumed

            compilerOptions.SystemIncludePaths = new Opus.Core.DirectoryCollection();

            compilerOptions.DisableWarnings = new Opus.Core.StringArray();
        }

        public CompilerOptionCollection()
            : base()
        {
        }

        public CompilerOptionCollection(Opus.Core.DependencyNode node)
        {
            this.SetNodeOwnership(node);
            this.InitializeDefaults(node);

            ICCompilerOptions compilerOptions = this as ICCompilerOptions;
            compilerOptions.AdditionalOptions = "";

            this.SetDelegates(node);
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            ObjectFile objectFileModule = node.Module as ObjectFile;
            if (null != objectFileModule)
            {
                string sourcePathName = (node.Module as ObjectFile).SourceFile.AbsolutePath;
                this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourcePathName);
            }
            else
            {
                this.OutputName = null;
            }

            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(C.Toolchain.ObjectFileOutputSubDirectory);
        }

        public string OutputName
        {
            get;
            set;
        }

        public string OutputDirectoryPath
        {
            get;
            set;
        }

        public string ObjectFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.ObjectFile];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.ObjectFile] = value;
            }
        }

        public string PreprocessedFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.PreprocessedFile];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.PreprocessedFile] = value;
            }
        }

        public override void FinalizeOptions(Opus.Core.Target target)
        {
            if (null != this.OutputName)
            {
#if true
                ICompilerInfo info = Opus.Core.State.Get("ToolsetInfo", target.Toolchain) as ICompilerInfo;
#else
                Toolchain toolchain = ToolchainFactory.GetTargetInstance(target);
#endif

                ICCompilerOptions options = this as ICCompilerOptions;
                if ((options.OutputType == ECompilerOutput.CompileOnly) && (null == this.ObjectFilePath))
                {
#if true
                    string objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + info.ObjectFileSuffix;
#else
                    string objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + toolchain.ObjectFileSuffix;
#endif
                    this.ObjectFilePath = objectPathname;
                }
                else if ((options.OutputType == ECompilerOutput.Preprocess) && (null == this.PreprocessedFilePath))
                {
#if true
                    string preprocessedPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + info.PreprocessedOutputSuffix;
#else
                    string preprocessedPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + toolchain.PreprocessedOutputSuffix;
#endif
                    this.PreprocessedFilePath = preprocessedPathname;
                }
            }

            base.FinalizeOptions(target);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}