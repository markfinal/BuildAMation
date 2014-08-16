// <copyright file="ThirdPartyModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        [Bam.Core.EmptyBuildFunction]
        public object
        Build(
            C.ThirdPartyModule moduleToBuild,
            out bool success)
        {
            success = true;
            return null;
        }
    }
}
