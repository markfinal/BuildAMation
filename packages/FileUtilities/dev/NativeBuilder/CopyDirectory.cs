// <copyright file="CopyDirectory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(FileUtilities.CopyDirectory moduleToBuild, out bool success)
        {
            success = true;
            return null;
        }
    }
}
