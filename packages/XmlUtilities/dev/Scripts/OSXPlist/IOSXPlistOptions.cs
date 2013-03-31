// <copyright file="IOSXPlistOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public interface IOSXPlistOptions
    {
        string CFBundleName
        {
            get;
            set;
        }

        string CFBundleDisplayName
        {
            get;
            set;
        }

        string CFBundleIdentifier
        {
            get;
            set;
        }

        string CFBundleVersion
        {
            get;
            set;
        }

        string CFBundleSignature
        {
            get;
            set;
        }

        string CFBundleExecutable
        {
            get;
            set;
        }

        string NSPrincipalClass
        {
            get;
            set;
        }
    }
}
