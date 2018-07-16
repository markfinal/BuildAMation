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
using Bam.Core;
using System.Linq;
namespace PluginTest
{
    public class Application :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/application/main.cpp");
            source.PrivatePatch(settings =>
                {
                    var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
                });

            this.RequiredToExist<Plugin>();

            if (this.Linker is GccCommon.LinkerBase)
            {
                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.Add("-ldl");
                    });
            }
        }
    }

    public class Plugin :
        C.Cxx.Plugin
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["pluginprefix"] = Bam.Core.TokenizedString.CreateVerbatim("test");
            this.Macros["pluginext"] = Bam.Core.TokenizedString.CreateVerbatim(".plugin");

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/plugin/pluginmain.cpp");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("DYNAMICLIB_SOURCE");
                });
        }
    }

    public sealed class RuntimePackage :
        Publisher.Collation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
#if BAM_V2
            this.Include<Application>(C.ConsoleApplication.ExecutableKey);
#else
            this.Include<Application>(C.ConsoleApplication.Key);
#endif
            (this.Find<Plugin>().First() as Publisher.CollatedObject).SetPublishingDirectory("$(0)/subdir", new[] { this.ExecutableDir });
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class DebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateSymbolsFrom<RuntimePackage>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class Stripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.StripBinariesFrom<RuntimePackage, DebugSymbols>();
        }
    }
}
