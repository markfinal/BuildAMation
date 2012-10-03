// <copyright file="SymLink.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [Opus.Core.AssignToolForModule(typeof(SymLinkTool),
                                   typeof(ExportOptionsDelegateAttribute),
                                   typeof(LocalOptionsDelegateAttribute),
                                   typeof(SymLinkOptionCollection))]
    [Opus.Core.ModuleToolAssignment(typeof(SymLinkTool))]
    public class SymLink : Opus.Core.IModule
    {
        void Opus.Core.IModule.ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

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

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;
    }
}