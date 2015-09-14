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
using QtCommon.V2.MocExtension;
namespace Test13
{
    sealed class QtApplicationV2 :
        C.V2.ConsoleApplication
    {
        public QtApplicationV2()
        {
            this.BitDepth = C.V2.EBit.ThirtyTwo;
        }

        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(pkgroot)/source/*.cpp");
            var mocHeaders = this.CreateHeaderContainer("$(pkgroot)/source/myobject*.h");
            foreach (var mocHeader in mocHeaders.Children)
            {
                /*var myobjectMocTuple = */ source.MocHeader(mocHeader as C.V2.HeaderFile);
            }

            this.PrivatePatch(settings =>
            {
                var gccLinker = settings as GccCommon.V2.ICommonLinkerOptions;
                if (gccLinker != null)
                {
                    gccLinker.CanUseOrigin = true;
                    gccLinker.RPath.Add("$ORIGIN");
                }
            });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.CompilePubliclyAndLinkAgainst<Qt.V2.CoreFramework>(source);
                this.CompilePubliclyAndLinkAgainst<Qt.V2.GuiFramework>(source);
            }
            else
            {
                this.CompileAndLinkAgainst<Qt.V2.Core>(source);
                this.CompileAndLinkAgainst<Qt.V2.Gui>(source);
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    sealed class RuntimePackage :
        Publisher.V2.Package
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            var app = this.Include<QtApplicationV2>(C.V2.ConsoleApplication.Key, EPublishingType.WindowedApplication);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
            }
            else
            {
                this.Include<Qt.V2.Core>(C.V2.DynamicLibrary.Key, ".", app);
                this.Include<Qt.V2.Gui>(C.V2.DynamicLibrary.Key, ".", app);
            }
        }
    }
}
