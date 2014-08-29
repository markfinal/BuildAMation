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
namespace CSharp
{
    public partial class OptionCollection :
        Bam.Core.BaseOptionCollection,
        IOptions,
        CommandLineProcessor.ICommandLineSupport,
        VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            var target = node.Target;

            var options = this as IOptions;
            options.Target = ETarget.Executable;
            options.NoLogo = true;
            options.Platform = EPlatform.AnyCpu;
            options.Checked = true;
            options.Unsafe = false;
            options.WarningLevel = EWarningLevel.Level4;
            options.WarningsAsErrors = true;
            options.Defines = new Bam.Core.StringArray();

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                options.DebugInformation = EDebugInformation.Full;
                options.Optimize = false;
                options.Defines.Add("DEBUG");
            }
            else
            {
                if (!target.HasConfiguration(Bam.Core.EConfiguration.Profile))
                {
                    options.DebugInformation = EDebugInformation.Disabled;
                }
                else
                {
                    options.DebugInformation = EDebugInformation.Full;
                    options.Defines.Add("TRACE");
                }
                options.Optimize = true;
            }

            options.References = new Bam.Core.FileCollection();
            options.Modules = new Bam.Core.FileCollection();
        }

        public
        OptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        public string OutputName
        {
            get;
            set;
        }

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            this.OutputName = node.ModuleName;
            (node.Module.Locations[Assembly.OutputDir] as Bam.Core.ScaffoldLocation).SpecifyStub(node.Module.Locations[Bam.Core.State.ModuleBuildDirLocationKey], "bin", Bam.Core.Location.EExists.WillExist);
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var options = this as IOptions;

            if (!node.Module.Locations[Assembly.OutputFile].IsValid)
            {
                string outputSuffix;
                switch (options.Target)
                {
                    case ETarget.Executable:
                    case ETarget.WindowsExecutable:
                        outputSuffix = ".exe";
                        break;

                    case ETarget.Library:
                        outputSuffix = ".dll";
                        break;

                    case ETarget.Module:
                        outputSuffix = ".netmodule";
                        break;

                    default:
                        throw new Bam.Core.Exception("Unrecognized CSharp.ETarget value");
                }

                (node.Module.Locations[Assembly.OutputFile] as Bam.Core.ScaffoldLocation).SpecifyStub(node.Module.Locations[Assembly.OutputDir], this.OutputName + outputSuffix, Bam.Core.Location.EExists.WillExist);
            }

            if (options.DebugInformation != EDebugInformation.Disabled)
            {
                var locationMap = node.Module.Locations;
                var pdbDir = locationMap[Assembly.PDBDir] as Bam.Core.ScaffoldLocation;
                if (!pdbDir.IsValid)
                {
                    pdbDir.SetReference(locationMap[Assembly.OutputDir]);
                }

                var pdbFile = locationMap[Assembly.PDBFile] as Bam.Core.ScaffoldLocation;
                if (!pdbFile.IsValid)
                {
                    pdbFile.SpecifyStub(pdbDir, this.OutputName + ".pdb", Bam.Core.Location.EExists.WillExist);
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

        VisualStudioProcessor.ToolAttributeDictionary
        VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(
            Bam.Core.Target target)
        {
            var dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, VisualStudioProcessor.EVisualStudioTarget.MSBUILD);
            return dictionary;
        }
    }
}
