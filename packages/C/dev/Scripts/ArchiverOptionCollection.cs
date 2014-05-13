// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ArchiverOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;

            var archiverOptions = this as IArchiverOptions;
            archiverOptions.AdditionalOptions = "";
        }

        protected override void SetNodeSpecificData(Opus.Core.DependencyNode node)
        {
            var locationMap = this.OwningNode.Module.Locations;
            var moduleBuildDir = locationMap[Opus.Core.State.ModuleBuildDirLocationKey];

            var location = locationMap[C.StaticLibrary.OutputDirLocKey] as Opus.Core.ScaffoldLocation;
            if (!location.IsValid)
            {
                var target = node.Target;
                var tool = target.Toolset.Tool(typeof(IArchiverTool)) as IArchiverTool;
                var outputDir = moduleBuildDir.SubDirectory(tool.StaticLibraryOutputSubDirectory);
                location.SetReference(outputDir);
            }

            base.SetNodeSpecificData(node);
        }

        public ArchiverOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public string OutputName
        {
            get;
            set;
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            var archiveFile = node.Module.Locations[C.StaticLibrary.OutputFileLocKey] as Opus.Core.ScaffoldLocation;
            if (!archiveFile.IsValid)
            {
                var target = node.Target;
                var tool = target.Toolset.Tool(typeof(IArchiverTool)) as IArchiverTool;
                var filename = tool.StaticLibraryPrefix + this.OutputName + tool.StaticLibrarySuffix;
                archiveFile.SpecifyStub(node.Module.Locations[C.StaticLibrary.OutputDirLocKey], filename, Opus.Core.Location.EExists.WillExist);
            }

            base.FinalizeOptions(node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}