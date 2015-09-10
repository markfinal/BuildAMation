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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=IOptions.cs
//     -n=CSharp
//     -c=OptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace CSharp
{
    public partial class OptionCollection
    {
        #region IOptions Option properties
        CSharp.ETarget IOptions.Target
        {
            get
            {
                return this.GetValueTypeOption<CSharp.ETarget>("Target", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<CSharp.ETarget>("Target", value);
                this.ProcessNamedSetHandler("TargetSetHandler", this["Target"]);
            }
        }
        bool IOptions.NoLogo
        {
            get
            {
                return this.GetValueTypeOption<bool>("NoLogo", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("NoLogo", value);
                this.ProcessNamedSetHandler("NoLogoSetHandler", this["NoLogo"]);
            }
        }
        CSharp.EPlatform IOptions.Platform
        {
            get
            {
                return this.GetValueTypeOption<CSharp.EPlatform>("Platform", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<CSharp.EPlatform>("Platform", value);
                this.ProcessNamedSetHandler("PlatformSetHandler", this["Platform"]);
            }
        }
        CSharp.EDebugInformation IOptions.DebugInformation
        {
            get
            {
                return this.GetValueTypeOption<CSharp.EDebugInformation>("DebugInformation", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<CSharp.EDebugInformation>("DebugInformation", value);
                this.ProcessNamedSetHandler("DebugInformationSetHandler", this["DebugInformation"]);
            }
        }
        bool IOptions.Checked
        {
            get
            {
                return this.GetValueTypeOption<bool>("Checked", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("Checked", value);
                this.ProcessNamedSetHandler("CheckedSetHandler", this["Checked"]);
            }
        }
        bool IOptions.Unsafe
        {
            get
            {
                return this.GetValueTypeOption<bool>("Unsafe", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("Unsafe", value);
                this.ProcessNamedSetHandler("UnsafeSetHandler", this["Unsafe"]);
            }
        }
        CSharp.EWarningLevel IOptions.WarningLevel
        {
            get
            {
                return this.GetValueTypeOption<CSharp.EWarningLevel>("WarningLevel", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<CSharp.EWarningLevel>("WarningLevel", value);
                this.ProcessNamedSetHandler("WarningLevelSetHandler", this["WarningLevel"]);
            }
        }
        bool IOptions.WarningsAsErrors
        {
            get
            {
                return this.GetValueTypeOption<bool>("WarningsAsErrors", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("WarningsAsErrors", value);
                this.ProcessNamedSetHandler("WarningsAsErrorsSetHandler", this["WarningsAsErrors"]);
            }
        }
        bool IOptions.Optimize
        {
            get
            {
                return this.GetValueTypeOption<bool>("Optimize", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("Optimize", value);
                this.ProcessNamedSetHandler("OptimizeSetHandler", this["Optimize"]);
            }
        }
        Bam.Core.FileCollection IOptions.References
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.FileCollection>("References", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.FileCollection>("References", value);
                this.ProcessNamedSetHandler("ReferencesSetHandler", this["References"]);
            }
        }
        Bam.Core.FileCollection IOptions.Modules
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.FileCollection>("Modules", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.FileCollection>("Modules", value);
                this.ProcessNamedSetHandler("ModulesSetHandler", this["Modules"]);
            }
        }
        Bam.Core.StringArray IOptions.Defines
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.StringArray>("Defines", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.StringArray>("Defines", value);
                this.ProcessNamedSetHandler("DefinesSetHandler", this["Defines"]);
            }
        }
        #endregion
    }
}
