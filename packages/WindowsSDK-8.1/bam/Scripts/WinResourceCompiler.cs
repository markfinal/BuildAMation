#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace WindowsSDK
{
    public abstract class WinResourceCompilerBase :
        C.WinResourceCompilerTool
    {
        protected void
        configure(
            string architecture)
        {
            if (this.EnvironmentVariables.ContainsKey("WindowsSdkDir"))
            {
                var tokenised_strings = new Bam.Core.TokenizedStringArray();
                tokenised_strings.AddRangeUnique(this.EnvironmentVariables["WindowsSdkDir"]);
                this.Macros.Add(
                    "CompilerPath",
                    Bam.Core.TokenizedString.Create(
                        System.String.Format("$(0)/bin/{0}/rc.exe", architecture),
                        null,
                        tokenised_strings
                    )
                );
            }
            else
            {
                throw new Bam.Core.Exception("Unable to determine resource compiler path, as %WindowsSdkDir% was not defined");
            }
            this.Macros.AddVerbatim("objext", ".res");
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["CompilerPath"];
            }
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new WinResourceCompilerSettings(module);
            return settings;
        }
    }

    [C.RegisterWinResourceCompiler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class WinResourceCompiler32 :
        WinResourceCompilerBase
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            var vcMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.EnvironmentVariables = vcMeta.Environment(C.EBit.ThirtyTwo);
            this.configure("x86");
            // now check the executable exists
            base.Init(parent);
        }
    }

    [C.RegisterWinResourceCompiler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    public sealed class WinResourceCompiler64 :
        WinResourceCompilerBase
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            var vcMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.EnvironmentVariables = vcMeta.Environment(C.EBit.SixtyFour);
            this.configure("x64");
            // now check the executable exists
            base.Init(parent);
        }
    }
}
