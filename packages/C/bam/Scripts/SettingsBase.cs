#region License
// Copyright (c) 2010-2019, Mark Final
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
using System.Linq;

// this assembly attribute allows the dynamic assembly generated for shared and delta
// settings for IDE projects to see the internal classes of the master package assembly
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("IDESharedSettings")]

namespace C
{
    /// <summary>
    /// Base class for all concrete settings classes. This is tuned towards compilation settings
    /// which can be delta'd in project generation.
    /// </summary>
    abstract class SettingsBase :
        Bam.Core.Settings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected SettingsBase()
        { }

        /// <summary>
        /// Construct an instance with the given layout
        /// </summary>
        /// <param name="layout">Layout of switches, inputs and outputs</param>
        protected SettingsBase(
            Bam.Core.Settings.ELayout layout)
            :
            base(layout)
        { }

        /// <summary>
        /// Create a delta settings collection
        /// </summary>
        /// <param name="sharedSettings">Settings to compare</param>
        /// <param name="module">For this module</param>
        /// <returns>Delta settings</returns>
        public SettingsBase
        CreateDeltaSettings(
            Bam.Core.Settings sharedSettings,
            Bam.Core.Module module)
        {
            var attributeType = typeof(Bam.Core.SettingsExtensionsAttribute);

            var moduleSpecificSettings = System.Activator.CreateInstance(module.Settings.GetType()) as SettingsBase;
            moduleSpecificSettings.AssignModule(module);
            var sharedInterfaces = sharedSettings.Interfaces();
            foreach (var i in module.Settings.Interfaces())
            {
                var attributeArray = i.GetCustomAttributes(attributeType, false);
                if (0 == attributeArray.Length)
                {
                    throw new Bam.Core.Exception(
                        $"Settings interface {i.ToString()} is missing attribute {attributeType.ToString()}"
                    );
                }

                var attribute = attributeArray[0] as Bam.Core.SettingsExtensionsAttribute;

                // if we match any of the shared interfaces, get a delta
                // otherwise, just clone the interface
                if (sharedInterfaces.Any(item => item == i))
                {
                    var deltaMethod = attribute.GetMethod("Delta", new[] { i, i, i });
                    if (null != deltaMethod)
                    {
                        Bam.Core.Log.DebugMessage($"Executing {deltaMethod.Name}");
                        deltaMethod.Invoke(null, new[] { moduleSpecificSettings, this, sharedSettings });
                    }
                    else
                    {
                        throw new Bam.Core.Exception(
                            $"Unable to find method {attribute.ExtensionsClassName}.Delta(this {i.ToString()}, {i.ToString()}, {i.ToString()})"
                        );
                    }
                }
                else
                {
                    var cloneMethod = attribute.GetMethod("Clone", new[] { i, i });
                    if (null != cloneMethod)
                    {
                        Bam.Core.Log.DebugMessage($"Executing {cloneMethod.Name}");
                        cloneMethod.Invoke(null, new[] { moduleSpecificSettings, this });
                    }
                    else
                    {
                        throw new Bam.Core.Exception(
                            $"Unable to find method {attribute.ExtensionsClassName}.Clone(this {i.ToString()}, {i.ToString()})"
                        );
                    }
                }
            }

            return moduleSpecificSettings;
        }

        private static Bam.Core.TypeArray
        SharedInterfaces(
            System.Collections.Generic.IEnumerable<Bam.Core.Module> objectFiles)
        {
            System.Collections.Generic.IEnumerable<System.Type> sharedInterfaces = null;
            foreach (var input in objectFiles)
            {
                var interfaces = input.Settings.GetType().GetInterfaces().Where(item => (item != typeof(Bam.Core.ISettingsBase)) && typeof(Bam.Core.ISettingsBase).IsAssignableFrom(item));
                if (null == sharedInterfaces)
                {
                    sharedInterfaces = interfaces;
                }
                else
                {
                    sharedInterfaces = sharedInterfaces.Intersect(interfaces);
                }
            }
            return new Bam.Core.TypeArray(sharedInterfaces.OrderByDescending(item =>
                {
                    var precedenceAttribs = item.GetCustomAttributes(typeof(Bam.Core.SettingsPrecedenceAttribute), false);
                    if (precedenceAttribs.Length > 0)
                    {
                        return (precedenceAttribs[0] as Bam.Core.SettingsPrecedenceAttribute).Order;
                    }
                    return 0;
                }));
        }

        /// <summary>
        /// Create shared settings for all object files provided
        /// </summary>
        /// <param name="objectFiles">List of object files to consider</param>
        /// <returns>Settings shared by all object files</returns>
        public static SettingsBase
        SharedSettings(
            System.Collections.Generic.IEnumerable<Bam.Core.Module> objectFiles)
        {
            var sharedInterfaces = SharedInterfaces(objectFiles);
            var implementedInterfaces = new Bam.Core.TypeArray(sharedInterfaces);

            // define a new type, that contains just the shared interfaces between all object files
            // (any interface not shared, must be cloned later)
            var typeSignature = "IDESharedSettings";
            var assemblyName = new System.Reflection.AssemblyName(typeSignature);
            var assemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                System.Reflection.Emit.AssemblyBuilderAccess.Run
            );
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var sharedSettingsTypeDefn = moduleBuilder.DefineType(typeSignature,
                System.Reflection.TypeAttributes.Public |
                System.Reflection.TypeAttributes.Class |
                System.Reflection.TypeAttributes.AutoClass |
                System.Reflection.TypeAttributes.AnsiClass |
                System.Reflection.TypeAttributes.BeforeFieldInit |
                System.Reflection.TypeAttributes.AutoLayout,
                typeof(C.SettingsBase),
                implementedInterfaces.ToArray());

                // TODO: is this necessary?
#if false
            sharedSettingsTypeDefn.DefineDefaultConstructor(
                System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.RTSpecialName);
#endif

            // implement 'automatic property' setter and getters for each property in each interface
            foreach (var i in sharedInterfaces)
            {
                var properties = i.GetProperties();
                foreach (var prop in properties)
                {
                    var dynamicProperty = sharedSettingsTypeDefn.DefineProperty(
                        System.String.Join(".", new[] { i.FullName, prop.Name }),
                        System.Reflection.PropertyAttributes.None,
                        prop.PropertyType,
                        System.Type.EmptyTypes
                    );
                    var field = sharedSettingsTypeDefn.DefineField("m" + prop.Name,
                        prop.PropertyType,
                        System.Reflection.FieldAttributes.Private);
                    var methodAttrs = System.Reflection.MethodAttributes.Public |
                        System.Reflection.MethodAttributes.HideBySig |
                        System.Reflection.MethodAttributes.Virtual;
                    if (prop.IsSpecialName)
                    {
                        methodAttrs |= System.Reflection.MethodAttributes.SpecialName;
                    }
                    var getter = sharedSettingsTypeDefn.DefineMethod("get_" + prop.Name,
                        methodAttrs,
                        prop.PropertyType,
                        System.Type.EmptyTypes);
                    var setter = sharedSettingsTypeDefn.DefineMethod("set_" + prop.Name,
                        methodAttrs,
                        null,
                        new[] { prop.PropertyType });
                    var getIL = getter.GetILGenerator();
                    getIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                    getIL.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                    getIL.Emit(System.Reflection.Emit.OpCodes.Ret);
                    dynamicProperty.SetGetMethod(getter);
                    var setIL = setter.GetILGenerator();
                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                    setIL.Emit(System.Reflection.Emit.OpCodes.Stfld, field);
                    setIL.Emit(System.Reflection.Emit.OpCodes.Ret);
                    dynamicProperty.SetSetMethod(setter);
                }
            }

            var sharedSettingsType = sharedSettingsTypeDefn.CreateType();
            var attributeType = typeof(Bam.Core.SettingsExtensionsAttribute);

            // now that we have an instance of the shared settings type, calculate the values of the
            // individual settings across all object files for all shared interfaces
            var commonSettings = System.Activator.CreateInstance(sharedSettingsType) as SettingsBase;
            // note: commonSettings have a null Module
            foreach (var i in sharedInterfaces)
            {
                var attributeArray = i.GetCustomAttributes(attributeType, false);
                if (0 == attributeArray.Length)
                {
                    throw new Bam.Core.Exception(
                        $"Settings interface {i.ToString()} is missing attribute {attributeType.ToString()}"
                    );
                }

                var attribute = attributeArray[0] as Bam.Core.SettingsExtensionsAttribute;

                var cloneSettingsMethod = attribute.GetMethod("Clone", new[] { i, i });
                if (null == cloneSettingsMethod)
                {
                    throw new Bam.Core.Exception(
                        $"Unable to find extension method {attribute.ExtensionsClassName}.Clone(this {i.ToString()}, {i.ToString()}, {i.ToString()}, {i.ToString()})"
                    );
                }

                var intersectSettingsMethod = attribute.GetMethod("Intersect", new[] { i, i });
                if (null == intersectSettingsMethod)
                {
                    throw new Bam.Core.Exception(
                        $"Unable to find extension method {attribute.ExtensionsClassName}.Intersect(this {i.ToString()}, {i.ToString()})"
                    );
                }

                var objectFileCount = objectFiles.Count();
                cloneSettingsMethod.Invoke(null, new[] { commonSettings, objectFiles.First().Settings });
                for (int objIndex = 1; objIndex < objectFileCount; ++objIndex)
                {
                    intersectSettingsMethod.Invoke(null, new[] { commonSettings, objectFiles.ElementAt(objIndex).Settings });
                }
            }
            return commonSettings;
        }
    }
}
