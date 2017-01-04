#region License
// Copyright (c) 2010-2017, Mark Final
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
    public abstract class CompilerBase :
        C.CompilerTool
    {
        protected CompilerBase()
        {
            this.InheritedEnvironmentVariables.Add("SystemRoot");
            // temp environment variables avoid generation of _CL_<hex> temporary files in the current directory
            this.InheritedEnvironmentVariables.Add("TEMP");
            this.InheritedEnvironmentVariables.Add("TMP");

            var meta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.Macros.Add("InstallPath", meta.InstallDir);
            this.Macros.Add("BinPath", this.CreateTokenizedString(@"$(InstallPath)\VC\bin"));
            this.Macros.AddVerbatim("objext", ".obj");

            if (null != meta.RequiredExecutablePaths)
            {
                this.EnvironmentVariables.Add("PATH", meta.RequiredExecutablePaths);
            }

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compilation = settings as C.ICommonCompilerSettings;
                    if (null != compilation)
                    {
                        compilation.SystemIncludePaths.AddUnique(this.CreateTokenizedString(@"$(InstallPath)\VC\include"));
                    }

                    var rcCompilation = settings as C.ICommonWinResourceCompilerSettings;
                    if (null != rcCompilation)
                    {
                        rcCompilation.IncludePaths.AddUnique(this.CreateTokenizedString(@"$(InstallPath)\VC\include"));
                    }
                });

            if (meta.UseWindowsSDKPublicPatches)
            {
                var windowsSDK = Bam.Core.Graph.Instance.FindReferencedModule<WindowsSDK.WindowsSDK>();
                this.UsePublicPatches(windowsSDK);
            }
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["CompilerPath"];
            }
        }

        public override string UseResponseFileOption
        {
            get
            {
                return "@";
            }
        }

        public override Bam.Core.Settings CreateDefaultSettings<T>(T module)
        {
            if (typeof(C.Cxx.ObjectFile).IsInstanceOfType(module) ||
                typeof(C.Cxx.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new VisualC.CxxCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.CObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new VisualC.CompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else
            {
                throw new Bam.Core.Exception("Could not determine type of module {0}", typeof(T).ToString());
            }
        }

        protected abstract void
        OverrideDefaultSettings(
            Bam.Core.Settings settings);
    }

    [C.RegisterCCompiler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public class Compiler32 :
        CompilerBase
    {
        public Compiler32()
        {
            this.Macros.Add("CompilerPath", this.CreateTokenizedString(@"$(BinPath)\cl.exe"));
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerSettings;
            cSettings.Bits = C.EBit.ThirtyTwo;
        }
    }

    [C.RegisterCxxCompiler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class CxxCompiler32 :
        Compiler32
    {
        public CxxCompiler32()
            : base()
        {
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
            base.OverrideDefaultSettings(settings);
            var cSettings = settings as C.ICommonCompilerSettings;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
        }
    }

    [C.RegisterCCompiler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    public class Compiler64 :
        CompilerBase
    {
        public Compiler64()
            : base()
        {
            this.Macros.Add("CompilerPath", this.CreateTokenizedString(@"$(BinPath)\x86_amd64\cl.exe"));
            // some DLLs exist only in the 32-bit bin folder
            if (this.EnvironmentVariables.ContainsKey("PATH"))
            {
                this.EnvironmentVariables["PATH"].Add(this.Macros["BinPath"]);
            }
            else
            {
                this.EnvironmentVariables.Add("PATH", new Bam.Core.TokenizedStringArray(this.Macros["BinPath"]));
            }
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerSettings;
            cSettings.Bits = C.EBit.SixtyFour;
        }
    }

    [C.RegisterCxxCompiler("VisualC", Bam.Core.EPlatform.Windows, C.EBit.SixtyFour)]
    public sealed class CxxCompiler64 :
        Compiler64
    {
        public CxxCompiler64()
            : base()
        {
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
            base.OverrideDefaultSettings(settings);
            var cSettings = settings as C.ICommonCompilerSettings;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
        }
    }
}
