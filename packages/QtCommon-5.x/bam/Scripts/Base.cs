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
namespace QtCommon
{
    public static class Configure
    {
        class QtInstallPath :
            Bam.Core.IStringCommandLineArgument,
            Bam.Core.ICommandLineArgumentDefault<string>
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
                    var graph = Bam.Core.Graph.Instance;
                    var qtVersion = graph.Packages.Where(item => item.Name == "Qt").ElementAt(0).Version;

                    switch (Bam.Core.OSUtilities.CurrentOS)
                    {
                        case Bam.Core.EPlatform.Windows:
                            return GetWindowsInstallPath(qtVersion);

                        case Bam.Core.EPlatform.Linux:
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
            var graph = Bam.Core.Graph.Instance;
            var qtVersion = graph.Packages.Where(item => item.Name == "Qt").ElementAt(0).Version;

            var qtInstallDir = Bam.Core.CommandLineProcessor.Evaluate(new QtInstallPath());
            if (!System.IO.Directory.Exists(qtInstallDir))
            {
                throw new Bam.Core.Exception("Qt install dir, {0}, does not exist", qtInstallDir);
            }
            InstallPath = Bam.Core.TokenizedString.Create(qtInstallDir, null);
        }

        public static Bam.Core.TokenizedString InstallPath
        {
            get;
            private set;
        }
    }

    public abstract class CommonFramework :
        C.ExternalFramework
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
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Macros.Add("QtFrameworkPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/lib", this));

            this.PublicPatch((settings, appliedTo) =>
            {
                var osxCompiler = settings as C.ICCompilerOptionsOSX;
                if (null != osxCompiler)
                {
                    osxCompiler.FrameworkSearchDirectories.AddUnique(this.Macros["QtFrameworkPath"]);
                }

                var osxLinker = settings as C.ILinkerOptionsOSX;
                if (null != osxLinker)
                {
                    osxLinker.Frameworks.AddUnique(Bam.Core.TokenizedString.Create("$(QtFrameworkPath)/Qt$(QtModuleName).framework", this));
                    osxLinker.FrameworkSearchDirectories.AddUnique(this.Macros["QtFrameworkPath"]);
                }
            });
        }

        public override void Evaluate()
        {
            this.ReasonToExecute = null;
        }

        protected override void ExecuteInternal(Bam.Core.ExecutionContext context)
        {
            // prebuilt - no execution
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // prebuilt - no execution policy
        }
    }

    public abstract class CommonModule :
        C.DynamicLibrary
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
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Macros.Add("QtIncludePath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/include", this));
            this.Macros.Add("QtLibraryPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/lib", this));
            this.Macros.Add("QtBinaryPath", Bam.Core.TokenizedString.Create("$(QtInstallPath)/bin", this));
            this.Macros.Add("QtConfig", Bam.Core.TokenizedString.Create((this.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug) ? "d" : string.Empty, null));

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.GeneratedPaths[Key] = Bam.Core.TokenizedString.Create("$(QtBinaryPath)/$(dynamicprefix)Qt5$(QtModuleName)$(QtConfig)$(dynamicext)", this);
                this.GeneratedPaths[ImportLibraryKey] = Bam.Core.TokenizedString.Create("$(QtLibraryPath)/$(libprefix)Qt5$(QtModuleName)$(QtConfig)$(libext)", this);
            }
            else
            {
                this.GeneratedPaths[Key] = Bam.Core.TokenizedString.Create("$(QtLibraryPath)/$(dynamicprefix)Qt5$(QtModuleName)$(dynamicext)", this);
            }

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerOptions;
                if (null != compiler)
                {
                    compiler.IncludePaths.AddUnique(this.Macros["QtIncludePath"]);
                }

                var linker = settings as C.ICommonLinkerOptions;
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

        protected override void ExecuteInternal(Bam.Core.ExecutionContext context)
        {
            // prebuilt - no execution
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // prebuilt - no execution policy
        }
    }
}
