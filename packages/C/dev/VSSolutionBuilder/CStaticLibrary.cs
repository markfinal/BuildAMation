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
namespace C
{
namespace V2
{
    public sealed class VSSolutionLibrarian :
        ILibrarianPolicy
    {
        void
        ILibrarianPolicy.Archive(
            StaticLibrary sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers)
        {
#if true
            if (0 == objectFiles.Count)
            {
                return;
            }

            var solution = Bam.Core.V2.Graph.Instance.MetaData as VSSolutionBuilder.V2.VSSolution;
            var project = solution.EnsureProjectExists(sender);
            var config = project.GetConfiguration(sender);

            config.SetType(VSSolutionBuilder.V2.VSProjectConfiguration.EType.StaticLibrary);
            config.SetPlatformToolset(VSSolutionBuilder.V2.VSProjectConfiguration.EPlatformToolset.v120); // TODO: get from VisualC
            config.SetOutputPath(libraryPath);
            config.EnableIntermediatePath();

            foreach (var header in headers)
            {
                config.AddHeaderFile(header as HeaderFile);
            }

#if true
            var compilerGroup = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.Compiler);
            if (objectFiles.Count > 1)
            {
                var vsConvertParameterTypes = new Bam.Core.TypeArray
                {
                    typeof(Bam.Core.V2.Module),
                    typeof(VSSolutionBuilder.V2.VSSettingsGroup),
                    typeof(string)
                };

                var sharedSettings = C.V2.SettingsBase.SharedSettings(
                    objectFiles,
                    typeof(VisualC.VSSolutionImplementation),
                    typeof(VisualStudioProcessor.V2.IConvertToProject),
                    vsConvertParameterTypes);
                (sharedSettings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, compilerGroup);

                foreach (var objFile in objectFiles)
                {
                    var deltaSettings = (objFile.Settings as C.V2.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    config.AddSourceFile(objFile, deltaSettings);
                }
            }
            else
            {
                (objectFiles[0].Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, compilerGroup);
                foreach (var objFile in objectFiles)
                {
                    config.AddSourceFile(objFile, null);
                }
            }
#else
            var commonObjectFile = (inputs[0] is Bam.Core.V2.IModuleGroup) ? inputs[0].Children[0] : inputs[0];
            var compilerGroup = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.Compiler);
            (commonObjectFile.Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, compilerGroup);

            foreach (var input in inputs)
            {
                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        C.V2.SettingsBase deltaSettings = null;
                        if (child != commonObjectFile)
                        {
                            deltaSettings = (child.Settings as C.V2.SettingsBase).Delta(commonObjectFile.Settings, child);
                        }
                        if (child.HasPatches)
                        {
                            C.V2.SettingsBase patchSettings = deltaSettings;
                            if (null == patchSettings)
                            {
                                patchSettings = System.Activator.CreateInstance(input.Settings.GetType(), child, false) as C.V2.SettingsBase;
                            }
                            else
                            {
                                patchSettings = deltaSettings.Clone(child);
                            }
                            child.ApplySettingsPatches(patchSettings, honourParents: false);
                        }

                        config.AddSourceFile(child, deltaSettings);
                    }
                }
                else
                {
                    C.V2.SettingsBase deltaSettings = null;
                    if (input != commonObjectFile)
                    {
                        deltaSettings = (input.Settings as C.V2.SettingsBase).Delta(commonObjectFile.Settings, input);
                    }
                    config.AddSourceFile(input, deltaSettings);
                }
            }
#endif

            var settingsGroup = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.Librarian);
            (sender.Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, settingsGroup);

            // order only dependencies
            foreach (var required in sender.Requirements)
            {
                if (null == required.MetaData)
                {
                    continue;
                }

                var requiredProject = required.MetaData as VSSolutionBuilder.V2.VSProject;
                if (null != requiredProject)
                {
                    project.RequiresProject(requiredProject);
                }
            }
#else
            // cannot tell the architecture from the Librarian tool, so look at all the inputs
            // these should be consistent
            VSSolutionBuilder.V2.VSSolutionMeta.EPlatform? platform = null;
            foreach (var input in inputs)
            {
                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        var obj = child as C.V2.ObjectFile;
                        var thisPlatform = (obj.Compiler is VisualC.Compiler64 || obj.Compiler is VisualC.CxxCompiler64) ? VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.SixtyFour : VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.ThirtyTwo;
                        if (null == platform)
                        {
                            platform = thisPlatform;
                        }
                        else if (platform != thisPlatform)
                        {
                            throw new Bam.Core.Exception("Inconsistent object file architectures");
                        }
                    }
                }
                else
                {
                    var obj = input as C.V2.ObjectFile;
                    var thisPlatform = (obj.Compiler is VisualC.Compiler64 || obj.Compiler is VisualC.CxxCompiler64) ? VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.SixtyFour : VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.ThirtyTwo;
                    if (null == platform)
                    {
                        platform = thisPlatform;
                    }
                    else if (platform != thisPlatform)
                    {
                        throw new Bam.Core.Exception("Inconsistent object file architectures");
                    }
                }
            }

            var library = new VSSolutionBuilder.V2.VSProjectStaticLibrary(sender, libraryPath, platform.Value);
            var commonObject = inputs[0];
            library.SetCommonCompilationOptions(commonObject, commonObject.Settings);

            foreach (var input in inputs)
            {
                C.V2.SettingsBase deltaSettings = null;
                if (input != commonObject)
                {
                    deltaSettings = (input.Settings as C.V2.SettingsBase).Delta(commonObject.Settings, input);
                }

                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        C.V2.SettingsBase patchSettings = deltaSettings;
                        if (child.HasPatches)
                        {
                            if (null == patchSettings)
                            {
                                patchSettings = System.Activator.CreateInstance(input.Settings.GetType(), child, false) as C.V2.SettingsBase;
                            }
                            else
                            {
                                patchSettings = deltaSettings.Clone(child);
                            }
                            child.ApplySettingsPatches(patchSettings, honourParents: false);
                        }
                        library.AddObjectFile(child, patchSettings);
                    }
                }
                else
                {
                    library.AddObjectFile(input, deltaSettings);
                }
            }

            foreach (var header in headers)
            {
                if (header is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in header.Children)
                    {
                        library.AddHeaderFile(child as HeaderFile);
                    }
                }
                else
                {
                    library.AddHeaderFile(header as HeaderFile);
                }
            }
#endif
        }
    }
}
}
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object
        Build(
            C.StaticLibrary moduleToBuild,
            out bool success)
        {
            var staticLibraryModule = moduleToBuild as Bam.Core.BaseModule;
            var node = staticLibraryModule.OwningNode;
            var target = node.Target;
            var moduleName = node.ModuleName;

            IProject projectData = null;
            // TODO: want to remove this
            lock (this.solutionFile.ProjectDictionary)
            {
                if (this.solutionFile.ProjectDictionary.ContainsKey(moduleName))
                {
                    projectData = this.solutionFile.ProjectDictionary[moduleName];
                }
                else
                {
                    success = true;
                    return null;
                }
            }

            {
                var platformName = VSSolutionBuilder.GetPlatformNameFromTarget(target);
                if (!projectData.Platforms.Contains(platformName))
                {
                    projectData.Platforms.Add(platformName);
                }
            }

            // solution folder
            {
                var groups = moduleToBuild.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleGroupAttribute), true);
                if (groups.Length > 0)
                {
                    projectData.GroupName = (groups as Bam.Core.ModuleGroupAttribute[])[0].GroupName;
                }
            }

            var staticLibraryOptions = staticLibraryModule.Options;

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
#if false
                    C.IArchiverOptions archiverOptions = staticLibraryOptions as C.IArchiverOptions;
                    C.IToolchainOptions toolchainOptions = archiverOptions.ToolchainOptionCollection as C.IToolchainOptions;
                    EProjectCharacterSet characterSet;
                    switch (toolchainOptions.CharacterSet)
                    {
                        case C.ECharacterSet.NotSet:
                            characterSet = EProjectCharacterSet.NotSet;
                            break;

                        case C.ECharacterSet.Unicode:
                            characterSet = EProjectCharacterSet.UniCode;
                            break;

                        case C.ECharacterSet.MultiByte:
                            characterSet = EProjectCharacterSet.MultiByte;
                            break;

                        default:
                            characterSet = EProjectCharacterSet.Undefined;
                            break;
                    }
                    configuration.CharacterSet = characterSet;
#endif
                    configuration = new ProjectConfiguration(configurationName, projectData);
                    projectData.Configurations.Add((Bam.Core.BaseTarget)target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    projectData.Configurations.AddExistingForTarget((Bam.Core.BaseTarget)target, configuration);
                }
            }

            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    // TODO: change to var
                    foreach (Bam.Core.Location location in headerFileCollection)
                    {
                        var headerPath = location.GetSinglePath();
                        var cProject = projectData as ICProject;
                        lock (cProject.HeaderFiles)
                        {
                            if (!cProject.HeaderFiles.Contains(headerPath))
                            {
                                var headerFile = new ProjectFile(headerPath);
                                cProject.HeaderFiles.Add(headerFile);
                            }
                        }
                    }
                }
            }

            configuration.Type = EProjectConfigurationType.StaticLibrary;

            var toolName = "VCLibrarianTool";
            var vcCLLibrarianTool = configuration.GetTool(toolName);
            if (null == vcCLLibrarianTool)
            {
                vcCLLibrarianTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLLibrarianTool);

                var archiverOptions = staticLibraryOptions as C.ArchiverOptionCollection;
                configuration.OutputDirectory = moduleToBuild.Locations[C.StaticLibrary.OutputDirLocKey];
                configuration.TargetName = archiverOptions.OutputName;

                if (staticLibraryOptions is VisualStudioProcessor.IVisualStudioSupport)
                {
                    var visualStudioProjectOption = staticLibraryOptions as VisualStudioProcessor.IVisualStudioSupport;
                    var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (var setting in settingsDictionary)
                    {
                        vcCLLibrarianTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Bam.Core.Exception("Archiver options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}
