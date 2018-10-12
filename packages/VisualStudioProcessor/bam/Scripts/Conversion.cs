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
namespace VisualStudioProcessor
{
    public static class VSSolutionConversion
    {
        public static void
        Convert(
            Bam.Core.Settings settings,
            System.Type real_settings_type,
            Bam.Core.Module module, // cannot use settings.Module as this may be null for per-file settings
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            VSSolutionBuilder.VSProjectConfiguration vsConfig,
            string condition = null)
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
                        item => full_property_interface_name.Equals(item.Name, System.StringComparison.Ordinal)
                    );
                    var property_value = value_settings_property.GetValue(settings);
                    if (null == property_value)
                    {
                        continue;
                    }
                    var attribute_settings_property = Bam.Core.Settings.FindProperties(real_settings_type).First(
                        item => full_property_interface_name.Equals(item.Name, System.StringComparison.Ordinal)
                    );
                    var attributeArray = attribute_settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
                        throw new Bam.Core.Exception(
                            "No VisualStudioProcessor attributes for property {0} in module {1}",
                            full_property_interface_name,
                            module.ToString()
                        );
                    }
                    if (attributeArray.First() is EnumAttribute)
                    {
                        var associated_attribute = attributeArray.First(
                            item => (item as EnumAttribute).Key.Equals(property_value)) as EnumAttribute;
                        if (associated_attribute.Target == BaseAttribute.TargetGroup.Settings)
                        {
                            System.Diagnostics.Debug.Assert(!associated_attribute.InheritExisting);
                            switch (associated_attribute.Mode)
                            {
                                case EnumAttribute.EMode.AsString:
                                    vsSettingsGroup.AddSetting(
                                        associated_attribute.Property,
                                        property_value.ToString(),
                                        condition: condition
                                    );
                                    break;

                                case EnumAttribute.EMode.AsInteger:
                                    vsSettingsGroup.AddSetting(
                                        associated_attribute.Property,
                                        ((int)property_value).ToString("D"),
                                        condition: condition
                                    );
                                    break;

                                case EnumAttribute.EMode.AsIntegerWithPrefix:
                                    vsSettingsGroup.AddSetting(
                                        associated_attribute.Property,
                                        System.String.Format(
                                            "{0}{1}",
                                            associated_attribute.Prefix,
                                            ((int)property_value).ToString("D")
                                        ),
                                        condition: condition
                                    );
                                    break;

                                case EnumAttribute.EMode.VerbatimString:
                                    vsSettingsGroup.AddSetting(
                                        associated_attribute.Property,
                                        associated_attribute.VerbatimString,
                                        condition: condition
                                    );
                                    break;

                                case EnumAttribute.EMode.Empty:
                                    vsSettingsGroup.AddSetting(
                                        associated_attribute.Property,
                                        "",
                                        condition: condition
                                    );
                                    break;

                                case EnumAttribute.EMode.NoOp:
                                    break;

                                default:
                                    throw new Bam.Core.Exception(
                                        "Unhandled enum mode, {0}",
                                        associated_attribute.Mode.ToString()
                                    );
                            }
                        }
                        else if (associated_attribute.Target == BaseAttribute.TargetGroup.Configuration)
                        {
                            var prop = vsConfig.GetType().GetProperty(associated_attribute.Property);
                            var setter = prop.GetSetMethod();
                            switch (associated_attribute.Mode)
                            {
                                case EnumAttribute.EMode.PassThrough:
                                    setter.Invoke(vsConfig, new[] { property_value });
                                    break;

                                default:
                                    throw new Bam.Core.Exception(
                                        "Unhandled enum mode, {0}",
                                        associated_attribute.Mode.ToString()
                                    );
                            }
                        }
                    }
                    else if (attributeArray.First() is PathAttribute)
                    {
                        var associated_attribute = attributeArray.First() as PathAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }

                        if (associated_attribute.Ignored)
                        {
                            continue;
                        }

                        // TODO: this will add an absolute path
                        // not sure how to set that it's relative to a VS macro
                        vsSettingsGroup.AddSetting(
                            associated_attribute.Property,
                            property_value as Bam.Core.TokenizedString,
                            condition: condition,
                            isPath: true
                        );

                        var prop = vsConfig.GetType().GetProperty(associated_attribute.Property);
                        if (null != prop)
                        {
                            var setter = prop.GetSetMethod();
                            if (null != setter)
                            {
                                setter.Invoke(vsConfig, new[] { property_value });
                            }
                        }

                        if (associated_attribute.BoolPropertyWhenValid != null)
                        {
                            vsSettingsGroup.AddSetting(
                                associated_attribute.BoolPropertyWhenValid,
                                true,
                                condition: condition
                            );
                        }
                    }
                    else if (attributeArray.First() is PathArrayAttribute)
                    {
                        var associated_attribute = attributeArray.First() as BaseAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }
                        vsSettingsGroup.AddSetting(
                            associated_attribute.Property,
                            property_value as Bam.Core.TokenizedStringArray,
                            condition: condition,
                            inheritExisting: associated_attribute.InheritExisting,
                            arePaths: true
                        );
                    }
                    else if (attributeArray.First() is StringAttribute)
                    {
                        throw new System.NotImplementedException();
                    }
                    else if (attributeArray.First() is StringArrayAttribute)
                    {
                        var associated_attribute = attributeArray.First() as BaseAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }
                        vsSettingsGroup.AddSetting(
                            associated_attribute.Property,
                            property_value as Bam.Core.StringArray,
                            condition: condition,
                            inheritExisting: associated_attribute.InheritExisting
                        );
                    }
                    else if (attributeArray.First() is BoolAttribute)
                    {
                        var value = (bool)property_value;
                        var associated_attribute = attributeArray.First() as BoolAttribute;
                        if (associated_attribute.Inverted)
                        {
                            value = !value;
                        }
                        if (associated_attribute.Target == BaseAttribute.TargetGroup.Settings)
                        {
                            if (null == associated_attribute.Truth && null == associated_attribute.Falisy)
                            {
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    value,
                                    condition: condition
                                );
                            }
                            else
                            {
                                vsSettingsGroup.AddSetting(
                                    associated_attribute.Property,
                                    value ? associated_attribute.Truth : associated_attribute.Falisy,
                                    condition: condition
                                );
                            }
                        }
                        else if (associated_attribute.Target == BaseAttribute.TargetGroup.Configuration)
                        {
                            var prop = vsConfig.GetType().GetProperty(associated_attribute.Property);
                            var setter = prop.GetSetMethod();
                            if (null == associated_attribute.Truth && null == associated_attribute.Falisy)
                            {
                                setter.Invoke(
                                    vsConfig,
                                    new[] { property_value }
                                );
                            }
                            else
                            {
                                setter.Invoke(
                                    vsConfig,
                                    new[] { value ? associated_attribute.Truth : associated_attribute.Falisy }
                                );
                            }
                        }
                    }
                    else if (attributeArray.First() is PreprocessorDefinesAttribute)
                    {
                        var associated_attribute = attributeArray.First() as BaseAttribute;
                        if (associated_attribute.Target != BaseAttribute.TargetGroup.Settings)
                        {
                            throw new Bam.Core.Exception(
                                "Unable to use property target, {0}",
                                associated_attribute.Target.ToString()
                            );
                        }
                        vsSettingsGroup.AddSetting(
                            associated_attribute.Property,
                            property_value as C.PreprocessorDefinitions,
                            condition: condition,
                            inheritExisting: associated_attribute.InheritExisting
                        );
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

            var output_file_attributes = real_settings_type.GetCustomAttributes(
                typeof(OutputPathAttribute),
                true // since generally specified in an abstract class
            ) as OutputPathAttribute[];
            if (!output_file_attributes.Any() && settings.Module.GeneratedPaths.Any())
            {
                throw new Bam.Core.Exception(
                    "There are no OutputPath attributes associated with the {0} settings class",
                    settings.ToString()
                );
            }
            var input_files_attributes = real_settings_type.GetCustomAttributes(
                typeof(InputPathsAttribute),
                true // since generally specified in an abstract class
            ) as InputPathsAttribute[];
            if (settings.Module.InputModules.Any())
            {
                if (!input_files_attributes.Any())
                {
                    throw new Bam.Core.Exception(
                        "There is no InputPaths attribute associated with the {0} settings class",
                        settings.ToString()
                    );
                }
                var attr = input_files_attributes.First();
                var max_files = attr.MaxFileCount;
                if (max_files >= 0)
                {
                    if (max_files != settings.Module.InputModules.Count())
                    {
                        throw new Bam.Core.Exception(
                            "InputPaths attribute specifies a maximum of {0} files, but {1} are available",
                            max_files,
                            settings.Module.InputModules.Count()
                        );
                    }
                }
            }
            if (real_settings_type == settings.GetType())
            {
                // only process outputs when NOT in a shared settings (compiled) object
                // these will be specified on the per-compiled object, rather than
                // the shared state
                ProcessOutputPaths(
                    settings,
                    module,
                    output_file_attributes,
                    vsConfig,
                    vsSettingsGroup,
                    condition
                );
            }

            ProcessInputPaths(
                settings,
                input_files_attributes,
                vsSettingsGroup,
                condition
            );
        }

        private static void
        ProcessInputPaths(
            Bam.Core.Settings settings,
            InputPathsAttribute[] input_files_attributes,
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition)
        {
            var matching_input_attr = input_files_attributes.First();
            if (matching_input_attr.HandledByMetaData)
            {
                // this input file has been dealt with in the metdata for Visual
                // Studio projects already
                return;
            }
            foreach (var input_module in settings.Module.InputModules.Select(item => item.Value))
            {
                try
                {
                    // note that this will add an absolute path
                    // it will be updated later using VS macros
                    vsSettingsGroup.AddSetting(
                        matching_input_attr.Property,
                        input_module.GeneratedPaths[matching_input_attr.Property],
                        condition: condition,
                        isPath: true
                    );
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    throw new Bam.Core.Exception(
                        "Unable to locate path key {0} for input module of type {1}",
                        matching_input_attr.PathKey,
                        input_module.ToString()
                    );
                }
            }
        }

        private static void
        ProcessOutputPaths(
            Bam.Core.Settings settings,
            Bam.Core.Module module,
            OutputPathAttribute[] output_file_attributes,
            VSSolutionBuilder.VSProjectConfiguration vsConfig,
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition)
        {
            foreach (var generatedPath in settings.Module.GeneratedPaths)
            {
                if (null == generatedPath.Value)
                {
                    continue;
                }
                var outputKey = generatedPath.Key;
                var matching_attr = output_file_attributes.FirstOrDefault(item => item.PathKey.Equals(outputKey, System.StringComparison.Ordinal));
                if (null == matching_attr)
                {
                    throw new Bam.Core.Exception(
                        "Unable to locate OutputPath class attribute on {0} for path key {1}",
                        settings.ToString(),
                        outputKey
                    );
                }
                if (matching_attr.HandledByMetaData)
                {
                    // this input file has been dealt with in the metdata for Visual
                    // Studio projects already
                    continue;
                }
                // note that this will add an absolute path
                // it will be updated later using VS macros
                vsSettingsGroup.AddSetting(
                    matching_attr.Property,
                    generatedPath.Value,
                    condition: condition,
                    isPath: true
                );
                if (matching_attr.EnableSideEffects)
                {
                    // there may be a property of the same name on the VS configuration
                    // if there is, set it with the path
                    var prop = vsConfig.GetType().GetProperty(matching_attr.Property);
                    if (null == prop)
                    {
                        throw new Bam.Core.Exception(
                            "A side-effect was enabled, but no property called {0} exists on the VSConfiguration",
                            matching_attr.Property
                        );
                    }
                    var setter = prop.GetSetMethod();
                    if (null == setter)
                    {
                        throw new Bam.Core.Exception(
                            "No setter was available on property {0} on the VSConfiguration",
                            matching_attr.Property
                        );
                    }
                    setter.Invoke(vsConfig, new[] { generatedPath.Value });
                }
            }
        }
    }
}
