// <copyright file="OptionUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class OptionUtilities
    {
        public static void AttachModuleOptionUpdatesFromType<AttributeType>(IModule module, System.Type type, Target target)
        {
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic |
                                                          System.Reflection.BindingFlags.Public |
                                                          System.Reflection.BindingFlags.Static |
                                                          System.Reflection.BindingFlags.Instance;
            System.Reflection.MethodInfo[] methods = type.GetMethods(bindingFlags);
            foreach (System.Reflection.MethodInfo method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(AttributeType), false);
                if (attributes.Length > 0)
                {
                    if (!method.IsStatic)
                    {
                        Log.DebugMessage("\tAdding update option instance delegate '{0}' (type '{3}') from '{1}' to '{2}'", method.Name, type.FullName, module.ToString(), typeof(AttributeType).ToString());

                        IModule moduleContainingMethod = ModuleUtilities.GetModule(type, target);
                        module.UpdateOptions += System.Delegate.CreateDelegate(typeof(UpdateOptionCollectionDelegate), moduleContainingMethod, method, true) as UpdateOptionCollectionDelegate;
                    }
                    else
                    {
                        Log.DebugMessage("\tAdding update option static delegate '{0}' (type '{3}') from '{1}' to '{2}'", method.Name, type.FullName, module.ToString(), typeof(AttributeType).ToString());
                        module.UpdateOptions += System.Delegate.CreateDelegate(typeof(UpdateOptionCollectionDelegate), method) as UpdateOptionCollectionDelegate;
                    }
                }
            }
        }

        // this version only applies the exported attribute type
        private static void AttachNodeOptionUpdatesToModule<ExportAttributeType>(IModule module, DependencyNode node)
        {
            System.Type nodeModuleType = node.Module.GetType();
            Target target = node.Target;

            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType, target);
            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType.BaseType, target);

            if (null != node.ExternalDependents)
            {
                foreach (Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    Core.Log.DebugMessage("External dependent '{0}' of '{1}'", dependentNode.UniqueModuleName, node.UniqueModuleName);

                    AttachNodeOptionUpdatesToModule<ExportAttributeType>(module, dependentNode);

                    if (null != dependentNode.Children)
                    {
                        //Core.IModule dependentModule = dependentNode.Module;
                        foreach (Core.DependencyNode childOfDependent in dependentNode.Children)
                        {
                            Core.IModule childModule = childOfDependent.Module;
                            System.Type childType = childModule.GetType();
                            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, childType, target);
                        }
                    }
                }
            }
        }

        // this applies both local and export, but not local to the external dependents
        private static void AttachNodeOptionUpdatesToModule<ExportAttributeType, LocalAttributeType>(IModule module, DependencyNode node)
        {
            System.Type nodeModuleType = node.Module.GetType();
            Target target = node.Target;

            // only apply local here
            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<LocalAttributeType>(module, nodeModuleType, target);
            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<LocalAttributeType>(module, nodeModuleType.BaseType, target);

            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType, target);
            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType.BaseType, target);

            if (null != node.ExternalDependents)
            {
                foreach (Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    Core.Log.DebugMessage("External dependent '{0}' of '{1}'", dependentNode.UniqueModuleName, node.UniqueModuleName);

                    AttachNodeOptionUpdatesToModule<ExportAttributeType>(module, dependentNode);

                    if (null != dependentNode.Children)
                    {
                        foreach (Core.DependencyNode childOfDependent in dependentNode.Children)
                        {
                            Core.IModule childModule = childOfDependent.Module;
                            System.Type childType = childModule.GetType();
                            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, childType, target);
                        }
                    }
                }
            }

            if (null != node.RequiredDependents)
            {
                foreach (Core.DependencyNode requiredNode in node.RequiredDependents)
                {
                    Core.Log.DebugMessage("Required dependent '{0}' of '{1}'", requiredNode.UniqueModuleName, node.UniqueModuleName);

                    AttachNodeOptionUpdatesToModule<ExportAttributeType>(module, requiredNode);

                    if (null != requiredNode.Children)
                    {
                        foreach (Core.DependencyNode childOfDependent in requiredNode.Children)
                        {
                            Core.IModule childModule = childOfDependent.Module;
                            System.Type childType = childModule.GetType();
                            Core.OptionUtilities.AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, childType, target);
                        }
                    }
                }
            }
        }

        private static void ProcessFieldAttributes(Core.IModule module, Core.Target target)
        {
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic |
                                                          System.Reflection.BindingFlags.Public |
                                                          System.Reflection.BindingFlags.Instance;
            System.Reflection.FieldInfo[] fields = module.GetType().GetFields(bindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                object[] attributes = field.GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    IFieldAttributeProcessor fieldAttributeProcessor = attribute as IFieldAttributeProcessor;
                    if (fieldAttributeProcessor != null)
                    {
                        fieldAttributeProcessor.Execute(field, module, target);
                    }
                }
            }
        }

        public static OptionCollectionType CreateOptionCollection<OptionCollectionType, ExportAttributeType, LocalAttributeType>(Core.DependencyNode node, string className) where OptionCollectionType : Core.BaseOptionCollection
        {
            Core.IModule module = node.Module;
            Core.Target target = node.Target;

            ProcessFieldAttributes(module, target);

            OptionCollectionType options;
            if (null != node.Parent && node.Parent.Module.Options is OptionCollectionType)
            {
                options = (node.Parent.Module.Options as OptionCollectionType).Clone() as OptionCollectionType;

                // claim ownership
                options.SetNodeOwnership(node);
                options.ProcessSetHandlers();
            }
            else
            {
                options = Core.OptionCollectionFactory.CreateOptionCollection<OptionCollectionType>(node);

                // apply export and local
                AttachNodeOptionUpdatesToModule<ExportAttributeType, LocalAttributeType>(module, node);

                // update option collections for the current "node group" (i.e. the top-most node of this type, and it's nested objects)
                Core.DependencyNode parentNode = node.Parent;
                while (parentNode != null)
                {
                    AttachNodeOptionUpdatesToModule<ExportAttributeType, LocalAttributeType>(module, parentNode);

                    // end when both the current and its parent node are not nested (as this is an entirely different node)
                    // TODO: module name is the same
                    if (!parentNode.IsModuleNested && (parentNode.Parent != null) && !parentNode.Parent.IsModuleNested)
                    {
                        break;
                    }

                    parentNode = parentNode.Parent;
                }
            }

            module.Options = options;

            module.ExecuteOptionUpdate(target);

            // TODO: the global option override ought to happen here
            var globalOverrides = State.ScriptAssembly.GetCustomAttributes(typeof(GlobalOptionCollectionOverrideAttribute), false);
            if (globalOverrides.Length > 0)
            {
                IGlobalOptionCollectionOverride instance = (globalOverrides[0] as GlobalOptionCollectionOverrideAttribute).OverrideInterface;
                instance.OverrideOptions(options, target);
            }

            return options;
        }
    }
}