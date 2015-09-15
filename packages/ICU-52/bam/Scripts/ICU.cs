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
namespace ICU
{
    public abstract class ICUBase :
        C.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros.Add("ICUInstallPath", Bam.Core.TokenizedString.Create("$(packagedir)/bin/win64-msvc10/bin64", this));
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.Macros.Add("ICUInstallPath", Bam.Core.TokenizedString.Create("$(packagedir)/bin/linux64-gcc44/usr/local/lib", this));
            }
            this.GeneratedPaths[C.DynamicLibrary.Key] = Bam.Core.TokenizedString.Create("$(ICUInstallPath)/$(dynamicprefix)$(OutputName)$(dynamicext)", this);
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            // do nothing
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // do nothing
        }
    }

    public sealed class ICUIN :
        ICUBase
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.Create("icuin52", null, verbatim:true);
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.Create("icui18n", null, verbatim:true);
                this.Macros["dynamicext"] = Bam.Core.TokenizedString.Create(".so.52", null, verbatim:true);
            }
            base.Init(parent);
        }
    }

    public sealed class ICUUC :
        ICUBase
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.Create("icuuc52", null, verbatim:true);
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.Create("icuuc", null, verbatim:true);
                this.Macros["dynamicext"] = Bam.Core.TokenizedString.Create(".so.52", null, verbatim:true);
            }
            base.Init(parent);
        }
    }

    public sealed class ICUDT :
        ICUBase
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.Create("icudt52", null, verbatim:true);
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                this.Macros["OutputName"] = Bam.Core.TokenizedString.Create("icudata", null, verbatim:true);
                this.Macros["dynamicext"] = Bam.Core.TokenizedString.Create(".so.52", null, verbatim:true);
            }
            base.Init(parent);
        }
    }
}
