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
        public override void
        Evaluate()
        {
            // TODO
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
        }

        private StripModule
        StripBinary(
            CollatedObject collatedFile,
            System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> referenceMap,
            ObjCopyModule debugSymbols = null)
        {
            var stripBinary = Bam.Core.Module.Create<StripModule>(preInitCallback: module =>
            {
                module.Macros.Add("StrippedRoot", module.CreateTokenizedString("$(buildroot)/$(encapsulatingmodulename)-$(config)"));
                module.ReferenceMap = referenceMap;
            });
            this.DependsOn(stripBinary);
            stripBinary.SourceModule = collatedFile;
            if (null != debugSymbols)
            {
                stripBinary.DebugSymbolsModule = debugSymbols;
            }
            if (collatedFile.Reference == null)
            {
                referenceMap.Add(collatedFile, stripBinary);
            }
            return stripBinary;
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
                // TODO: deal with all types
                if (!(req is CollatedFile))
                {
                    continue;
                }
                var source = req.SourceModule;
                if (!(source is C.ConsoleApplication))
                {
                    // TODO: just copy
                    continue;
                }
                var moduleIsPrebuilt = (source.GetType().GetCustomAttributes(typeof(C.PrebuiltAttribute), true).Length > 0);
                if (moduleIsPrebuilt)
                {
                    // TODO: just copy
                    continue;
                }
                if (Bam.Core.OSUtilities.IsWindowsHosting)
                {
                    if (req.SourceModule.Tool.Macros.Contains("pdbext"))
                    {
                        // TODO: just copy
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
        }
    }
}
