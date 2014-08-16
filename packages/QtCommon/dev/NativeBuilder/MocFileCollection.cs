// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        [Bam.Core.EmptyBuildFunction]
        public object
        Build(
            QtCommon.MocFileCollection moduleToBuild,
            out bool success)
        {
            success = true;
            return null;
        }
    }
}
