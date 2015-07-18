#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
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
                        osxLinker.Frameworks.Add("Cocoa");
                    }

                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
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
                        if (target.HasPlatform(Bam.Core.EPlatform.Unix))
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
