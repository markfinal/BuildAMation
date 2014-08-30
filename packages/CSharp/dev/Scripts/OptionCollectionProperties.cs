#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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
