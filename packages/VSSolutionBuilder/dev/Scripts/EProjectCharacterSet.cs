// <copyright file="EProjectCharacterSet.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public enum EProjectCharacterSet
    {
#if true
        Undefined = -1,
        NotSet,
        UniCode,
        MultiByte
#else
        Undefined = -1,
        NotSet = C.ECharacterSet.NotSet,
        Unicode = C.ECharacterSet.Unicode,
        MultiByte = C.ECharacterSet.MultiByte
#endif
    }
}
