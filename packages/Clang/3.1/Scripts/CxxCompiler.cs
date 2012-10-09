// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class CxxCompiler : C.CxxCompiler, Opus.Core.ITool
    {
        // TODO: this needs to be shared
        private static string InstallPath
        {
            get;
            set;
        }

        static CxxCompiler()
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                InstallPath = @"D:\dev\Thirdparty\Clang\3.1\build\bin\Release";
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            // TOOD: extensions to be stored centrally?
            return System.IO.Path.Combine(InstallPath, "clang++.exe");
        }

        #endregion
    }
}
