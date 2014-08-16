// <copyright file="ThirdPartyModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
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
