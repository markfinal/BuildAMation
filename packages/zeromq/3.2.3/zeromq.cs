#region License
// Copyright 2010-2014 Mark Final
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
#endregion
namespace zeromq
{
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

#if OPUSPACKAGE_PUBLISHER_DEV
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

#if OPUSPACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations]
        Bam.Core.Array<Publisher.PublishDependency> publish = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile)
            );
#endif
    }
}
