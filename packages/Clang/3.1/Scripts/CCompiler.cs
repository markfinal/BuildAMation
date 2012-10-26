// <copyright file="CCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class CCompiler : C.Compiler, C.ICompiler, Opus.Core.ITool
    {
        private Opus.Core.IToolset toolset = Opus.Core.ToolsetFactory.CreateToolset(typeof(Clang.Toolset));

        public CCompiler(Opus.Core.Target target)
        {
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            // TODO: can we have this extension somewhere central?
            return System.IO.Path.Combine(this.toolset.InstallPath(target), "clang.exe");
        }

        #endregion

        #region ICompiler Members

        Opus.Core.StringArray C.ICompiler.IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return new Opus.Core.StringArray();
        }

        Opus.Core.StringArray C.ICompiler.IncludePathCompilerSwitches
        {
            get
            {
                Opus.Core.StringArray switches = new Opus.Core.StringArray();
                switches.Add("-I");
                return switches;
            }
        }

        #endregion
    }
}
