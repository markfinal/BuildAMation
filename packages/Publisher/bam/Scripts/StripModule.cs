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
    public class StripModule :
        Bam.Core.Module
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Stripped Binary Destination");

        private CollatedObject TheSourceModule;
        private IStripToolPolicy Policy;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<StripTool>();
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
            this.Policy.Strip(this, context, this.TheSourceModule.GeneratedPaths[CollatedObject.Key], this.GeneratedPaths[Key]);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Publisher." + mode + "Strip";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<IStripToolPolicy>.Create(className);
            }
        }

        public System.Collections.Generic.Dictionary<CollatedObject, Bam.Core.Module> ReferenceMap
        {
            get;
            set;
        }

        public ObjCopyModule DebugSymbolsModule
        {
            get;
            set;
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

                Bam.Core.TokenizedString referenceFilePath = null;
                if (value.Reference != null)
                {
                    if (null == this.ReferenceMap)
                    {
                        throw new Bam.Core.Exception("Missing mapping of CollatedFiles to StripModule");
                    }
                    if (!this.ReferenceMap.ContainsKey(value.Reference))
                    {
                        throw new Bam.Core.Exception("Unable to find CollatedFile reference to {0} in the reference map", value.Reference.SourceModule.ToString());
                    }

                    var newRef = this.ReferenceMap[value.Reference];
                    referenceFilePath = newRef.GeneratedPaths[Key];
                }
                var destinationDirectory = Collation.GenerateFileCopyDestination(
                    this,
                    referenceFilePath,
                    value.SubDirectory,
                    this.Dependees[0].GeneratedPaths[StrippedBinaryCollation.Key]); // path of the debug symbol collation root
                this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(0)/@filename($(1))",
                    destinationDirectory,
                    value.GeneratedPaths[CollatedObject.Key]));
            }
        }
    }
}
