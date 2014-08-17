#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace QtCommon
{
    public sealed partial class MocOptionCollection :
        Bam.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport,
        IMocOptions
    {
        public
        MocOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            var options = this as IMocOptions;
            options.IncludePaths = new Bam.Core.DirectoryCollection();
            options.Defines = new C.DefineCollection();
            options.DoNotGenerateIncludeStatement = false;
            options.DoNotDisplayWarnings = false;
            options.PathPrefix = null;

            // version number of the current Qt package
            var QtVersion = Bam.Core.State.PackageInfo["Qt"].Version;
            var QtVersionFormatted = QtVersion.Replace(".", "0");
            var VersionDefine = "QT_VERSION=0x0" + QtVersionFormatted;
            options.Defines.Add(VersionDefine);
        }

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            if (!locationMap[MocFile.OutputDir].IsValid)
            {
                (locationMap[MocFile.OutputDir] as Bam.Core.ScaffoldLocation).SpecifyStub(locationMap[Bam.Core.State.ModuleBuildDirLocationKey], "src", Bam.Core.Location.EExists.WillExist);
            }
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var module = node.Module;
            var mocModule = module as QtCommon.MocFile;
            if (null != mocModule)
            {
                var locationMap = module.Locations;

                var mocFile = locationMap[MocFile.OutputFile] as Bam.Core.ScaffoldLocation;
                if (!mocFile.IsValid)
                {
                    var sourceFilePath = mocModule.SourceFileLocation.GetSinglePath();
                    var filename = MocFile.Prefix + System.IO.Path.GetFileNameWithoutExtension(sourceFilePath) + ".cpp";
                    mocFile.SpecifyStub(locationMap[MocFile.OutputDir], filename, Bam.Core.Location.EExists.WillExist);
                }
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
