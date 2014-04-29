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
            var resourceModule = node.Module as Win32Resource;
            if (null != resourceModule)
            {
                // this only requires the end path - so grab it from the Location without resolving it
                var location = resourceModule.ResourceFileLocation;
                var sourcePathName = string.Empty;
                if (location is Opus.Core.FileLocation)
                {
                    sourcePathName = location.AbsolutePath;
                }
                else if (location is Opus.Core.DirectoryLocation)
                {
                    throw new Opus.Core.Exception("Cannot use a directory for compiler options");
                }
                else
                {
                    sourcePathName = (location as Opus.Core.ScaffoldLocation).Pattern;
                }
                this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourcePathName);
            }

            var target = node.Target;
            var compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
            this.OutputDirectoryPath = node.GetTargettedModuleBuildDirectory(compilerTool.ObjectFileOutputSubDirectory);
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            if (!this.OutputPaths.Has(C.OutputFileFlags.Win32CompiledResource))
            {
                var target = node.Target;
                var resourceCompilerTool = target.Toolset.Tool(typeof(IWinResourceCompilerTool)) as IWinResourceCompilerTool;
                var objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + resourceCompilerTool.CompiledResourceSuffix;
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
                return this.OutputPaths[C.OutputFileFlags.Win32CompiledResource];
            }

            set
            {
                this.OutputPaths[C.OutputFileFlags.Win32CompiledResource] = value;
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            var directories = new Opus.Core.DirectoryCollection();
            directories.Add(System.IO.Path.GetDirectoryName(this.CompiledResourceFilePath));
            return directories;
        }
    }
}
