#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
