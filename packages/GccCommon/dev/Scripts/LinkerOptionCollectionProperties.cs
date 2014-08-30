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
//     -i=../../../C/dev/Scripts/ILinkerOptions.cs&../../../C/dev/Scripts/ILinkerOptionsOSX.cs&ILinkerOptions.cs
//     -n=GccCommon
//     -c=LinkerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace GccCommon
{
    public partial class LinkerOptionCollection
    {
        #region C.ILinkerOptions Option properties
        C.ELinkerOutput C.ILinkerOptions.OutputType
        {
            get
            {
                return this.GetValueTypeOption<C.ELinkerOutput>("OutputType", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.ELinkerOutput>("OutputType", value);
                this.ProcessNamedSetHandler("OutputTypeSetHandler", this["OutputType"]);
            }
        }
        bool C.ILinkerOptions.DoNotAutoIncludeStandardLibraries
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotAutoIncludeStandardLibraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotAutoIncludeStandardLibraries", value);
                this.ProcessNamedSetHandler("DoNotAutoIncludeStandardLibrariesSetHandler", this["DoNotAutoIncludeStandardLibraries"]);
            }
        }
        bool C.ILinkerOptions.DebugSymbols
        {
            get
            {
                return this.GetValueTypeOption<bool>("DebugSymbols", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DebugSymbols", value);
                this.ProcessNamedSetHandler("DebugSymbolsSetHandler", this["DebugSymbols"]);
            }
        }
        C.ESubsystem C.ILinkerOptions.SubSystem
        {
            get
            {
                return this.GetValueTypeOption<C.ESubsystem>("SubSystem", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.ESubsystem>("SubSystem", value);
                this.ProcessNamedSetHandler("SubSystemSetHandler", this["SubSystem"]);
            }
        }
        bool C.ILinkerOptions.DynamicLibrary
        {
            get
            {
                return this.GetValueTypeOption<bool>("DynamicLibrary", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DynamicLibrary", value);
                this.ProcessNamedSetHandler("DynamicLibrarySetHandler", this["DynamicLibrary"]);
            }
        }
        Bam.Core.DirectoryCollection C.ILinkerOptions.LibraryPaths
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.DirectoryCollection>("LibraryPaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.DirectoryCollection>("LibraryPaths", value);
                this.ProcessNamedSetHandler("LibraryPathsSetHandler", this["LibraryPaths"]);
            }
        }
        Bam.Core.FileCollection C.ILinkerOptions.StandardLibraries
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.FileCollection>("StandardLibraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.FileCollection>("StandardLibraries", value);
                this.ProcessNamedSetHandler("StandardLibrariesSetHandler", this["StandardLibraries"]);
            }
        }
        Bam.Core.FileCollection C.ILinkerOptions.Libraries
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.FileCollection>("Libraries", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.FileCollection>("Libraries", value);
                this.ProcessNamedSetHandler("LibrariesSetHandler", this["Libraries"]);
            }
        }
        bool C.ILinkerOptions.GenerateMapFile
        {
            get
            {
                return this.GetValueTypeOption<bool>("GenerateMapFile", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("GenerateMapFile", value);
                this.ProcessNamedSetHandler("GenerateMapFileSetHandler", this["GenerateMapFile"]);
            }
        }
        string C.ILinkerOptions.AdditionalOptions
        {
            get
            {
                return this.GetReferenceTypeOption<string>("AdditionalOptions", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("AdditionalOptions", value);
                this.ProcessNamedSetHandler("AdditionalOptionsSetHandler", this["AdditionalOptions"]);
            }
        }
        int C.ILinkerOptions.MajorVersion
        {
            get
            {
                return this.GetValueTypeOption<int>("MajorVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<int>("MajorVersion", value);
                this.ProcessNamedSetHandler("MajorVersionSetHandler", this["MajorVersion"]);
            }
        }
        int C.ILinkerOptions.MinorVersion
        {
            get
            {
                return this.GetValueTypeOption<int>("MinorVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<int>("MinorVersion", value);
                this.ProcessNamedSetHandler("MinorVersionSetHandler", this["MinorVersion"]);
            }
        }
        int C.ILinkerOptions.PatchVersion
        {
            get
            {
                return this.GetValueTypeOption<int>("PatchVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<int>("PatchVersion", value);
                this.ProcessNamedSetHandler("PatchVersionSetHandler", this["PatchVersion"]);
            }
        }
        #endregion
        #region C.ILinkerOptionsOSX Option properties
        Bam.Core.StringArray C.ILinkerOptionsOSX.Frameworks
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.StringArray>("Frameworks", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.StringArray>("Frameworks", value);
                this.ProcessNamedSetHandler("FrameworksSetHandler", this["Frameworks"]);
            }
        }
        Bam.Core.DirectoryCollection C.ILinkerOptionsOSX.FrameworkSearchDirectories
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
        bool C.ILinkerOptionsOSX.SuppressReadOnlyRelocations
        {
            get
            {
                return this.GetValueTypeOption<bool>("SuppressReadOnlyRelocations", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("SuppressReadOnlyRelocations", value);
                this.ProcessNamedSetHandler("SuppressReadOnlyRelocationsSetHandler", this["SuppressReadOnlyRelocations"]);
            }
        }
        #endregion
        #region ILinkerOptions Option properties
        bool ILinkerOptions.CanUseOrigin
        {
            get
            {
                return this.GetValueTypeOption<bool>("CanUseOrigin", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("CanUseOrigin", value);
                this.ProcessNamedSetHandler("CanUseOriginSetHandler", this["CanUseOrigin"]);
            }
        }
        bool ILinkerOptions.AllowUndefinedSymbols
        {
            get
            {
                return this.GetValueTypeOption<bool>("AllowUndefinedSymbols", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("AllowUndefinedSymbols", value);
                this.ProcessNamedSetHandler("AllowUndefinedSymbolsSetHandler", this["AllowUndefinedSymbols"]);
            }
        }
        Bam.Core.StringArray ILinkerOptions.RPath
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.StringArray>("RPath", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.StringArray>("RPath", value);
                this.ProcessNamedSetHandler("RPathSetHandler", this["RPath"]);
            }
        }
        bool ILinkerOptions.SixtyFourBit
        {
            get
            {
                return this.GetValueTypeOption<bool>("SixtyFourBit", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("SixtyFourBit", value);
                this.ProcessNamedSetHandler("SixtyFourBitSetHandler", this["SixtyFourBit"]);
            }
        }
        #endregion
    }
}
