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
using System.Linq;
namespace Publisher
{
#if D_NEW_PUBLISHING
    public abstract class StrippedBinaryCollation :
        Bam.Core.Module
    {
        private IStrippedBinaryCollationPolicy Policy = null;

        public sealed override void
        Evaluate()
        {
            // TODO
        }

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.CollateStrippedBinaries(this, context);
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "StrippedBinaryCollation";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<IStrippedBinaryCollationPolicy>.Create(className);
                    }
                    break;
            }
        }
    }
#else
    /// <summary>
    /// Derive from this module to generate a standalone directory of stripped binaries and other
    /// collated files that mirrors a collated publishing root. This is identical to that publishing
    /// root other than all binaries are stripped.
    /// This mirror folder can be distributed as is to users, or further processed by Installer
    /// modules in Bam.
    /// On Windows VisualC, all files are simply copied, as they do not contain debug information.
    /// On Linux, strip and objcopy is used to strip binaries, but link back to already hived off debug symbol files.
    /// On OSX, strip is used to strip binaries.
    /// </summary>
    public abstract class StrippedBinaryCollation :
        Bam.Core.Module
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Stripped Collation Root");
        private IStrippedBinaryCollationPolicy Policy = null;

        protected StrippedBinaryCollation()
        {
            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
        }

        public sealed override void
        Evaluate()
        {
            // TODO
        }

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.CollateStrippedBinaries(this, context);
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
            case "MakeFile":
                {
                    var className = "Publisher." + mode + "StrippedBinaryCollation";
                    this.Policy = Bam.Core.ExecutionPolicyUtilities<IStrippedBinaryCollationPolicy>.Create(className);
                }
                break;
            }
        }

        private Bam.Core.PathKey ReferenceKey
        {
            get;
            set;
        }

        private System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> RefMap
        {
            get;
            set;
        }

        private StripModule
        StripBinary(
            CollatedObject collatedFile,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap,
            ObjCopyModule debugSymbols = null)
        {
            var stripBinary = Bam.Core.Module.Create<StripModule>(preInitCallback: module =>
            {
                module.ReferenceMap = referenceMap;
            });
            this.DependsOn(stripBinary);
            stripBinary.SourceModule = collatedFile;
            if (null != debugSymbols)
            {
                stripBinary.DebugSymbolsModule = debugSymbols;
                stripBinary.Requires(debugSymbols);
            }
            if (collatedFile.Reference == null)
            {
                referenceMap.Add(collatedFile, stripBinary);
                this.ReferenceKey = StripModule.Key;
            }
            return stripBinary;
        }

        private void
        CloneFile(
            CollatedObject collatedFile,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap)
        {
            var clonedFile = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
            {
                Bam.Core.TokenizedString referenceFilePath = null;
                if (collatedFile.Reference != null)
                {
                    if (!referenceMap.ContainsKey(collatedFile.Reference))
                    {
                        throw new Bam.Core.Exception("Unable to find CollatedFile reference to {0} in the reference map", collatedFile.Reference.SourceModule.ToString());
                    }

                    var newRef = referenceMap[collatedFile.Reference];
                    // the PathKey depends on whether the reference came straight as a clone, or after being stripped
                    referenceFilePath = newRef.GeneratedPaths[this.ReferenceKey];
                }

                module.Macros["CopyDir"] = Collation.GenerateFileCopyDestination(
                    this,
                    referenceFilePath,
                    collatedFile.SubDirectory,
                    this.GeneratedPaths[Key]);
            });
            this.DependsOn(clonedFile);

            clonedFile.SourceModule = collatedFile;
            clonedFile.SourcePath = collatedFile.GeneratedPaths[CollatedObject.Key];
            clonedFile.SubDirectory = collatedFile.SubDirectory;

            if (collatedFile.Reference == null)
            {
                referenceMap.Add(collatedFile, clonedFile);
                this.ReferenceKey = CollatedObject.Key;
            }
        }

        private void
        CloneDirectory(
            CollatedObject collatedDir,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap)
        {
            var clonedDir = Bam.Core.Module.Create<CollatedDirectory>(preInitCallback: module =>
            {
                if (!referenceMap.ContainsKey(collatedDir.Reference))
                {
                    throw new Bam.Core.Exception("Unable to find CollatedDirectory reference to {0} in the reference map", collatedDir.Reference.SourceModule.ToString());
                }

                var newRef = referenceMap[collatedDir.Reference];
                var referenceFilePath = newRef.GeneratedPaths[this.ReferenceKey];

                module.Macros["CopyDir"] = Collation.GenerateDirectoryCopyDestination(
                    module,
                    referenceFilePath,
                    collatedDir.SubDirectory,
                    collatedDir.SourcePath);
            });
            this.DependsOn(clonedDir);

            clonedDir.SourceModule = collatedDir;
            clonedDir.SourcePath = collatedDir.GeneratedPaths[CollatedObject.Key];
            clonedDir.SubDirectory = collatedDir.SubDirectory;
            if (collatedDir.Macros["CopiedFilename"].IsAliased)
            {
                clonedDir.Macros["CopiedFilename"].Aliased(collatedDir.Macros["CopiedFilename"]);
            }
        }

        private void
        CloneSymbolicLink(
            CollatedObject collatedSymlink,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap)
        {
            var clonedSymLink = Bam.Core.Module.Create<CollatedSymbolicLink>(preInitCallback: module =>
                {
                    if (!referenceMap.ContainsKey(collatedSymlink.Reference))
                    {
                        throw new Bam.Core.Exception("Unable to find CollatedSymbolicLink reference to {0} in the reference map", collatedSymlink.Reference.SourceModule.ToString());
                    }

                    var newRef = referenceMap[collatedSymlink.Reference];
                    var referenceFilePath = newRef.GeneratedPaths[this.ReferenceKey];

                    module.Macros["CopyDir"] = Collation.GenerateSymbolicLinkCopyDestination(
                        this,
                        referenceFilePath,
                        collatedSymlink.SubDirectory);
                });
            this.DependsOn(clonedSymLink);

            clonedSymLink.SourceModule = collatedSymlink;
            clonedSymLink.SourcePath = collatedSymlink.GeneratedPaths[CollatedObject.Key];
            clonedSymLink.SubDirectory = collatedSymlink.SubDirectory;
            clonedSymLink.AssignLinkTarget(collatedSymlink.Macros["LinkTarget"]);
        }

        /// <summary>
        /// Create a stripped file mirror from the result of collation and debug symbol creation.
        /// Both previous steps are required in order to fulfil being able to provide an application
        /// release to end-users without debug information, and yet the developer is still able to
        /// debug issues by combining debug files with the stripped binaries.
        /// </summary>
        /// <typeparam name="RuntimeModule">The 1st type parameter.</typeparam>
        /// <typeparam name="DebugSymbolModule">The 2nd type parameter.</typeparam>
        public void
        StripBinariesFrom<RuntimeModule, DebugSymbolModule>()
            where RuntimeModule : Collation, new()
            where DebugSymbolModule : DebugSymbolCollation, new()
        {
            var runtimeDependent = Bam.Core.Graph.Instance.FindReferencedModule<RuntimeModule>();
            if (null == runtimeDependent)
            {
                return;
            }
            var debugSymbolDependent = Bam.Core.Graph.Instance.FindReferencedModule<DebugSymbolModule>();
            if (null == debugSymbolDependent)
            {
                return;
            }
            this.DependsOn(runtimeDependent);
            this.DependsOn(debugSymbolDependent);

            var referenceMap = new System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module>();
            foreach (CollatedObject req in runtimeDependent.Requirements.Where(item => item is CollatedObject))
            {
                if (req is CollatedSymbolicLink)
                {
                    this.CloneSymbolicLink(req, referenceMap);
                }
                else if (req is CollatedDirectory)
                {
                    this.CloneDirectory(req, referenceMap);
                }
                else if (req is CollatedFile)
                {
                    var source = req.SourceModule;
                    if (!(source is C.ConsoleApplication))
                    {
                        this.CloneFile(req, referenceMap);
                        continue;
                    }

                    if ((source as C.CModule).IsPrebuilt)
                    {
                        this.CloneFile(req, referenceMap);
                        continue;
                    }

                    // the remaining files will have been built, and therefore have some data that may need stripping
                    if (Bam.Core.OSUtilities.IsWindowsHosting)
                    {
                        if (req.SourceModule.Tool.Macros.Contains("pdbext"))
                        {
                            this.CloneFile(req, referenceMap);
                        }
                        else
                        {
                            // assume that debug symbols have been extracted into a separate file
                            var debugSymbols = debugSymbolDependent.Dependents.FirstOrDefault(item => (item is ObjCopyModule) && (item as ObjCopyModule).SourceModule == req);
                            if (null == debugSymbols)
                            {
                                throw new Bam.Core.Exception("Unable to locate debug symbol generation for {0}", req.SourceModule.ToString());
                            }
                            // then strip the binary
                            var stripped = this.StripBinary(req, referenceMap, debugSymbols: debugSymbols as ObjCopyModule);
                            // and add a link back to the debug symbols on the stripped binary
                            var linkBack = DebugSymbolCollation.LinkBackToDebugSymbols(stripped, referenceMap);
                            linkBack.DependsOn(stripped);
                            this.DependsOn(linkBack);
                        }
                    }
                    else if (Bam.Core.OSUtilities.IsLinuxHosting)
                    {
                        // assume that debug symbols have been extracted into a separate file
                        var debugSymbols = debugSymbolDependent.Dependents.FirstOrDefault(item => (item is ObjCopyModule) && (item as ObjCopyModule).SourceModule == req);
                        if (null == debugSymbols)
                        {
                            throw new Bam.Core.Exception("Unable to locate debug symbol generation for {0}", req.SourceModule.ToString());
                        }
                        // then strip the binary
                        var stripped = this.StripBinary(req, referenceMap, debugSymbols: debugSymbols as ObjCopyModule);
                        // and add a link back to the debug symbols on the stripped binary
                        var linkBack = DebugSymbolCollation.LinkBackToDebugSymbols(stripped, referenceMap);
                        linkBack.DependsOn(stripped);
                        this.DependsOn(linkBack);
                    }
                    else if (Bam.Core.OSUtilities.IsOSXHosting)
                    {
                        this.StripBinary(req, referenceMap);
                    }
                }
                else
                {
                    throw new Bam.Core.Exception("Unhandled collation module: {0}", req.ToString());
                }
            }
            this.RefMap = referenceMap;
        }

        /// <summary>
        /// Allow additional files to be added to the stripped collation, e.g. documentation, which may have been
        /// generated following the initial collation.
        /// </summary>
        /// <typeparam name="DependentModule">Module type containing the file to incorporate into the collation.</typeparam>
        /// <param name="key">The PathKey of the above module, containing the path to the file.</param>
        /// <param name="subDirectory">The subdirectory of the collation in which to write the file.</param>
        /// <param name="reference">The reference from the original Collation that the subdirectory specified is relative to. This references is translated into the stripped directory hierarchy before applying the subdirectory.</param>
        /// <returns>A reference to the collated file.</returns>
        public CollatedFile
        Include<DependentModule>(
            Bam.Core.PathKey key,
            string subDirectory,
            CollatedObject reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.Requires(dependent);
            this.Requires(dependent.Tool);

            var strippedInitialRef = this.RefMap[reference];

            var subDir = Bam.Core.TokenizedString.CreateVerbatim(subDirectory);
            var copyFileModule = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
                {
                Bam.Core.TokenizedString referenceFilePath = strippedInitialRef.GeneratedPaths[this.ReferenceKey];
                    this.RegisterGeneratedFile(Key, module.CreateTokenizedString("@dir($(0))", dependent.GeneratedPaths[key]));
                    module.Macros["CopyDir"] = Collation.GenerateFileCopyDestination(
                        module,
                        referenceFilePath,
                        subDir,
                        this.GeneratedPaths[Key]);
                });
            this.Requires(copyFileModule);

            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.GeneratedPaths[key];
            copyFileModule.Reference = strippedInitialRef as CollatedFile;
            copyFileModule.SubDirectory = subDir;
            return copyFileModule;
        }
    }
#endif
}
