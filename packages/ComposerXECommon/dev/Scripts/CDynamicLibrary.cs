// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>IntelCommon package</summary>
// <author>Mark Final</author>
namespace C
{
    public partial class DynamicLibrary
    {
        [LocalCompilerOptionsDelegate]
        protected static void IntelCommonDynamicLibrarySetPositionIndependentCode(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsUnix(target.Platform))
            {
                IntelCommon.ICCompilerOptions compilerOptions = module.Options as IntelCommon.ICCompilerOptions;
                if (null != compilerOptions)
                {
                    compilerOptions.PositionIndependentCode = true;
                }
            }
        }
    }
}