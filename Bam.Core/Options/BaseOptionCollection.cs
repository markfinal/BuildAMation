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
using System.Linq;
namespace Bam.Core
{
namespace V2
{
    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public sealed class SettingsExtensionsAttribute : System.Attribute
    {
        public SettingsExtensionsAttribute(System.Type extensionClass)
        {
            this.ClassType = extensionClass;
        }

        public System.Type ClassType
        {
            get;
            private set;
        }

        public System.Reflection.MethodInfo
        GetMethod(
            string name,
            params System.Type[] inputs)
        {
            return this.ClassType.GetMethod(name, inputs);
        }
    }

    public interface ISettingsBase
    {
    }

    public interface ISitePolicy
    {
        void
        DefineLocalSettings(
            Settings settings,
            Module module);
    }

    /// <summary>
    /// Abstract base class for all settings
    /// </summary>
    public abstract class Settings
    {
        private static ISitePolicy LocalPolicy = null;

        static Settings()
        {
            var localPolicies = State.ScriptAssembly.GetTypes().Where(t => typeof(ISitePolicy).IsAssignableFrom(t));
            var numLocalPolicies = localPolicies.Count();
            if (numLocalPolicies > 0)
            {
                if (numLocalPolicies > 1)
                {
                    throw new Exception("Too many site policies exist in the package assembly");
                }

                LocalPolicy = System.Activator.CreateInstance(localPolicies.ElementAt(0)) as ISitePolicy;
            }
        }

        protected void
        InitializeAllInterfaces(
            Module module,
            bool emptyFirst,
            bool useDefaults)
        {
            var attributeType = typeof(SettingsExtensionsAttribute);
            var moduleType = typeof(Module);
            var baseI = typeof(ISettingsBase);
            foreach (var i in this.GetType().GetInterfaces())
            {
                // is it a true settings interface?
                if (!baseI.IsAssignableFrom(i))
                {
                    Log.DebugMessage("Ignored interface {0} on {1}", i.ToString(), this.GetType().ToString());
                    continue;
                }
                if (i == baseI)
                {
                    continue;
                }

                var attributeArray = i.GetCustomAttributes(attributeType, false);
                if (0 == attributeArray.Length)
                {
                    throw new Exception("Settings interface {0} is missing attribute {1}", i.ToString(), attributeType.ToString());
                }

                var attribute = attributeArray[0] as SettingsExtensionsAttribute;

                // TODO: the Empty function could be replaced by the auto-property initializers in C#6.0 (when Mono catches up)
                // although it won't then be a centralized definition, so the extension method as-is is probably better
                if (emptyFirst)
                {
                    var emptyMethod = attribute.GetMethod("Empty", new[] { i });
                    if (null != emptyMethod)
                    {
                        Log.DebugMessage("Executing {0}", emptyMethod.ToString());
                        emptyMethod.Invoke(null, new[] { this });
                    }
                    else
                    {
                        Log.DebugMessage("Unable to find method {0}.Empty({1})", attribute.ClassType.ToString(), i.ToString());
                    }
                }

                if (useDefaults)
                {
                    var defaultMethod = attribute.GetMethod("Defaults", new[] { i, moduleType });
                    if (null == defaultMethod)
                    {
                        throw new Exception("Unable to find method {0}.Defaults({1}, {2})", attribute.ClassType.ToString(), i.ToString(), moduleType.ToString());
                    }
                    Log.DebugMessage("Executing {0}", defaultMethod.ToString());
                    defaultMethod.Invoke(null, new object[] { this, module });

                }
            }

            // once all interfaces have been initialized, apply the site policy if one exists
            if (null != LocalPolicy && useDefaults)
            {
                LocalPolicy.DefineLocalSettings(this, module);
            }
        }
    }
}
}
