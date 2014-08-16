// <copyright file="EPlatform.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    [System.Flags]
    public enum EPlatform
    {
        Invalid = 0,
        Win32   = (1 << 0),
        Win64   = (1 << 1),
        Unix32  = (1 << 2),
        Unix64  = (1 << 3),
        OSX32   = (1 << 4),
        OSX64   = (1 << 5),

        Windows = Win32 | Win64,
        Unix    = Unix32 | Unix64,
        OSX     = OSX32 | OSX64,
        Posix   = Unix | OSX,

        NotWindows = ~Windows,
        NotUnix    = ~Unix,
        NotOSX     = ~OSX,
        NotPosix   = ~Posix,

        All        = Windows | Unix | OSX
    }
}
