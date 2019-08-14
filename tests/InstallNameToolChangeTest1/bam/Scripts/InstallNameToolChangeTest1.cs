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
using System.Linq;
namespace InstallNameToolChangeTest1
{
    class DynamicLib :
        C.DynamicLibrary
    {
        protected override void
        Init()
        {
            base.Init();

            this.CreateCSourceContainer("$(packagedir)/source/lib/*.c");

            this.PublicPatch((settings, appliedTo) =>
            {
                if (settings is C.ICommonPreprocessorSettings preprocessor)
                {
                    preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/source/lib"));
                }
            });
        }
    }

    class Executable :
        C.ConsoleApplication
    {
        protected override void
        Init()
        {
            base.Init();

            var source = this.CreateCSourceContainer("$(packagedir)/source/app/*.c");
            this.CompileAndLinkAgainst<DynamicLib>(source);
        }
    }

    sealed class ChangeId :
        Publisher.ChangeNameOSX
    {
        protected override void
        Init()
        {
            base.Init();

            this.Source = Bam.Core.Graph.Instance.FindReferencedModule<Executable>();

            this.PrivatePatch(settings =>
                {
                    var install_name = settings as Publisher.IInstallNameToolSettings;
                    install_name.OldName = "@rpath/libDynamicLib.1.dylib";
                    install_name.NewName = "@rpath/foo/libDynamicLib.1.dylib"; // add a 'foo' subdirectory
                });
        }
    }

    sealed class Runtime :
        Publisher.Collation
    {
        protected override void
        Init()
        {
            base.Init();

            this.SetDefaultMacrosAndMappings(EPublishingType.ConsoleApplication);

            var anchor = this.Include<Executable>(C.ConsoleApplication.ExecutableKey);

            // move the dependent dynamiclib to a foo subdirectory
            var moved_dylib = this.Find<DynamicLib>().First();
            (moved_dylib as Publisher.CollatedObject).SetPublishingDirectory("$(publishdir)/foo");

            // make sure that changing the executable's install_name for the dynamiclib happens _before_
            // it is collated
            (anchor as Bam.Core.Module).DependsOn(Bam.Core.Graph.Instance.FindReferencedModule<ChangeId>());
        }
    }
}
