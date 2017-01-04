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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=IOSXPlistOptions.cs
//     -n=XmlUtilities
//     -c=OSXPlistWriterOptionCollection
//     -p
//     -d
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace XmlUtilities
{
    public partial class OSXPlistWriterOptionCollection
    {
        #region IOSXPlistOptions Option properties
        string IOSXPlistOptions.CFBundleName
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleName", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleName", value);
                this.ProcessNamedSetHandler("CFBundleNameSetHandler", this["CFBundleName"]);
            }
        }
        string IOSXPlistOptions.CFBundleDisplayName
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleDisplayName", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleDisplayName", value);
                this.ProcessNamedSetHandler("CFBundleDisplayNameSetHandler", this["CFBundleDisplayName"]);
            }
        }
        string IOSXPlistOptions.CFBundleIdentifier
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleIdentifier", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleIdentifier", value);
                this.ProcessNamedSetHandler("CFBundleIdentifierSetHandler", this["CFBundleIdentifier"]);
            }
        }
        string IOSXPlistOptions.CFBundleVersion
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleVersion", value);
                this.ProcessNamedSetHandler("CFBundleVersionSetHandler", this["CFBundleVersion"]);
            }
        }
        string IOSXPlistOptions.CFBundleSignature
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleSignature", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleSignature", value);
                this.ProcessNamedSetHandler("CFBundleSignatureSetHandler", this["CFBundleSignature"]);
            }
        }
        string IOSXPlistOptions.CFBundleExecutable
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleExecutable", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleExecutable", value);
                this.ProcessNamedSetHandler("CFBundleExecutableSetHandler", this["CFBundleExecutable"]);
            }
        }
        string IOSXPlistOptions.CFBundleShortVersionString
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleShortVersionString", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CFBundleShortVersionString", value);
                this.ProcessNamedSetHandler("CFBundleShortVersionStringSetHandler", this["CFBundleShortVersionString"]);
            }
        }
        string IOSXPlistOptions.LSMinimumSystemVersion
        {
            get
            {
                return this.GetReferenceTypeOption<string>("LSMinimumSystemVersion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("LSMinimumSystemVersion", value);
                this.ProcessNamedSetHandler("LSMinimumSystemVersionSetHandler", this["LSMinimumSystemVersion"]);
            }
        }
        string IOSXPlistOptions.NSHumanReadableCopyright
        {
            get
            {
                return this.GetReferenceTypeOption<string>("NSHumanReadableCopyright", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("NSHumanReadableCopyright", value);
                this.ProcessNamedSetHandler("NSHumanReadableCopyrightSetHandler", this["NSHumanReadableCopyright"]);
            }
        }
        string IOSXPlistOptions.NSMainNibFile
        {
            get
            {
                return this.GetReferenceTypeOption<string>("NSMainNibFile", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("NSMainNibFile", value);
                this.ProcessNamedSetHandler("NSMainNibFileSetHandler", this["NSMainNibFile"]);
            }
        }
        string IOSXPlistOptions.NSPrincipalClass
        {
            get
            {
                return this.GetReferenceTypeOption<string>("NSPrincipalClass", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("NSPrincipalClass", value);
                this.ProcessNamedSetHandler("NSPrincipalClassSetHandler", this["NSPrincipalClass"]);
            }
        }
        #endregion
    }
}
