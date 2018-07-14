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
        protected BaseAttribute(
            string property)
        {
            this.Property = property;
        }

        public string Property
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
            string property
        )
            :
            base(property)
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
            string property
        )
            :
            base(property)
        {}
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PathArrayAttribute :
        BaseAttribute
    {
        public PathArrayAttribute(
            string property
        )
            :
            base(property)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class StringAttribute :
        BaseAttribute
    {
        public StringAttribute(
            string property
        )
            :
            base(property)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class StringArrayAttribute :
        BaseAttribute
    {
        public StringArrayAttribute(
            string property
        )
            :
            base(property)
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class BoolAttribute :
        BaseAttribute
    {
        public BoolAttribute(
            string property
        )
            :
            base(property)
        {}
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PreprocessorDefinesAttribute :
        BaseAttribute
    {
        public PreprocessorDefinesAttribute(
            string property)
            :
            base(property)
        { }
    }

    public static class XcodeConversion
    {
        public static void
        Convert(
            Bam.Core.Settings settings,
            System.Type real_settings_type,
            Bam.Core.Module module // cannot use settings.Module as this may be null for per-file settings
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
                        throw new Bam.Core.Exception(
                            "No XcodeProcessor attributes for property {0} in module {1}",
                            full_property_interface_name,
                            module.ToString()
                        );
                    }
                    if (attributeArray.First() is EnumAttribute)
                    {
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is PathAttribute)
                    {
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is PathArrayAttribute)
                    {
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is StringAttribute)
                    {
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is StringArrayAttribute)
                    {
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is BoolAttribute)
                    {
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is PreprocessorDefinesAttribute)
                    {
                        throw new System.NotImplementedException();
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
