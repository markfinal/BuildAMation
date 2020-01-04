#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Derive from this module to generate a standalone directory of extracted debug symbol files
    /// that mirrors a collated publishing root. This mirror folder can be hived off and stored by
    /// developers. It can be dropped on top of stripped binaries in order to produce a fully
    /// debuggable solution, debug symbols beside executables.
    /// On Windows VisualC, this copies the PDB files.
    /// On Linux, this uses objcopy to extract the symbol data.
    /// On OSX, this uses dsymutil to extract symbol bundles.
    /// </summary>
    abstract class DebugSymbolCollation :
        Bam.Core.Module,
        ICollation
    {
        /// <summary>
        /// Path key to the debug symbol collation directory
        /// </summary>
        public const string DebugSymbolsDirectoryKey = "Debug Symbol Collation Root";

        // this is doubling up the cost of the this.Requires list, but at less runtime cost
        // for expanding each CollatedObject to peek as it's properties
        private readonly System.Collections.Generic.List<ICollatedObject> collatedObjects = new System.Collections.Generic.List<ICollatedObject>();

        /// <summary>
        /// Initialise this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();

            this.RegisterGeneratedFile(
                DebugSymbolsDirectoryKey,
                this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"),
                true
            );

            // one value, as debug symbols are not generated in IDE projects
            this.Macros.Add(
                "publishroot",
                this.GeneratedPaths[DebugSymbolsDirectoryKey]
            );
        }

        protected sealed override void
        EvaluateInternal()
        {
            // TODO
        }

        /// <summary>
        /// Execute the tool on this module
        /// </summary>
        /// <param name="context">in this context</param>
        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.AddCheckpoint(
                        this,
                        excludingGeneratedPath: DebugSymbolsDirectoryKey
                    );
                    break;
#endif

                default:
                    // does not need to do anything
                    break;
            }
        }

        private void
        CreatedSYMBundle(
            ICollatedObject collatedFile)
        {
            var createDebugSymbols = Bam.Core.Module.Create<DSymUtilModule>(preInitCallback: module =>
                {
                    module.SourceModule = collatedFile.SourceModule;
                    module.SourcePathKey = collatedFile.SourcePathKey;
                    module.Macros.Add("publishingdir", collatedFile.PublishingDirectory.Clone(module));
                    module.EncapsulatingCollation = this;
                });
            this.Requires(createDebugSymbols);

            createDebugSymbols.Macros.Add("publishdir", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));

            // dependents might reference the anchor's OutputName macro, e.g. dylibs copied into an application bundle
            createDebugSymbols.Macros.Add("AnchorOutputName", (collatedFile as CollatedObject).Macros.GetUnformatted("AnchorOutputName"));

            this.collatedObjects.Add(createDebugSymbols);
        }

        private void
        CopyDebugSymbols(
            ICollatedObject collatedFile)
        {
            var createDebugSymbols = Bam.Core.Module.Create<MakeDebugSymbolFile>(preInitCallback: module =>
                {
                    module.SourceModule = collatedFile.SourceModule;
                    module.SourcePathKey = collatedFile.SourcePathKey;
                    module.Macros.Add("publishingdir", collatedFile.PublishingDirectory.Clone(module));
                    module.EncapsulatingCollation = this;
                });
            this.Requires(createDebugSymbols);

            createDebugSymbols.Macros.Add("publishdir", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));

            createDebugSymbols.PrivatePatch(settings =>
                {
                    var objCopySettings = settings as IObjCopyToolSettings;
                    objCopySettings.OnlyKeepDebug = true;
                });

            this.collatedObjects.Add(createDebugSymbols);
        }

        private void
        CopyPDB(
            ICollatedObject collatedFile)
        {
            var copyPDBModule = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
                {
                    module.SourceModule = collatedFile.SourceModule;
                    // TODO: there has not been a check whether this is a valid path or not (i.e. were debug symbols enabled for link?)
                    module.SourcePathKey = C.ConsoleApplication.PDBKey;
                    module.SetPublishingDirectory("$(0)", collatedFile.PublishingDirectory.Clone(module));
                    module.EncapsulatingCollation = this;
                });
            this.Requires(copyPDBModule);

            copyPDBModule.Macros.Add("publishdir", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));

            // TODO: since PDBs aren't guaranteed to exist as it depends on build settings, allow missing files to go through
            this.collatedObjects.Add(copyPDBModule);
        }

        private void
        eachAnchorDependent(
            ICollatedObject collatedObj,
            object customData)
        {
            var sourceModule = collatedObj.SourceModule;
            if (sourceModule != null)
            {
                Bam.Core.Log.DebugMessage($"\t'{sourceModule.ToString()}'");
            }
            else
            {
                Bam.Core.Log.DebugMessage($"\t'{(collatedObj as CollatedObject).SourcePath.ToString()}'");
            }

            if ((collatedObj as CollatedObject).Ignore)
            {
                return;
            }

            var cModule = sourceModule as C.CModule;
            if (null == cModule)
            {
                // e.g. a shared object symbolic link
                return;
            }

            if (cModule.IsPrebuilt)
            {
                return;
            }

            if (sourceModule.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (sourceModule.Tool.Macros.ContainsUnformatted("pdbext"))
                {
                    this.CopyPDB(collatedObj);
                }
                else
                {
                    this.CopyDebugSymbols(collatedObj);
                }
            }
            else if (sourceModule.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.CopyDebugSymbols(collatedObj);
            }
            else if (sourceModule.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.CreatedSYMBundle(collatedObj);
            }
            else
            {
                throw new Bam.Core.Exception($"Unsupported platform '{sourceModule.BuildEnvironment.Platform.ToString()}'");
            }
        }

        private void
        findDependentsofAnchor(
            Collation collation,
            ICollatedObject anchor,
            object customData)
        {
            if (null != anchor.SourceModule)
            {
                Bam.Core.Log.DebugMessage($"Debug Symbols Anchor '{anchor.SourceModule.ToString()}'");
            }
            else
            {
                Bam.Core.Log.DebugMessage($"Pre existing Debug Symbols Anchor '{(anchor as CollatedObject).SourcePath.ToString()}'");
            }
            collation.ForEachCollatedObjectFromAnchor(anchor, eachAnchorDependent, customData);
        }

        /// <summary>
        /// Create a symbol data mirror from the result of collation.
        /// </summary>
        /// <typeparam name="DependentModule">The Collation module type to source debug symbols from.</typeparam>
        public void
        CreateSymbolsFrom<DependentModule>() where DependentModule : Collation, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            // debug symbols are made after the initial collation
            this.DependsOn(dependent);

            (dependent as Collation).ForEachAnchor(findDependentsofAnchor, null);
        }

        /// <summary>
        /// Locate the debug symbols associated with a particular source module.
        /// </summary>
        /// <param name="module">Module from which debug symbols were created.</param>
        /// <returns>The associated debug symbols module.</returns>
        public ICollatedObject
        FindDebugSymbols(
            Bam.Core.Module module)
        {
            foreach (var debugSymbols in this.collatedObjects)
            {
                if (debugSymbols.SourceModule == module)
                {
                    return debugSymbols;
                }
            }
            return null;
        }
    }
}
