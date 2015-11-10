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
    public abstract class DebugSymbolCollation :
        Bam.Core.Module
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Debug Symbol Collation Root");
        private IDebugSymbolCollationPolicy Policy = null;

        protected DebugSymbolCollation()
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
            this.Policy.CollateDebugSymbols(this, context);
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "DebugSymbolCollation";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<IDebugSymbolCollationPolicy>.Create(className);
                    }
                    break;
            }
        }

        private void
        CopyPDB(
            CollatedObject collatedFile,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap)
        {
            var copyPDBModule = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
            {
                Bam.Core.TokenizedString referenceFilePath = null;
                if (collatedFile.Reference != null)
                {
                    if (!referenceMap.ContainsKey(collatedFile.Reference))
                    {
                        throw new Bam.Core.Exception("Unable to find CollatedFile reference to {0} in the reference map", collatedFile.Reference.SourceModule.ToString());
                    }

                    var newRef = referenceMap[collatedFile.Reference];
                    referenceFilePath = newRef.GeneratedPaths[CollatedObject.Key];
                }

                module.Macros["CopyDir"] = Collation.GenerateFileCopyDestination(
                    this,
                    referenceFilePath,
                    collatedFile.SubDirectory,
                    this.GeneratedPaths[Key]);
            });
            this.DependsOn(copyPDBModule);

            copyPDBModule.SourceModule = collatedFile.SourceModule;
            // TODO: there has not been a check whether this is a valid path or not (i.e. were debug symbols enabled for link?)
            copyPDBModule.SourcePath = collatedFile.SourceModule.GeneratedPaths[C.ConsoleApplication.PDBKey];
            copyPDBModule.SubDirectory = collatedFile.SubDirectory;

            // since PDBs aren't guaranteed to exist as it depends on build settings, allow missing files to go through
            copyPDBModule.FailWhenSourceDoesNotExist = false;

            if (collatedFile.Reference == null)
            {
                referenceMap.Add(collatedFile, copyPDBModule);
            }
        }

        private void
        CopyDebugSymbols(
            CollatedObject collatedFile,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap)
        {
            var createDebugSymbols = Bam.Core.Module.Create<ObjCopyModule>(preInitCallback: module =>
            {
                module.ReferenceMap = referenceMap;
            });
            this.DependsOn(createDebugSymbols);
            createDebugSymbols.SourceModule = collatedFile;
            createDebugSymbols.PrivatePatch(settings =>
            {
                var objCopySettings = settings as IObjCopyToolSettings;
                objCopySettings.Mode = EObjCopyToolMode.OnlyKeepDebug;
            });
            if (collatedFile.Reference == null)
            {
                referenceMap.Add(collatedFile, createDebugSymbols);
            }
        }

        public static ObjCopyModule
        LinkBackToDebugSymbols(
            Bam.Core.Module source,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap)
        {
            var linkDebugSymbols = Bam.Core.Module.Create<ObjCopyModule>(preInitCallback: module =>
            {
                module.ReferenceMap = referenceMap;
            });
            linkDebugSymbols.SourceModule = source;
            linkDebugSymbols.PrivatePatch(settings =>
            {
                var objCopySettings = settings as IObjCopyToolSettings;
                objCopySettings.Mode = EObjCopyToolMode.AddGNUDebugLink;
            });
            return linkDebugSymbols;
        }

        private void
        CreatedSYMBundle(
            CollatedObject collatedFile,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap)
        {
            var createDebugSymbols = Bam.Core.Module.Create<DSymUtilModule>(preInitCallback: module =>
            {
                module.ReferenceMap = referenceMap;
            });
            this.DependsOn(createDebugSymbols);
            createDebugSymbols.SourceModule = collatedFile;
            if (collatedFile.Reference == null)
            {
                referenceMap.Add(collatedFile, createDebugSymbols);
            }
        }

        public void
        CreateSymbolsFrom<DependentModule>() where DependentModule : Collation, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            this.DependsOn(dependent);

            var referenceMap = new System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module>();
            foreach (CollatedObject req in dependent.Requirements.Where(item => item is CollatedObject))
            {
                if (!(req is CollatedFile))
                {
                    continue;
                }
                var source = req.SourceModule;
                if (!(source is C.ConsoleApplication))
                {
                    continue;
                }
                if ((source as C.CModule).IsPrebuilt)
                {
                    continue;
                }
                if (Bam.Core.OSUtilities.IsWindowsHosting)
                {
                    if (req.SourceModule.Tool.Macros.Contains("pdbext"))
                    {
                        this.CopyPDB(req, referenceMap);
                    }
                    else
                    {
                        this.CopyDebugSymbols(req, referenceMap);
                    }
                }
                else if (Bam.Core.OSUtilities.IsLinuxHosting)
                {
                    this.CopyDebugSymbols(req, referenceMap);
                }
                else if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    this.CreatedSYMBundle(req, referenceMap);
                }
            }
        }
    }
}
