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
using Bam.Core;
using QtCommon.MocExtension;
namespace Qt5Test1
{
    sealed class Qt5Application :
        C.Cxx.GUIApplication
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var mocHeaders = this.CreateHeaderContainer("$(packagedir)/source/*.h");
            var source = this.CreateCxxSourceContainer("$(packagedir)/source/*.cpp");
            foreach (var mocHeader in mocHeaders.Children)
            {
                var myObjectMocTuple = source.MocHeader(mocHeader as C.HeaderFile);

                // first item in Tuple is the generated moc source file
                myObjectMocTuple.Item1.PrivatePatch(settings =>
                    {
                        var mocSettings = settings as QtCommon.IMocSettings;
                        mocSettings.PreprocessorDefinitions.Add("GENERATING_MOC");
                    });

                // second item in Tuple is the C++ compilation of that generated source
                myObjectMocTuple.Item2.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("COMPILING_GENERATED_MOC");
                    });
            }

            source.PrivatePatch(settings =>
                {
                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        gccCompiler.PositionIndependentCode = true;
                    }
                });

            this.PrivatePatch(settings =>
            {
                var gccLinker = settings as GccCommon.ICommonLinkerSettings;
                if (gccLinker != null)
                {
                    gccLinker.CanUseOrigin = true;
                    gccLinker.RPath.Add("$ORIGIN");
                }
            });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.CompilePubliclyAndLinkAgainst<Qt.CoreFramework>(source);
                this.CompilePubliclyAndLinkAgainst<Qt.WidgetsFramework>(source);
            }
            else
            {
                this.CompileAndLinkAgainst<Qt.Core>(source);
                this.CompileAndLinkAgainst<Qt.Widgets>(source);
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    this.LinkAgainst<Qt.Gui>();
                }
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }
        }
    }

    sealed class RuntimePackage :
        Publisher.Package
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var app = this.Include<Qt5Application>(C.ConsoleApplication.Key, EPublishingType.WindowedApplication);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
            }
            else
            {
                this.Include<Qt.Core>(C.DynamicLibrary.Key, ".", app);
                this.Include<Qt.Widgets>(C.DynamicLibrary.Key, ".", app);
                this.Include<Qt.Gui>(C.DynamicLibrary.Key, ".", app);
                this.Include<ICU.ICUIN>(C.DynamicLibrary.Key, ".", app);
                this.Include<ICU.ICUUC>(C.DynamicLibrary.Key, ".", app);
                this.Include<ICU.ICUDT>(C.DynamicLibrary.Key, ".", app);
            }
        }
    }

    sealed class TarBallInstaller :
        Installer.TarBall
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.SourceFolder<RuntimePackage>(Publisher.Package.PackageRoot);
        }
    }
}
