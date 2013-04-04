// <copyright file="IOSXPlistOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public interface IOSXPlistOptions
    {
        // StateOnly
        string CFBundleName
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleDisplayName
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleIdentifier
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleVersion
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleSignature
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleExecutable
        {
            get;
            set;
        }

        // StateOnly
        string CFBundleShortVersionString
        {
            get;
            set;
        }

        // StateOnly
        string LSMinimumSystemVersion
        {
            get;
            set;
        }

        // StateOnly
        string NSHumanReadableCopyright
        {
            get;
            set;
        }

        // StateOnly
        string NSMainNibFile
        {
            get;
            set;
        }

        // StateOnly
        string NSPrincipalClass
        {
            get;
            set;
        }
    }
}
