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
namespace Bam.Core
{
    /// <summary>
    /// Abstract base class for all settings
    /// </summary>
    public abstract class Settings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected Settings()
            :
            this(ELayout.Unassigned)
        { }

        /// <summary>
        /// Construct an instance of the Settings.
        /// </summary>
        /// <param name="layout">Layout of the command line generated from the settings.</param>
        protected Settings(
            ELayout layout)
        {
            this.CommandLayout = layout;
            this._Properties = FindProperties(this.GetType());
        }

        private class InterfaceData
        {
            public System.Type interfaceType;
            public System.Reflection.MethodInfo defaultMethod;
        }

        private class SettingsInterfaces
        {
            public Bam.Core.Array<InterfaceData> Data = new Array<InterfaceData>();
        }

        // TODO: Could this be System.Collections.Concurrent.ConcurrentDictionary to avoid the explicit lock below?
        private static System.Collections.Generic.Dictionary<System.Type, SettingsInterfaces> Cache = new System.Collections.Generic.Dictionary<System.Type, SettingsInterfaces>();

        private Array<System.Reflection.PropertyInfo> _Properties;

        private static SettingsInterfaces
        GetSettingsInterfaces(
            System.Type settingsType,
            System.Collections.Generic.IEnumerable<System.Type> currentInterfaces)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(settingsType))
                {
                    return Cache[settingsType];
                }

                var attributeType = typeof(SettingsExtensionsAttribute);
                var interfaces = new SettingsInterfaces();
                foreach (var interfaceType in currentInterfaces)
                {
                    var attributeArray = interfaceType.GetCustomAttributes(attributeType, false);
                    if (0 == attributeArray.Length)
                    {
                        throw new Exception(
                            $"Settings interface {interfaceType.ToString()} is missing attribute {attributeType.ToString()}"
                        );
                    }

                    var attribute = attributeArray[0] as SettingsExtensionsAttribute;
                    // cannot replace Defaults extension methods with C# 6.0+ property initialisers easily
                    // since there is no centralised initialisation (a copy of each common property per C toolchain, for instance)
                    // and cannot inherit from a base class because each property has different attributes depending on the toolchain
                    var newInterface = new InterfaceData
                    {
                        interfaceType = interfaceType,
                        defaultMethod = attribute.GetMethod("Defaults", new[] { interfaceType, typeof(Module) })
                    };
                    interfaces.Data.Add(newInterface);
                }
                Cache.Add(settingsType, interfaces);

                return Cache[settingsType];
            }
        }

        /// <summary>
        /// Return each interface implemented, implementing ISettingsBase, and ordered by precedence with
        /// SettingsPrecedenceAttribute.
        /// </summary>
        public System.Collections.Generic.IEnumerable<System.Type>
        Interfaces()
        {
            // find true interfaces
            var baseI = typeof(ISettingsBase);
            // note that GetInterfaces does not return in a deterministic order
            // so use any precedence to order those that need to be (highest first)
            var interfaces = this.GetType().GetInterfaces().
                Where(item => (item != baseI) && baseI.IsAssignableFrom(item)).
                OrderByDescending(
                    item =>
                    {
                        var precedenceAttribs = item.GetCustomAttributes(typeof(SettingsPrecedenceAttribute), false);
                        if (precedenceAttribs.Length > 0)
                        {
                            return (precedenceAttribs[0] as SettingsPrecedenceAttribute).Order;
                        }
                        return 0;
                    });

            foreach (var i in interfaces)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Utility function to gather all the properties on a Settings
        /// class hierarchy.
        /// </summary>
        /// <param name="settingsType">Concrete Settings type to start from.</param>
        /// <returns>Array of PropertyInfo.</returns>
        public static Array<System.Reflection.PropertyInfo>
        FindProperties(
            System.Type settingsType)
        {
            // TODO: this does seem to find more properties than it needs to

            // since flattening the hierachy doesn't expose private property
            // implementations on base classes, recurse
            var properties = settingsType.GetProperties(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic
            );
            var props = new Bam.Core.Array<System.Reflection.PropertyInfo>(properties);
            var baseType = settingsType.BaseType;
            if (null == baseType)
            {
                return props;
            }
            props.AddRangeUnique(FindProperties(baseType));
            return props;
        }

        /// <summary>
        /// Read only access to the flattened list of properties for all interfaces
        /// on this Settings type, in the order of the interface priorities.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<System.Reflection.PropertyInfo> Properties => this._Properties.ToReadOnlyCollection();

        /// <summary>
        /// Get or set the Module associated with the Settings instance.
        /// </summary>
        /// <value>The module.</value>
        public Module Module { get; private set; }

        /// <summary>
        /// Perform validation on this Settings instance, after all patches have been
        /// applied.
        /// Allows any derived class to ensure it is self consistent.
        /// There is no default validation.
        /// </summary>
        public virtual void
        Validate()
        {}

        /// <summary>
        /// If settings are linearised to a command line, layout of where files and options commands are placed.
        /// </summary>
        public enum ELayout
        {
            /// <summary>
            /// File layout is unassigned - this is invalid.
            /// </summary>
            Unassigned,

            /// <summary>
            /// Command lines appear as:
            /// [non-path switches] [output paths] [input paths]
            /// </summary>
            Cmds_Outputs_Inputs,

            /// <summary>
            /// Command lines appear as:
            /// [non-path switches] [input paths] [output paths]
            /// </summary>
            Cmds_Inputs_Outputs,

            /// <summary>
            /// Command lines appear as:
            /// [input paths] [non-path switches] [output paths]
            /// </summary>
            Inputs_Cmds_Outputs,

            /// <summary>
            /// Command lines appear as:
            /// [input paths] [output paths] [non-path switches]
            /// </summary>
            Inputs_Outputs_Cmds
        }

        /// <summary>
        /// Access to the specified layout for commands.
        /// </summary>
        public ELayout CommandLayout { get; private set; }

        /// <summary>
        /// Defines the local policy to use for all settings
        /// </summary>
        /// <value>The local policy.</value>
        static public ISitePolicy LocalPolicy { get; set; }

        /// <summary>
        /// Hook into the Settings initialisation system to allow specific
        /// Settings-derived classes to change their defaults.
        /// This is executed after all of the Interfaces, but before the LocalPolicy.
        /// </summary>
        protected virtual void
        ModifyDefaults()
        {}

        /// <summary>
        /// For all interfaces in the settings, assign the default value to each property,
        /// and assign the module to the settings.
        /// For all settings interfaces, assign the default value via each interfaces' Default
        /// extension method defined by SettingsExtensionsAttribute on each interface.
        /// Also call this Settings class' ModifyDefaults() function
        /// which can be overridden in subclasses.
        /// Then call the LocalPolicy if it's been defined.
        /// </summary>
        /// <param name="module">Module that the Settings applies to</param>
        public void
        SetModuleAndDefaultPropertyValues(
            Module module)
        {
            this.AssignModule(module);

            var data = GetSettingsInterfaces(this.GetType(), this.Interfaces());
            foreach (var i in data.Data)
            {
                i.defaultMethod?.Invoke(null, new object[] { this, module });
            }
            ModifyDefaults();
            LocalPolicy?.DefineLocalSettings(this, module);
        }

        /// <summary>
        /// Assign the Module that owns these settings.
        /// </summary>
        /// <param name="module">Module that the Settings applies to.</param>
        public void
        AssignModule(
            Module module)
        {
            this.Module = module;
        }
    }
}
