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
using Bam.Core;
namespace InstallerTest1
{
    public sealed class CExecutableRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
            this.SetDefaultMacros(EPublishingType.WindowedApplication);
            this.Include<CExecutable>(C.GUIApplication.Key);

            var app = this.Find<CExecutable>();

            // copy the required runtime library next to the binary
            if (this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                (app.SourceModule as CExecutable).Linker is VisualCCommon.LinkerBase)
            {
                // just C runtime here
                var runtimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualCCommon.IRuntimeLibraryPathMeta>("VisualC");
                this.IncludeFiles(runtimeLibrary.CRuntimePaths((app.SourceModule as C.CModule).BitDepth), this.ExecutableDir);
            }
#else
            var app = this.Include<CExecutable>(C.GUIApplication.Key, EPublishingType.WindowedApplication);
            this.Include<CDynamicLibrary>(C.DynamicLibrary.Key, ".", app);

            // copy the required runtime library next to the binary
            // just C runtime here
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                (app.SourceModule as CExecutable).Linker is VisualCCommon.LinkerBase)
            {
                var runtimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualCCommon.IRuntimeLibraryPathMeta>("VisualC");
                foreach (var runtimelib in runtimeLibrary.CRuntimePaths((app.SourceModule as C.CModule).BitDepth))
                {
                    this.IncludeFile(runtimelib, ".", app);
                }
            }
#endif
        }
    }

    public sealed class CxxExecutableRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

#if D_NEW_PUBLISHING
            this.SetDefaultMacros(EPublishingType.WindowedApplication);
            this.Include<CxxExecutable>(C.Cxx.GUIApplication.Key);

            var app = this.Find<CxxExecutable>();

            // copy the required runtime library next to the binary
            if (this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                (app.SourceModule as CxxExecutable).Linker is VisualCCommon.LinkerBase)
            {
                // both C and C++ runtimes here
                var runtimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualCCommon.IRuntimeLibraryPathMeta>("VisualC");
                this.IncludeFiles(runtimeLibrary.CRuntimePaths((app.SourceModule as C.CModule).BitDepth), this.ExecutableDir);
                this.IncludeFiles(runtimeLibrary.CxxRuntimePaths((app.SourceModule as C.CModule).BitDepth), this.ExecutableDir);
            }
#else
            var app = this.Include<CxxExecutable>(C.Cxx.GUIApplication.Key, EPublishingType.WindowedApplication);
            this.Include<CxxDynamicLibrary>(C.Cxx.DynamicLibrary.Key, ".", app);

            // copy the required runtime library next to the binary
            // both C and C++ runtimes here
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                (app.SourceModule as CxxExecutable).Linker is VisualCCommon.LinkerBase)
            {
                var runtimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualCCommon.IRuntimeLibraryPathMeta>("VisualC");
                foreach (var runtimelib in runtimeLibrary.CRuntimePaths((app.SourceModule as C.CModule).BitDepth))
                {
                    this.IncludeFile(runtimelib, ".", app);
                }
                foreach (var runtimelib in runtimeLibrary.CxxRuntimePaths((app.SourceModule as C.CModule).BitDepth))
                {
                    this.IncludeFile(runtimelib, ".", app);
                }
            }
#endif
        }
    }
}
