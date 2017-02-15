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
namespace C
{
    /// <summary>
    /// Generic object file.
    /// </summary>
    public abstract class ObjectFileBase :
        CModule,
        Bam.Core.IChildModule,
        Bam.Core.IInputPath,
        IRequiresSourceModule
    {
        static public Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Compiled/assembled Object File");

        private Bam.Core.Module Parent = null;
        protected SourceFile SourceModule;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.PerformCompilation = true;
        }

        public override string CustomOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        SourceFile IRequiresSourceModule.Source
        {
            get
            {
                return this.SourceModule;
            }

            set
            {
                if (null != this.SourceModule)
                {
                    throw new Bam.Core.Exception("Source module already set on this object file, to '{0}'", this.SourceModule.InputPath.Parse());
                }
                this.SourceModule = value;
                this.DependsOn(value);
                this.GeneratedPaths[Key] = this.CreateTokenizedString(
                    "$(packagebuilddir)/$(moduleoutputdir)/@changeextension(@trimstart(@relativeto($(0),$(packagedir)),../),$(objext))",
                    value.GeneratedPaths[SourceFile.Key]);
            }
        }

        public Bam.Core.TokenizedString InputPath
        {
            get
            {
                if (null == this.SourceModule)
                {
                    throw new Bam.Core.Exception("Source module not yet set on this object file");
                }
                return this.SourceModule.InputPath;
            }
            set
            {
                if (null != this.SourceModule)
                {
                    throw new Bam.Core.Exception("Source module already set on this object file, to '{0}'", this.SourceModule.InputPath.Parse());
                }

                // this cannot be a referenced module, since there will be more than one object
                // of this type (generally)
                // but this does mean there may be many instances of this 'type' of module
                // and for multi-configuration builds there may be many instances of the same path
                var source = Bam.Core.Module.Create<SourceFile>();
                source.InputPath = value;
                (this as IRequiresSourceModule).Source = source;
            }
        }

        Bam.Core.Module Bam.Core.IChildModule.Parent
        {
            get
            {
                return this.Parent;
            }
            set
            {
                this.Parent = value;
            }
        }

        public bool PerformCompilation
        {
            get;
            set;
        }
    }
}
