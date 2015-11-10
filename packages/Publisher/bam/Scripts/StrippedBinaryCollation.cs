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
namespace Publisher
{
    public abstract class StrippedBinaryCollation :
        Bam.Core.Module
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Stripped Collation Root");

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
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
        }

        private Bam.Core.PathKey ReferenceKey
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

            clonedFile.SourceModule = collatedFile.SourceModule;
            clonedFile.SourcePath = collatedFile.SourcePath;
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
                    this,
                    referenceFilePath,
                    collatedDir.SubDirectory,
                    collatedDir.SourcePath);
            });
            this.DependsOn(clonedDir);

            clonedDir.SourceModule = collatedDir.SourceModule;
            clonedDir.SourcePath = collatedDir.SourcePath;
            clonedDir.SubDirectory = collatedDir.SubDirectory;
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

            clonedSymLink.SourceModule = collatedSymlink.SourceModule;
            clonedSymLink.SourcePath = collatedSymlink.SourcePath;
            clonedSymLink.SubDirectory = collatedSymlink.SubDirectory;
            clonedSymLink.AssignLinkTarget(collatedSymlink.Macros["LinkTarget"]);
        }

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
                            var debugSymbols = debugSymbolDependent.Dependents.Where(item => (item is ObjCopyModule) && (item as ObjCopyModule).SourceModule == req).FirstOrDefault();
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
                        var debugSymbols = debugSymbolDependent.Dependents.Where(item => (item is ObjCopyModule) && (item as ObjCopyModule).SourceModule == req).FirstOrDefault();
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
        }
    }
}
