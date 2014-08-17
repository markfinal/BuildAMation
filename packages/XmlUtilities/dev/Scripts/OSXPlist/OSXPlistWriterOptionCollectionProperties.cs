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
#endregion

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
