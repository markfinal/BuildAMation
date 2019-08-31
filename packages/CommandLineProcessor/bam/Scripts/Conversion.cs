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
namespace CommandLineProcessor
{
    /// <summary>
    /// Helper class for converting Settings to command lines.
    /// </summary>
    static class NativeConversion
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
                    $"Attribute expected an enum (or nullable enum), but property {propertyInfo.Name} is of type {propertyInfo.PropertyType.ToString()}"
                );
            }
            var matching_attribute = attributeArray.FirstOrDefault(
                item => (item as EnumAttribute).Key.Equals(propertyValue)
            ) as BaseAttribute;
            if (null == matching_attribute)
            {
                var message = new System.Text.StringBuilder();
                message.Append($"Unable to locate enumeration mapping of '{propertyValue.GetType().ToString()}.{propertyValue.ToString()}' ");
                message.Append($"for property {interfacePropertyInfo.DeclaringType.FullName}.{interfacePropertyInfo.Name}");
                throw new Bam.Core.Exception(message.ToString());
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
                        $"Attribute expected either a Bam.Core.TokenizedString or string, but property {propertyInfo.Name} is of type {propertyInfo.PropertyType.ToString()}"
                    );
                }
            }
            if (typeof(Bam.Core.TokenizedString).IsAssignableFrom(propertyInfo.PropertyType))
            {
                commandLine.Add(
                    $"{(attributeArray.First() as BaseAttribute).CommandSwitch}{(propertyValue as Bam.Core.TokenizedString).ToStringQuoteIfNecessary()}"
                );
            }
            else
            {
                var path = module.GeneratedPaths[propertyValue as string];
                commandLine.Add(
                    $"{(attributeArray.First() as BaseAttribute).CommandSwitch}{path.ToStringQuoteIfNecessary()}"
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
                    $"Attribute expected a Bam.Core.TokenizedStringArray, but property {propertyInfo.Name} is of type {propertyInfo.PropertyType.ToString()}"
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            foreach (var path in (propertyValue as Bam.Core.TokenizedStringArray).ToEnumerableWithoutDuplicates())
            {
                // TODO: a special case is needed for this being requested in Xcode mode
                // which is done when there are overrides per source file
                commandLine.Add(
                    $"{command_switch}{path.ToStringQuoteIfNecessary()}"
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
                    $"Attribute expected a Bam.Core.TokenizedStringArray, but property {propertyInfo.Name} is of type {propertyInfo.PropertyType.ToString()}"
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            foreach (var path in (propertyValue as Bam.Core.TokenizedStringArray).ToEnumerableWithoutDuplicates())
            {
                commandLine.Add(
                    $"{command_switch}{System.IO.Path.GetFileNameWithoutExtension(path.ToStringQuoteIfNecessary())}"
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
                    $"Attribute expected a string, but property {propertyInfo.Name} is of type {propertyInfo.PropertyType.ToString()}"
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            if (command_switch.EndsWith("=", System.StringComparison.Ordinal) &&
                (propertyValue as string).Contains("=", System.StringComparison.Ordinal))
            {
                // the double quotes are needed for MakeFiles.
                // escaping the equals sign in the value did not work
                // and single quotes worked in MakeFiles but didn't in Native builds
                commandLine.Add(
                    $"{command_switch}\"{propertyValue as string}\""
                );
            }
            else
            {
                commandLine.Add(
                    $"{command_switch}{propertyValue as string}"
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
                    $"Attribute expected a Bam.Core.StringArray, but property {propertyInfo.Name} is of type {propertyInfo.PropertyType.ToString()}"
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            foreach (var str in (propertyValue as Bam.Core.StringArray))
            {
                // TODO: a special case is needed for this being requested in Xcode mode
                // which is done when there are overrides per source file
                commandLine.Add(
                    $"{command_switch}{str}"
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
                    $"Attribute expected an bool (or nullable bool), but property {propertyInfo.Name} is of type {propertyInfo.PropertyType.ToString()}"
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
                    $"Attribute expected a C.PreprocessorDefinitions, but property {propertyInfo.Name} is of type {propertyInfo.PropertyType.ToString()}"
                );
            }
            var command_switch = (attributeArray.First() as BaseAttribute).CommandSwitch;
            foreach (var define in (propertyValue as C.PreprocessorDefinitions))
            {
                if (null == define.Value)
                {
                    commandLine.Add($"-D{define.Key}");
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
                    commandLine.Add($"-D{define.Key}={defineValue}");
                }
            }
        }

        /// <summary>
        /// Convert a Settings instance to command lines.
        /// </summary>
        /// <param name="settings">Settings instance to convert properties to command line.</param>
        /// <param name="module">The Module owning the Settings.</param>
        /// <param name="createDelta">Optional whether to create a delta command line (for per-file command lines). Default to false.</param>
        /// <returns></returns>
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
            if (settings.CommandLayout == Bam.Core.Settings.ELayout.Unassigned)
            {
                throw new Bam.Core.Exception(
                    $"Command layout for {settings.ToString()} settings is unassigned. Check that the constructor updates the layout on the base class"
                );
            }
            var commandLine = new Bam.Core.StringArray();
            //Bam.Core.Log.MessageAll($"Module: {module.ToString()}");
            //Bam.Core.Log.MessageAll($"Settings: {settings.ToString()}");
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
                    //Bam.Core.Log.MessageAll($"\t{settings_property.ToString()}");
                    var attributeArray = settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
                        throw new Bam.Core.Exception(
                            $"No attributes available for mapping property {full_property_interface_name} to command line switches for module {module.ToString()} and settings {settings.ToString()}"
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
                            $"Unhandled attribute {attributeArray.First().ToString()} for property {settings_property.Name} in {module.ToString()}"
                        );
                    }
                }
            }
            //Bam.Core.Log.MessageAll($"{module.ToString()}: Executing '{commandLine.ToString(' ')}'");

            if (createDelta)
            {
                // don't want to introduce outputs or inputs in deltas
                return commandLine;
            }

            if (null == settings.Module)
            {
                throw new Bam.Core.Exception(
                    $"No Module was associated with the settings class {settings.ToString()}"
                );
            }

            var output_file_attributes = settings.GetType().GetCustomAttributes(
                typeof(OutputPathAttribute),
                true // since generally specified in an abstract class
            ) as OutputPathAttribute[];
            if (!output_file_attributes.Any() && module.GeneratedPaths.Any())
            {
                throw new Bam.Core.Exception(
                    $"There are no OutputPath attributes associated with the {settings.ToString()} settings class"
                );
            }
            var input_files_attributes = settings.GetType().GetCustomAttributes(
                typeof(InputPathsAttribute),
                true // since generally specified in an abstract class
            ) as InputPathsAttribute[];
            if (module.InputModulePaths.Any())
            {
                if (!input_files_attributes.Any())
                {
                    var message = new System.Text.StringBuilder();
                    message.AppendLine(
                        $"There is no InputPaths attribute associated with the {settings.ToString()} settings class and module {module.ToString()}"
                    );
                    message.AppendLine("The following input paths were identified for the module:");
                    foreach (var (inputModule,inputPathKey) in module.InputModulePaths)
                    {
                        message.Append($"\t{inputModule.ToString()}[{inputPathKey}]");
                        if (inputModule.GeneratedPaths.ContainsKey(inputPathKey))
                        {
                            message.Append($" = '{inputModule.GeneratedPaths[inputPathKey].ToString()}'");
                        }
                        message.AppendLine();
                    }
                    throw new Bam.Core.Exception(message.ToString());
                }
                var attr = input_files_attributes.First();
                var max_files = attr.MaxFileCount;
                if (max_files >= 0)
                {
                    if (max_files != module.InputModulePaths.Count())
                    {
                        throw new Bam.Core.Exception(
                            $"InputPaths attribute specifies a maximum of {max_files} files, but {module.InputModulePaths.Count()} are available"
                        );
                    }
                }
            }
            switch (settings.CommandLayout)
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
                        $"Unhandled file layout {settings.CommandLayout.ToString()} for settings {settings.ToString()}"
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
            foreach (var (inputModule, inputPathKey) in module.InputModulePaths)
            {
                var matching_input_attr = input_files_attributes.FirstOrDefault(
                    item => inputPathKey.Equals(item.PathKey, System.StringComparison.Ordinal)
                );
                if (null == matching_input_attr)
                {
                    // first look to see if there's a generic 'catch all files' attribute
                    // before failing
                    var match_any = input_files_attributes.FirstOrDefault(item => item is AnyInputFileAttribute);
                    if (null == match_any)
                    {
                        var message = new System.Text.StringBuilder();
                        message.AppendLine($"Unable to locate an InputPathsAttribute or AnyInputFileAttribute suitable for this input:");
                        message.AppendLine($"\tModule : {inputModule.ToString()}");
                        message.AppendLine($"\tPathkey: {inputPathKey}");
                        message.AppendLine($"while dealing with inputs on module {module.ToString()}.");
                        message.AppendLine("Possible reasons:");
                        message.AppendLine($"\tDoes module {module.ToString()} override the Dependents property?");
                        message.AppendLine($"\tIs settings class {settings.ToString()} missing an InputPaths attribute?");
                        throw new Bam.Core.Exception(message.ToString());
                    }
                    matching_input_attr = match_any;
                }
                var input_path = inputModule.GeneratedPaths[inputPathKey];
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
                        $"{matching_input_attr.CommandSwitch}{modifiedPath.ToStringQuoteIfNecessary()}"
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
                        $"{matching_input_attr.CommandSwitch}{modifiedPath.ToStringQuoteIfNecessary()}"
                    );
                }
                else
                {
                    commandLine.Add(
                        $"{matching_input_attr.CommandSwitch}{input_path.ToStringQuoteIfNecessary()}"
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
                        $"Unable to locate OutputPath class attribute on {settings.ToString()} for path key {outputKey}"
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
                        $"{matching_attr.CommandSwitch}{modifiedPath.ToStringQuoteIfNecessary()}"
                    );
                }
                else
                {
                    commandLine.Add(
                        $"{matching_attr.CommandSwitch}{module.GeneratedPaths[outputKey].ToStringQuoteIfNecessary()}"
                    );
                }
            }
        }
    }
}
