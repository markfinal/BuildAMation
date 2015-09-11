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
namespace QtCommon
{
namespace V2
{
    namespace MocExtension
    {
        public static class MocExtension
        {
            public static System.Tuple<Bam.Core.V2.Module, Bam.Core.V2.Module>
            MocHeader(
                this C.Cxx.V2.ObjectFileCollection module,
                C.V2.HeaderFile header)
            {
                // moc file
                var mocFile = Bam.Core.V2.Module.Create<MocModule>(module);
                mocFile.SourceHeader = header;
                // TODO: reinstate this - but causes an exception in finding the encapsulating module
                //mocFile.DependsOn(header);

                // compile source
                var objFile = module.AddFile(MocModule.Key, mocFile);

                return new System.Tuple<Bam.Core.V2.Module, Bam.Core.V2.Module>(mocFile, objFile);
            }
        }
    }

    public sealed class MocSettings :
        Bam.Core.V2.Settings
    {
    }

    public sealed class MocTool :
        Bam.Core.V2.PreBuiltTool
    {
        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            return new MocSettings();
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return Bam.Core.V2.TokenizedString.Create(System.IO.Path.Combine(new[] { QtCommon.V2.Configure.InstallPath.Parse(), "bin", "moc" }), null);
            }
        }

        public override void Evaluate()
        {
            // TODO: should be able to check the executable if it used a proper TokenizedString
            this.ReasonToExecute = null;
        }
    }

    public interface IMocGenerationPolicy
    {
        void
        Moc(
            MocModule sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.ICommandLineTool mocCompiler,
            Bam.Core.V2.TokenizedString generatedMocSource,
            C.V2.HeaderFile source);
    }

    public class MocModule :
        C.V2.SourceFile
    {
        private Bam.Core.V2.PreBuiltTool Compiler;
        private C.V2.HeaderFile SourceHeaderModule;
        private IMocGenerationPolicy Policy = null;

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, Bam.Core.V2.TokenizedString.Create("$(encapsulatingpkgbuilddir)/$(config)/@basename($(mocheaderpath))_moc.cpp", this));
            this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<MocTool>();
            this.Requires(this.Compiler);
        }

        public C.V2.HeaderFile SourceHeader
        {
            get
            {
                return this.SourceHeaderModule;
            }
            set
            {
                this.SourceHeaderModule = value;
                this.Macros.Add("mocheaderpath", value.InputPath);
            }
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var generatedPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(generatedPath))
            {
                this.ReasonToExecute = Bam.Core.V2.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var sourceFileWriteTime = System.IO.File.GetLastWriteTime(generatedPath);
            var headerFileWriteTime = System.IO.File.GetLastWriteTime(this.SourceHeaderModule.InputPath.Parse());
            if (headerFileWriteTime > sourceFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.V2.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.SourceHeaderModule.InputPath);
                return;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            this.Policy.Moc(this, context, this.Compiler, this.GeneratedPaths[Key], this.SourceHeader);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "QtCommon.V2." + mode + "MocGeneration";
            this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<IMocGenerationPolicy>.Create(className);
        }
    }
}
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

#if true
#else
            // version number of the current Qt package
            var QtVersion = Bam.Core.State.PackageInfo["Qt"].Version;
            var QtVersionFormatted = QtVersion.Replace(".", "0");
            var VersionDefine = "QT_VERSION=0x0" + QtVersionFormatted;
            options.Defines.Add(VersionDefine);
#endif
        }

        public string OutputDirectoryPath
        {
            get;
            set;
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
