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
            else if (target.HasPlatform(Bam.Core.EPlatform.Linux))
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
