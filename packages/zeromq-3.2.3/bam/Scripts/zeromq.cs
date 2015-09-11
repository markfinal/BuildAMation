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
using System.Linq;
namespace zeromq
{
    public sealed class ZMQPlatformHeader :
        C.V2.CModule
    {
        private static Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("ZeroMQ platform header");

        protected override void
        Init(Module parent)
        {
            base.Init(parent);
            this.GeneratedPaths.Add(Key, Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(config)/platform.hpp", this));
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.V2.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }

            // platform.hpp.in should never change, so don't check it
        }

        protected override void
        ExecuteInternal(ExecutionContext context)
        {
            var source = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/src/platform.hpp.in", this);

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
                        else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
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

    public sealed class ZMQSharedLibraryV2 :
        C.Cxx.V2.DynamicLibrary
    {
        protected override void Init(Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.Macros.Add("zmqsrcdir", Bam.Core.V2.TokenizedString.Create("$(pkgroot)/src", this));

            var source = this.CreateCxxSourceContainer("$(zmqsrcdir)/*.cpp", macroModuleOverride: this);

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.PreprocessorDefines.Add("DLL_EXPORT");

                    var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;

                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.Add(TokenizedString.Create("$(pkgroot)/builds/msvc", this));
                    }
                });
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX | Bam.Core.EPlatform.Unix))
            {
                // TODO: is there a call for a CompileWith function?
                var platformHeader = Bam.Core.V2.Graph.Instance.FindReferencedModule<ZMQPlatformHeader>();
                source.DependsOn(platformHeader);
                source.UsePublicPatches(platformHeader);
                // TODO: end of function

                source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.IncludePaths.Add(Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(config)", this));
                });

                if (this.Linker is Clang.V2.LinkerBase)
                {
                    var ipc_listener = source.Children.Where(item => (item as C.V2.ObjectFile).InputPath.Parse().EndsWith("ipc_listener.cpp"));
                    ipc_listener.ElementAt(0).PrivatePatch(settings => {
                        var compiler = settings as C.V2.ICommonCompilerOptions;
                        compiler.DisableWarnings.Add("deprecated-declarations");
                    });
                }
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);
            }

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.Add(TokenizedString.Create("$(pkgroot)/include", this));
                    }
                });

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.V2.ICommonLinkerOptions;
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        if (this.Linker is VisualC.V2.LinkerBase)
                        {
                            linker.Libraries.Add("Ws2_32.lib");
                            linker.Libraries.Add("Advapi32.lib");
                        }
                        else if (this.Linker is Mingw.V2.LinkerBase)
                        {
                            linker.Libraries.Add("-lws2_32");
                            linker.Libraries.Add("-ladvapi32");
                        }
                    }
                    else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
                    {
                        linker.Libraries.Add("-lpthread");
                    }
                });
        }
    }

    class ZMQSharedLibrary :
        C.DynamicLibrary
    {
        public
        ZMQSharedLibrary(
            Bam.Core.Target target)
        {
            var zmqDir = this.PackageLocation.SubDirectory("zeromq-3.2.3");
            var zmqIncludeDir = zmqDir.SubDirectory("include");
            this.headers.Include(zmqIncludeDir, "*.h");

#if D_PACKAGE_PUBLISHER_DEV
            // TODO: can this be automated?
            if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                this.publish.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MajorVersionSymlink));
                this.publish.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MinorVersionSymlink));
                this.publish.Add(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.LinkerSymlink));
            }
#endif
        }

        class SourceFiles :
            C.Cxx.ObjectFileCollection
        {
            public
            SourceFiles()
            {
                var zmqDir = this.PackageLocation.SubDirectory("zeromq-3.2.3");
                var zmqSrcDir = zmqDir.SubDirectory("src");
                this.Include(zmqSrcDir, "*.cpp");

                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(IncludePath);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(InternalIncludePath);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(Exceptions);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(DisableWarnings);
                this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(DllExport);
            }

            void
            DllExport(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                if (target.HasPlatform(Bam.Core.EPlatform.Windows) && target.HasToolsetType(typeof(VisualC.Toolset)))
                {
                    var options = module.Options as C.ICCompilerOptions;
                    if (null != options)
                    {
                        options.Defines.Add("DLL_EXPORT");
                    }
                }
            }

            void
            DisableWarnings(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var options = module.Options as VisualCCommon.ICCompilerOptions;
                if (null != options)
                {
                    options.WarningLevel = VisualCCommon.EWarningLevel.Level3;
                }

                if (target.HasPlatform(Bam.Core.EPlatform.Windows) && target.HasToolsetType(typeof(VisualC.Toolset)))
                {
                    var cOptions = module.Options as C.ICCompilerOptions;
                    if (null != cOptions)
                    {
                        cOptions.Defines.Add("_CRT_SECURE_NO_WARNINGS");
                        cOptions.DisableWarnings.Add("4099"); // C4099: 'zmq::i_msg_sink' : type name first seen using 'class' now seen using 'struct'
                        cOptions.DisableWarnings.Add("4800"); // warning C4800: 'const int' : forcing value to bool 'true' or 'false' (performance warning)
                    }
                }
            }

            void
            Exceptions(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var options = module.Options as C.ICxxCompilerOptions;
                if (null != options)
                {
                    options.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
                }
            }

            void
            InternalIncludePath(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var options = module.Options as C.ICCompilerOptions;
                if (null != options)
                {
                    if (target.HasPlatform(Bam.Core.EPlatform.Windows))
                    {
                        var zmqDir = this.PackageLocation.SubDirectory("zeromq-3.2.3");
                        var zmqBuildsDir = zmqDir.SubDirectory("builds");
                        options.IncludePaths.Include(zmqBuildsDir, "msvc");
                    }
                }
            }

            [C.ExportCompilerOptionsDelegate]
            void
            IncludePath(
                Bam.Core.IModule module,
                Bam.Core.Target target)
            {
                var options = module.Options as C.ICCompilerOptions;
                if (null != options)
                {
                    var zmqDir = this.PackageLocation.SubDirectory("zeromq-3.2.3");
                    options.IncludePaths.Include(zmqDir, "include");
                }
            }
        }

        [Bam.Core.SourceFiles]
        SourceFiles source = new SourceFiles();

        [C.HeaderFiles]
        Bam.Core.FileCollection headers = new Bam.Core.FileCollection();

        [Bam.Core.DependentModules(Platform=Bam.Core.EPlatform.Windows, ToolsetTypes=new [] { typeof(VisualC.Toolset)})]
        Bam.Core.TypeArray winDependents = new Bam.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
            );

        [C.RequiredLibraries(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.StringArray winLibs = new Bam.Core.StringArray(
            "Ws2_32.lib",
            "Advapi32.lib"
            );

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publish = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile)
            );
#endif
    }
}
