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
    public abstract class CompilerOptionCollection :
        Bam.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            var compilerOptions = this as ICCompilerOptions;

            var target = node.Target;

            // process character set early, as it sets #defines
            compilerOptions.CharacterSet = ECharacterSet.NotSet;

            compilerOptions.OutputType = ECompilerOutput.CompileOnly;
            compilerOptions.WarningsAsErrors = true;
            compilerOptions.IgnoreStandardIncludePaths = true;
            compilerOptions.TargetLanguage = ETargetLanguage.Default;

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                compilerOptions.DebugSymbols = true;
                compilerOptions.Optimization = EOptimization.Off;
                compilerOptions.OmitFramePointer = false;
            }
            else
            {
                if (!target.HasConfiguration(Bam.Core.EConfiguration.Profile))
                {
                    compilerOptions.DebugSymbols = false;
                }
                else
                {
                    compilerOptions.DebugSymbols = true;
                }
                compilerOptions.Optimization = EOptimization.Speed;
                compilerOptions.OmitFramePointer = true;
            }
            compilerOptions.CustomOptimization = "";
            compilerOptions.ShowIncludes = false;

            compilerOptions.Defines = new DefineCollection();
            compilerOptions.Undefines = new DefineCollection();

            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                compilerOptions.Defines.Add(System.String.Format("D_BAM_PLATFORM_WINDOWS"));
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                compilerOptions.Defines.Add(System.String.Format("D_BAM_PLATFORM_UNIX"));
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                compilerOptions.Defines.Add(System.String.Format("D_BAM_PLATFORM_OSX"));
            }

            {
                var is64bit = Bam.Core.OSUtilities.Is64Bit(target);
                var bits = (is64bit) ? 64 : 32;
                compilerOptions.Defines.Add(System.String.Format("D_BAM_PLATFORM_BITS={0}", bits.ToString()));
            }
            {
                var isLittleEndian = Bam.Core.State.IsLittleEndian;
                if (isLittleEndian)
                {
                    compilerOptions.Defines.Add("D_BAM_PLATFORM_LITTLEENDIAN");
                }
                else
                {
                    compilerOptions.Defines.Add("D_BAM_PLATFORM_BIGENDIAN");
                }
            }
            compilerOptions.Defines.Add(System.String.Format("D_BAM_CONFIGURATION_{0}", ((Bam.Core.BaseTarget)target).ConfigurationName('u')));
            compilerOptions.Defines.Add(System.String.Format("D_BAM_TOOLCHAIN_{0}", target.ToolsetName('u')));

            compilerOptions.IncludePaths = new Bam.Core.DirectoryCollection();
            compilerOptions.IncludePaths.Add("."); // explicitly add the one that is assumed

            compilerOptions.SystemIncludePaths = new Bam.Core.DirectoryCollection();

            compilerOptions.DisableWarnings = new Bam.Core.StringArray();

            if (this is ICxxCompilerOptions)
            {
                compilerOptions.LanguageStandard = ELanguageStandard.Cxx98;
            }
            else
            {
                compilerOptions.LanguageStandard = ELanguageStandard.C89;
            }

            compilerOptions.AdditionalOptions = "";
        }

        public
        CompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            var moduleBuildDir = this.GetModuleLocation(Bam.Core.State.ModuleBuildDirLocationKey);
            var outputFileDir = this.GetModuleLocation(C.ObjectFile.OutputDir);
            if (!outputFileDir.IsValid)
            {
                var target = node.Target;
                var compilerTool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
                var objBuildDir = moduleBuildDir.SubDirectory(compilerTool.ObjectFileOutputSubDirectory);
                (outputFileDir as Bam.Core.ScaffoldLocation).SetReference(objBuildDir);
            }

            // don't operate on collections of modules
            var objectFileModule = node.Module as ObjectFile;
            if (null != objectFileModule)
            {
                // this only requires the end path - so grab it from the Location without resolving it
                var location = objectFileModule.SourceFileLocation;
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
            else
            {
                this.OutputName = null;
            }
        }

        public string OutputName
        {
            get;
            set;
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            // don't operate on collections of modules
            var objectFileModule = node.Module as ObjectFile;
            if (null == objectFileModule)
            {
                return;
            }
            var outputFileLocation = node.Module.Locations[C.ObjectFile.OutputFile] as Bam.Core.ScaffoldLocation;
            if (!outputFileLocation.IsValid)
            {
                if (null == this.OutputName)
                {
                    // in the case of procedurally generated source, the output name may not have been set yet
                    var sourceLoc = objectFileModule.SourceFileLocation;
                    var sourceLocPath = sourceLoc.GetSingleRawPath();
                    this.OutputName = System.IO.Path.GetFileNameWithoutExtension(sourceLocPath);
                }

                var target = node.Target;
                var tool = target.Toolset.Tool(typeof(ICompilerTool)) as ICompilerTool;
                var options = this as ICCompilerOptions;
                var suffix = (options.OutputType == ECompilerOutput.Preprocess) ?
                    tool.PreprocessedOutputSuffix :
                    tool.ObjectFileSuffix;
                outputFileLocation.SpecifyStub(node.Module.Locations[C.ObjectFile.OutputDir], this.OutputName + suffix, Bam.Core.Location.EExists.WillExist);
            }
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
