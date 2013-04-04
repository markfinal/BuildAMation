// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=IOSXPlistOptions.cs -n=XmlUtilities -c=OSXPlistWriterOptionCollection -p -d -pv=PrivateData
namespace XmlUtilities
{
    public partial class OSXPlistWriterOptionCollection
    {
        #region IOSXPlistOptions Option properties
        string IOSXPlistOptions.CFBundleName
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CFBundleName");
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
                return this.GetReferenceTypeOption<string>("CFBundleDisplayName");
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
                return this.GetReferenceTypeOption<string>("CFBundleIdentifier");
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
                return this.GetReferenceTypeOption<string>("CFBundleVersion");
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
                return this.GetReferenceTypeOption<string>("CFBundleSignature");
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
                return this.GetReferenceTypeOption<string>("CFBundleExecutable");
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
                return this.GetReferenceTypeOption<string>("CFBundleShortVersionString");
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
                return this.GetReferenceTypeOption<string>("LSMinimumSystemVersion");
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
                return this.GetReferenceTypeOption<string>("NSHumanReadableCopyright");
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
                return this.GetReferenceTypeOption<string>("NSMainNibFile");
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
                return this.GetReferenceTypeOption<string>("NSPrincipalClass");
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
