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
            var moduleBuildDir = this.GetModuleLocation(Bam.Core.State.ModuleBuildDirLocationKey);
            var location = this.GetModuleLocation(C.StaticLibrary.OutputDirLocKey) as Bam.Core.ScaffoldLocation;
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
