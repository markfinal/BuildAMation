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
            var bindingFlags = System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Public |
                               System.Reflection.BindingFlags.Static |
                               System.Reflection.BindingFlags.Instance |
                               System.Reflection.BindingFlags.FlattenHierarchy;
            var methods = type.GetMethods(bindingFlags);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(AttributeType), false);
                if (0 == attributes.Length)
                {
                    continue;
                }

                if (!method.IsStatic)
                {
                    Log.DebugMessage("{4}{2} += {1}'s instance update '{0}' (type {3})",
                                     method.Name,
                                     type.FullName,
                                     module.ToString(),
                                     typeof(AttributeType).ToString(),
                                     new string('\t', depth));

                    var moduleContainingMethod = ModuleUtilities.GetModule(type, target);
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

        private static void RecursivelyAttachExportUpdates<ExportAttributeType>(DependencyNode node, DependencyNode owningNode, BaseModule module, int newDepth, DependencyNodeCollection collection, string collectionType)
        {
            if (null == collection)
            {
                return;
            }

            foreach (var dependentNode in collection)
            {
                Log.DebugMessage("\tAttaching {0} dependent '{1}' of '{2}' to option updates", collectionType, dependentNode.UniqueModuleName, node.UniqueModuleName);

                AttachNodeOptionUpdatesToModule<ExportAttributeType>(module, dependentNode, newDepth);

                if (null == dependentNode.Children)
                {
                    continue;
                }

                foreach (var childOfDependent in dependentNode.Children)
                {
                    var childModule = childOfDependent.Module;
                    var childType = childModule.GetType();

                    if (!owningNode.ExportedUpdatesAdded.Contains(childType))
                    {
                        var childToolset = ModuleUtilities.GetToolsetForModule(childType);
                        var childTarget = Target.GetInstance((BaseTarget)node.Target, childToolset);
                        AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, childType, childTarget, newDepth);
                        owningNode.ExportedUpdatesAdded.Add(childType);
                    }
                }
            }
        }

        // this version only applies the exported attribute type
        private static void AttachNodeOptionUpdatesToModule<ExportAttributeType>(BaseModule module, DependencyNode node, int depth)
        {
            var nodeModuleType = node.Module.GetType();
            var target = node.Target;
            var owningNode = module.OwningNode;
            int newDepth = depth + 1;

            if (!owningNode.ExportedUpdatesAdded.Contains(nodeModuleType))
            {
                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType, target, newDepth);
                owningNode.ExportedUpdatesAdded.Add(nodeModuleType);
            }
            if (!owningNode.ExportedUpdatesAdded.Contains(nodeModuleType.BaseType))
            {
                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType.BaseType, target, newDepth);
                owningNode.ExportedUpdatesAdded.Add(nodeModuleType.BaseType);
            }

            RecursivelyAttachExportUpdates<ExportAttributeType>(node, owningNode, module, newDepth, node.ExternalDependents, "External");
        }

        // this applies both local and export, but not local to the external dependents
        private static void AttachNodeOptionUpdatesToModule<ExportAttributeType, LocalAttributeType>(BaseModule module, DependencyNode node, int depth)
        {
            var nodeModuleType = node.Module.GetType();
            var target = node.Target;
            var owningNode = module.OwningNode;
            int newDepth = depth + 1;

            // only apply local here
            if (!owningNode.LocalUpdatesAdded.Contains(nodeModuleType))
            {
                AttachModuleOptionUpdatesFromType<LocalAttributeType>(module, nodeModuleType, target, newDepth);
                owningNode.LocalUpdatesAdded.Add(nodeModuleType);
            }
            if (!owningNode.LocalUpdatesAdded.Contains(nodeModuleType.BaseType))
            {
                AttachModuleOptionUpdatesFromType<LocalAttributeType>(module, nodeModuleType.BaseType, target, newDepth);
                owningNode.LocalUpdatesAdded.Add(nodeModuleType.BaseType);
            }

            if (!owningNode.ExportedUpdatesAdded.Contains(nodeModuleType))
            {
                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType, target, newDepth);
                owningNode.ExportedUpdatesAdded.Add(nodeModuleType);
            }
            if (!owningNode.ExportedUpdatesAdded.Contains(nodeModuleType.BaseType))
            {
                AttachModuleOptionUpdatesFromType<ExportAttributeType>(module, nodeModuleType.BaseType, target, newDepth);
                owningNode.ExportedUpdatesAdded.Add(nodeModuleType.BaseType);
            }

            RecursivelyAttachExportUpdates<ExportAttributeType>(node, owningNode, module, newDepth, node.ExternalDependents, "External");

            RecursivelyAttachExportUpdates<ExportAttributeType>(node, owningNode, module, newDepth, node.RequiredDependents, "Required");
        }

        private static void ProcessFieldAttributes(IModule module, Target target)
        {
            var bindingFlags = System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Public |
                               System.Reflection.BindingFlags.Instance;
            var fields = module.GetType().GetFields(bindingFlags);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    var fieldAttributeProcessor = attribute as IFieldAttributeProcessor;
                    if (fieldAttributeProcessor != null)
                    {
                        fieldAttributeProcessor.Execute(field, module, target);
                    }
                }
            }
        }

        public static void CreateOptionCollection<OptionCollectionType, ExportAttributeType, LocalAttributeType>(DependencyNode node) where OptionCollectionType : BaseOptionCollection
        {
            var module = node.Module;
            var target = node.Target;

            ProcessFieldAttributes(module, target);

            OptionCollectionType options;
            if (null != node.Parent && node.Parent.Module.Options is OptionCollectionType)
            {
                Log.DebugMessage("\tCloning option collection for node '{0}' from '{1}'", node.UniqueModuleName, node.Parent.UniqueModuleName);

                options = (node.Parent.Module.Options as OptionCollectionType).Clone() as OptionCollectionType;

                // claim ownership
                options.SetNodeOwnership(node);
            }
            else
            {
                Log.DebugMessage("\tCreating new collection", node.UniqueModuleName);

                options = OptionCollectionFactory.CreateOptionCollection<OptionCollectionType>(node);

                // apply export and local
                AttachNodeOptionUpdatesToModule<ExportAttributeType, LocalAttributeType>(module, node, 0);

                // update option collections for the current "node group" (i.e. the top-most node of this type, and it's nested objects)
                var parentNode = node.Parent;
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

            // assign options so that they can have their updates executed
            module.Options = options;

            // execute the global override before any other, so that local delegates can do overrides
            var globalOverrides = State.ScriptAssembly.GetCustomAttributes(typeof(GlobalOptionCollectionOverrideAttribute), false);
            foreach (var globalOverride in globalOverrides)
            {
                var instance = (globalOverride as GlobalOptionCollectionOverrideAttribute).OverrideInterface;
                instance.OverrideOptions(options, target);
            }

            module.ExecuteOptionUpdate(target);
        }
    }
}