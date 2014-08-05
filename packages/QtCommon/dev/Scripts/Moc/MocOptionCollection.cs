// <copyright file="MocOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public sealed partial class MocOptionCollection :
        Opus.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport,
        IMocOptions
    {
        public
        MocOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode node)
        {
            var options = this as IMocOptions;
            options.IncludePaths = new Opus.Core.DirectoryCollection();
            options.Defines = new C.DefineCollection();
            options.DoNotGenerateIncludeStatement = false;
            options.DoNotDisplayWarnings = false;
            options.PathPrefix = null;

            // version number of the current Qt package
            var QtVersion = Opus.Core.State.PackageInfo["Qt"].Version;
            var QtVersionFormatted = QtVersion.Replace(".", "0");
            var VersionDefine = "QT_VERSION=0x0" + QtVersionFormatted;
            options.Defines.Add(VersionDefine);
        }

        protected override void
        SetNodeSpecificData(
            Opus.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            if (!locationMap[MocFile.OutputDir].IsValid)
            {
                (locationMap[MocFile.OutputDir] as Opus.Core.ScaffoldLocation).SpecifyStub(locationMap[Opus.Core.State.ModuleBuildDirLocationKey], "src", Opus.Core.Location.EExists.WillExist);
            }
        }

        public override void
        FinalizeOptions(
            Opus.Core.DependencyNode node)
        {
            var module = node.Module;
            var mocModule = module as QtCommon.MocFile;
            if (null != mocModule)
            {
                var locationMap = module.Locations;

                var mocFile = locationMap[MocFile.OutputFile] as Opus.Core.ScaffoldLocation;
                if (!mocFile.IsValid)
                {
                    var sourceFilePath = mocModule.SourceFileLocation.GetSinglePath();
                    var filename = MocFile.Prefix + System.IO.Path.GetFileNameWithoutExtension(sourceFilePath) + ".cpp";
                    mocFile.SpecifyStub(locationMap[MocFile.OutputDir], filename, Opus.Core.Location.EExists.WillExist);
                }
            }

            base.FinalizeOptions(node);
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
