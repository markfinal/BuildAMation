// <copyright file="Win32ResoureCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    sealed class Win32ResourceCompiler : C.Win32ResourceCompilerBase
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

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.platformBinFolder, "rc.exe");
        }

        public override string InputFileSwitch
        {
            get
            {
                return string.Empty;
            }
        }

        public override string OutputFileSwitch
        {
            get
            {
                return "/fo ";
            }
        }
    }
}
