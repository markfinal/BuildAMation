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

            compilerOptions.ToolchainOptionCollection = ToolchainOptionCollection.GetSharedFromNode(node);

            Opus.Core.Target target = node.Target;

            Opus.Core.EConfiguration configuration = target.Configuration;

            compilerOptions.OutputType = ECompilerOutput.CompileOnly;
            compilerOptions.WarningsAsErrors = true;
            compilerOptions.IgnoreStandardIncludePaths = true;
            compilerOptions.TargetLanguage = ETargetLanguage.Default;

            if (Opus.Core.EConfiguration.Debug == configuration)
            {
                compilerOptions.DebugSymbols = true;
                compilerOptions.Optimization = EOptimization.Off;
            }
            else
            {
                if (Opus.Core.EConfiguration.Profile != configuration)
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
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_PLATFORM_{0}", target.Platform.ToString().ToUpper()));
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_CONFIGURATION_{0}", target.Configuration.ToString().ToUpper()));
            compilerOptions.Defines.Add(System.String.Format("D_OPUS_TOOLCHAIN_{0}", target.Toolchain.ToUpper()));

            compilerOptions.IncludePaths = new Opus.Core.DirectoryCollection();
            compilerOptions.IncludePaths.Add(".", true); // explicitly add the one that is assumed

            compilerOptions.SystemIncludePaths = new Opus.Core.DirectoryCollection();
        }

        public CompilerOptionCollection()
            : base()
        {
        }

        public CompilerOptionCollection(Opus.Core.DependencyNode node)
        {
            this.SetNodeOwnership(node);

            this.InitializeDefaults(node);
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            ObjectFile objectFileModule = node.Module as ObjectFile;
            if (null != objectFileModule)
            {
                string sourcePathName = System.IO.Path.Combine(node.Package.Directory, (node.Module as ObjectFile).SourceFile.RelativePath);
                this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourcePathName);
            }
            else
            {
                this.OutputName = node.ModuleName;
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

        public abstract string ObjectFilePath
        {
            get;
            set;
        }

        public abstract string PreprocessedFilePath
        {
            get;
            set;
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(System.Text.StringBuilder commandLineStringBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target);
        }

        public abstract Opus.Core.DirectoryCollection DirectoriesToCreate();
    }
}