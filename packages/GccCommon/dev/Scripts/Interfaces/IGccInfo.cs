// <copyright file="IGccInfo.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public interface IGCCInfo
    {
        string GccVersion(Opus.Core.Target target);

        string MachineType(Opus.Core.Target target);

        string GxxIncludePath(Opus.Core.Target target);
    }
}
