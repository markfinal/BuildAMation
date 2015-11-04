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
    public class ObjCopyModule :
        Bam.Core.Module
    {
        public static Bam.Core.FileKey Key = Bam.Core.FileKey.Generate("ObjCopy Destination");

        private CollatedObject TheSourceModule;
        private IObjCopyToolPolicy Policy;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<ObjCopyTool>();
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
            this.Policy.ObjCopy(this, context, this.TheSourceModule.GeneratedPaths[CollatedObject.CopiedObjectKey], this.GeneratedPaths[Key]);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Publisher." + mode + "ObjCopy";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<IObjCopyToolPolicy>.Create(className);
            }
        }

        public CollatedObject SourceModule
        {
            get
            {
                return this.TheSourceModule;
            }

            set
            {
                this.TheSourceModule = value;
                this.DependsOn(value);
                // Note: these paths match those in Collation.CreateCollatedFile
                if (null != value.SubDirectory)
                {
                    this.RegisterGeneratedFile(Key, this.CreateTokenizedString("@normalize($(DebugSymbolRoot)/$(0)/@filename($(1)).debug)",
                        value.SubDirectory,
                        value.GeneratedPaths[CollatedObject.CopiedObjectKey]));
                }
                else
                {
                    this.RegisterGeneratedFile(Key, this.CreateTokenizedString("@normalize($(DebugSymbolRoot)/@filename($(0)).debug)",
                        value.GeneratedPaths[CollatedObject.CopiedObjectKey]));
                }
            }
        }
    }
}
