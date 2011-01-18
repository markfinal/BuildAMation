namespace Qt
{
    public class ExportMocOptionsDelegateAttribute : System.Attribute
    {
    }

    public class LocalMocOptionsDelegateAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(MocTool),
                                   typeof(ExportMocOptionsDelegateAttribute),
                                   typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(MocOptionCollection))]
    public abstract class MocModule : Opus.Core.IModule, Opus.Core.IInjectModules
    {
        public void ExecuteOptionUpdate(Opus.Core.Target target)
        {
            if (null != this.UpdateOptions)
            {
                this.UpdateOptions(this, target);
            }
        }

        public Opus.Core.ModuleCollection GetNestedDependents(Opus.Core.Target target)
        {
            return null;
        }

        public Opus.Core.BaseOptionCollection Options
        {
            get;
            set;
        }

        public event Opus.Core.UpdateOptionCollectionDelegate UpdateOptions;

        public Opus.Core.ModuleCollection GetInjectedModules(Opus.Core.Target target)
        {
            return new Opus.Core.ModuleCollection();
        }
    }
}