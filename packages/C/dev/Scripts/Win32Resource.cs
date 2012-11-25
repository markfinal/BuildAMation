// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ console application
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(IWinResourceCompilerTool))]
    public class Win32Resource : Opus.Core.BaseModule
    {
        public Win32Resource()
        {
            this.ResourceFile = new Opus.Core.File();
        }

        public Opus.Core.File ResourceFile
        {
            get;
            private set;
        }
    }
}