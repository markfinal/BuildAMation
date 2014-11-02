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
//     -i=IMocOptions.cs
//     -n=QtCommon
//     -c=MocOptionCollection
//     -p
//     -d
//     -dd=c:/dev/BuildAMation/packages/CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=MocPrivateData
#endregion // BamOptionGenerator
namespace QtCommon
{
    public partial class MocOptionCollection
    {
        #region IMocOptions Option properties
        Bam.Core.DirectoryCollection IMocOptions.IncludePaths
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.DirectoryCollection>("IncludePaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.DirectoryCollection>("IncludePaths", value);
                this.ProcessNamedSetHandler("IncludePathsSetHandler", this["IncludePaths"]);
            }
        }
        C.DefineCollection IMocOptions.Defines
        {
            get
            {
                return this.GetReferenceTypeOption<C.DefineCollection>("Defines", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<C.DefineCollection>("Defines", value);
                this.ProcessNamedSetHandler("DefinesSetHandler", this["Defines"]);
            }
        }
        bool IMocOptions.DoNotGenerateIncludeStatement
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotGenerateIncludeStatement", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotGenerateIncludeStatement", value);
                this.ProcessNamedSetHandler("DoNotGenerateIncludeStatementSetHandler", this["DoNotGenerateIncludeStatement"]);
            }
        }
        bool IMocOptions.DoNotDisplayWarnings
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotDisplayWarnings", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotDisplayWarnings", value);
                this.ProcessNamedSetHandler("DoNotDisplayWarningsSetHandler", this["DoNotDisplayWarnings"]);
            }
        }
        string IMocOptions.PathPrefix
        {
            get
            {
                return this.GetReferenceTypeOption<string>("PathPrefix", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("PathPrefix", value);
                this.ProcessNamedSetHandler("PathPrefixSetHandler", this["PathPrefix"]);
            }
        }
        #endregion
    }
}
