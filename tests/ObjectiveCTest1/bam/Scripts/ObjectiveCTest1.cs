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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace ObjectiveCTest1
{
    sealed class ProgramV2 :
        C.V2.ConsoleApplication
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var source = this.CreateObjectiveCSourceContainer();
            source.AddFile("$(pkgroot)/source/main.m");

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                source.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.V2.ICommonCompilerOptions;
                        compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("/usr/include/GNUstep", null, verbatim: true));

                        var objcCompiler = settings as C.V2.IObjectiveCOnlyCompilerOptions;
                        objcCompiler.ConstantStringClass = "NSConstantString";
                    });
            }

            this.PrivatePatch(settings =>
                {
                    var osxLinker = settings as C.V2.ILinkerOptionsOSX;
                    if (null != osxLinker)
                    {
                        osxLinker.Frameworks.Add(Bam.Core.V2.TokenizedString.Create("Cocoa", null, verbatim:true));
                    }

                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                    {
                        var linker = settings as C.V2.ICommonLinkerOptions;
                        linker.Libraries.Add("-lobjc");
                        linker.Libraries.Add("-lgnustep-base");
                    }
                });
        }
    }

    class Program :
        C.Application
    {
        public
        Program()
        {
            this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target) {
                var link = module.Options as C.ILinkerOptionsOSX;
                if (null != link)
                {
                    link.Frameworks.Add("Cocoa");
                }
            };
        }

        class Source :
            C.ObjC.ObjectFileCollection
        {
            public
            Source()
            {
                var sourceDir = this.PackageLocation.SubDirectory("source");
                this.Include(sourceDir, "*.m");
                this.UpdateOptions += delegate(Bam.Core.IModule module, Bam.Core.Target target) {
                    var compileOptions = module.Options as C.ICCompilerOptions;
                    if (null != compileOptions)
                    {
                        if (target.HasPlatform(Bam.Core.EPlatform.Linux))
                        {
                            compileOptions.IncludePaths.Add("/usr/include/GNUstep");
                        }
                    }
                };
            }
        }

        [Bam.Core.SourceFiles]
        Source source = new Source();
    }
}
