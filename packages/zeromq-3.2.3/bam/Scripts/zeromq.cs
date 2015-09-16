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
using System.Linq;
namespace zeromq
{
    public sealed class ZMQPlatformHeader :
        C.CModule
    {
        private static Bam.Core.FileKey Key = Bam.Core.FileKey.Generate("ZeroMQ platform header");

        protected override void
        Init(Module parent)
        {
            base.Init(parent);
            this.GeneratedPaths.Add(Key, Bam.Core.TokenizedString.Create("$(packagebuilddir)/$(config)/platform.hpp", this));
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }

            // platform.hpp.in should never change, so don't check it
        }

        protected override void
        ExecuteInternal(ExecutionContext context)
        {
            var source = Bam.Core.TokenizedString.Create("$(packagedir)/src/platform.hpp.in", this);

            // parse the input header, and modify it while writing it out
            // modifications are platform specific
            using (System.IO.TextReader readFile = new System.IO.StreamReader(source.Parse()))
            {
                var destPath = this.GeneratedPaths[Key].Parse();
                var destDir = System.IO.Path.GetDirectoryName(destPath);
                if (!System.IO.Directory.Exists(destDir))
                {
                    System.IO.Directory.CreateDirectory(destDir);
                }
                using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
                {
                    string line;
                    while ((line = readFile.ReadLine()) != null)
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                        {
                            // change #undefs to #defines
                            // some need to have a non-zero value, rather than just be defined
                            if (line.Contains("#undef ZMQ_HAVE_OSX") ||
                                line.Contains("#undef ZMQ_HAVE_UIO"))
                            {
                                var split = line.Split(new [] { ' ' });
                                writeFile.WriteLine("#define " + split[1] + " 1");
                                continue;
                            }
                        }
                        else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                        {
                            // change #undefs to #defines
                            // some need to have a non-zero value, rather than just be defined
                            if (line.Contains("#undef ZMQ_HAVE_LINUX") ||
                                line.Contains("#undef ZMQ_HAVE_UIO"))
                            {
                                var split = line.Split(new [] { ' ' });
                                writeFile.WriteLine("#define " + split[1] + " 1");
                                continue;
                            }
                        }
                        writeFile.WriteLine(line);
                    }
                }
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // TODO: do nothing
        }
    }

    public sealed class ZMQSharedLibrary :
        C.Cxx.DynamicLibrary
    {
        protected override void Init(Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros.Add("zmqsrcdir", Bam.Core.TokenizedString.Create("$(packagedir)/src", this));

            var source = this.CreateCxxSourceContainer("$(zmqsrcdir)/*.cpp", macroModuleOverride: this);

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("DLL_EXPORT");

                    var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;

                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.Add(TokenizedString.Create("$(packagedir)/builds/msvc", this));
                    }
                });
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX | Bam.Core.EPlatform.Linux))
            {
                // TODO: is there a call for a CompileWith function?
                var platformHeader = Bam.Core.Graph.Instance.FindReferencedModule<ZMQPlatformHeader>();
                source.DependsOn(platformHeader);
                source.UsePublicPatches(platformHeader);
                // TODO: end of function

                source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.Add(Bam.Core.TokenizedString.Create("$(packagebuilddir)/$(config)", this));
                });

                if (this.Linker is ClangCommon.LinkerBase)
                {
                    var ipc_listener = source.Children.Where(item => (item as C.ObjectFile).InputPath.Parse().EndsWith("ipc_listener.cpp"));
                    ipc_listener.ElementAt(0).PrivatePatch(settings => {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.Add("deprecated-declarations");
                    });
                }
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.Add(TokenizedString.Create("$(packagedir)/include", this));
                    }
                });

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.ICommonLinkerSettings;
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        if (this.Linker is VisualCCommon.LinkerBase)
                        {
                            linker.Libraries.Add("Ws2_32.lib");
                            linker.Libraries.Add("Advapi32.lib");
                        }
                        else if (this.Linker is MingwCommon.LinkerBase)
                        {
                            linker.Libraries.Add("-lws2_32");
                            linker.Libraries.Add("-ladvapi32");
                        }
                    }
                    else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                    {
                        linker.Libraries.Add("-lpthread");
                    }
                });
        }
    }
}
