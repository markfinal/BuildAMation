// <copyright file="ModuleUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class ModuleUtilities
    {
        public static string GetToolchainImplementation(System.Type moduleType)
        {
            // TODO: should this start at moduleType.BaseType?
            System.Type toolchainType = moduleType;
            string toolchainImplementation = null;
            for (;;)
            {
                var assignToolAttributes = toolchainType.GetCustomAttributes(typeof(AssignToolForModuleAttribute), false);
                if (assignToolAttributes.Length > 0)
                {
                    AssignToolForModuleAttribute attribute = assignToolAttributes[0] as AssignToolForModuleAttribute;
                    if (attribute.OptionsType != null)
                    {
                        toolchainImplementation = attribute.ToolType.Name.ToLower();
                    }
                    break;
                }

                toolchainType = toolchainType.BaseType;
                if (null == toolchainType)
                {
                    throw new Exception(System.String.Format("Unable to locate an identifiable toolchain for module '{0}'", moduleType.ToString()), false);
                }
            }

            if (null == toolchainImplementation)
            {
                string toolchainName = toolchainType.Namespace;
                // look up toolchain used for this
                if (!State.Has("Toolchains", toolchainName))
                {
                    throw new Exception(System.String.Format("Toolchain implementation for '{0}' has not been specified", toolchainName), false);
                }
                toolchainImplementation = State.Get("Toolchains", toolchainName) as string;
            }

            return toolchainImplementation;
        }

        private static TypeArray GetFieldsWithAttributeType<T>(IModule module, Target target) where T : class, ITargetFilters
        {
            System.Type type = module.GetType();
            System.Reflection.FieldInfo[] fieldInfoArray = type.GetFields(System.Reflection.BindingFlags.NonPublic |
                                                                          System.Reflection.BindingFlags.Public |
                                                                          System.Reflection.BindingFlags.Instance |
                                                                          System.Reflection.BindingFlags.FlattenHierarchy);
            TypeArray dependentsList = new TypeArray();
            foreach (System.Reflection.FieldInfo fieldInfo in fieldInfoArray)
            {
                T[] attributes = fieldInfo.GetCustomAttributes(typeof(T), false) as T[];
                if (attributes.Length > 0)
                {
                    if (attributes.Length > 1)
                    {
                        throw new Exception(System.String.Format("More than one attribute not supported on field '{0}'", fieldInfo.Name));
                    }

                    string[] targetFilters = attributes[0].TargetFilters;
                    bool targetFiltersMatch = target.MatchFilters(targetFilters);
                    if (targetFiltersMatch)
                    {
                        System.Type[] values = null;
                        if (fieldInfo.GetValue(module) is Array<System.Type>)
                        {
                            values = (fieldInfo.GetValue(module) as Array<System.Type>).ToArray();
                        }
                        else if (fieldInfo.GetValue(module) is System.Type[])
                        {
                            values = fieldInfo.GetValue(module) as System.Type[];
                        }
                        else
                        {
                            throw new Exception(System.String.Format("{0} field in {1} must be of type System.Type[], Opus.Core.TypeArray or Opus.Core.Array<System.Type>", typeof(T).ToString(), module.ToString()), false);
                        }

                        dependentsList.AddRange(values);
                    }
                }
            }

            if (0 == dependentsList.Count)
            {
                return null;
            }
            else
            {
                return dependentsList;
            }
        }

        public static TypeArray GetExternalDependents(IModule module, Target target)
        {
            return GetFieldsWithAttributeType<Core.DependentModulesAttribute>(module, target);
        }

        public static TypeArray GetRequiredDependents(IModule module, Target target)
        {
            return GetFieldsWithAttributeType<Core.RequiredModulesAttribute>(module, target);
        }

        public static IModule GetModule(System.Type type, Core.Target target)
        {
            DependencyGraph graph = State.Get("System", "Graph") as DependencyGraph;
            if (null == graph)
            {
                throw new Exception("Dependency graph has not yet been constructed");
            }

            foreach (DependencyNode node in graph)
            {
                bool typeMatch = node.Module.GetType() == type;
                bool targetMatch = node.Target == target;
                if (typeMatch && targetMatch)
                {
                    return node.Module;
                }
            }

            throw new Exception(System.String.Format("Unable to locate module for type '{0}' for target '{1}'", type, target), false);
        }
    }
}