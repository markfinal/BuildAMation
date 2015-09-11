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
namespace QtCommon
{
namespace V2
{
    public static class Configure
    {
        class QtInstallPath :
            Bam.Core.V2.IStringCommandLineArgument,
            Bam.Core.V2.ICommandLineArgumentDefault<string>
        {
            string ICommandLineArgument.ContextHelp
            {
                get
                {
                    return "Define the Qt install location";
                }
            }

            string ICommandLineArgument.LongName
            {
                get
                {
                    return "--Qt.installPath";
                }
            }

            string ICommandLineArgument.ShortName
            {
                get
                {
                    return null;
                }
            }

            string ICommandLineArgumentDefault<string>.Default
            {
                get
                {
                    var graph = Bam.Core.V2.Graph.Instance;
                    var qtVersion = graph.Packages.Where(item => item.Name == "Qt").ElementAt(0).Version;

                    switch (Bam.Core.OSUtilities.CurrentOS)
                    {
                        case Bam.Core.EPlatform.Windows:
                            return GetWindowsInstallPath(qtVersion);

                        case Bam.Core.EPlatform.Unix:
                            return GetLinuxInstallPath(qtVersion);

                        case Bam.Core.EPlatform.OSX:
                            return GetOSXInstallPath(qtVersion);
                    }

                    throw new Bam.Core.Exception("Unable to determine default Qt {0} installation", qtVersion);
                }
            }

            private static string
            GetWindowsInstallPath(
                string qtVersion)
            {
                using (var key = Bam.Core.Win32RegistryUtilities.OpenCUSoftwareKey(System.String.Format(@"Microsoft\Windows\CurrentVersion\Uninstall\Qt {0}", qtVersion)))
                {
                    if (null == key)
                    {
                        throw new Bam.Core.Exception("Qt libraries for {0} were not installed", qtVersion);
                    }

                    var installPath = key.GetValue("InstallLocation") as string;
                    if (null == installPath)
                    {
                        throw new Bam.Core.Exception("Unable to locate InstallLocation registry key for Qt {0}", qtVersion);
                    }

                    // precompiled binaries now have a subdirectory indicating their flavour
                    installPath += @"\5.3\msvc2013_64_opengl";

                    Bam.Core.Log.DebugMessage("Qt installation folder is {0}", installPath);
                    return installPath;
                }

                throw new Bam.Core.Exception("Unable to find Qt installation path");
            }

            private static string
            GetLinuxInstallPath(
                string qtVersion)
            {
                var homeDir = System.Environment.GetEnvironmentVariable("HOME");
                if (null == homeDir)
                {
                    throw new Bam.Core.Exception("Unable to determine home directory");
                }

                var qtVersionSplit = qtVersion.Split('.');

                var installPath = System.String.Format("{0}/Qt{1}/{2}.{3}/gcc_64", homeDir, qtVersion, qtVersionSplit[0], qtVersionSplit[1]);
                return installPath;
            }

            private static string
            GetOSXInstallPath(
                string qtVersion)
            {
                var homeDir = System.Environment.GetEnvironmentVariable("HOME");
                if (null == homeDir)
                {
                    throw new Bam.Core.Exception("Unable to determine home directory");
                }

                var qtVersionSplit = qtVersion.Split('.');

                var installPath = System.String.Format("{0}/Qt{1}/{2}.{3}/clang_64", homeDir, qtVersion, qtVersionSplit[0], qtVersionSplit[1]);
                return installPath;
            }
        }

        static Configure()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            var qtVersion = graph.Packages.Where(item => item.Name == "Qt").ElementAt(0).Version;

            var qtInstallDir = Bam.Core.V2.CommandLineProcessor.Evaluate(new QtInstallPath());
            InstallPath = Bam.Core.V2.TokenizedString.Create(qtInstallDir, null);
        }

        public static Bam.Core.V2.TokenizedString InstallPath
        {
            get;
            private set;
        }
    }

    public abstract class CommonFramework :
        C.V2.ExternalFramework
    {
        protected CommonFramework(
            string moduleName) :
            base()
        {
            this.Macros.Add("QtModuleName", moduleName);
            this.Macros.Add("QtInstallPath", Configure.InstallPath);
        }

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.Macros.Add("QtFrameworkPath", Bam.Core.V2.TokenizedString.Create("$(QtInstallPath)/lib", this));

            this.PublicPatch((settings, appliedTo) =>
            {
                var osxCompiler = settings as C.V2.ICCompilerOptionsOSX;
                if (null != osxCompiler)
                {
                    osxCompiler.FrameworkSearchDirectories.AddUnique(this.Macros["QtFrameworkPath"]);
                }

                var osxLinker = settings as C.V2.ILinkerOptionsOSX;
                if (null != osxLinker)
                {
                    osxLinker.Frameworks.AddUnique(Bam.Core.V2.TokenizedString.Create("$(QtFrameworkPath)/Qt$(QtModuleName).framework", this));
                    osxLinker.FrameworkSearchDirectories.AddUnique(this.Macros["QtFrameworkPath"]);
                }
            });
        }

        public override void Evaluate()
        {
            this.ReasonToExecute = null;
        }

        protected override void ExecuteInternal(Bam.Core.V2.ExecutionContext context)
        {
            // prebuilt - no execution
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // prebuilt - no execution policy
        }
    }

    public abstract class CommonModule :
        C.V2.DynamicLibrary
    {
        protected CommonModule(
            string moduleName) :
            base()
        {
            this.Macros.Add("QtModuleName", moduleName);
            this.Macros.Add("QtInstallPath", Configure.InstallPath);
        }

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.Macros.Add("QtIncludePath", Bam.Core.V2.TokenizedString.Create("$(QtInstallPath)/include", this));
            this.Macros.Add("QtLibraryPath", Bam.Core.V2.TokenizedString.Create("$(QtInstallPath)/lib", this));
            this.Macros.Add("QtBinaryPath", Bam.Core.V2.TokenizedString.Create("$(QtInstallPath)/bin", this));
            this.Macros.Add("QtConfig", Bam.Core.V2.TokenizedString.Create((this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug) ? "d" : string.Empty, null));

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.GeneratedPaths[Key] = Bam.Core.V2.TokenizedString.Create("$(QtBinaryPath)/$(dynamicprefix)Qt5$(QtModuleName)$(QtConfig)$(dynamicext)", this);
                this.GeneratedPaths[ImportLibraryKey] = Bam.Core.V2.TokenizedString.Create("$(QtLibraryPath)/$(libprefix)Qt5$(QtModuleName)$(QtConfig)$(libext)", this);
            }
            else
            {
                this.GeneratedPaths[Key] = Bam.Core.V2.TokenizedString.Create("$(QtLibraryPath)/$(dynamicprefix)Qt5$(QtModuleName)$(dynamicext)", this);
            }

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                if (null != compiler)
                {
                    compiler.IncludePaths.AddUnique(this.Macros["QtIncludePath"]);
                }

                var linker = settings as C.V2.ICommonLinkerOptions;
                if (null != linker)
                {
                    linker.LibraryPaths.AddUnique(this.Macros["QtLibraryPath"]);
                }
            });
        }

        public override void Evaluate()
        {
            this.ReasonToExecute = null;
        }

        protected override void ExecuteInternal(Bam.Core.V2.ExecutionContext context)
        {
            // prebuilt - no execution
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // prebuilt - no execution policy
        }
    }
}
    public abstract class Base :
        ThirdPartyModule
    {
        protected
        Base()
        {
            this.QtToolset = Bam.Core.State.Get("Toolset", "Qt") as Toolset;
#if D_PACKAGE_PUBLISHER_DEV
            // TODO: can this be automated?
            if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                this.publishKeys.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MajorVersionSymlink));
                this.publishKeys.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.MinorVersionSymlink));
                this.publishKeys.AddUnique(new Publisher.PublishDependency(C.PosixSharedLibrarySymlinks.LinkerSymlink));
            }
#endif
        }

        private Toolset QtToolset
        {
            get;
            set;
        }

        protected void
        AddIncludePath(
            C.ICCompilerOptions options,
            Bam.Core.Target target,
            string moduleName)
        {
            var includePath = this.QtToolset.GetIncludePath((Bam.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(includePath))
            {
                options.IncludePaths.Add(includePath);
                if (this.QtToolset.IncludePathIncludesQtModuleName)
                {
                    includePath = System.IO.Path.Combine(includePath, moduleName);
                    options.IncludePaths.Add(includePath);
                }
            }
        }

        protected void
        AddFrameworkIncludePath(
            C.ICCompilerOptionsOSX options,
            Bam.Core.Target target)
        {
            var libraryPath = this.QtToolset.GetLibraryPath((Bam.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(libraryPath))
            {
                options.FrameworkSearchDirectories.Add(libraryPath);
            }
        }

        protected void
        AddLibraryPath(
            C.ILinkerOptions options,
            Bam.Core.Target target)
        {
            var libraryPath = this.QtToolset.GetLibraryPath((Bam.Core.BaseTarget)target);
            if (!string.IsNullOrEmpty(libraryPath))
            {
                if (target.HasPlatform(Bam.Core.EPlatform.OSX))
                {
                    var osxOptions = options as C.ILinkerOptionsOSX;
                    osxOptions.FrameworkSearchDirectories.Add(libraryPath);
                }
                else
                {
                    options.LibraryPaths.Add(libraryPath);
                }
            }
        }

        protected void
        AddModuleLibrary(
            C.ILinkerOptions options,
            Bam.Core.Target target,
            bool hasQtPrefix,
            string moduleName)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                {
                    options.Libraries.Add(System.String.Format("{0}{1}d.lib", hasQtPrefix ? "Qt5" : string.Empty, moduleName));
                }
                else
                {
                    options.Libraries.Add(System.String.Format("{0}{1}.lib", hasQtPrefix ? "Qt5" : string.Empty, moduleName));
                }
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                options.Libraries.Add(System.String.Format("-l{0}{1}", hasQtPrefix ? "Qt5" : string.Empty, moduleName));
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var osxLinkerOptions = options as C.ILinkerOptionsOSX;
                osxLinkerOptions.Frameworks.Add(System.String.Format("{0}{1}", hasQtPrefix ? "Qt" : string.Empty, moduleName));
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        protected void
        GetModuleDynamicLibrary(
            Bam.Core.Target target,
            string moduleName)
        {
            if (target.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                var binPath = (this.QtToolset as Bam.Core.IToolset).BinPath((Bam.Core.BaseTarget)target);
                string dynamicLibraryName = null;
                if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                {
                    dynamicLibraryName = System.String.Format("{0}d.dll", moduleName);
                }
                else
                {
                    dynamicLibraryName = System.String.Format("{0}.dll", moduleName);
                }
                var dynamicLibraryPath = System.IO.Path.Combine(binPath, dynamicLibraryName);
                this.Locations[C.DynamicLibrary.OutputFile] = Bam.Core.FileLocation.Get(dynamicLibraryPath);
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Unix))
            {
                var libPath = this.QtToolset.GetLibraryPath((Bam.Core.BaseTarget)target);
                var version = (this.QtToolset as Bam.Core.IToolset).Version((Bam.Core.BaseTarget)target);
                var versionSplit = version.Split('.');
                var majorVersion = versionSplit[0];
                var minorVersion = versionSplit[1];
                var patchVersion = versionSplit[2];

                // real library name
                var realDynamicLibraryLeafname = System.String.Format("lib{0}.so.{1}.{2}.{3}", moduleName, majorVersion, minorVersion, patchVersion);
                var realDynamicLibraryPath = System.IO.Path.Combine(libPath, realDynamicLibraryLeafname);
                this.Locations[C.DynamicLibrary.OutputFile] = Bam.Core.FileLocation.Get(realDynamicLibraryPath);

                // so library name (major version)
                var soNameDynamicLibraryLeafname = System.String.Format("lib{0}.so.{1}", moduleName, majorVersion);
                var soNameDynamicLibraryPath = System.IO.Path.Combine(libPath, soNameDynamicLibraryLeafname);
                this.Locations[C.PosixSharedLibrarySymlinks.MajorVersionSymlink] = Bam.Core.SymlinkLocation.Get(soNameDynamicLibraryPath);

                // minor version library name
                var minorVersionDynamicLibraryLeafname = System.String.Format("lib{0}.so.{1}.{2}", moduleName, majorVersion, minorVersion);
                var minorVersionDynamicLibraryPath = System.IO.Path.Combine(libPath, minorVersionDynamicLibraryLeafname);
                this.Locations[C.PosixSharedLibrarySymlinks.MinorVersionSymlink] = Bam.Core.SymlinkLocation.Get(minorVersionDynamicLibraryPath);

                // linker library name
                var linkerDynamicLibraryLeafname = System.String.Format("lib{0}.so", moduleName);
                var linkerDynamicLibraryPath = System.IO.Path.Combine(libPath, linkerDynamicLibraryLeafname);
                this.Locations[C.PosixSharedLibrarySymlinks.LinkerSymlink] = Bam.Core.SymlinkLocation.Get(linkerDynamicLibraryPath);
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                // TODO: this needs some rework with publishing, as it ought to be a framework
#if false
                return Bam.Core.FileLocation.Get(moduleName);
#endif
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

#if D_PACKAGE_PUBLISHER_DEV
        [Publisher.CopyFileLocations(Platform = Bam.Core.EPlatform.NotOSX)]
        protected Bam.Core.Array<Publisher.PublishDependency> publishKeys = new Bam.Core.Array<Publisher.PublishDependency>(
            new Publisher.PublishDependency(C.DynamicLibrary.OutputFile));
#endif
    }
}
