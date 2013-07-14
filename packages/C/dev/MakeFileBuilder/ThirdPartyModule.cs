// <copyright file="ThirdPartyModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(C.ThirdPartyModule moduleToBuild, out bool success)
        {
            success = true;
            return null;
        }
    }
}