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
namespace MingwCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            var localCompilerOptions = this as ICCompilerOptions;
            localCompilerOptions.AllWarnings = true;
            localCompilerOptions.ExtraWarnings = true;

            base.SetDefaultOptionValues(node);

            var target = node.Target;
            localCompilerOptions.SixtyFourBit = target.HasPlatform(Bam.Core.EPlatform.Win64);

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                localCompilerOptions.StrictAliasing = false;
                localCompilerOptions.InlineFunctions = false;
            }
            else
            {
                localCompilerOptions.StrictAliasing = true;
                localCompilerOptions.InlineFunctions = true;
            }

            var cCompilerOptions = this as C.ICCompilerOptions;
            cCompilerOptions.TargetLanguage = C.ETargetLanguage.C;

            var toolset = target.Toolset;
            var compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            cCompilerOptions.SystemIncludePaths.AddRange(compilerTool.IncludePaths((Bam.Core.BaseTarget)node.Target));

            localCompilerOptions.Pedantic = true;
        }

        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
