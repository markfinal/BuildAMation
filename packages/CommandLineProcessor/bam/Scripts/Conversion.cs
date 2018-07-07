#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace CommandLineProcessor
{
#if BAM_V2
    public abstract class BaseAttribute :
        System.Attribute
    {
        protected BaseAttribute(
            string command_switch)
        {
            this.CommandSwitch = command_switch;
        }

        public string CommandSwitch
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class EnumAttribute :
        BaseAttribute
    {
        public EnumAttribute(
            object key,
            string command_switch)
            :
            base(command_switch)
        {
            this.Key = key as System.Enum;
        }

        public System.Enum Key
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PathAttribute :
        BaseAttribute
    {
        public PathAttribute(
            string command_switch)
            :
            base(command_switch)
        {}
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PathArrayAttribute :
        BaseAttribute
    {
        public PathArrayAttribute(
            string command_switch)
            :
            base(command_switch)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class StringArrayAttribute :
        BaseAttribute
    {
        public StringArrayAttribute(
            string command_switch)
            :
            base(command_switch)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class BoolAttribute :
        BaseAttribute
    {
        public BoolAttribute(
            string true_command_switch,
            string false_command_switch)
            :
            base(true_command_switch)
        {
            this.FalseCommandSwitch = false_command_switch;
        }

        public string TrueCommandSwitch
        {
            get
            {
                return this.CommandSwitch;
            }
        }

        public string FalseCommandSwitch
        {
            get;
            private set;
        }
    }

    public static class NativeConversion
    {
        private static void
        HandleEnum(
            Bam.Core.StringArray commandLine,
            System.Reflection.PropertyInfo interfacePropertyInfo,
            System.Reflection.PropertyInfo propertyInfo,
            object[] attributeArray,
            object propertyValue)
        {
            if (!(typeof(System.Enum).IsAssignableFrom(propertyInfo.PropertyType) ||
                  (propertyInfo.PropertyType.IsGenericType &&
                   propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(System.Nullable<>)
                  )
                 )
               )
            {
                throw new Bam.Core.Exception(
                    "Attribute expected an enum (or nullable enum), but property {0} is of type {1}",
                    propertyInfo.Name,
                    propertyInfo.PropertyType.ToString()
                );
            }
            var matching_attribute = attributeArray.FirstOrDefault(
                item => (item as EnumAttribute).Key.Equals(propertyValue)
            ) as BaseAttribute;
            if (null == matching_attribute)
            {
                throw new Bam.Core.Exception(
                    "Unable to locate enumeration mapping for {0}",
                    interfacePropertyInfo.GetType().FullName
                );
            }
            commandLine.Add(matching_attribute.CommandSwitch);
        }

        private static void
        HandleSinglePath(
            Bam.Core.StringArray commandLine,
            System.Reflection.PropertyInfo interfacePropertyInfo,
            System.Reflection.PropertyInfo propertyInfo,
            object[] attributeArray,
            object propertyValue)
        {
            if (null == propertyValue)
            {
                return;
            }
            if (!typeof(Bam.Core.TokenizedString).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new Bam.Core.Exception(
                    "Attribute expected a Bam.Core.TokenizedString, but property is of type {0}",
                    propertyInfo.PropertyType.ToString()
                );
            }
            commandLine.Add(
                System.String.Format(
                    "{0}{1}",
                    (attributeArray.First() as BaseAttribute).CommandSwitch,
                    (propertyValue as Bam.Core.TokenizedString).ToStringQuoteIfNecessary()
                )
            );
        }

        private static void
        HandlePathArray(
            Bam.Core.StringArray commandLine,
            System.Reflection.PropertyInfo interfacePropertyInfo,
            System.Reflection.PropertyInfo propertyInfo,
            object[] attributeArray,
            object propertyValue)
        {
            if (!typeof(Bam.Core.TokenizedStringArray).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new Bam.Core.Exception(
                    "Attribute expected a Bam.Core.TokenizedStringArray, but property is of type {0}",
                    propertyInfo.PropertyType.ToString()
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            foreach (var path in (propertyValue as Bam.Core.TokenizedStringArray).ToEnumerableWithoutDuplicates())
            {
                // TODO: a special case is needed for this being requested in Xcode mode
                // which is done when there are overrides per source file
                commandLine.Add(
                    System.String.Format(
                        "{0}{1}",
                        command_switch,
                        path.ToStringQuoteIfNecessary()
                    )
                );
            }
        }

        private static void
        HandleStringArray(
            Bam.Core.StringArray commandLine,
            System.Reflection.PropertyInfo interfacePropertyInfo,
            System.Reflection.PropertyInfo propertyInfo,
            object[] attributeArray,
            object propertyValue)
        {
            if (!typeof(Bam.Core.StringArray).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new Bam.Core.Exception(
                    "Attribute expected a Bam.Core.StringArray, but property is of type {0}",
                    propertyInfo.PropertyType.ToString()
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            foreach (var str in (propertyValue as Bam.Core.StringArray))
            {
                // TODO: a special case is needed for this being requested in Xcode mode
                // which is done when there are overrides per source file
                commandLine.Add(
                    System.String.Format(
                        "{0}{1}",
                        command_switch,
                        str
                    )
                );
            }
        }
        private static void
        HandleBool(
            Bam.Core.StringArray commandLine,
            System.Reflection.PropertyInfo interfacePropertyInfo,
            System.Reflection.PropertyInfo propertyInfo,
            object[] attributeArray,
            object propertyValue)
        {
            if (!(typeof(bool).IsAssignableFrom(propertyInfo.PropertyType) ||
                  (propertyInfo.PropertyType.IsGenericType &&
                   propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(System.Nullable<>)
                  )
                 )
               )
            {
                throw new Bam.Core.Exception(
                    "Attribute expected an bool (or nullable bool), but property {0} is of type {1}",
                    propertyInfo.Name,
                    propertyInfo.PropertyType.ToString()
                );
            }
            bool value = (bool)propertyValue;
            var attr = attributeArray.First() as BoolAttribute;
            if (value)
            {
                var truth_command = attr.TrueCommandSwitch;
                if (!System.String.IsNullOrEmpty(truth_command))
                {
                    commandLine.Add(truth_command);
                }
            }
            else
            {
                var false_command = attr.FalseCommandSwitch;
                if (!System.String.IsNullOrEmpty(false_command))
                {
                    commandLine.Add(false_command);
                }
            }
        }

        public static Bam.Core.StringArray
        Convert(
            Bam.Core.Module module)
        {
            var commandLine = new Bam.Core.StringArray();
            //Bam.Core.Log.MessageAll("Module: {0}", module.ToString());
            //Bam.Core.Log.MessageAll("Settings: {0}", module.Settings.ToString());
            foreach (var settings_interface in module.Settings.Interfaces())
            {
                //Bam.Core.Log.MessageAll(settings_interface.ToString());
                foreach (var interface_property in settings_interface.GetProperties())
                {
                    var settings_property = module.Settings.Properties.First(item => item.Name.EndsWith(interface_property.Name));
                    //Bam.Core.Log.MessageAll("\t{0}", settings_property.ToString());
                    var attributeArray = settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
                        continue;
                    }
                    var property_value = settings_property.GetValue(module.Settings);
                    if (attributeArray.First() is EnumAttribute)
                    {
                        HandleEnum(
                            commandLine,
                            interface_property,
                            settings_property,
                            attributeArray,
                            property_value
                        );
                    }
                    else if (attributeArray.First() is PathAttribute)
                    {
                        HandleSinglePath(
                            commandLine,
                            interface_property,
                            settings_property,
                            attributeArray,
                            property_value
                        );
                    }
                    else if (attributeArray.First() is PathArrayAttribute)
                    {
                        HandlePathArray(
                            commandLine,
                            interface_property,
                            settings_property,
                            attributeArray,
                            property_value
                        );
                    }
                    else if (attributeArray.First() is StringArrayAttribute)
                    {
                        HandleStringArray(
                            commandLine,
                            interface_property,
                            settings_property,
                            attributeArray,
                            property_value
                        );
                    }
                    else if (attributeArray.First() is BoolAttribute)
                    {
                        HandleBool(
                            commandLine,
                            interface_property,
                            settings_property,
                            attributeArray,
                            property_value
                        );
                    }
                    else
                    {
                        throw new Bam.Core.Exception(
                            "Unhandled attribute {0} for property {1} in {2}",
                            attributeArray.First().ToString(),
                            settings_property.Name,
                            module.ToString()
                        );
                    }
                }
            }
            //Bam.Core.Log.MessageAll("{0}: Executing '{1}'", module.ToString(), commandLine.ToString(' '));
            return commandLine;
        }
    }
#endif

    public static class Conversion
    {
        public static void
        Convert(
            System.Type conversionClass,
            Bam.Core.Settings toolSettings,
            Bam.Core.StringArray commandLine)
        {
            var stringArrayType = typeof(Bam.Core.StringArray);
            foreach (var i in toolSettings.Interfaces())
            {
                var method = conversionClass.GetMethod("Convert", new[] { i, stringArrayType });
                if (null == method)
                {
                    throw new Bam.Core.Exception("Unable to locate method {0}.Convert({1}, {2})",
                        conversionClass.ToString(),
                        i.ToString(),
                        stringArrayType);
                }
                var commands = new Bam.Core.StringArray();
                try
                {
                    method.Invoke(null, new object[] { toolSettings, commands });
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    throw new Bam.Core.Exception(exception.InnerException, "Command line conversion error:");
                }
                commandLine.AddRange(commands);
            }
        }
    }
}
