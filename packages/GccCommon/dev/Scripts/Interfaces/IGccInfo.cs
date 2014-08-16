// <copyright file="IGccInfo.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public interface IGCCInfo
    {
        string
        GccVersion(
            Bam.Core.Target target);

        string
        MachineType(
            Bam.Core.Target target);

        string
        GxxIncludePath(
            Bam.Core.Target target);
    }
}
