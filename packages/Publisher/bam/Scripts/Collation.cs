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
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Publishing Root");
        private Bam.Core.Array<CollatedFile> CopiedFrameworks = new Bam.Core.Array<CollatedFile>();
        private Bam.Core.Array<ChangeNameOSX> ChangedNamedBinaries = new Bam.Core.Array<ChangeNameOSX>();

        public enum EPublishingType
        {
            ConsoleApplication,
            WindowedApplication
        }

        protected Collation()
        {
            if (!Bam.Core.Graph.Instance.BuildModeMetaData.PublishBesideExecutable)
            {
                this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
            }
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

        public static Bam.Core.TokenizedString
        GenerateFileCopyDestination(
            Bam.Core.Module module,
            Bam.Core.TokenizedString referenceFilePath,
            Bam.Core.TokenizedString subDirectory,
            Bam.Core.TokenizedString unReferencedRoot)
        {
            if (referenceFilePath != null)
            {
                if (null != subDirectory)
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)",
                        referenceFilePath,
                        subDirectory);
                }
                else
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/)",
                        referenceFilePath);
                }
            }
            else
            {
                if (null != subDirectory)
                {
                    return module.CreateTokenizedString("@normalize($(0)/$(1)/)",
                        unReferencedRoot,
                        subDirectory);
                }
                else
                {
                    return module.CreateTokenizedString("@normalize($(0)/)",
                        unReferencedRoot);
                }
            }
        }

        public static Bam.Core.TokenizedString
        GenerateDirectoryCopyDestination(
            Bam.Core.Module module,
            Bam.Core.TokenizedString referenceFilePath,
            Bam.Core.TokenizedString subDirectory,
            Bam.Core.TokenizedString sourcePath)
        {
            // Windows XCOPY requires the directory name to be added to the destination, while Posix cp does not
            if (null != subDirectory)
            {
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/$(1)/@filename($(2))/)",
                        referenceFilePath,
                        subDirectory,
                        sourcePath);
                }
                else
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)",
                        referenceFilePath,
                        subDirectory);
                }
            }
            else
            {
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return module.CreateTokenizedString("@normalize(@dir($(0))/@filename($(1))/)",
                        referenceFilePath,
                        sourcePath);
                }
                else
                {
                    return module.Macros["CopyDir"] = module.CreateTokenizedString("@normalize(@dir($(0))/)",
                        referenceFilePath);
                }
            }
        }

        public static Bam.Core.TokenizedString
        GenerateSymbolicLinkCopyDestination(
            Bam.Core.Module module,
            Bam.Core.TokenizedString referenceFilePath,
            Bam.Core.TokenizedString subDirectory)
        {
            if (null != subDirectory)
            {
                return module.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)",
                    referenceFilePath,
                    subDirectory);
            }
            else
            {
                return module.CreateTokenizedString("@normalize(@dir($(0))/)",
                    referenceFilePath);
            }
        }

        private CollatedFile
        CreateCollatedFile(
            Bam.Core.Module sourceModule,
            Bam.Core.TokenizedString sourcePath,
            CollatedFile reference,
            Bam.Core.TokenizedString subDirectory)
        {
            var copyFileModule = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
                {
                    Bam.Core.TokenizedString referenceFilePath = null;
                    if (null != reference)
                    {
                        referenceFilePath = reference.GeneratedPaths[CollatedObject.Key];
                    }
                    else
                    {
                        if (!this.GeneratedPaths.ContainsKey(Key))
                        {
                            this.RegisterGeneratedFile(Key, module.CreateTokenizedString("@dir($(0))", sourcePath));
                        }
                    }
                    module.Macros["CopyDir"] = GenerateFileCopyDestination(
                        module,
                        referenceFilePath,
                        subDirectory,
                        this.GeneratedPaths[Key]);
                });
            this.Requires(copyFileModule);

            copyFileModule.SourceModule = sourceModule;
            copyFileModule.SourcePath = sourcePath;
            copyFileModule.Reference = reference;
            copyFileModule.SubDirectory = subDirectory;
            return copyFileModule;
        }

        private CollatedDirectory
        CreateCollatedDirectory(
            Bam.Core.Module sourceModule,
            Bam.Core.TokenizedString sourcePath,
            CollatedFile reference,
            Bam.Core.TokenizedString subDirectory)
        {
            if (null == reference)
            {
                throw new Bam.Core.Exception("Collating a directory requires a collated file as reference");
            }

            var copyDirectoryModule = Bam.Core.Module.Create<CollatedDirectory>(preInitCallback: module =>
            {
                module.Macros["CopyDir"] = GenerateDirectoryCopyDestination(
                    module,
                    reference.GeneratedPaths[CollatedObject.Key],
                    subDirectory,
                    sourcePath);
            });
            this.Requires(copyDirectoryModule);

            copyDirectoryModule.SourceModule = sourceModule;
            copyDirectoryModule.SourcePath = sourcePath;
            copyDirectoryModule.Reference = reference;
            copyDirectoryModule.SubDirectory = subDirectory;
            return copyDirectoryModule;
        }

        private CollatedSymbolicLink
        CreateCollatedSymbolicLink(
            Bam.Core.Module sourceModule,
            Bam.Core.TokenizedString sourcePath,
            CollatedFile reference,
            Bam.Core.TokenizedString subDirectory)
        {
            if (null == reference)
            {
                throw new Bam.Core.Exception("Collating a symbolic link requires a collated file as reference");
            }

            var copySymlinkModule = Bam.Core.Module.Create<CollatedSymbolicLink>(preInitCallback: module =>
            {
                module.Macros["CopyDir"] = GenerateSymbolicLinkCopyDestination(
                    module,
                    reference.GeneratedPaths[CollatedObject.Key],
                    subDirectory);
            });
            this.Requires(copySymlinkModule);

            copySymlinkModule.SourceModule = sourceModule;
            copySymlinkModule.SourcePath = sourcePath;
            copySymlinkModule.Reference = reference;
            copySymlinkModule.SubDirectory = subDirectory;
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
            var copySymlink = this.CreateCollatedSymbolicLink(
                copyFileModule.SourceModule,
                copyFileModule.SourceModule.Macros["SOName"],
                copyFileModule.Reference,
                copyFileModule.SubDirectory);
            copySymlink.AssignLinkTarget(copySymlink.CreateTokenizedString("@filename($(0))", copyFileModule.SourcePath));
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
            Bam.Core.PathKey key,
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

            var copyFileModule = this.CreateCollatedFile(
                dependent,
                dependent.GeneratedPaths[key],
                null,
                Bam.Core.TokenizedString.CreateVerbatim(destSubDir));

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
            Bam.Core.PathKey key,
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            var copyFileModule = this.CreateCollatedFile(
                dependent,
                dependent.GeneratedPaths[key],
                reference,
                Bam.Core.TokenizedString.CreateVerbatim(subdir));

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

            var copyFileModule = this.CreateCollatedFile(
                dependent,
                dependent.CreateTokenizedString(parameterizedFilePath),
                reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));

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
            var copyFileModule = this.CreateCollatedFile(
                null,
                parameterizedFilePath,
                reference,
                Bam.Core.TokenizedString.CreateVerbatim(subdir));

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
            this.CreateCollatedDirectory(null, parameterizedPath, reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
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

            var framework = dependent as C.OSXFramework;
            if (null == framework)
            {
                throw new Bam.Core.Exception("Module {0} did not derive from {1}", dependent.GetType().ToString(), typeof(C.OSXFramework).ToString());
            }
            var frameworkPath = framework.FrameworkPath;

            var dirPublishedModules = new Bam.Core.Array<CollatedDirectory>();
            if (null != framework.DirectoriesToPublish)
            foreach (var dirData in framework.DirectoriesToPublish)
            {
                var dir = dirData.SourcePath;
                var copyDir = this.CreateCollatedDirectory(
                    dependent,
                    this.CreateTokenizedString("$(0)/$(1)", frameworkPath, dir),
                    reference,
                    this.CreateTokenizedString("$(0)/$(1)", subdirTS, dirData.DestinationPath != null ? dirData.DestinationPath : dir));
                dirPublishedModules.Add(copyDir);
            }
            var filePublishedModules = new Bam.Core.Array<CollatedFile>();
            if (null != framework.FilesToPublish)
            {
                foreach (var fileData in framework.FilesToPublish)
                {
                    var file = fileData.SourcePath;
                    var copyFile = this.CreateCollatedFile(
                        dependent,
                        this.CreateTokenizedString("$(0)/$(1)", frameworkPath, file),
                        reference,
                        this.CreateTokenizedString("$(0)/@dir($(1))", subdirTS, fileData.DestinationPath != null ? fileData.DestinationPath : file));
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
                    var copySymlink = this.CreateCollatedSymbolicLink(
                        dependent,
                        this.CreateTokenizedString("$(0)/$(1)", frameworkPath, symlink),
                        reference,
                        this.CreateTokenizedString("$(0)/@dir($(1))", subdirTS, symlink));
                    copySymlink.AssignLinkTarget(symlinkData.DestinationPath);
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
