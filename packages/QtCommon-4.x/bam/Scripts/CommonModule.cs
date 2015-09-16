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
using Bam.Core;
namespace QtCommon
{
    public abstract class CommonModule :
        C.DynamicLibrary
    {
        protected CommonModule(
            string moduleName) :
            base()
        {
            this.Macros.Add("QtModuleName", moduleName);
            this.Macros.Add("QtInstallPath", Configure.InstallPath);
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Macros.Add("QtIncludePath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/include", this));
            this.Macros.Add("QtLibraryPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/lib", this));
            this.Macros.Add("QtBinaryPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/bin", this));
            this.Macros.Add("QtConfig", Bam.Core.TokenizedString.Create((this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug) ? "d" : string.Empty, null));

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.GeneratedPaths[Key] = Bam.Core.TokenizedString.Create("$(QtBinaryPath)/$(dynamicprefix)Qt$(QtModuleName)$(QtConfig)4$(dynamicext)", this);
                this.GeneratedPaths[ImportLibraryKey] = Bam.Core.TokenizedString.Create("$(QtLibraryPath)/$(libprefix)Qt$(QtModuleName)$(QtConfig)4$(libext)", this);
            }
            else
            {
                this.GeneratedPaths[Key] = Bam.Core.TokenizedString.Create("$(QtLibraryPath)/$(dynamicprefix)Qt$(QtModuleName)$(dynamicext)", this);
            }

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                if (null != compiler)
                {
                    compiler.IncludePaths.AddUnique(this.Macros["QtIncludePath"]);
                }

                var linker = settings as C.ICommonLinkerSettings;
                if (null != linker)
                {
                    linker.LibraryPaths.AddUnique(this.Macros["QtLibraryPath"]);
                }
            });
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            // prebuilt - no execution
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // prebuilt - no execution policy
        }
    }
}
