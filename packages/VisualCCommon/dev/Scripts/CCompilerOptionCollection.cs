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
namespace VisualCCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions,
        VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var target = node.Target;

            var compilerInterface = this as ICCompilerOptions;
            compilerInterface.NoLogo = true;

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                compilerInterface.MinimalRebuild = true;
                compilerInterface.BasicRuntimeChecks = EBasicRuntimeChecks.StackFrameAndUninitializedVariables;
                compilerInterface.SmallerTypeConversionRuntimeCheck = true;
                compilerInterface.InlineFunctionExpansion = EInlineFunctionExpansion.None;
                compilerInterface.EnableIntrinsicFunctions = false;
            }
            else
            {
                compilerInterface.MinimalRebuild = false;
                compilerInterface.BasicRuntimeChecks = EBasicRuntimeChecks.None;
                compilerInterface.SmallerTypeConversionRuntimeCheck = false;
                compilerInterface.InlineFunctionExpansion = EInlineFunctionExpansion.AnySuitable;
                compilerInterface.EnableIntrinsicFunctions = true;
            }

            var compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            (this as C.ICCompilerOptions).SystemIncludePaths.AddRange(compilerTool.IncludePaths((Bam.Core.BaseTarget)target));

            (this as C.ICCompilerOptions).TargetLanguage = C.ETargetLanguage.C;

            compilerInterface.WarningLevel = EWarningLevel.Level4;

            compilerInterface.DebugType = EDebugType.Embedded;

            // disable browse information to improve build speed
            compilerInterface.BrowseInformation = EBrowseInformation.None;

            compilerInterface.StringPooling = true;
            compilerInterface.DisableLanguageExtensions = false;
            compilerInterface.ForceConformanceInForLoopScope = true;
            compilerInterface.UseFullPaths = true;
            compilerInterface.CompileAsManaged = EManagedCompilation.NoCLR;
            compilerInterface.RuntimeLibrary = ERuntimeLibrary.MultiThreadedDLL;
            compilerInterface.ForcedInclude = new Bam.Core.StringArray();
        }

        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var options = this as ICCompilerOptions;
            if (options.DebugType != EDebugType.Embedded)
            {
                var locationMap = node.Module.Locations;
                var pdbDirLoc = locationMap[CCompiler.PDBDir] as Bam.Core.ScaffoldLocation;
                if (!pdbDirLoc.IsValid)
                {
                    pdbDirLoc.SetReference(locationMap[C.Application.OutputDir]);
                }

                var pdbFileLoc = locationMap[CCompiler.PDBFile] as Bam.Core.ScaffoldLocation;
                if (!pdbFileLoc.IsValid)
                {
                    pdbFileLoc.SpecifyStub(pdbDirLoc, this.OutputName + ".pdb", Bam.Core.Location.EExists.WillExist);
                }
            }

            base.FinalizeOptions(node);
        }

        VisualStudioProcessor.ToolAttributeDictionary
        VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(
            Bam.Core.Target target)
        {
            var vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            var dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}
