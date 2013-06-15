// <copyright file="Win32ResourceCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    // TODO: this does not implement any options interface
    public class Win32ResourceCompilerOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // do nothing yet
        }

        public Win32ResourceCompilerOptionCollection()
            : base()
        {
        }

        public Win32ResourceCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode owningNode)
        {
            // do nothing
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            Win32Resource resourceModule = node.Module as Win32Resource;
            if (null != resourceModule)
            {
                string sourcePathName = resourceModule.ResourceFile.AbsolutePath;
                this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourcePathName);
            }

            Opus.Core.Target target = node.Target;
            ICompilerTool compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(compilerTool.ObjectFileOutputSubDirectory);
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            if (!this.OutputPaths.Has(C.OutputFileFlags.Win32CompiledResource))
            {
                Opus.Core.Target target = node.Target;
                var resourceCompilerTool = target.Toolset.Tool(typeof(IWinResourceCompilerTool)) as IWinResourceCompilerTool;
                string objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + resourceCompilerTool.CompiledResourceSuffix;
                this.CompiledResourceFilePath = objectPathname;
            }

            base.FinalizeOptions(node);
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

        public string CompiledResourceFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.Win32CompiledResource][0];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.Win32CompiledResource] = new Opus.Core.StringArray(value);
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directories = new Opus.Core.DirectoryCollection();
            directories.Add(System.IO.Path.GetDirectoryName(this.CompiledResourceFilePath), false);
            return directories;
        }
    }
}
