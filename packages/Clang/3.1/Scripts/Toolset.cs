// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public class Toolset : Opus.Core.IToolset/*, C.ICompilerInfo*/
    {
        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return @"D:\dev\Thirdparty\Clang\3.1\build\bin\Release";
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            throw new System.NotImplementedException();
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return "3.1";
        }

        #endregion

#if false
        #region ICompilerInfo Members

        string C.ICompilerInfo.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerInfo.ObjectFileSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.ICompilerInfo.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerInfo.IncludePaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion
#endif
    }
}
