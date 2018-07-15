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
#if BAM_V2
using System.Linq;
#endif
namespace XcodeProjectProcessor
{
#if BAM_V2
    public abstract class BaseAttribute :
        System.Attribute
    {
        public enum ValueType
        {
            Unique,
            MultiValued
        }

        protected BaseAttribute(
            string property,
            ValueType type,
            bool ignore = false)
        {
            this.Property = property;
            this.Type = type;
            this.Ignore = ignore;
        }

        public string Property
        {
            get;
            private set;
        }

        public ValueType Type
        {
            get;
            private set;
        }

        public bool Ignore
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public abstract class EnumAttribute :
        BaseAttribute
    {
        protected EnumAttribute(
            object key,
            string property,
            string value,
            ValueType type,
            bool ignore = false
        )
            :
            base(property, type, ignore)
        {
            this.Key = key as System.Enum;
            this.Value = value;
        }

        public System.Enum Key
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }
    }

    public sealed class UniqueEnumAttribute :
        EnumAttribute
    {
        public UniqueEnumAttribute(
            object key,
            string property,
            string value,
            bool ignore = false
        )
            :
            base(key, property, value, ValueType.Unique, ignore: ignore)
        { }

        public UniqueEnumAttribute(
            object key,
            string property,
            string value,
            string property2,
            string value2
        )
            :
            this(key, property, value)
        {
            this.Property2 = property2;
            this.Value2 = value2;
        }

        public string Property2
        {
            get;
            private set;
        }

        public string Value2
        {
            get;
            private set;
        }
    }

    public sealed class MultiEnumAttribute :
        EnumAttribute
    {
        public MultiEnumAttribute(
            object key,
            string property,
            string value,
            bool ignore = false
        )
            :
            base(key, property, value, ValueType.MultiValued, ignore: ignore)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PathAttribute :
        BaseAttribute
    {
        public PathAttribute(
            string property,
            bool ignore = false
        )
            :
            base(property, ValueType.Unique, ignore: ignore)
        {}
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PathArrayAttribute :
        BaseAttribute
    {
        public PathArrayAttribute(
            string property,
            bool ignore = false
        )
            :
            base(property, ValueType.MultiValued, ignore: ignore)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class StringAttribute :
        BaseAttribute
    {
        public StringAttribute(
            string property,
            bool ignore = false
        )
            :
            base(property, ValueType.Unique, ignore: ignore)
        {}
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class StringArrayAttribute :
        BaseAttribute
    {
        public StringArrayAttribute(
            string property,
            string prefix = null,
            bool spacesSeparate = false,
            bool ignore = false
        )
            :
            base(property, ValueType.MultiValued, ignore: ignore)
        {
            this.Prefix = prefix;
            this.SpacesSeparate = spacesSeparate;
        }

        public string Prefix
        {
            get;
            private set;
        }

        // any extra arguments separated by spaces splits them onto separate lines in the project file
        public bool SpacesSeparate
        {
            get;
            private set;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public abstract class BoolAttribute :
        BaseAttribute
    {
        protected BoolAttribute(
            string property,
            string truth_value,
            string false_value,
            BaseAttribute.ValueType type,
            bool ignore = false
        )
            :
            base(property, type, ignore: ignore)
        {
            this.Truth = truth_value;
            this.Falisy = false_value;
        }

        public string Truth
        {
            get;
            private set;
        }

        public string Falisy
        {
            get;
            private set;
        }
    }

    public sealed class UniqueBoolAttribute :
        BoolAttribute
    {
        public UniqueBoolAttribute(
            string property,
            string truth_value,
            string false_value,
            bool ignore = false
        )
            :
            base(property, truth_value, false_value, ValueType.Unique, ignore: ignore)
        {}
    }

    public sealed class MultiBoolAttribute :
        BoolAttribute
    {
        public MultiBoolAttribute(
            string property,
            string truth_value,
            string false_value,
            bool ignore = false
        )
            :
            base(property, truth_value, false_value, ValueType.MultiValued, ignore: ignore)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PreprocessorDefinesAttribute :
        BaseAttribute
    {
        public PreprocessorDefinesAttribute(
            string property,
            bool ignore = false
        )
            :
            base(property, ValueType.MultiValued, ignore: ignore)
        { }
    }

    public static class XcodeConversion
    {
        public static void
        Convert(
            Bam.Core.Settings settings,
            System.Type real_settings_type,
            Bam.Core.Module module, // cannot use settings.Module as this may be null for per-file settings
            XcodeBuilder.Configuration configuration
        )
        {
            foreach (var settings_interface in settings.Interfaces())
            {
                foreach (var interface_property in settings_interface.GetProperties())
                {
                    // must use the fully qualified property name from the originating interface
                    // to look for the instance in the concrete settings class
                    // this is to allow for the same property leafname to appear in multiple interfaces
                    var full_property_interface_name = System.String.Join(
                        ".",
                        new[] { interface_property.DeclaringType.FullName, interface_property.Name }
                    );
                    var value_settings_property = settings.Properties.First(
                        item => full_property_interface_name == item.Name
                    );
                    var property_value = value_settings_property.GetValue(settings);
                    if (null == property_value)
                    {
                        continue;
                    }
                    var attribute_settings_property = Bam.Core.Settings.FindProperties(real_settings_type).First(
                        item => full_property_interface_name == item.Name
                    );
                    var attributeArray = attribute_settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
#if true
                        Bam.Core.Log.MessageAll("MISSING {0}", full_property_interface_name);
                        continue;
#else
                        throw new Bam.Core.Exception(
                            "No XcodeProcessor attributes for property {0} in module {1}",
                            full_property_interface_name,
                            module.ToString()
                        );
#endif
                    }
                    if (attributeArray.First() is EnumAttribute)
                    {
                        var associated_attribute = attributeArray.First(
                            item => (item as EnumAttribute).Key.Equals(property_value)) as EnumAttribute;
                        if (associated_attribute.Ignore)
                        {
                            continue;
                        }
                        switch (associated_attribute.Type)
                        {
                            case BaseAttribute.ValueType.Unique:
                                {
                                    var new_config_value = new XcodeBuilder.UniqueConfigurationValue(associated_attribute.Value);
                                    configuration[associated_attribute.Property] = new_config_value;

                                    var unique_associated_attr = (associated_attribute as UniqueEnumAttribute);
                                    if (unique_associated_attr.Property2 != null)
                                    {
                                        var new_config_value2 = new XcodeBuilder.UniqueConfigurationValue(unique_associated_attr.Value2);
                                        configuration[unique_associated_attr.Property2] = new_config_value2;
                                    }
                                }
                                break;

                            case BaseAttribute.ValueType.MultiValued:
                                {
                                    var new_config_value = new XcodeBuilder.MultiConfigurationValue(associated_attribute.Value);
                                    configuration[associated_attribute.Property] = new_config_value;
                                }
                                break;

                            default:
                                throw new Bam.Core.Exception(
                                    "Unknown Xcode configuration value type, {0}",
                                    associated_attribute.Type.ToString()
                                );
                        }
                    }
                    else if (attributeArray.First() is PathAttribute)
                    {
                        var associated_attr = attributeArray.First() as PathAttribute;
                        if (associated_attr.Ignore)
                        {
                            continue;
                        }
                        var path = new XcodeBuilder.UniqueConfigurationValue((property_value as Bam.Core.TokenizedString).ToString());
                        configuration[associated_attr.Property] = path;
                    }
                    else if (attributeArray.First() is PathArrayAttribute)
                    {
                        var associated_attr = attributeArray.First() as PathArrayAttribute;
                        if (associated_attr.Ignore)
                        {
                            continue;
                        }
                        var paths = new XcodeBuilder.MultiConfigurationValue();
                        foreach (var path in (property_value as Bam.Core.TokenizedStringArray).ToEnumerableWithoutDuplicates())
                        {
                            var fullPath = path.ToString();
                            var relPath = Bam.Core.RelativePathUtilities.GetPath(fullPath, configuration.Project.SourceRoot);
                            // spaces need to be double escaped
                            if (Bam.Core.RelativePathUtilities.IsPathAbsolute(relPath))
                            {
                                if (fullPath.Contains(" "))
                                {
                                    fullPath = fullPath.Replace(" ", "\\\\ ");
                                }
                                paths.Add(fullPath);
                            }
                            else
                            {
                                if (relPath.Contains(" "))
                                {
                                    relPath = relPath.Replace(" ", "\\\\ ");
                                }
                                paths.Add(System.String.Format("$(SRCROOT)/{0}", relPath));
                            }
                        }
                        configuration[associated_attr.Property] = paths;
                    }
                    else if (attributeArray.First() is StringAttribute)
                    {
                        var associated_attr = attributeArray.First() as StringAttribute;
                        if (associated_attr.Ignore)
                        {
                            continue;
                        }
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is StringArrayAttribute)
                    {
                        var associated_attr = attributeArray.First() as StringArrayAttribute;
                        if (associated_attr.Ignore)
                        {
                            continue;
                        }
                        var values = new XcodeBuilder.MultiConfigurationValue();
                        var prefix = (associated_attr.Prefix != null) ? associated_attr.Prefix : System.String.Empty;
                        foreach (var item in property_value as Bam.Core.StringArray)
                        {
                            if (associated_attr.SpacesSeparate)
                            {
                                foreach (var split_item in item.Split(' '))
                                {
                                    values.Add(System.String.Format("{0}{1}", prefix, item));
                                }
                            }
                            else
                            {
                                values.Add(System.String.Format("{0}{1}", prefix, item));
                            }
                        }
                        configuration[associated_attr.Property] = values;
                    }
                    else if (attributeArray.First() is BoolAttribute)
                    {
                        var associated_attr = attributeArray.First() as BoolAttribute;
                        if (associated_attr.Ignore)
                        {
                            continue;
                        }
                        var real_value = (bool)property_value;
                        if (associated_attr.Type == BaseAttribute.ValueType.MultiValued)
                        {
                            var new_config_value = new XcodeBuilder.MultiConfigurationValue();
                            if (real_value)
                            {
                                new_config_value.Add(associated_attr.Truth);
                            }
                            else
                            {
                                new_config_value.Add(associated_attr.Falisy);
                            }
                            configuration[associated_attr.Property] = new_config_value;
                        }
                        else if (associated_attr.Type == BaseAttribute.ValueType.Unique)
                        {
                            var new_config_value = new XcodeBuilder.UniqueConfigurationValue(
                                real_value ?
                                associated_attr.Truth :
                                associated_attr.Falisy
                            );
                            configuration[associated_attr.Property] = new_config_value;
                        }
                        else
                        {
                            throw new Bam.Core.Exception("Unrecognised value type, {0}", associated_attr.Type.ToString());
                        }
                    }
                    else if (attributeArray.First() is PreprocessorDefinesAttribute)
                    {
                        var associated_attr = attributeArray.First() as PreprocessorDefinesAttribute;
                        if (associated_attr.Ignore)
                        {
                            continue;
                        }
                        var defines = new XcodeBuilder.MultiConfigurationValue();
                        foreach (var define in property_value as C.PreprocessorDefinitions)
                        {
                            if (null == define.Value)
                            {
                                defines.Add(define.Key);
                            }
                            else
                            {
                                var defineValue = define.Value.ToString();
                                if (defineValue.Contains(" "))
                                {
                                    defineValue = defineValue.Replace(" ", "\\\\ ");
                                }
                                if (defineValue.Contains("\""))
                                {
                                    // note the number of back slashes here
                                    // required to get \\\" for each " in the original value
                                    defineValue = defineValue.Replace("\"", "\\\\\\\"");
                                }
                                defines.Add(System.String.Format("{0}={1}", define.Key, defineValue));
                            }
                        }
                        configuration[associated_attr.Property] = defines;
                    }
                    else
                    {
                        throw new Bam.Core.Exception(
                            "Unhandled attribute {0} for property {1} in {2}",
                            attributeArray.First().ToString(),
                            attribute_settings_property.Name,
                            module.ToString()
                        );
                    }
                }
            }
        }
    }
#endif

    public static class Conversion
    {
        public static void
        Convert(
            System.Type conversionClass,
            Bam.Core.Settings toolSettings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            var moduleType = typeof(Bam.Core.Module);
            var xcodeConfigurationType = typeof(XcodeBuilder.Configuration);
            foreach (var i in toolSettings.Interfaces())
            {
                var method = conversionClass.GetMethod("Convert", new[] { i, moduleType, xcodeConfigurationType });
                if (null == method)
                {
                    throw new Bam.Core.Exception("Unable to locate method {0}.Convert({1}, {2}, {3})",
                        conversionClass.ToString(),
                        i.ToString(),
                        moduleType,
                        xcodeConfigurationType);
                }
                try
                {
                    method.Invoke(null, new object[] { toolSettings, module, configuration });
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    throw new Bam.Core.Exception(exception.InnerException, "Xcode conversion error:");
                }
            }
        }
    }
}
