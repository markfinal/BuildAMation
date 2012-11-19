// <copyright file="CompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class CompilerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            ICCompilerOptions compilerOptions = this as ICCompilerOptions;

            Opus.Core.Target target = node.Target;

            compilerOptions.OutputType = ECompilerOutput.CompileOnly;
            compilerOptions.WarningsAsErrors = true;
            compilerOptions.IgnoreStandardIncludePaths = true;
            compilerOptions.TargetLanguage = ETargetLanguage.Default;

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                compilerOptions.DebugSymbols = true;
                compilerOptions.Optimization = EOptimization.Off;
                compilerOptions.OmitFramePointer = false;
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
                compilerOptions.OmitFramePointer = true;
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
            compilerOptions.AdditionalOptions = "";
        }

        public CompilerOptionCollection()
            : base()
        {
        }

        public CompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
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

            Opus.Core.Target target = node.Target;
            ICompilerTool compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(compilerTool.ObjectFileOutputSubDirectory);
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
                ICompilerTool compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
                ICCompilerOptions options = this as ICCompilerOptions;
                if ((options.OutputType == ECompilerOutput.CompileOnly) && (null == this.ObjectFilePath))
                {
                    string objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + compilerTool.ObjectFileSuffix;
                    this.ObjectFilePath = objectPathname;
                }
                else if ((options.OutputType == ECompilerOutput.Preprocess) && (null == this.PreprocessedFilePath))
                {
                    string preprocessedPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + compilerTool.PreprocessedOutputSuffix;
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