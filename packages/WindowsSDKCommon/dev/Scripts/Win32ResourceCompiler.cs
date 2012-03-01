// <copyright file="Win32ResoureCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>WindowsSDKCommon package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportWin32ResourceCompilerOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalWin32ResourceCompilerOptionsDelegateAttribute : System.Attribute
    {
    }

    public sealed class Win32ResourceCompiler : Opus.Core.ITool
    {
        private string platformBinFolder;

        public Win32ResourceCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            this.platformBinFolder = WindowsSDK.WindowsSDK.BinPath(target);
        }

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.platformBinFolder, "rc.exe");
        }
    }
}