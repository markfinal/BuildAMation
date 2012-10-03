// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace C
{
    public partial class DynamicLibrary
    {
        [LocalCompilerOptionsDelegate]
        protected static void GccCommonDynamicLibrarySetPositionIndependentCode(Opus.Core.IModule module, Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsUnix(target))
            {
                GccCommon.ICCompilerOptions compilerOptions = module.Options as GccCommon.ICCompilerOptions;
                if (null != compilerOptions)
                {
                    compilerOptions.PositionIndependentCode = true;
                }
            }
        }
    }
}