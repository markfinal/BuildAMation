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
//     -i=../../../C/dev/Scripts/ICCompilerOptionsOSX.cs
//     -n=Clang
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=ClangCommon.PrivateData
//     -e
#endregion // BamOptionGenerator
namespace Clang
{
    public partial class CCompilerOptionCollection
    {
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
        #endregion
    }
}
