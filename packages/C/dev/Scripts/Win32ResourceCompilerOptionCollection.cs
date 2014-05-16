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

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode owningNode)
        {
            // do nothing
        }

        protected override void SetNodeSpecificData(Opus.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            var moduleBuildDir = locationMap[Opus.Core.State.ModuleBuildDirLocationKey];

            var outputFileDir = locationMap[C.Win32Resource.OutputDir];
            if (!outputFileDir.IsValid)
            {
                var target = node.Target;
                var compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
                var objBuildDir = moduleBuildDir.SubDirectory(compilerTool.ObjectFileOutputSubDirectory);
                (outputFileDir as Opus.Core.ScaffoldLocation).SetReference(objBuildDir);
            }

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
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
#if true
            var locationMap = node.Module.Locations;
            if (!locationMap[C.Win32Resource.OutputFile].IsValid)
            {
                var target = node.Target;
                var resourceCompilerTool = target.Toolset.Tool(typeof(IWinResourceCompilerTool)) as IWinResourceCompilerTool;
                var objectFile = this.OutputName + resourceCompilerTool.CompiledResourceSuffix;
                (locationMap[C.Win32Resource.OutputFile] as Opus.Core.ScaffoldLocation).SpecifyStub(locationMap[C.Win32Resource.OutputDir], objectFile, Opus.Core.Location.EExists.WillExist);
            }
#else
            if (!this.OutputPaths.Has(C.OutputFileFlags.Win32CompiledResource))
            {
                var target = node.Target;
                var resourceCompilerTool = target.Toolset.Tool(typeof(IWinResourceCompilerTool)) as IWinResourceCompilerTool;
                var objectPathname = System.IO.Path.Combine(this.OutputDirectoryPath, this.OutputName) + resourceCompilerTool.CompiledResourceSuffix;
                this.CompiledResourceFilePath = objectPathname;
            }
#endif

            base.FinalizeOptions(node);
        }

        public string OutputName
        {
            get;
            set;
        }

#if true
#else
        public string OutputDirectoryPath
        {
            get;
            set;
        }
#endif

#if true
#else
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
#endif

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}
