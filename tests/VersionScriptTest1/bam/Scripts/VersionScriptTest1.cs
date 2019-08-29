#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace VersionScriptTest1
{
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    class Application :
        C.ConsoleApplication
    {
        protected override void
        Init()
        {
            base.Init();

            var source = this.CreateCSourceCollection("$(packagedir)/source/application/main.c");
            this.CompileAndLinkAgainst<Library>(source);

            this.PrivatePatch(settings =>
                {
                    var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                    gccLinker.CanUseOrigin = true;
                    gccLinker.RPath.AddUnique("$ORIGIN");
                });
        }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    class Library :
        C.DynamicLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            var source = this.CreateCSourceCollection("$(packagedir)/source/library/library.c");
            source.PrivatePatch(settings =>
                {
                    // make everything visible
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    gccCompiler.Visibility = GccCommon.EVisibility.Default;
                });

            // limit exports with version script
            var versionScript = Bam.Core.Graph.Instance.FindReferencedModule<VersionScript>();
            this.DependsOn(versionScript);
            this.PrivatePatch(settings =>
                {
                    var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                    gccLinker.VersionScript = versionScript.InputPath;
                });

            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/source/library"));
                    }
                });
        }
    }

    class VersionScript :
        C.VersionScript
    {
        public override Bam.Core.TokenizedString OutputPath => this.CreateTokenizedString("$(packagebuilddir)/$(config)/verscript.map");

        protected override string Contents
        {
            get
            {
                var contents = new System.Text.StringBuilder();
                contents.AppendLine("LIB_v1");
                contents.AppendLine("{");
                contents.AppendLine("\tglobal:");
                contents.AppendLine("\t\tExportedFunction;");
                contents.AppendLine("\tlocal:");
                contents.AppendLine("\t\t*;");
                contents.AppendLine("};");
                return contents.ToString();
            }
        }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux)]
    sealed class RuntimePackage :
        Publisher.Collation
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);
            this.Include<Application>(C.ConsoleApplication.ExecutableKey);
        }
    }
}
