// <copyright file="ObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C object file
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICompilerTool))]
    public class ObjectFile : Opus.Core.BaseModule
    {
        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        public ObjectFile()
        {
            this.SourceFile = new Opus.Core.File();
        }

        public void Include(Opus.Core.Location baseLocation, string pattern)
        {
            this.SourceFile.Include(baseLocation, pattern);
        }
    }
}