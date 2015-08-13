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
        Bam.Core.V2.Tool
    {
        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            return new MocSettings();
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return Bam.Core.V2.TokenizedString.Create(System.IO.Path.Combine(new [] {QtCommon.V2.Configure.InstallPath.Parse(), "bin", "moc"}), null);
            }
        }
    }

    public interface IMocGenerationPolicy
    {
        void
        Moc(
            MocModule sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.Tool mocCompiler,
            Bam.Core.V2.TokenizedString generatedMocSource,
            C.V2.HeaderFile source);
    }

    public class MocModule :
        C.V2.SourceFile
    {
        private Bam.Core.V2.Tool Compiler;
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
            this.IsUpToDate = false;
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
