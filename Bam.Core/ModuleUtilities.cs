#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    public static class ModuleUtilities
    {
        public static IToolset
        GetToolsetForModule(
            System.Type moduleType)
        {
            var type = moduleType;
            while (type != null)
            {
                // get the type of the tool associated with the module
                // the type may be set on any of it's base classes
                var t = type.GetCustomAttributes(typeof(ModuleToolAssignmentAttribute), true);
                if (0 == t.Length)
                {
                    throw new Exception("Unable to determine tool assignment to module type '{0}'", moduleType.ToString());
                }

                if (t.Length > 1)
                {
                    throw new Exception("There are {0} ModuleToolAssignments to the module type '{1}'. There should be only one.", t.Length, moduleType.ToString());
                }

                var attr = t[0] as ModuleToolAssignmentAttribute;
                var toolType = attr.ToolType;
                if (null == toolType)
                {
                    throw new Exception("The tool type in the ModuleToolAssignment for module type '{0}' cannot be null", moduleType.ToString());
                }

                if (!toolType.IsInterface)
                {
                    throw new Exception("The tool type '{0}' in the ModuleToolAssignment for module type '{1}' must be an interface", toolType.ToString(), moduleType.ToString());
                }

                var providers = toolType.GetCustomAttributes(typeof(AssignToolsetProviderAttribute), false);
                if (0 == providers.Length)
                {
                    throw new Exception("The tool interface '{0}' the ModuleToolAssignment for module type '{1}' has no toolsets assigned to it", toolType.ToString(), moduleType.ToString());
                }

                var toolsetName = (providers[0] as AssignToolsetProviderAttribute).ToolsetName(toolType);
                if (!State.Has("Toolset", toolsetName))
                {
                    throw new Exception("Toolset '{0}' not registered", toolsetName);
                }

                var toolset = State.Get("Toolset", toolsetName) as IToolset;
                //Log.DebugMessage("\tToolset for tool type '{0}' is '{1}'", toolType.ToString(), toolset.ToString());
                return toolset;
            }

            throw new Exception("Unable to locate toolchain for module '{0}'", moduleType.ToString());
        }

        private static TypeArray
        GetFieldsWithAttributeType<T>(
            IModule module,
            Target target) where T : class, ITargetFilters
        {
            var type = module.GetType();
            var fieldInfoArray = type.GetFields(System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Instance |
                                                System.Reflection.BindingFlags.FlattenHierarchy);
            var dependentsList = new TypeArray();
            foreach (var fieldInfo in fieldInfoArray)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(T), false) as T[];
                if (attributes.Length > 0)
                {
                    if (attributes.Length > 1)
                    {
                        throw new Exception("More than one attribute not supported on field '{0}'", fieldInfo.Name);
                    }

                    var targetFiltersMatch = TargetUtilities.MatchFilters(target, attributes[0]);
                    if (targetFiltersMatch)
                    {
                        System.Type[] values = null;
                        var fieldValue = fieldInfo.GetValue(module);
                        if (fieldValue is Array<System.Type>)
                        {
                            values = (fieldValue as Array<System.Type>).ToArray();
                        }
                        else if (fieldValue is System.Type[])
                        {
                            values = fieldValue as System.Type[];
                        }
                        else
                        {
                            throw new Exception("{0} field in {1} is of type {2} but must be of type System.Type[], Bam.Core.TypeArray or Bam.Core.Array<System.Type>", typeof(T).ToString(), module.ToString(), fieldValue.GetType());
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

        // TODO: should be able to roll GetExternalDependents and GetRequiredDependents into one
        // function, and return both results.
        // Might have to make DependentModulesAttribute and RequiredModulesAttribute derive from a common
        // interface/base class so that both can be extracted at once in reflection
        // This will improve performance as we only have to use reflection on the fields once
        // and both are always called in close proximity to each other
        public static TypeArray
        GetExternalDependents(
            IModule module,
            Target target)
        {
            return GetFieldsWithAttributeType<Core.DependentModulesAttribute>(module, target);
        }

        public static TypeArray
        GetRequiredDependents(
            IModule module,
            Target target)
        {
            return GetFieldsWithAttributeType<Core.RequiredModulesAttribute>(module, target);
        }

        public static TypeArray
        GetSiblingModuleTypes(
            IModule module,
            Target target)
        {
            return GetFieldsWithAttributeType<SiblingModulesAttribute>(module, target);
        }

        private static System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Core.BaseTarget, DependencyNode>> typeBaseTargetToNodeDictionary = new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Core.BaseTarget, DependencyNode>>();

        public static DependencyNode
        GetNode(
            System.Type type,
            Core.BaseTarget baseTarget)
        {
            if (typeBaseTargetToNodeDictionary.ContainsKey(type) &&
                typeBaseTargetToNodeDictionary[type].ContainsKey(baseTarget))
            {
                return typeBaseTargetToNodeDictionary[type][baseTarget];
            }

            var graph = State.Get("System", "Graph") as DependencyGraph;
            if (null == graph)
            {
                throw new Exception("Dependency graph has not yet been constructed");
            }

            foreach (DependencyNode node in graph)
            {
                var moduleType = node.Module.GetType();
                var typeMatch = (moduleType == type);
                var baseTargetMatch = ((BaseTarget)node.Target == baseTarget);
                if (typeMatch)
                {
                    if (baseTargetMatch)
                    {
                        // modules that create executables that are used as part of the build
                        // can query into the database from multiple use threads, so make it safe
                        lock (typeBaseTargetToNodeDictionary)
                        {
                            if (!typeBaseTargetToNodeDictionary.ContainsKey(type))
                            {
                                typeBaseTargetToNodeDictionary.Add(type, new System.Collections.Generic.Dictionary<Core.BaseTarget, DependencyNode>());
                            }
                            if (!typeBaseTargetToNodeDictionary[type].ContainsKey(baseTarget))
                            {
                                typeBaseTargetToNodeDictionary[type][baseTarget] = node;
                            }
                        }
                        return node;
                    }
                }
                else
                {
                    var baseType = moduleType.BaseType;
                    while ((null != baseType) && !baseType.IsInterface)
                    {
                        if ((baseType == type) && baseTargetMatch)
                        {
                            // modules that create executables that are used as part of the build
                            // can query into the database from multiple use threads, so make it safe
                            lock (typeBaseTargetToNodeDictionary)
                            {
                                if (!typeBaseTargetToNodeDictionary.ContainsKey(type))
                                {
                                    typeBaseTargetToNodeDictionary.Add(type, new System.Collections.Generic.Dictionary<Core.BaseTarget, DependencyNode>());
                                }
                                if (!typeBaseTargetToNodeDictionary[type].ContainsKey(baseTarget))
                                {
                                    typeBaseTargetToNodeDictionary[type][baseTarget] = node;
                                }
                            }
                            return node;
                        }

                        baseType = baseType.BaseType;
                    }
                }
            }

            return null;
        }

        public static IModule
        GetModule(
            System.Type type,
            Core.BaseTarget baseTarget)
        {
            var node = GetNode(type, baseTarget);
            if (null != node)
            {
                var module = node.Module;
                return module;
            }

            return null;
        }

        private static System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Core.Target, IModule>> typeTargetToModuleDictionary = new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Core.Target, IModule>>();

        public static IModule
        GetModule(
            System.Type type,
            Core.Target target)
        {
            if (typeTargetToModuleDictionary.ContainsKey(type) &&
                typeTargetToModuleDictionary[type].ContainsKey(target))
            {
                return typeTargetToModuleDictionary[type][target];
            }

            var graph = State.Get("System", "Graph") as DependencyGraph;
            if (null == graph)
            {
                throw new Exception("Dependency graph has not yet been constructed");
            }

            foreach (DependencyNode node in graph)
            {
                var moduleType = node.Module.GetType();
                var typeMatch = (moduleType == type);
                var targetMatch = (node.Target == target);
                if (typeMatch)
                {
                    if (targetMatch)
                    {
                        // modules that create executables that are used as part of the build
                        // can query into the database from multiple use threads, so make it safe
                        lock (typeTargetToModuleDictionary)
                        {
                            if (!typeTargetToModuleDictionary.ContainsKey(type))
                            {
                                typeTargetToModuleDictionary.Add(type, new System.Collections.Generic.Dictionary<Core.Target, IModule>());
                            }
                            if (!typeTargetToModuleDictionary[type].ContainsKey(target))
                            {
                                typeTargetToModuleDictionary[type][target] = node.Module;
                            }
                        }
                        return node.Module;
                    }
                }
                else
                {
                    var baseType = moduleType.BaseType;
                    while ((null != baseType) && !baseType.IsInterface)
                    {
                        if ((baseType == type) && targetMatch)
                        {
                            // modules that create executables that are used as part of the build
                            // can query into the database from multiple use threads, so make it safe
                            lock (typeTargetToModuleDictionary)
                            {
                                if (!typeTargetToModuleDictionary.ContainsKey(type))
                                {
                                    typeTargetToModuleDictionary.Add(type, new System.Collections.Generic.Dictionary<Core.Target, IModule>());
                                }
                                if (!typeTargetToModuleDictionary[type].ContainsKey(target))
                                {
                                    typeTargetToModuleDictionary[type][target] = node.Module;
                                }
                            }
                            return node.Module;
                        }

                        baseType = baseType.BaseType;
                    }
                }
            }

            return null;
        }

        public static IModule
        GetModuleNoToolchain(
            System.Type type,
            Core.Target target)
        {
            // Intentionally NOT using typeTargetToModuleDictionary

            var graph = State.Get("System", "Graph") as DependencyGraph;
            if (null == graph)
            {
                throw new Exception("Dependency graph has not yet been constructed");
            }

            foreach (DependencyNode node in graph)
            {
                var moduleType = node.Module.GetType();
                var typeMatch = (moduleType == type);
                var targetMatch = (BaseTarget)node.Target == (BaseTarget)target;
                if (typeMatch)
                {
                    if (targetMatch)
                    {
                        return node.Module;
                    }
                }
                else
                {
                    var baseType = moduleType.BaseType;
                    while ((null != baseType) && !baseType.IsInterface)
                    {
                        if ((baseType == type) && targetMatch)
                        {
                            return node.Module;
                        }

                        baseType = baseType.BaseType;
                    }
                }
            }

            return null;
        }

        public static Array<System.Reflection.FieldInfo>
        GetSourceFilesFromModuleType(
            System.Type moduleType)
        {
            var sourceFilesFields = new Array<System.Reflection.FieldInfo>();

            var fieldInfoArray = moduleType.GetFields(System.Reflection.BindingFlags.NonPublic |
                                                      System.Reflection.BindingFlags.Public |
                                                      System.Reflection.BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfoArray)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(SourceFilesAttribute), false);
                if (attributes.Length > 0)
                {
                    sourceFilesFields.Add(fieldInfo);
                }
            }

            return sourceFilesFields;
        }
    }
}
