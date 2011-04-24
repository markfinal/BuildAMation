// <copyright file="ThirdPartyModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(C.ThirdPartyModule thirdPartyModule, out bool success)
        {
            success = true;
            return null;
        }
    }
}