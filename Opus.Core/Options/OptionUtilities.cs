// <copyright file="OptionUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class OptionUtilities
    {
        public static void AttachModuleOptionUpdatesFromType<AttributeType>(IModule module, System.Type type, Target target, int depth)
        {
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic |
                                                          System.Reflection.BindingFlags.Public |
                                                          System.Reflection.BindingFlags.Static |
                                                          System.Reflection.BindingFlags.Instance |
                                                          System.Reflection.BindingFlags.FlattenHierarchy;
            System.Reflection.MethodInfo[] methods = type.GetMethods(bindingFlags);
            foreach (System.Reflection.MethodInfo method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(AttributeType), false);
                if (attributes.Length > 0)
                {
                    if (!method.IsStatic)
                    {
                        Log.DebugMessage("{4}{2} += {1}'s instance update '{0}' (type {3})",
                                         method.Name,
                                         type.FullName,
                                         module.ToString(),
                                         typeof(AttributeType).ToString(),
                                         new string('\t', depth));

                        IModule moduleContainingMethod = ModuleUtilities.GetModule(type, target);
                        if (null == moduleContainingMethod)
                        {
                            throw new Exception("While adding option update delegate '{0}', cannot find module of type '{1}' in module '{2}' for target '{3}'", method.Name, type.FullName, module.GetType().FullName, target.ToString());
                        }
                        module.UpdateOptions += System.Delegate.CreateDelegate(typeof(UpdateOptionCollectionDelegate), moduleContainingMethod, method, true) as UpdateOptionCollectionDelegate;
                    }
                    else
                    {
                        Log.DebugMessage("{4}{2} += {1}'s static update '{0}' (type {3})",
                                         method.Name,
                                         type.FullName,
                                         module.ToString(),
                                         typeof(AttributeType).ToString(),
                                         new string('\t', depth));
                        module.UpdateOptions += System.Delegate.CreateDelegate(typeof(UpdateOptionCollectionDelegate), method) as UpdateOptionCollectionDelegate;
                    }
                }
            }
        }

        // this version only applies the exported attribute type
        private static void AttachNodeOptionUpdatesToModule<ExportAttributeType>(BaseModule module, DependencyNode node, int depth)
        {
            System.Type nodeModuleType = node.Module.GetType();
            Target target = node.Target;
            DependencyNode owningNode = module.OwningNode;

            if (!owningNode.ExportedUpdatesAdded.Contains(nodeModuleType))
            {
                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType, target, depth + 1);
                owningNode.ExportedUpdatesAdded.Add(nodeModuleType);
            }
            if (!owningNode.ExportedUpdatesAdded.Contains(nodeModuleType.BaseType))
            {
                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType.BaseType, target, depth + 1);
                owningNode.ExportedUpdatesAdded.Add(nodeModuleType.BaseType);
            }

            if (null != node.ExternalDependents)
            {
                foreach (DependencyNode dependentNode in node.ExternalDependents)
                {
                    Log.DebugMessage("External dependent '{0}' of '{1}'", dependentNode.UniqueModuleName, node.UniqueModuleName);

                    AttachNodeOptionUpdatesToModule<ExportAttributeType>(module, dependentNode, depth + 1);

                    if (null != dependentNode.Children)
                    {
                        //IModule dependentModule = dependentNode.Module;
                        foreach (DependencyNode childOfDependent in dependentNode.Children)
                        {
                            IModule childModule = childOfDependent.Module;
                            System.Type childType = childModule.GetType();

                            if (!owningNode.ExportedUpdatesAdded.Contains(childType))
                            {
                                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, childType, target, depth + 1);
                                owningNode.ExportedUpdatesAdded.Add(childType);
                            }
                        }
                    }
                }
            }
        }

        // this applies both local and export, but not local to the external dependents
        private static void AttachNodeOptionUpdatesToModule<ExportAttributeType, LocalAttributeType>(BaseModule module, DependencyNode node, int depth)
        {
            System.Type nodeModuleType = node.Module.GetType();
            Target target = node.Target;
            DependencyNode owningNode = module.OwningNode;

            // only apply local here
            if (!owningNode.LocalUpdatesAdded.Contains(nodeModuleType))
            {
                AttachModuleOptionUpdatesFromType<LocalAttributeType>(module, nodeModuleType, target, depth + 1);
                owningNode.LocalUpdatesAdded.Add(nodeModuleType);
            }
            if (!owningNode.LocalUpdatesAdded.Contains(nodeModuleType.BaseType))
            {
                AttachModuleOptionUpdatesFromType<LocalAttributeType>(module, nodeModuleType.BaseType, target, depth + 1);
                owningNode.LocalUpdatesAdded.Add(nodeModuleType.BaseType);
            }

            if (!owningNode.ExportedUpdatesAdded.Contains(nodeModuleType))
            {
                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType, target, depth + 1);
                owningNode.ExportedUpdatesAdded.Add(nodeModuleType);
            }
            if (!owningNode.ExportedUpdatesAdded.Contains(nodeModuleType.BaseType))
            {
                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType.BaseType, target, depth + 1);
                owningNode.ExportedUpdatesAdded.Add(nodeModuleType.BaseType);
            }

            if (null != node.ExternalDependents)
            {
                foreach (DependencyNode dependentNode in node.ExternalDependents)
                {
                    Log.DebugMessage("External dependent '{0}' of '{1}'", dependentNode.UniqueModuleName, node.UniqueModuleName);

                    AttachNodeOptionUpdatesToModule<ExportAttributeType>(module, dependentNode, depth + 1);

                    if (null != dependentNode.Children)
                    {
                        foreach (DependencyNode childOfDependent in dependentNode.Children)
                        {
                            IModule childModule = childOfDependent.Module;
                            System.Type childType = childModule.GetType();

                            if (!owningNode.ExportedUpdatesAdded.Contains(childType))
                            {
                                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, childType, target, depth + 1);
                                owningNode.ExportedUpdatesAdded.Add(childType);
                            }
                        }
                    }
                }
            }

            if (null != node.RequiredDependents)
            {
                foreach (DependencyNode requiredNode in node.RequiredDependents)
                {
                    Log.DebugMessage("Required dependent '{0}' of '{1}'", requiredNode.UniqueModuleName, node.UniqueModuleName);

                    AttachNodeOptionUpdatesToModule<ExportAttributeType>(module, requiredNode, depth + 1);

                    if (null != requiredNode.Children)
                    {
                        foreach (DependencyNode childOfDependent in requiredNode.Children)
                        {
                            IModule childModule = childOfDependent.Module;
                            System.Type childType = childModule.GetType();

                            if (!owningNode.ExportedUpdatesAdded.Contains(childType))
                            {
                                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, childType, target, depth + 1);
                                owningNode.ExportedUpdatesAdded.Add(childType);
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessFieldAttributes(IModule module, Target target)
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

        public static OptionCollectionType CreateOptionCollection<OptionCollectionType, ExportAttributeType, LocalAttributeType>(DependencyNode node) where OptionCollectionType : BaseOptionCollection
        {
            BaseModule module = node.Module;
            Target target = node.Target;

            ProcessFieldAttributes(module, target);

            OptionCollectionType options;
            if (null != node.Parent && node.Parent.Module.Options is OptionCollectionType)
            {
                options = (node.Parent.Module.Options as OptionCollectionType).Clone() as OptionCollectionType;

                // claim ownership
                options.SetNodeOwnership(node);
            }
            else
            {
                Log.DebugMessage("Creating option collection for node '{0}'", node.UniqueModuleName);

                options = OptionCollectionFactory.CreateOptionCollection<OptionCollectionType>(node);

                // apply export and local
                AttachNodeOptionUpdatesToModule<ExportAttributeType, LocalAttributeType>(module, node, 0);

                // update option collections for the current "node group" (i.e. the top-most node of this type, and it's nested objects)
                DependencyNode parentNode = node.Parent;
                while (parentNode != null)
                {
                    // TODO: I don't know if this is needed or not
#if false
                    // can only inherit option types if the Targets match
                    // TODO: this is not necessarily true - for example, the common C options (include paths etc)
                    // can be shared, but toolchain specific options cannot
                    // so this needs to be a bit more sophisticated
                    // but then, thinking about it, the option update delegates should be casting to the appropriate interfaces in order to apply themselves
                    if (parentNode.Target != module.OwningNode.Target)
                    {
                        parentNode = parentNode.Parent;
                        continue;
                    }
#endif

                    AttachNodeOptionUpdatesToModule<ExportAttributeType, LocalAttributeType>(module, parentNode, 0);

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

            // execute the global override before any other, so that local delegates can do overrides
            var globalOverrides = State.ScriptAssembly.GetCustomAttributes(typeof(GlobalOptionCollectionOverrideAttribute), false);
            foreach (var globalOverride in globalOverrides)
            {
                IGlobalOptionCollectionOverride instance = (globalOverride as GlobalOptionCollectionOverrideAttribute).OverrideInterface;
                instance.OverrideOptions(options, target);
            }

            module.ExecuteOptionUpdate(target);
            options.FinalizeOptions(target);

            return options;
        }
    }
}