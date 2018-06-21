#region License
// Copyright (c) 2010-2018, Mark Final
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

        // this is doubling up the cost of the this.Requires list, but at less runtime cost
        // for expanding each CollatedObject to peek as it's properties
        private System.Collections.Generic.Dictionary<ICollatedObject, ICollatedObject> collatedObjects = new System.Collections.Generic.Dictionary<ICollatedObject, ICollatedObject>();

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));

            // one value, as stripped binaries are not generated in IDE projects
            this.Macros.Add("publishroot", this.GeneratedPaths[Key]);
        }

        protected sealed override void
        EvaluateInternal()
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

        private StripModule
        StripBinary(
            ICollatedObject collatedFile)
        {
            var stripBinary = Bam.Core.Module.Create<StripModule>(preInitCallback: module =>
                {
                    module.SourceModule = collatedFile as Bam.Core.Module;
                    module.SourcePathKey = CollatedObject.Key;
                    module.Macros.Add("publishingdir", collatedFile.PublishingDirectory.Clone(module));
                });

            this.DependsOn(stripBinary);
            stripBinary.DependsOn(collatedFile as Bam.Core.Module);

            // dependents might reference the anchor's OutputName macro, e.g. dylibs copied into an application bundle
            stripBinary.Macros.Add("AnchorOutputName", (collatedFile as CollatedObject).Macros["AnchorOutputName"]);

            stripBinary.Macros.Add("publishdir", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));

            stripBinary.Anchor = collatedFile.Anchor;

            this.collatedObjects.Add(collatedFile, stripBinary);

            return stripBinary;
        }

        private CollationType
        CloneObject<CollationType>(
            ICollatedObject collatedObject) where CollationType : CollatedObject, new()
        {
            var clonedFile = Bam.Core.Module.Create<CollationType>(preInitCallback: module =>
                {
                    // no need to take RenameLeaf macro into account, as the rename occurred
                    // in the original collation, so this is a straight copy
                    module.SourceModule = collatedObject as Bam.Core.Module;
                    module.SourcePathKey = CollatedObject.Key;
                    module.SetPublishingDirectory("$(0)", collatedObject.PublishingDirectory.Clone(module));
                });
            this.DependsOn(clonedFile);

            clonedFile.Anchor = collatedObject.Anchor;

            clonedFile.Macros.Add("publishdir", this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));

            // dependents might reference the anchor's OutputName macro, e.g. dylibs copied into an application bundle
            clonedFile.Macros.Add("AnchorOutputName", (collatedObject as CollatedObject).Macros["AnchorOutputName"]);

            this.collatedObjects.Add(collatedObject, clonedFile);

            return clonedFile;
        }

        private void
        CloneFile(
            ICollatedObject collatedObject)
        {
            CloneObject<CollatedFile>(collatedObject);
        }

        private void
        CloneDirectory(
            ICollatedObject collatedObject)
        {
            CloneObject<CollatedDirectory>(collatedObject);
        }

        private void
        CloneOSXFramework(
            ICollatedObject collatedObject)
        {
            var clonedFramework = CloneObject<CollatedOSXFramework>(collatedObject);
            clonedFramework.UsePublicPatches(collatedObject as CollatedOSXFramework);
        }

        private void
        eachAnchorDependent(
            ICollatedObject collatedObj,
            object customData)
        {
            var sourceModule = collatedObj.SourceModule;
            if (sourceModule != null)
            {
                Bam.Core.Log.DebugMessage("\t'{0}'", sourceModule.ToString());
            }
            else
            {
                Bam.Core.Log.DebugMessage("\t'{0}'", (collatedObj as CollatedObject).SourcePath.ToString());
            }

            if ((collatedObj as CollatedObject).Ignore)
            {
                return;
            }

            var cModule = sourceModule as C.CModule;
            if (null == cModule)
            {
                // e.g. a shared object symbolic link
                if (collatedObj is CollatedFile)
                {
                    this.CloneFile(collatedObj);
                }
                else if (collatedObj is CollatedDirectory)
                {
                    this.CloneDirectory(collatedObj);
                }
                return;
            }

            if (cModule.IsPrebuilt)
            {
                if (collatedObj is CollatedOSXFramework)
                {
                    this.CloneOSXFramework(collatedObj);
                }
                else
                {
                    this.CloneFile(collatedObj);
                }
                return;
            }

            if (sourceModule.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (sourceModule.Tool.Macros.Contains("pdbext"))
                {
                    this.CloneFile(collatedObj);
                }
                else
                {
                    var stripped = this.StripBinary(collatedObj);
                    var debugSymbolsCollation = customData as DebugSymbolCollation;
                    var debugSymbols = debugSymbolsCollation.FindDebugSymbols(collatedObj.SourceModule) as ObjCopyModule;
                    if (null != debugSymbols)
                    {
                        var linkBack = debugSymbols.LinkBackToDebugSymbols(stripped);
                        this.DependsOn(linkBack);
                    }
                }
            }
            else if (sourceModule.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                var stripped = this.StripBinary(collatedObj);
                var debugSymbolsCollation = customData as DebugSymbolCollation;
                var debugSymbols = debugSymbolsCollation.FindDebugSymbols(collatedObj.SourceModule) as ObjCopyModule;
                if (null != debugSymbols)
                {
                    var linkBack = debugSymbols.LinkBackToDebugSymbols(stripped);
                    this.DependsOn(linkBack);
                }
            }
            else if (sourceModule.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                this.StripBinary(collatedObj);
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported platform '{0}'", sourceModule.BuildEnvironment.Platform.ToString());
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
                Bam.Core.Log.DebugMessage("Stripped Anchor '{0}'", anchor.SourceModule.ToString());
            }
            else
            {
                Bam.Core.Log.DebugMessage("Pre existing Stripped Anchor '{0}'", (anchor as CollatedObject).SourcePath.ToString());
            }
            collation.ForEachCollatedObjectFromAnchor(anchor, eachAnchorDependent, customData);
        }

        /// <summary>
        /// Create a stripped file mirror from the result of collation and debug symbol creation.
        /// Both previous steps are required in order to fulfil being able to provide an application
        /// release to end-users without debug information, and yet the developer is still able to
        /// debug issues by combining debug files with the stripped binaries.
        /// </summary>
        /// <typeparam name="RuntimeModule">The Collation module type from which to strip binaries for.</typeparam>
        /// <typeparam name="DebugSymbolModule">The DebugSymbolCollation module type used to link stripped binaries to debug symbols.</typeparam>
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

            // stripped binaries are made after the initial collation and debug symbol generation
            this.DependsOn(runtimeDependent);
            this.DependsOn(debugSymbolDependent);

            (runtimeDependent as Collation).ForEachAnchor(findDependentsofAnchor, debugSymbolDependent);
        }

        private Bam.Core.Module
        findAnchor(
            CollatedObject anchor)
        {
            foreach (var obj in this.collatedObjects)
            {
                if (obj.Key == anchor)
                {
                    return obj.Value as Bam.Core.Module;
                }
            }
            throw new Bam.Core.Exception("Unable to find stripped collation object for '{0}'", (anchor as ICollatedObject).SourceModule.ToString());
        }

        /// <summary>
        /// Allow additional files to be added to the stripped collation, e.g. documentation, which may have been
        /// generated following the initial collation.
        /// </summary>
        /// <typeparam name="DependentModule">Module type containing the file to incorporate into the collation.</typeparam>
        /// <param name="key">The PathKey of the above module, containing the path to the file.</param>
        /// <param name="collator">The original collator from which the stripped objects will be sourced.</param>
        /// <param name="anchor">The anchor in the stripped collation.</param>
        /// <returns>A reference to the stripped collated file.</returns>
        public ICollatedObject
        Include<DependentModule>(
            Bam.Core.PathKey key,
            Collation collator,
            CollatedObject anchor) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            var modulePublishDir = collator.Mapping.FindPublishDirectory(dependent, key);

            var collatedFile = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
                {
                    module.SourceModule = dependent;
                    module.SourcePathKey = key;
                    module.Anchor = anchor;
                    module.SetPublishingDirectory("$(0)", new[] { modulePublishDir });
                });

            var strippedAnchor = this.findAnchor(anchor);

            collatedFile.Macros.Add("publishdir", strippedAnchor.Macros["publishdir"]);

            // dependents might reference the anchor's OutputName macro, e.g. dylibs copied into an application bundle
            collatedFile.Macros.Add("AnchorOutputName", (anchor as CollatedObject).Macros["AnchorOutputName"]);

            this.Requires(collatedFile);
            return collatedFile;
        }
    }
}
