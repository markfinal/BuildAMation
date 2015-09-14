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
            public static System.Tuple<Bam.Core.Module, Bam.Core.Module>
            MocHeader(
                this C.Cxx.V2.ObjectFileCollection module,
                C.V2.HeaderFile header)
            {
                // moc file
                var mocFile = Bam.Core.Module.Create<MocModule>(module);
                mocFile.SourceHeader = header;
                // TODO: reinstate this - but causes an exception in finding the encapsulating module
                //mocFile.DependsOn(header);

                // compile source
                var objFile = module.AddFile(MocModule.Key, mocFile);

                return new System.Tuple<Bam.Core.Module, Bam.Core.Module>(mocFile, objFile);
            }
        }
    }

    public sealed class MocSettings :
        Bam.Core.Settings
    {
    }

    public sealed class MocTool :
        Bam.Core.PreBuiltTool
    {
        public override Bam.Core.Settings CreateDefaultSettings<T>(T module)
        {
            return new MocSettings();
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.Create(System.IO.Path.Combine(new[] { QtCommon.V2.Configure.InstallPath.Parse(), "bin", "moc" }), null);
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
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool mocCompiler,
            Bam.Core.TokenizedString generatedMocSource,
            C.V2.HeaderFile source);
    }

    public class MocModule :
        C.V2.SourceFile
    {
        private Bam.Core.PreBuiltTool Compiler;
        private C.V2.HeaderFile SourceHeaderModule;
        private IMocGenerationPolicy Policy = null;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, Bam.Core.TokenizedString.Create("$(encapsulatingpkgbuilddir)/$(config)/@basename($(mocheaderpath))_moc.cpp", this));
            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<MocTool>();
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
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var sourceFileWriteTime = System.IO.File.GetLastWriteTime(generatedPath);
            var headerFileWriteTime = System.IO.File.GetLastWriteTime(this.SourceHeaderModule.InputPath.Parse());
            if (headerFileWriteTime > sourceFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.SourceHeaderModule.InputPath);
                return;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            this.Policy.Moc(this, context, this.Compiler, this.GeneratedPaths[Key], this.SourceHeader);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "QtCommon.V2." + mode + "MocGeneration";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<IMocGenerationPolicy>.Create(className);
        }
    }
}
}
