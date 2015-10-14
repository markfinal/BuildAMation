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
namespace Publisher
{
    public abstract class Collation :
        Bam.Core.Module
    {
        private ICollationPolicy Policy = null;
        public static Bam.Core.FileKey PublishingRoot = Bam.Core.FileKey.Generate("Publishing Root");
        private Bam.Core.Array<CollatedFile> CopiedFrameworks = new Bam.Core.Array<CollatedFile>();
        private Bam.Core.Array<ChangeNameOSX> ChangedNamedBinaries = new Bam.Core.Array<ChangeNameOSX>();

        public enum EPublishingType
        {
            ConsoleApplication,
            WindowedApplication
        }

        protected Collation()
        {
            this.RegisterGeneratedFile(PublishingRoot, this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
        }

        private string
        PublishingPath(
            Bam.Core.Module module,
            EPublishingType type)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX) &&
                (EPublishingType.WindowedApplication == type))
            {
                return module.CreateTokenizedString("$(OutputName).app/Contents/MacOS").Parse();
            }
            return null;
        }

        private CollatedFile
        CreateCollatedFile(
            CollatedFile reference = null,
            Bam.Core.TokenizedString subDirectory = null)
        {
            var copyFileModule = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
                {
                    if (reference != null)
                    {
                        if (null != subDirectory)
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], subDirectory);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey]);
                        }
                    }
                    else
                    {
                        if (null != subDirectory)
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/$(1)/)", this.GeneratedPaths[PublishingRoot], subDirectory);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/)", this.GeneratedPaths[PublishingRoot]);
                        }
                    }
                });
            this.Requires(copyFileModule);
            if (null != reference)
            {
                copyFileModule.Reference = reference;
            }
            if (null != subDirectory)
            {
                copyFileModule.SubDirectory = subDirectory;
            }
            return copyFileModule;
        }

        private CollatedDirectory
        CreateCollatedDirectory(
            CollatedFile reference = null,
            Bam.Core.TokenizedString subDirectory = null)
        {
            var copyDirectoryModule = Bam.Core.Module.Create<CollatedDirectory>(preInitCallback: module =>
            {
                // Windows XCOPY requires the directory name to be added to the destination, while Posix cp does not
                if (reference != null)
                {
                    if (null != subDirectory)
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/$(1)/@filename($(2))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], subDirectory, (module as CollatedDirectory).SourcePath);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], subDirectory);
                        }
                    }
                    else
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/@filename($(1))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], (module as CollatedDirectory).SourcePath);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey]);
                        }
                    }
                }
                else
                {
                    if (null != subDirectory)
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/$(1)/@filename($(2))/)", this.GeneratedPaths[PublishingRoot], subDirectory, (module as CollatedDirectory).SourcePath);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/$(1)/)", this.GeneratedPaths[PublishingRoot], subDirectory);
                        }
                    }
                    else
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/@filename($(1))/)", this.GeneratedPaths[PublishingRoot], (module as CollatedDirectory).SourcePath);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/)", this.GeneratedPaths[PublishingRoot]);
                        }
                    }
                }
            });
            this.Requires(copyDirectoryModule);
            if (null != reference)
            {
                copyDirectoryModule.Reference = reference;
            }
            if (null != subDirectory)
            {
                copyDirectoryModule.SubDirectory = subDirectory;
            }
            return copyDirectoryModule;
        }

        private CollatedSymbolicLink
        CreateCollatedSymbolicLink(
            CollatedFile reference = null,
            Bam.Core.TokenizedString subDirectory = null)
        {
            var copySymlinkModule = Bam.Core.Module.Create<CollatedSymbolicLink>(preInitCallback: module =>
            {
                if (reference != null)
                {
                    if (null != subDirectory)
                    {
                        module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], subDirectory);
                    }
                    else
                    {
                        module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey]);
                    }
                }
                else
                {
                    if (null != subDirectory)
                    {
                        module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/$(1)/)", this.GeneratedPaths[PublishingRoot], subDirectory);
                    }
                    else
                    {
                        module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/)", this.GeneratedPaths[PublishingRoot]);
                    }
                }
            });
            this.Requires(copySymlinkModule);
            if (null != reference)
            {
                copySymlinkModule.Reference = reference;
            }
            if (null != subDirectory)
            {
                copySymlinkModule.SubDirectory = subDirectory;
            }
            return copySymlinkModule;
        }

        private void
        AddOSXChangeIDNameForBinary(
            CollatedFile copyFileModule)
        {
            if (!this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                return;
            }
            var changeIDName = Bam.Core.Module.Create<ChangeNameOSX>();
            changeIDName.Source = copyFileModule;
            changeIDName.Frameworks = this.CopiedFrameworks;
            this.ChangedNamedBinaries.Add(changeIDName);
            this.Requires(changeIDName);

            foreach (var framework in this.CopiedFrameworks)
            {
                changeIDName.Requires(framework);
            }
        }

        private void
        CopySONameSymlink(
            CollatedFile copyFileModule)
        {
            if (!this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                return;
            }
            var copySymlink = this.CreateCollatedSymbolicLink(copyFileModule.Reference, copyFileModule.SubDirectory);
            copySymlink.SourceModule = copyFileModule.SourceModule;
            copySymlink.SourcePath = copyFileModule.SourceModule.Macros["SOName"];
            copySymlink.LinkTarget(copySymlink.CreateTokenizedString("@filename($(0))", copyFileModule.SourcePath));
        }

        private bool
        IsReferenceAWindowedApp(
            CollatedFile reference)
        {
            if (null == reference.SubDirectory)
            {
                return false;
            }
            return reference.SubDirectory.Parse().Contains(".app");
        }

        public CollatedFile
        Include<DependentModule>(
            Bam.Core.FileKey key,
            EPublishingType type,
            string subdir = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            var path = this.PublishingPath(dependent, type);
            string destSubDir;
            if (null == path)
            {
                destSubDir = subdir;
            }
            else
            {
                if (null != subdir)
                {
                    destSubDir = System.IO.Path.Combine(path, subdir);
                }
                else
                {
                    destSubDir = path;
                }
            }

            var copyFileModule = this.CreateCollatedFile(subDirectory: Bam.Core.TokenizedString.CreateVerbatim(destSubDir));
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.GeneratedPaths[key];

            if (EPublishingType.WindowedApplication == type)
            {
                if (C.ConsoleApplication.Key == key)
                {
                    this.AddOSXChangeIDNameForBinary(copyFileModule);
                }
            }

            return copyFileModule;
        }

        public CollatedFile
        Include<DependentModule>(
            Bam.Core.FileKey key,
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            var copyFileModule = this.CreateCollatedFile(reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.GeneratedPaths[key];

            if (this.IsReferenceAWindowedApp(reference))
            {
                if (C.ConsoleApplication.Key == key)
                {
                    this.AddOSXChangeIDNameForBinary(copyFileModule);
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                if ((dependent is C.IDynamicLibrary) && dependent.Macros.Contains("SOName"))
                {
                    this.CopySONameSymlink(copyFileModule);
                }
            }

            return copyFileModule;
        }

        public void
        IncludeFiles<DependentModule>(
            string parameterizedFilePath,
            string subdir,
            CollatedFile reference,
            bool isExecutable = false) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            var copyFileModule = this.CreateCollatedFile(reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.CreateTokenizedString(parameterizedFilePath);

            if (isExecutable)
            {
                if (this.IsReferenceAWindowedApp(reference))
                {
                    this.AddOSXChangeIDNameForBinary(copyFileModule);
                }
            }
        }

        public void
        IncludeFile(
            string parameterizedFilePath,
            string subdir,
            CollatedFile reference,
            bool isExecutable = false)
        {
            var tokenString = this.CreateTokenizedString(parameterizedFilePath);
            this.IncludeFile(tokenString, subdir, reference, isExecutable);
        }

        public void
        IncludeFile(
            Bam.Core.TokenizedString parameterizedFilePath,
            string subdir,
            CollatedFile reference,
            bool isExecutable = false)
        {
            var copyFileModule = this.CreateCollatedFile(reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
            copyFileModule.SourcePath = parameterizedFilePath;

            if (isExecutable)
            {
                if (this.IsReferenceAWindowedApp(reference))
                {
                    this.AddOSXChangeIDNameForBinary(copyFileModule);
                }
            }
        }

        public void
        IncludeDirectory(
            Bam.Core.TokenizedString parameterizedPath,
            string subdir,
            CollatedFile reference)
        {
            var copyDirectoryModule = this.CreateCollatedDirectory(reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
            copyDirectoryModule.SourcePath = parameterizedPath;
        }

        public void
        IncludeFramework<DependentModule>(
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            // TODO: confirm that reference was created in WindowedApplication mode

            var subdirTS = Bam.Core.TokenizedString.CreateVerbatim(subdir);

            var framework = dependent as C.ExternalFramework;
            if (null == framework)
            {
                throw new Bam.Core.Exception("Module {0} did not derive from {1}", dependent.GetType().ToString(), typeof(C.ExternalFramework).ToString());
            }
            var frameworkPath = framework.FrameworkPath;

            var dirPublishedModules = new Bam.Core.Array<CollatedDirectory>();
            if (null != framework.DirectoriesToPublish)
            foreach (var dirData in framework.DirectoriesToPublish)
            {
                var dir = dirData.SourcePath;
                var copyDir = this.CreateCollatedDirectory(reference, this.CreateTokenizedString("$(0)/$(1)", subdirTS, dirData.DestinationPath != null ? dirData.DestinationPath : dir));
                copyDir.SourceModule = dependent;
                copyDir.SourcePath = this.CreateTokenizedString("$(0)/$(1)", frameworkPath, dir);
                dirPublishedModules.Add(copyDir);
            }
            var filePublishedModules = new Bam.Core.Array<CollatedFile>();
            if (null != framework.FilesToPublish)
            {
                foreach (var fileData in framework.FilesToPublish)
                {
                    var file = fileData.SourcePath;
                    var copyFile = this.CreateCollatedFile(reference, this.CreateTokenizedString("$(0)/@dir($(1))", subdirTS, fileData.DestinationPath != null ? fileData.DestinationPath : file));
                    copyFile.SourceModule = dependent;
                    copyFile.SourcePath = this.CreateTokenizedString("$(0)/$(1)", frameworkPath, file);
                    foreach (var publishedDir in dirPublishedModules)
                    {
                        copyFile.Requires(publishedDir);
                    }
                    filePublishedModules.Add(copyFile);

                    if (file == framework.Macros["FrameworkLibraryPath"])
                    {
                        var updateIDName = Bam.Core.Module.Create<IdNameOSX>();
                        updateIDName.Source = copyFile;
                        this.Requires(updateIDName);
                        this.CopiedFrameworks.Add(copyFile);

                        foreach (var changedName in this.ChangedNamedBinaries)
                        {
                            changedName.Requires(updateIDName);
                        }

                        if (this.IsReferenceAWindowedApp(reference))
                        {
                            this.AddOSXChangeIDNameForBinary(copyFile);
                        }
                    }
                }
            }
            if (null != framework.SymlinksToPublish)
            {
                foreach (var symlinkData in framework.SymlinksToPublish)
                {
                    var symlink = symlinkData.SourcePath;
                    var copySymlink = this.CreateCollatedSymbolicLink(reference, this.CreateTokenizedString("$(0)/@dir($(1))", subdirTS, symlink));
                    copySymlink.SourceModule = dependent;
                    copySymlink.SourcePath = this.CreateTokenizedString("$(0)/$(1)", frameworkPath, symlink);
                    copySymlink.LinkTarget(symlinkData.DestinationPath);
                    foreach (var publishedDir in dirPublishedModules)
                    {
                        copySymlink.Requires(publishedDir);
                    }
                    foreach (var publishedFile in filePublishedModules)
                    {
                        copySymlink.Requires(publishedFile);
                    }
                }
            }
        }

        public void
        ChangeRPath(
            CollatedFile source,
            string newRPath)
        {
            var change = Bam.Core.Module.Create<ChangeRPathModule>();
            change.Source = source;
            change.NewRPath = newRPath;
            this.Requires(change);
        }

        public override void
        Evaluate()
        {
            // TODO
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.Collate(this, context);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "Collation";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<ICollationPolicy>.Create(className);
                    }
                    break;
            }
        }
    }
}
