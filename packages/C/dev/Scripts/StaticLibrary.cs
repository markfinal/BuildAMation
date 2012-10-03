// <copyright file="StaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    /// <summary>
    /// C/C++ static library
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(Archiver),
                                   typeof(ExportArchiverOptionsDelegateAttribute),
                                   typeof(LocalArchiverOptionsDelegateAttribute),
                                   ClassNames.ArchiverToolOptions)]
    [Opus.Core.ModuleToolAssignment(typeof(Archiver))]
    public class StaticLibrary : Opus.Core.IModule, Opus.Core.INestedDependents
    {
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

        Opus.Core.ModuleCollection Opus.Core.INestedDependents.GetNestedDependents(Opus.Core.Target target)
        {
            Opus.Core.ModuleCollection collection = new Opus.Core.ModuleCollection();

            System.Type type = this.GetType();
            System.Reflection.FieldInfo[] fieldInfoArray = type.GetFields(System.Reflection.BindingFlags.NonPublic |
                                                                          System.Reflection.BindingFlags.Public |
                                                                          System.Reflection.BindingFlags.Instance);
            foreach (System.Reflection.FieldInfo fieldInfo in fieldInfoArray)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                if (attributes.Length > 0)
                {
                    Opus.Core.ITargetFilters targetFilters = attributes[0] as Opus.Core.ITargetFilters;
                    if (!Opus.Core.TargetUtilities.MatchFilters(target, targetFilters))
                    {
                        Opus.Core.Log.DebugMessage("Source file field '{0}' of module '{1}' with filters '{2}' does not match target '{3}'", fieldInfo.Name, type.ToString(), targetFilters.ToString(), target.ToString());
                        continue;
                    }

                    Opus.Core.IModule module = fieldInfo.GetValue(this) as Opus.Core.IModule;
                    if (null == module)
                    {
                        throw new Opus.Core.Exception(System.String.Format("Field '{0}', marked with Opus.Core.SourceFiles attribute, must be derived from type Core.IModule", fieldInfo.Name));
                    }
                    collection.Add(module);
                }
            }

            return collection;
        }
    }
}