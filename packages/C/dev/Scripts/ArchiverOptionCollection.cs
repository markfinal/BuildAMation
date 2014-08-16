// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public abstract class ArchiverOptionCollection :
        Bam.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;

            var archiverOptions = this as IArchiverOptions;
            archiverOptions.AdditionalOptions = "";
        }

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            var locationMap = this.OwningNode.Module.Locations;
            var moduleBuildDir = locationMap[Bam.Core.State.ModuleBuildDirLocationKey];

            var location = locationMap[C.StaticLibrary.OutputDirLocKey] as Bam.Core.ScaffoldLocation;
            if (!location.IsValid)
            {
                var target = node.Target;
                var tool = target.Toolset.Tool(typeof(IArchiverTool)) as IArchiverTool;
                var outputDir = moduleBuildDir.SubDirectory(tool.StaticLibraryOutputSubDirectory);
                location.SetReference(outputDir);
            }

            base.SetNodeSpecificData(node);
        }

        public
        ArchiverOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        public string OutputName
        {
            get;
            set;
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var archiveFile = node.Module.Locations[C.StaticLibrary.OutputFileLocKey] as Bam.Core.ScaffoldLocation;
            if (!archiveFile.IsValid)
            {
                var target = node.Target;
                var tool = target.Toolset.Tool(typeof(IArchiverTool)) as IArchiverTool;
                var filename = tool.StaticLibraryPrefix + this.OutputName + tool.StaticLibrarySuffix;
                archiveFile.SpecifyStub(node.Module.Locations[C.StaticLibrary.OutputDirLocKey], filename, Bam.Core.Location.EExists.WillExist);
            }

            base.FinalizeOptions(node);
        }

        void
        CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(
            Bam.Core.StringArray commandLineBuilder,
            Bam.Core.Target target,
            Bam.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}
