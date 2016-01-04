#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace ClangCommon
{
    public abstract class CompilerBase :
        C.CompilerTool
    {
        protected Bam.Core.TokenizedStringArray arguments = new Bam.Core.TokenizedStringArray();

        protected CompilerBase()
        {
            this.Macros.AddVerbatim("objext", ".o");

            var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim(System.String.Format("--sdk {0}", clangMeta.SDK)));
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.CreateVerbatim("xcrun");
            }
        }

        public override Bam.Core.TokenizedStringArray InitialArguments
        {
            get
            {
                return this.arguments;
            }
        }
    }

    [C.RegisterCCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterCCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class CCompiler :
        CompilerBase
    {
        public CCompiler()
        {
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang"));
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Clang.CompilerSettings(module);
            return settings;
        }
    }

    [C.RegisterCxxCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterCxxCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class CxxCompiler :
        CompilerBase
    {
        public CxxCompiler()
        {
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang++"));
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Clang.CxxCompilerSettings(module);
            return settings;
        }
    }

    [C.RegisterObjectiveCCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class ObjectiveCCompiler :
        CompilerBase
    {
        public ObjectiveCCompiler()
        {
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang"));
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Clang.ObjectiveCCompilerSettings(module);
            return settings;
        }
    }

    [C.RegisterObjectiveCxxCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCxxCompiler("Clang", Bam.Core.EPlatform.OSX, C.EBit.SixtyFour)]
    public sealed class ObjectiveCxxCompiler :
    CompilerBase
    {
        public ObjectiveCxxCompiler()
        {
            this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim("clang++"));
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            var settings = new Clang.ObjectiveCxxCompilerSettings(module);
            return settings;
        }
    }
}
