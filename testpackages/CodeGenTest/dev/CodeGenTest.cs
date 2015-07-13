#region License
// Copyright 2010-2015 Mark Final
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

    public class GeneratedSourceModule :
        C.V2.SourceFile
    {
        private Bam.Core.V2.Tool Compiler;

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
            var args = new Bam.Core.StringArray();
            args.Add(Bam.Core.V2.TokenizedString.Create("$(buildroot)", this).Parse());
            args.Add("Generated");
            CommandLineProcessor.V2.Processor.Execute(context, this.Compiler, args);
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

            var generatedSourceTuple = source.GenerateSource();

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
            CodeGenOptionCollection options = module.Options as CodeGenOptionCollection;
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
