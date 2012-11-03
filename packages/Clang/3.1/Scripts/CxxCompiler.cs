// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class CxxCompiler : C.ICxxCompilerTool
    {
        private Opus.Core.IToolset toolset;

        public CxxCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ICompilerTool Members

        string C.ICompilerTool.PreprocessedOutputSuffix
        {
            get
            {
                return ".ii";
            }
        }

        string C.ICompilerTool.ObjectFileSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.ICompilerTool.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePaths(Opus.Core.Target target)
        {
            return new Opus.Core.StringArray();
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-I");
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            // TODO: can we have this file extension somewhere central?
            return System.IO.Path.Combine(this.toolset.InstallPath((Opus.Core.BaseTarget)target), "clang++.exe");
        }

        #endregion

#if false
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
#endif
    }
}
