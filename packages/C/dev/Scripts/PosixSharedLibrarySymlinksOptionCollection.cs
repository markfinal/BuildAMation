// <copyright file="PosixSharedLibrarySymlinksOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    // TODO: this does not implement any options interface
    public class PosixSharedLibrarySymlinksOptionCollection :
        Opus.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // do nothing yet
        }

        public PosixSharedLibrarySymlinksOptionCollection(Opus.Core.DependencyNode node)
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

            var moduleWithPostAction = node.ExternalDependents[0].Module;

            (node.Module as PosixSharedLibrarySymlinks).RealSharedLibraryFileLocation = moduleWithPostAction.Locations[C.DynamicLibrary.OutputFile];

            var outputFileDir = locationMap[C.PosixSharedLibrarySymlinks.OutputDir] as Opus.Core.ScaffoldLocation;
            if (!outputFileDir.IsValid)
            {
                outputFileDir.SetReference(moduleWithPostAction.Locations[C.DynamicLibrary.OutputDir]);
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target, excludedOptionNames);
        }
    }
}
