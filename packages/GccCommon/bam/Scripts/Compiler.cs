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
using System.Linq;
namespace GccCommon
{
    public abstract class CompilerBase :
        C.CompilerTool
    {
        protected CompilerBase()
        {
            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("objext", ".o");

            var gccPackage = Bam.Core.Graph.Instance.Packages.Where(item => item.Name == "Gcc").First();
            var suffix = gccPackage.MetaData["ToolSuffix"] as string;
            this.Macros.Add("CompilerSuffix", suffix);
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
            // NOTE: note that super classes need to be checked last in order to
            // honour the class hierarchy
            if (typeof(C.ObjCxx.ObjectFile).IsInstanceOfType(module) ||
                typeof(C.ObjCxx.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new Gcc.ObjectiveCxxCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.ObjC.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.ObjC.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new Gcc.ObjectiveCCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.Cxx.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.Cxx.ObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new Gcc.CxxCompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.ObjectFile).IsInstanceOfType(module) ||
                     typeof(C.CObjectFileCollection).IsInstanceOfType(module))
            {
                var settings = new Gcc.CompilerSettings(module);
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else
            {
                throw new Bam.Core.Exception("Could not determine type of module {0}", typeof(T).ToString());
            }
        }

        public override void
        CompileAsShared(
            Bam.Core.Settings settings)
        {
            var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
            gccCompiler.PositionIndependentCode = true;
        }

        protected abstract void
        OverrideDefaultSettings(
            Bam.Core.Settings settings);
    }

    [C.RegisterCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class CCompiler :
        CompilerBase
    {
        public CCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/gcc$(CompilerSuffix)", this));
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerSettings;
            cSettings.TargetLanguage = C.ETargetLanguage.C;
        }
    }

    [C.RegisterCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class CxxCompiler :
        CompilerBase
    {
        public CxxCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/g++$(CompilerSuffix)", this));
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerSettings;
            cSettings.TargetLanguage = C.ETargetLanguage.Cxx;
        }
    }

    [C.RegisterObjectiveCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class ObjectiveCCompiler :
        CompilerBase
    {
        public ObjectiveCCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/gcc$(CompilerSuffix)", this));
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
            var compiler = settings as C.ICommonCompilerSettings;
            compiler.TargetLanguage = C.ETargetLanguage.ObjectiveC;
            var cCompiler = settings as C.ICOnlyCompilerSettings;
            cCompiler.LanguageStandard = C.ELanguageStandard.C99;
        }
    }

    [C.RegisterObjectiveCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.ThirtyTwo)]
    [C.RegisterObjectiveCxxCompiler("GCC", Bam.Core.EPlatform.Linux, C.EBit.SixtyFour)]
    public sealed class ObjectiveCxxCompiler :
        CompilerBase
    {
        public ObjectiveCxxCompiler()
        {
            this.Macros.Add("CompilerPath", Bam.Core.TokenizedString.Create("$(InstallPath)/g++$(CompilerSuffix)", this));
        }

        protected override void
        OverrideDefaultSettings(
            Bam.Core.Settings settings)
        {
            var cSettings = settings as C.ICommonCompilerSettings;
            cSettings.TargetLanguage = C.ETargetLanguage.ObjectiveCxx;
        }
    }
}
