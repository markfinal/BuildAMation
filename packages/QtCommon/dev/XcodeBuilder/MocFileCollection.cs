// <copyright file="MocFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(QtCommon.MocFileCollection moduleToBuild, out bool success)
        {
            success = true;
            return null;
        }
    }
}
