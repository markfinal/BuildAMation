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
//     -i=ICCompilerOptions.cs&../../../C/dev/Scripts/ICCompilerOptionsOSX.cs
//     -n=LLVMGcc
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=GccCommon.PrivateData
//     -e
#endregion // BamOptionGenerator
namespace LLVMGcc
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option properties
        LLVMGcc.EVisibility ICCompilerOptions.Visibility
        {
            get
            {
                return this.GetValueTypeOption<LLVMGcc.EVisibility>("Visibility", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<LLVMGcc.EVisibility>("Visibility", value);
                this.ProcessNamedSetHandler("VisibilitySetHandler", this["Visibility"]);
            }
        }
        #endregion
        #region C.ICCompilerOptionsOSX Option properties
        Bam.Core.DirectoryCollection C.ICCompilerOptionsOSX.FrameworkSearchDirectories
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.DirectoryCollection>("FrameworkSearchDirectories", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.DirectoryCollection>("FrameworkSearchDirectories", value);
                this.ProcessNamedSetHandler("FrameworkSearchDirectoriesSetHandler", this["FrameworkSearchDirectories"]);
            }
        }
        string C.ICCompilerOptionsOSX.SDKVersion
        {
            get
            {
                return this.GetReferenceTypeOption<string>("SDKVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("SDKVersion", value);
                this.ProcessNamedSetHandler("SDKVersionSetHandler", this["SDKVersion"]);
            }
        }
        string C.ICCompilerOptionsOSX.DeploymentTarget
        {
            get
            {
                return this.GetReferenceTypeOption<string>("DeploymentTarget", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("DeploymentTarget", value);
                this.ProcessNamedSetHandler("DeploymentTargetSetHandler", this["DeploymentTarget"]);
            }
        }
        C.EOSXPlatform C.ICCompilerOptionsOSX.SupportedPlatform
        {
            get
            {
                return this.GetValueTypeOption<C.EOSXPlatform>("SupportedPlatform", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.EOSXPlatform>("SupportedPlatform", value);
                this.ProcessNamedSetHandler("SupportedPlatformSetHandler", this["SupportedPlatform"]);
            }
        }
        string C.ICCompilerOptionsOSX.CompilerName
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CompilerName", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CompilerName", value);
                this.ProcessNamedSetHandler("CompilerNameSetHandler", this["CompilerName"]);
            }
        }
        #endregion
    }
}
