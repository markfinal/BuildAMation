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
namespace CodeGenTest2
{
    public class ExportCodeGenOptionsDelegateAttribute :
        System.Attribute
    {}

    public class LocalCodeGenOptionsDelegateAttribute :
        System.Attribute
    {}

    public sealed class PrivateData :
        CommandLineProcessor.ICommandLineDelegate
    {
        public
        PrivateData(
            CommandLineProcessor.Delegate commandLineDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
        }

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }
    }

    public sealed partial class CodeGenOptionCollection :
        Bam.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport,
        ICodeGenOptions
    {
        public
        CodeGenOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
        {
            var options = this as ICodeGenOptions;
            options.OutputSourceDirectory = owningNode.GetTargettedModuleBuildDirectoryLocation("src").GetSingleRawPath();
            options.OutputName = "function";

            if (!owningNode.Module.Locations[CodeGenModule.OutputDir].IsValid)
            {
                (owningNode.Module.Locations[CodeGenModule.OutputDir] as Bam.Core.ScaffoldLocation).SetReference(owningNode.GetTargettedModuleBuildDirectoryLocation("src"));
            }
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            if (!node.Module.Locations[CodeGenModule.OutputFile].IsValid)
            {
                var options = node.Module.Options as ICodeGenOptions;
                (node.Module.Locations[CodeGenModule.OutputFile] as Bam.Core.ScaffoldLocation).SpecifyStub(node.Module.Locations[CodeGenModule.OutputDir], options.OutputName + ".c", Bam.Core.Location.EExists.WillExist);
            }

            base.FinalizeOptions (node);
        }

        void
        CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(
            Bam.Core.StringArray commandLineStringBuilder,
            Bam.Core.Target target,
            Bam.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target, excludedOptionNames);
        }
    }

    class CodeGeneratorTool :
        CSharp.Executable
    {
        public static string VersionString
        {
            get
            {
                return "dev";
            }
        }

        public
        CodeGeneratorTool()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            var codegentoolSourceDir = sourceDir.SubDirectory("codegentool");
            this.source = Bam.Core.FileLocation.Get(codegentoolSourceDir, "main.cs");
        }

        [Bam.Core.SourceFiles]
        Bam.Core.Location source;
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(ICodeGenTool))]
    public abstract class CodeGenModule :
        Bam.Core.BaseModule,
        Bam.Core.IInjectModules
    {
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("GeneratedSource", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("GeneratedSourceDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        [Bam.Core.RequiredModules]
        protected Bam.Core.TypeArray requiredModules = new Bam.Core.TypeArray(typeof(CodeGeneratorTool));

        #region IInjectModules Members

        string
        Bam.Core.IInjectModules.GetInjectedModuleNameSuffix(
            Bam.Core.BaseTarget baseTarget)
        {
            return "CodeGen2";
        }

        System.Type
        Bam.Core.IInjectModules.GetInjectedModuleType(
            Bam.Core.BaseTarget baseTarget)
        {
            return typeof(C.ObjectFile);
        }

        Bam.Core.DependencyNode
        Bam.Core.IInjectModules.GetInjectedParentNode(
            Bam.Core.DependencyNode node)
        {
            // return null because the injected node is not required to be poked into a parent module
            return null;
        }

        void
        Bam.Core.IInjectModules.ModuleCreationFixup(
            Bam.Core.DependencyNode created,
            Bam.Core.DependencyNode owner)
        {
            // associate the location of the generated file, with that of the output file
            // that caused the injection event
            var sourceFile = new Bam.Core.ScaffoldLocation(Bam.Core.ScaffoldLocation.ETypeHint.File);
            sourceFile.SetReference(owner.Module.Locations[OutputFile]);

            var createdModule = created.Module as C.ObjectFile;
            createdModule.SourceFileLocation = sourceFile;
        }

        #endregion
    }
}
