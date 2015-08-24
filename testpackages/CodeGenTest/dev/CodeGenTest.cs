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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
using CodeGenTest.V2.CodeGenExtension;
namespace CodeGenTest
{
namespace V2
{
    namespace CodeGenExtension
    {
        public static class CodeGenExtension
        {
            public static System.Tuple<Bam.Core.V2.Module, Bam.Core.V2.Module>
            GenerateSource(
                this C.V2.CObjectFileCollection module)
            {
                // generated source file
                var generatedSourceFile = Bam.Core.V2.Module.Create<GeneratedSourceModule>(module);

                // compile source
                var objFile = module.AddFile(GeneratedSourceModule.Key, generatedSourceFile);

                return new System.Tuple<Bam.Core.V2.Module, Bam.Core.V2.Module>(generatedSourceFile, objFile);
            }
        }
    }

    public sealed class BuildCodeGenTool :
        C.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/codegentool/main.c");

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    public sealed class GeneratedSourceSettings :
        Bam.Core.V2.Settings
    {
    }

    public sealed class GeneratedSourceTool :
        Bam.Core.V2.Tool
    {
        private C.V2.CModule buildTool;

        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.buildTool = Bam.Core.V2.Graph.Instance.FindReferencedModule<BuildCodeGenTool>();
            this.Requires(this.buildTool);
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            return new GeneratedSourceSettings();
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return this.buildTool.GeneratedPaths[C.V2.ConsoleApplication.Key];
            }
        }
    }

    public interface IGeneratedSourcePolicy
    {
        void
        GenerateSource(
            GeneratedSourceModule sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.Tool compiler,
            Bam.Core.V2.TokenizedString generatedFilePath);
    }

    public class GeneratedSourceModule :
        C.V2.SourceFile
    {
        private Bam.Core.V2.Tool Compiler;
        private IGeneratedSourcePolicy Policy;

        public GeneratedSourceModule()
        {
            this.RegisterGeneratedFile(Key, Bam.Core.V2.TokenizedString.Create("$(buildroot)/Generated.c", this));
            this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<GeneratedSourceTool>();
            this.Requires(this.Compiler);
        }

        public override void
        Evaluate()
        {
            this.IsUpToDate = false;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }

            this.Policy.GenerateSource(this, context, this.Compiler, this.GeneratedPaths[Key]);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            if (mode == "Native")
            {
                var className = "CodeGenTest.V2." + mode + "GenerateSource";
                this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<IGeneratedSourcePolicy>.Create(className);
            }
        }
    }
}
    public sealed class TestAppV2 :
        C.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer();
            source.AddFile("$(pkgroot)/source/testapp/main.c");

            /*var generatedSourceTuple = */source.GenerateSource();

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    // Define module classes here
    class TestAppGeneratedSource :
        CodeGenModule
    {
        public
        TestAppGeneratedSource()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(TestAppGeneratedSource_UpdateOptions);
        }

        void
        TestAppGeneratedSource_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            //CodeGenOptionCollection options = module.Options as CodeGenOptionCollection;
        }
    }

    class TestApp :
        C.Application
    {
        public
        TestApp()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(TestApp_UpdateOptions);
        }

        void
        TestApp_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            C.ILinkerOptions options = module.Options as C.ILinkerOptions;
            options.DoNotAutoIncludeStandardLibraries = false;
        }

        class SourceFiles :
            C.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                var testAppDir = sourceDir.SubDirectory("testapp");
                this.Include(testAppDir, "main.c");
            }

            [Bam.Core.DependentModules]
            Bam.Core.TypeArray vcDependencies = new Bam.Core.TypeArray(typeof(TestAppGeneratedSource));
        }

        [Bam.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray vcDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
