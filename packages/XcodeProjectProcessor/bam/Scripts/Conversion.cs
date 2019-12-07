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
namespace XcodeProjectProcessor
{
    /// <summary>
    /// Handles conversion from BAM settings into Xcode projects.
    /// </summary>
    static class XcodeConversion
    {
        private static void
        HandleFrameworks(
            Bam.Core.Module module,
            Bam.Core.TokenizedStringArray frameworks,
            Bam.Core.TokenizedStringArray frameworkSearchPaths
        )
        {
            var target = module.MetaData as XcodeBuilder.Target;
            var project = target.Project;
            foreach (var framework in frameworks.ToEnumerableWithoutDuplicates())
            {
                var framework_path = framework.ToString();
                if (!framework_path.EndsWith(".framework"))
                {
                    framework_path += ".framework";
                }
                if (Bam.Core.RelativePathUtilities.IsPathAbsolute(framework_path))
                {
                    target.EnsureFrameworksBuildFileExists(
                        framework,
                        XcodeBuilder.FileReference.EFileType.WrapperFramework,
                        XcodeBuilder.FileReference.ESourceTree.Absolute
                    );
                }
                else
                {
                    var found = false;
                    foreach (var searchPath in frameworkSearchPaths.ToEnumerableWithoutDuplicates())
                    {
                        var potential_framework_path = System.IO.Path.Combine(searchPath.ToString(), framework_path);
                        if (System.IO.Directory.Exists(potential_framework_path))
                        {
                            Bam.Core.Log.MessageAll(
                                $"Found framework at {potential_framework_path}"
                            );
                            found = true;
                            throw new System.NotImplementedException();
                        }
                    }
                    if (!found)
                    {
                        target.EnsureFrameworksBuildFileExists(
                            Bam.Core.TokenizedString.CreateVerbatim("System/Library/Frameworks/" + framework_path),
                            XcodeBuilder.FileReference.EFileType.WrapperFramework,
                            XcodeBuilder.FileReference.ESourceTree.SDKRoot
                        );
                    }
                }
            }
        }

        private static bool
        FindLibrary(
            string proposed_library_filename,
            Bam.Core.TokenizedStringArray librarySearchPaths,
            XcodeBuilder.Target target,
            XcodeBuilder.FileReference.EFileType fileType)
        {
            // look in user library paths
            foreach (var searchPath in librarySearchPaths.ToEnumerableWithoutDuplicates())
            {
                var searchPath_string = searchPath.ToString();
                var proposed_path = System.IO.Path.Combine(searchPath_string, proposed_library_filename);
                if (System.IO.File.Exists(proposed_path))
                {
                    target.EnsureFrameworksBuildFileExists(
                        Bam.Core.TokenizedString.CreateVerbatim(proposed_path),
                        fileType,
                        XcodeBuilder.FileReference.ESourceTree.Absolute
                    );
                    return true;
                }
            }
            // look in system library paths
            var proposed_system_lib_path = System.IO.Path.Combine("/usr/lib", proposed_library_filename);
            if (System.IO.File.Exists(proposed_system_lib_path))
            {
                target.EnsureFrameworksBuildFileExists(
                    Bam.Core.TokenizedString.CreateVerbatim(proposed_system_lib_path),
                    fileType,
                    XcodeBuilder.FileReference.ESourceTree.Absolute
                );
                return true;
            }
            // look in SDK library paths
            var meta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
            var proposed_sdk_lib_path = System.IO.Path.Combine(meta.SDKPath, "usr/lib");
            proposed_sdk_lib_path = System.IO.Path.Combine(proposed_sdk_lib_path, proposed_library_filename);
            if (System.IO.File.Exists(proposed_sdk_lib_path))
            {
                target.EnsureFrameworksBuildFileExists(
                    Bam.Core.TokenizedString.CreateVerbatim(System.IO.Path.Combine("usr/lib", proposed_library_filename)),
                    fileType,
                    XcodeBuilder.FileReference.ESourceTree.SDKRoot
                );
                return true;
            }
            return false;
        }

        private static void
        HandleLibraryArray(
            Bam.Core.Module module,
            Bam.Core.StringArray libraries,
            Bam.Core.TokenizedStringArray librarySearchPaths
        )
        {
            // TODO: should support .tbd files (text based definition)
            var target = module.MetaData as XcodeBuilder.Target;
            foreach (var library in libraries)
            {
                if (System.IO.Path.IsPathRooted(library))
                {
                    target.EnsureFrameworksBuildFileExists(
                        Bam.Core.TokenizedString.CreateVerbatim(library),
                        XcodeBuilder.FileReference.EFileType.DynamicLibrary,
                        XcodeBuilder.FileReference.ESourceTree.Absolute
                    );
                    continue;
                }

                var stripped_library_name = library.Replace("-l", System.String.Empty);
                var proposed_library_filename = $"lib{stripped_library_name}.dylib";
                if (FindLibrary(
                        proposed_library_filename,
                        librarySearchPaths,
                        target,
                        XcodeBuilder.FileReference.EFileType.DynamicLibrary
                    ))
                {
                    continue;
                }
                proposed_library_filename = $"lib{stripped_library_name}.a";
                if (FindLibrary(
                        proposed_library_filename,
                        librarySearchPaths,
                        target,
                        XcodeBuilder.FileReference.EFileType.Archive
                    ))
                {
                    continue;
                }
                var message = new System.Text.StringBuilder();
                message.AppendLine($"Unable to locate library '{stripped_library_name}' on any system or user search path");
                if (librarySearchPaths.Any())
                {
                    message.AppendLine("User search paths are:");
                    foreach (var path in librarySearchPaths)
                    {
                        message.AppendLine($"\t{path.ToString()}");
                    }
                }
                throw new Bam.Core.Exception(message.ToString());
            }
        }

        /// <summary>
        /// Convert BAM Module settings into Xcode project configuration properties.
        /// </summary>
        /// <param name="settings">The Module's settings.</param>
        /// <param name="module">Module associated with the settings. This may be null for per-file settings.</param>
        /// <param name="configuration">The Xcode project Configuration to complete.</param>
        /// <param name="settingsTypeOverride">Optional, override the settings type. Default is null.</param>
        public static void
        Convert(
            Bam.Core.Settings settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration,
            System.Type settingsTypeOverride = null
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
                        item => full_property_interface_name.Equals(item.Name, System.StringComparison.Ordinal)
                    );
                    var property_value = value_settings_property.GetValue(settings);
                    if (null == property_value)
                    {
                        continue;
                    }
                    var attribute_settings_property = Bam.Core.Settings.FindProperties(settingsTypeOverride ?? settings.GetType()).First(
                        item => full_property_interface_name.Equals(item.Name, System.StringComparison.Ordinal)
                    );
                    var attributeArray = attribute_settings_property.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (!attributeArray.Any())
                    {
                        throw new Bam.Core.Exception(
                            $"No XcodeProcessor attributes for property {full_property_interface_name} in module {module.ToString()}"
                        );
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
                                    $"Unknown Xcode configuration value type, {associated_attribute.Type.ToString()}"
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
                            if (fullPath.StartsWith('@'))
                            {
                                // e.g. @executable_path ...
                                paths.Add(fullPath);
                                continue;
                            }
                            var relPath = Bam.Core.RelativePathUtilities.GetRelativePathFromRoot(
                                configuration.Project.SourceRoot,
                                fullPath
                            );
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
                                if (associated_attr.PrefixWithSrcRoot)
                                {
                                    paths.Add($"$(SRCROOT)/{relPath}");
                                }
                                else
                                {
                                    paths.Add(relPath);
                                }
                            }
                        }
                        configuration[associated_attr.Property] = paths;
                    }
                    else if (attributeArray.First() is FrameworkArrayAttribute)
                    {
                        var associated_attr = attributeArray.First() as FrameworkArrayAttribute;
                        if (associated_attr.Ignore)
                        {
                            continue;
                        }
                        HandleFrameworks(
                            module,
                            property_value as Bam.Core.TokenizedStringArray,
                            (settings as C.ICommonLinkerSettingsOSX).FrameworkSearchPaths
                        );
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
                                    values.Add($"{prefix}{item}");
                                }
                            }
                            else
                            {
                                values.Add($"{prefix}{item}");
                            }
                        }
                        configuration[associated_attr.Property] = values;
                    }
                    else if (attributeArray.First() is LibraryArrayAttribute)
                    {
                        HandleLibraryArray(
                            module,
                            property_value as Bam.Core.StringArray,
                            (settings as C.ICommonLinkerSettings).LibraryPaths
                        );
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
                            throw new Bam.Core.Exception($"Unrecognised value type, {associated_attr.Type.ToString()}");
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
                                defines.Add($"{define.Key}={defineValue}");
                            }
                        }
                        configuration[associated_attr.Property] = defines;
                    }
                    else
                    {
                        throw new Bam.Core.Exception(
                            $"Unhandled attribute {attributeArray.First().ToString()} for property {attribute_settings_property.Name} in {module.ToString()}"
                        );
                    }
                }
            }
        }
    }
}
