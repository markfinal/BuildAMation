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
    [Opus.Core.AssignToolForModule(typeof(C.Win32ResourceCompilerBase),
                                   typeof(ExportWin32ResourceCompilerOptionsDelegateAttribute),
                                   typeof(LocalWin32ResourceCompilerOptionsDelegateAttribute),
                                   ClassNames.Win32ResourceCompilerToolOptions)]
    [Opus.Core.ModuleToolAssignment(typeof(C.Win32ResourceCompilerBase))]
    public class Win32Resource : Opus.Core.IModule
    {
        public Win32Resource()
        {
            this.ResourceFile = new Opus.Core.File();
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        Opus.Core.BaseOptionCollection Opus.Core.IModule.Options
        {
            get;
            set;
        }

        Opus.Core.DependencyNode Opus.Core.IModule.OwningNode
        {
            get;
            set;
        }

        public Opus.Core.ProxyModulePath ProxyPath
        {
            get;
            set;
        }

        void Opus.Core.IModule.ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (this.UpdateOptions != null)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.File ResourceFile
        {
            get;
            private set;
        }
    }
}