// <copyright file="EConfiguration.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    [System.Flags]
    public enum EConfiguration
    {
        Invalid   = 0,
        Debug     = (1 << 0),
        Optimized = (1 << 1),
        Profile   = (1 << 2),
        Final     = (1 << 3),
        All       = Debug | Optimized | Profile | Final
    }
}
