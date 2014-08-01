// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=IOSXPlistOptions.cs
//     -n=XmlUtilities
//     -c=OSXPlistWriterOptionCollection
//     -p
//     -d
//     -pv=PrivateData

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
