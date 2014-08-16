// <copyright file="WindowsApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ Windows application
    /// </summary>
    public class WindowsApplication : Application
    {
        [LocalCompilerOptionsDelegate]
        private static void WindowsApplicationCompilerOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var compilerOptions = module.Options as ICCompilerOptions;
                compilerOptions.Defines.Add("_WINDOWS");
            }
        }

        [LocalLinkerOptionsDelegate]
        private static void WindowApplicationLinkerOptions(Bam.Core.IModule module, Bam.Core.Target target)
        {
            if (Bam.Core.OSUtilities.IsWindows(target))
            {
                var linkerOptions = module.Options as ILinkerOptions;
                linkerOptions.SubSystem = ESubsystem.Windows;
            }
        }
    }
}