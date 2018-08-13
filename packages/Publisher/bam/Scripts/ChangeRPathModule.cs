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
namespace Publisher
{
    public class ChangeRPathModule :
        Bam.Core.Module
    {
        private C.ConsoleApplication TheSource;
#if BAM_V2
#else
        private IChangeRPathPolicy Policy;
#endif

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<ChangeRPathTool>();
        }

#if BAM_V2
#else
        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "Publisher." + mode + "ChangeRPath";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<IChangeRPathPolicy>.Create(className);
    }
#endif

        protected override void
        EvaluateInternal()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
#if BAM_V2
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
            case "MakeFile":
                MakeFileBuilder.Support.Add(this);
                break;
#endif

#if D_PACKAGE_NATIVEBUILDER
            case "Native":
                NativeBuilder.Support.RunCommandLineTool(this, context);
                break;
#endif

            default:
                throw new System.NotImplementedException();
            }
#else
            this.Policy.Change(this, context, this.Source, this.NewRPath);
#endif
        }

        public C.ConsoleApplication Source
        {
            get
            {
                return this.TheSource;
            }

            set
            {
                this.TheSource = value;
                this.DependsOn(value);
            }
        }

#if BAM_V2
#else
        public string NewRPath
        {
            get;
            set;
        }
#endif

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(
                    C.ConsoleApplication.ExecutableKey,
                    this.Source
                );
            }
        }
    }
}
