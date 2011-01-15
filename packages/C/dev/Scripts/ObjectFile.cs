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
    [Opus.Core.AssignToolForModule(typeof(Compiler),
                                   typeof(ExportCompilerOptionsDelegateAttribute),
                                   typeof(LocalCompilerOptionsDelegateAttribute),
                                   ClassNames.CCompilerToolOptions)]
    public class ObjectFile : Opus.Core.IModule
    {
        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public ObjectFile(string absoluteFilePath)
        {
            this.SourceFile = new Opus.Core.File(absoluteFilePath);
        }

        public ObjectFile(params string[] fileParts)
        {
            this.SourceFile = new Opus.Core.File(fileParts);
        }

        public Opus.Core.File SourceFile
        {
            get;
            private set;
        }

        public Opus.Core.ModuleCollection GetNestedDependents(Opus.Core.Target target)
        {
            return null;
        }
    }
}