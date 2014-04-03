// <copyright file="IOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public interface IOptions
    {
        CSharp.ETarget Target
        {
            get;
            set;
        }

        bool NoLogo
        {
            get;
            set;
        }

        CSharp.EPlatform Platform
        {
            get;
            set;
        }

        CSharp.EDebugInformation DebugInformation
        {
            get;
            set;
        }

        bool Checked
        {
            get;
            set;
        }

        bool Unsafe
        {
            get;
            set;
        }

        CSharp.EWarningLevel WarningLevel
        {
            get;
            set;
        }

        bool WarningsAsErrors
        {
            get;
            set;
        }

        bool Optimize
        {
            get;
            set;
        }

        Opus.Core.FileCollection References
        {
            get;
            set;
        }

        Opus.Core.FileCollection Modules
        {
            get;
            set;
        }

        Opus.Core.StringArray Defines
        {
            get;
            set;
        }
    }
}