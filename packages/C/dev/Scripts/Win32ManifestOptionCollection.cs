// <copyright file="Win32ManifestOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    // TODO: this does not implement any options interface
    public class Win32ManifestOptionCollection :
        Opus.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport
    {
        protected override void
        SetDelegates(
            Opus.Core.DependencyNode node)
        {}

        public
        Win32ManifestOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode owningNode)
        {}

        protected override void
        SetNodeSpecificData(
            Opus.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;

            var binaryModule = node.ExternalDependents[0].Module;

            var outputFileDir = locationMap[C.Win32Manifest.OutputDir] as Opus.Core.ScaffoldLocation;
            if (!outputFileDir.IsValid)
            {
                outputFileDir.SetReference(binaryModule.Locations[C.Application.OutputDir]);
            }

            var outputFile = locationMap[C.Win32Manifest.OutputFile] as Opus.Core.ScaffoldLocation;
            if (!outputFile.IsValid)
            {
                outputFile.SetReference(binaryModule.Locations[C.Application.OutputFile]);
            }
        }

        void
        CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(
            Opus.Core.StringArray commandLineBuilder,
            Opus.Core.Target target,
            Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}
