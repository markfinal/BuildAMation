// <copyright file="ITargetFilters.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface ITargetFilters
    {
        EPlatform Platform
        {
            get;
            set;
        }

        EConfiguration Configuration
        {
            get;
            set;
        }

        // NEW STYLE
#if true
        System.Type[] ToolsetTypes
        {
            get;
            set;
        }
#else
        string[] Toolchains
        {
            get;
            set;
        }
#endif
    }
}