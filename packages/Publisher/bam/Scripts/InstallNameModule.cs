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
namespace Publisher
{
    /// <summary>
    /// Abstract module representing a file having had install_name_tool run on it
    /// </summary>
    abstract class InstallNameModule :
        Bam.Core.Module
    {
        /// <summary>
        /// The original Module applied to
        /// </summary>
        protected Bam.Core.Module CopiedFileModule;

        protected override void
        Init()
        {
            base.Init();
            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<InstallNameTool>();
        }

        protected override void
        EvaluateInternal()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(this, moduleToAppendTo: this.Source);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeBuilder.Support.RunCommandLineTool(this, context);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    {
                        XcodeBuilder.Support.AddPostBuildStepForCommandLineTool(
                            this,
                            this.Source, // add it to the source module's target
                            out XcodeBuilder.Target target,
                            out XcodeBuilder.Configuration configuration
                        );
                    }
                    break;
#endif
            }
        }

        /// <summary>
        /// /copydoc Bam.Core.Module.InputModulePaths
        /// </summary>
        public override System.Collections.Generic.IEnumerable<(Bam.Core.Module module, string pathKey)> InputModulePaths
        {
            get
            {
                yield return (this.CopiedFileModule, C.ConsoleApplication.ExecutableKey);
            }
        }

        /// <summary>
        /// Get or set the source module to apply the install_name_tool to
        /// </summary>
        public Bam.Core.Module Source
        {
            get
            {
                return this.CopiedFileModule;
            }
            set
            {
                this.CopiedFileModule = value;
                this.DependsOn(value);
            }
        }
    }
}
