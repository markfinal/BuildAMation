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
            if (null == propertyValue)
            {
                return;
            }
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
                    "Unable to locate enumeration mapping of '{0}.{1}' for property {2}.{3}",
                    propertyValue.GetType().ToString(),
                    propertyValue.ToString(),
                    interfacePropertyInfo.DeclaringType.FullName,
                    interfacePropertyInfo.Name
                );
            }
            var cmd = matching_attribute.CommandSwitch;
            if (!System.String.IsNullOrEmpty(cmd))
            {
                commandLine.Add(cmd);
            }
        }

        private static void
        HandleSinglePath(
            Bam.Core.Module module,
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
                if (!typeof(string).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    throw new Bam.Core.Exception(
                        "Attribute expected either a Bam.Core.TokenizedString or string, but property {0} is of type {1}",
                        propertyInfo.Name,
                        propertyInfo.PropertyType.ToString()
                    );
                }
            }
            if (typeof(Bam.Core.TokenizedString).IsAssignableFrom(propertyInfo.PropertyType))
            {
                commandLine.Add(
                    System.String.Format(
                        "{0}{1}",
                        (attributeArray.First() as BaseAttribute).CommandSwitch,
                        (propertyValue as Bam.Core.TokenizedString).ToStringQuoteIfNecessary()
                    )
                );
            }
            else
            {
                var path = module.GeneratedPaths[propertyValue as string];
                commandLine.Add(
                    System.String.Format(
                        "{0}{1}",
                        (attributeArray.First() as BaseAttribute).CommandSwitch,
                        path.ToStringQuoteIfNecessary()
                    )
                );
            }
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
                    "Attribute expected a Bam.Core.TokenizedStringArray, but property {0} is of type {1}",
                    propertyInfo.Name,
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
        HandleFrameworkArray(
            Bam.Core.StringArray commandLine,
            System.Reflection.PropertyInfo interfacePropertyInfo,
            System.Reflection.PropertyInfo propertyInfo,
            object[] attributeArray,
            object propertyValue)
        {
            if (!typeof(Bam.Core.TokenizedStringArray).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new Bam.Core.Exception(
                    "Attribute expected a Bam.Core.TokenizedStringArray, but property {0} is of type {1}",
                    propertyInfo.Name,
                    propertyInfo.PropertyType.ToString()
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            foreach (var path in (propertyValue as Bam.Core.TokenizedStringArray).ToEnumerableWithoutDuplicates())
            {
                commandLine.Add(
                    System.String.Format(
                        "{0}{1}",
                        command_switch,
                        System.IO.Path.GetFileNameWithoutExtension(path.ToStringQuoteIfNecessary())
                    )
                );
            }
        }

        private static void
        HandleSingleString(
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
            if (!typeof(string).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new Bam.Core.Exception(
                    "Attribute expected a string, but property {0} is of type {1}",
                    propertyInfo.Name,
                    propertyInfo.PropertyType.ToString()
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            commandLine.Add(
                System.String.Format(
                    "{0}{1}",
                    command_switch,
                    propertyValue as string
                )
            );
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
                    "Attribute expected a Bam.Core.StringArray, but property {0} is of type {1}",
                    propertyInfo.Name,
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

        private static void
        HandlePreprocessorDefines(
            Bam.Core.StringArray commandLine,
            System.Reflection.PropertyInfo interfacePropertyInfo,
            System.Reflection.PropertyInfo propertyInfo,
            object[] attributeArray,
            object propertyValue)
        {
            if (!typeof(C.PreprocessorDefinitions).IsAssignableFrom(propertyInfo.PropertyType))
            {
                throw new Bam.Core.Exception(
                    "Attribute expected a C.PreprocessorDefinitions, but property {0} is of type {1}",
                    propertyInfo.Name,
                    propertyInfo.PropertyType.ToString()
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            foreach (var define in (propertyValue as C.PreprocessorDefinitions))
            {
                if (null == define.Value)
                {
                    commandLine.Add(System.String.Format("-D{0}", define.Key));
                }
                else
                {
                    var defineValue = define.Value.ToString();
                    if (defineValue.Contains("\""))
                    {
                        if (Bam.Core.Graph.Instance.Mode.Equals("Xcode", System.StringComparison.Ordinal))
                        {
                            // note the number of back slashes here
                            // required to get \\\" for each " in the original value
                            defineValue = defineValue.Replace("\"", "\\\\\\\"");
                        }
                        else
                        {
                            defineValue = defineValue.Replace("\"", "\\\"");
                        }
                    }
                    defineValue = Bam.Core.IOWrapper.EncloseSpaceContainingPathWithDoubleQuotes(defineValue);
                    commandLine.Add(System.String.Format("-D{0}={1}", define.Key, defineValue));
                }
            }
        }

        public static Bam.Core.StringArray
        Convert(
            Bam.Core.Settings settings,
            Bam.Core.Module module,
            bool createDelta = false)
        {
            if (null == settings)
            {
                return new Bam.Core.StringArray();
            }
            if (settings.FileLayout == Bam.Core.Settings.ELayout.Unassigned)
            {
                throw new Bam.Core.Exception(
                    "File layout for {0} settings is unassigned. Override AssignFileLayout in this class.",
                    settings.ToString()
                );
            }
            var commandLine = new Bam.Core.StringArray();
            //Bam.Core.Log.MessageAll("Module: {0}", module.ToString());
            //Bam.Core.Log.MessageAll("Settings: {0}", settings.ToString());
            foreach (var settings_interface in settings.Interfaces())
            {
                //Bam.Core.Log.MessageAll(settings_interface.ToString());
                foreach (var interface_property in settings_interface.GetProperties())
                {
                    // must use the fully qualified property name from the originating interface
                    // to look for the instance in the concrete settings class
                    // this is to allow for the same property leafname to appear in multiple interfaces
                    var full_property_interface_name = System.String.Join(".", new[] { interface_property.DeclaringType.FullName, interface_property.Name });
                    var settings_property = settings.Properties.First(
                        item => full_property_interface_name.Equals(item.Name, System.StringComparison.Ordinal)
                    );
                    //Bam.Core.Log.MessageAll("\t{0}", settings_property.ToString());
                    var attributeArray = settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
                        throw new Bam.Core.Exception(
                            "No attributes available for mapping property {0} to command line switches for module {1} and settings {2}",
                            full_property_interface_name,
                            module.ToString(),
                            settings.ToString()
                        );
                    }
                    var property_value = settings_property.GetValue(settings);
                    if (null == property_value)
                    {
                        continue;
                    }
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
                            module,
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
                    else if (attributeArray.First() is FrameworkArrayAttribute)
                    {
                        HandleFrameworkArray(
                            commandLine,
                            interface_property,
                            settings_property,
                            attributeArray,
                            property_value
                        );
                    }
                    else if (attributeArray.First() is StringAttribute)
                    {
                        HandleSingleString(
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
                    else if (attributeArray.First() is PreprocessorDefinesAttribute)
                    {
                        HandlePreprocessorDefines(
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

            if (createDelta)
            {
                // don't want to introduce outputs or inputs in deltas
                return commandLine;
            }

            if (null == settings.Module)
            {
                throw new Bam.Core.Exception(
                    "No Module was associated with the settings class {0}",
                    settings.ToString()
                );
            }

            var output_file_attributes = settings.GetType().GetCustomAttributes(
                typeof(OutputPathAttribute),
                true // since generally specified in an abstract class
            ) as OutputPathAttribute[];
            if (!output_file_attributes.Any() && module.GeneratedPaths.Any())
            {
                throw new Bam.Core.Exception(
                    "There are no OutputPath attributes associated with the {0} settings class",
                    settings.ToString()
                );
            }
            var input_files_attributes = settings.GetType().GetCustomAttributes(
                typeof(InputPathsAttribute),
                true // since generally specified in an abstract class
            ) as InputPathsAttribute[];
            if (module.InputModules.Any())
            {
                if (!input_files_attributes.Any())
                {
                    var message = new System.Text.StringBuilder();
                    message.AppendFormat(
                        "There is no InputPaths attribute associated with the {0} settings class and module {1}",
                        settings.ToString(),
                        module.ToString()
                    );
                    message.AppendLine();
                    message.AppendLine("The following input paths were identified for the module:");
                    foreach (var input in module.InputModules)
                    {
                        message.AppendFormat("\t{0}[{1}]", input.Value.ToString(), input.Key);
                        if (input.Value.GeneratedPaths.ContainsKey(input.Key))
                        {
                            message.AppendFormat(" = '{0}'", input.Value.GeneratedPaths[input.Key].ToString());
                        }
                        message.AppendLine();
                    }
                    throw new Bam.Core.Exception(message.ToString());
                }
                var attr = input_files_attributes.First();
                var max_files = attr.MaxFileCount;
                if (max_files >= 0)
                {
                    if (max_files != module.InputModules.Count())
                    {
                        throw new Bam.Core.Exception(
                            "InputPaths attribute specifies a maximum of {0} files, but {1} are available",
                            max_files,
                            module.InputModules.Count()
                        );
                    }
                }
            }
            switch (settings.FileLayout)
            {
                case Bam.Core.Settings.ELayout.Cmds_Outputs_Inputs:
                    ProcessOutputPaths(settings, module, commandLine, output_file_attributes);
                    ProcessInputPaths(settings, module, commandLine, input_files_attributes);
                    break;

                case Bam.Core.Settings.ELayout.Cmds_Inputs_Outputs:
                    ProcessInputPaths(settings, module, commandLine, input_files_attributes);
                    ProcessOutputPaths(settings, module, commandLine, output_file_attributes);
                    break;

                case Bam.Core.Settings.ELayout.Inputs_Cmds_Outputs:
                    {
                        var newCommandLine = new Bam.Core.StringArray();
                        ProcessInputPaths(settings, module, newCommandLine, input_files_attributes);
                        newCommandLine.AddRange(commandLine);
                        ProcessOutputPaths(settings, module, newCommandLine, output_file_attributes);
                        commandLine = newCommandLine;
                    }
                    break;

                case Bam.Core.Settings.ELayout.Inputs_Outputs_Cmds:
                    {
                        var newCommandLine = new Bam.Core.StringArray();
                        ProcessInputPaths(settings, module, newCommandLine, input_files_attributes);
                        ProcessOutputPaths(settings, module, newCommandLine, output_file_attributes);
                        newCommandLine.AddRange(commandLine);
                        commandLine = newCommandLine;
                    }
                    break;

                default:
                    throw new Bam.Core.Exception(
                        "Unhandled file layout {0} for settings {1}",
                        settings.FileLayout.ToString(),
                        settings.ToString()
                    );
            }
            return commandLine;
        }

        private static void
        ProcessInputPaths(
            Bam.Core.Settings settings,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine,
            InputPathsAttribute[] input_files_attributes)
        {
            if (!input_files_attributes.Any())
            {
                return;
            }
            foreach (var input_module_and_pathkey in module.InputModules)
            {
                var matching_input_attr = input_files_attributes.FirstOrDefault(
                    item => input_module_and_pathkey.Key.Equals(item.PathKey, System.StringComparison.Ordinal)
                );
                if (null == matching_input_attr)
                {
                    // first look to see if there's a generic 'catch all files' attribute
                    // before failing
                    var match_any = input_files_attributes.FirstOrDefault(item => item is AnyInputFileAttribute);
                    if (null == match_any)
                    {
                        throw new Bam.Core.Exception(
                            "Unable to locate InputPathsAttribute suitable for input module {0} and path key {1} while dealing with inputs on module {2}.\n" +
                            "Does module {2} override the InputModules property?\n" +
                            "Is settings class {3} missing an InputPaths attribute?",
                            input_module_and_pathkey.Value.ToString(),
                            input_module_and_pathkey.Key,
                            module.ToString(),
                            settings.ToString()
                        );
                    }
                    matching_input_attr = match_any;
                }
                var input_path = input_module_and_pathkey.Value.GeneratedPaths[input_module_and_pathkey.Key];
#if D_PACKAGE_PUBLISHER
                if (matching_input_attr is AnyInputFileAttribute &&
                    (matching_input_attr as AnyInputFileAttribute).PathModifierIfDirectory != null &&
                    module is Publisher.CollatedDirectory)
                {
                    var modifiedPath = Bam.Core.TokenizedString.Create(
                        (matching_input_attr as AnyInputFileAttribute).PathModifierIfDirectory,
                        module,
                        new Bam.Core.TokenizedStringArray(input_path)
                    );
                    if (!modifiedPath.IsParsed)
                    {
                        modifiedPath.Parse();
                    }
                    commandLine.Add(
                        System.String.Format(
                            "{0}{1}",
                            matching_input_attr.CommandSwitch,
                            modifiedPath.ToStringQuoteIfNecessary()
                        )
                    );
                    continue;
                }
#endif
                if (null != matching_input_attr.PathModifier)
                {
                    var modifiedPath = Bam.Core.TokenizedString.Create(
                        matching_input_attr.PathModifier,
                        module,
                        new Bam.Core.TokenizedStringArray(input_path)
                    );
                    if (!modifiedPath.IsParsed)
                    {
                        modifiedPath.Parse();
                    }
                    commandLine.Add(
                        System.String.Format(
                            "{0}{1}",
                            matching_input_attr.CommandSwitch,
                            modifiedPath.ToStringQuoteIfNecessary()
                        )
                    );
                }
                else
                {
                    commandLine.Add(
                        System.String.Format(
                            "{0}{1}",
                            matching_input_attr.CommandSwitch,
                            input_path.ToStringQuoteIfNecessary()
                        )
                    );
                }
            }
        }

        private static void
        ProcessOutputPaths(
            Bam.Core.Settings settings,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine,
            OutputPathAttribute[] output_file_attributes)
        {
            foreach (var generatedPath in module.GeneratedPaths)
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
                if (matching_attr.Ignore)
                {
                    continue;
                }
                if (null != matching_attr.PathModifier)
                {
                    var modifiedPath = Bam.Core.TokenizedString.Create(
                        matching_attr.PathModifier,
                        module,
                        new Bam.Core.TokenizedStringArray(module.GeneratedPaths[outputKey])
                    );
                    if (!modifiedPath.IsParsed)
                    {
                        modifiedPath.Parse();
                    }
                    commandLine.Add(
                        System.String.Format(
                            "{0}{1}",
                            matching_attr.CommandSwitch,
                            modifiedPath.ToStringQuoteIfNecessary()
                        )
                    );
                }
                else
                {
                    commandLine.Add(
                        System.String.Format(
                            "{0}{1}",
                            matching_attr.CommandSwitch,
                            module.GeneratedPaths[outputKey].ToStringQuoteIfNecessary()
                        )
                    );
                }
            }
        }
    }
}
