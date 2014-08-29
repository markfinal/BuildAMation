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
#endregion // License
namespace C
{
    // TODO: this does not implement any options interface
    public class Win32ResourceCompilerOptionCollection :
        Bam.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport
    {
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {}

        public
        Win32ResourceCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
        {}

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            var moduleBuildDir = locationMap[Bam.Core.State.ModuleBuildDirLocationKey];

            var outputFileDir = locationMap[C.Win32Resource.OutputDir];
            if (!outputFileDir.IsValid)
            {
                var target = node.Target;
                var compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
                var objBuildDir = moduleBuildDir.SubDirectory(compilerTool.ObjectFileOutputSubDirectory);
                (outputFileDir as Bam.Core.ScaffoldLocation).SetReference(objBuildDir);
            }

            var resourceModule = node.Module as Win32Resource;
            if (null != resourceModule)
            {
                // this only requires the end path - so grab it from the Location without resolving it
                var location = resourceModule.ResourceFileLocation;
                var sourcePathName = string.Empty;
                if (location is Bam.Core.FileLocation)
                {
                    sourcePathName = location.AbsolutePath;
                }
                else if (location is Bam.Core.DirectoryLocation)
                {
                    throw new Bam.Core.Exception("Cannot use a directory for compiler options");
                }
                else
                {
                    sourcePathName = (location as Bam.Core.ScaffoldLocation).Pattern;
                }
                this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourcePathName);
            }
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            if (!locationMap[C.Win32Resource.OutputFile].IsValid)
            {
                var target = node.Target;
                var resourceCompilerTool = target.Toolset.Tool(typeof(IWinResourceCompilerTool)) as IWinResourceCompilerTool;
                var objectFile = this.OutputName + resourceCompilerTool.CompiledResourceSuffix;
                (locationMap[C.Win32Resource.OutputFile] as Bam.Core.ScaffoldLocation).SpecifyStub(locationMap[C.Win32Resource.OutputDir], objectFile, Bam.Core.Location.EExists.WillExist);
            }

            base.FinalizeOptions(node);
        }

        public string OutputName
        {
            get;
            set;
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
