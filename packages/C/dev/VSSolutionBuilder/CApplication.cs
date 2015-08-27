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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
using C.V2.DefaultSettings;
namespace C
{
namespace V2
{
    public static class ObjectFileExtensions
    {
        public static Bam.Core.Array<Bam.Core.V2.Module>
        LinearObjectFileList(
            this System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles)
        {
            var list = new Bam.Core.Array<Bam.Core.V2.Module>();
            foreach (var input in objectFiles)
            {
                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        list.Add(child);
                    }
                }
                else
                {
                    list.Add(input);
                }
            }
            return list;
        }

        public static Bam.Core.TypeArray
        SharedInterfaces(
            this System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles)
        {
            var interfaces = new Bam.Core.TypeArray();
            foreach (var input in objectFiles)
            {
                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        var childInterfaces = child.Settings.GetType().GetInterfaces().Where(item => (item != typeof(ISettingsBase)) && typeof(ISettingsBase).IsAssignableFrom(item));
                        if (interfaces.Count == 0)
                        {
                            interfaces.AddRangeUnique(new Bam.Core.TypeArray(childInterfaces));
                        }
                        else
                        {
                            interfaces.Complement(new Bam.Core.TypeArray(childInterfaces));
                        }
                    }
                }
                else
                {
                    var childInterfaces = input.Settings.GetType().GetInterfaces();
                    if (interfaces.Count == 0)
                    {
                        interfaces.AddRangeUnique(new Bam.Core.TypeArray(childInterfaces));
                    }
                    else
                    {
                        interfaces.Complement(new Bam.Core.TypeArray(childInterfaces));
                    }
                }
            }
            return interfaces;
        }
    }

    public sealed partial class VSSolutionLinker :
        ILinkerPolicy
    {
        void
        ILinkerPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks)
        {
#if true
            if (0 == objectFiles.Count)
            {
                return;
            }

            var solution = Bam.Core.V2.Graph.Instance.MetaData as VSSolutionBuilder.V2.VSSolution;
            var project = solution.EnsureProjectExists(sender);
            var config = project.GetConfiguration(sender);

            config.SetType((sender is DynamicLibrary) ? VSSolutionBuilder.V2.VSProjectConfiguration.EType.DynamicLibrary : VSSolutionBuilder.V2.VSProjectConfiguration.EType.Application);
            config.SetPlatformToolset(VSSolutionBuilder.V2.VSProjectConfiguration.EPlatformToolset.v120); // TODO: get from VisualC
            config.SetOutputPath(executablePath);
            config.EnableIntermediatePath();

            foreach (var header in headers)
            {
                if (header is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in header.Children)
                    {
                        config.AddHeaderFile(child as HeaderFile);
                    }
                }
                else
                {
                    config.AddHeaderFile(header as HeaderFile);
                }
            }

            var compilerGroup = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.Compiler);
            var list = objectFiles.LinearObjectFileList();
            if (list.Count > 1)
            {
                // find the lowest common denominator across all compiled source
                var sharedInterfaces = objectFiles.SharedInterfaces();
                var implementedInterfaces = new Bam.Core.TypeArray(sharedInterfaces);
                implementedInterfaces.Add(typeof(VisualStudioProcessor.V2.IConvertToProject));

                var typeSignature = "MyDynamicType";
                var an = new System.Reflection.AssemblyName(typeSignature);
                var assemblyBuilder = System.AppDomain.CurrentDomain.DefineDynamicAssembly(an, System.Reflection.Emit.AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule", true);
                var tb = moduleBuilder.DefineType(typeSignature,
                    System.Reflection.TypeAttributes.Public |
                    System.Reflection.TypeAttributes.Class |
                    System.Reflection.TypeAttributes.AutoClass |
                    System.Reflection.TypeAttributes.AnsiClass |
                    System.Reflection.TypeAttributes.BeforeFieldInit |
                    System.Reflection.TypeAttributes.AutoLayout,
                    typeof(C.V2.SettingsBase),
                    implementedInterfaces.ToArray());

                var constructor = tb.DefineDefaultConstructor(System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.RTSpecialName);

                foreach (var commonInt in sharedInterfaces)
                {
                    var props = commonInt.GetProperties();
                    foreach (var prop in props)
                    {
                        var dynamicProperty = tb.DefineProperty(prop.Name,
                            System.Reflection.PropertyAttributes.None,
                            prop.PropertyType,
                            null);
                        var field = tb.DefineField("m" + prop.Name,
                            prop.PropertyType,
                            System.Reflection.FieldAttributes.Private);
                        var methodAttrs = System.Reflection.MethodAttributes.Public |
                            System.Reflection.MethodAttributes.HideBySig |
                            System.Reflection.MethodAttributes.Virtual;
                        if (prop.IsSpecialName)
                        {
                            methodAttrs |= System.Reflection.MethodAttributes.SpecialName;
                        }
                        var getter = tb.DefineMethod("get_" + prop.Name,
                            methodAttrs,
                            prop.PropertyType,
                            System.Type.EmptyTypes);
                        var setter = tb.DefineMethod("set_" + prop.Name,
                            methodAttrs,
                            null,
                            new[] { prop.PropertyType });
                        var getIL = getter.GetILGenerator();
                        getIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                        getIL.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                        getIL.Emit(System.Reflection.Emit.OpCodes.Ret);
                        dynamicProperty.SetGetMethod(getter);
                        var setIL = setter.GetILGenerator();
                        setIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                        setIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                        setIL.Emit(System.Reflection.Emit.OpCodes.Stfld, field);
                        setIL.Emit(System.Reflection.Emit.OpCodes.Ret);
                        dynamicProperty.SetSetMethod(setter);
                    }
                }

                var convert = tb.DefineMethod("Convert",
                    System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Final | System.Reflection.MethodAttributes.HideBySig | System.Reflection.MethodAttributes.NewSlot | System.Reflection.MethodAttributes.Virtual,
                    null,
                    new[] { typeof(Bam.Core.V2.Module), typeof(VSSolutionBuilder.V2.VSSettingsGroup), typeof(string) });
                var convertIL = convert.GetILGenerator();
                foreach (var i in sharedInterfaces)
                {
                    var meth = typeof(VisualC.VSSolutionImplementation).GetMethod("Convert", new[] { i, typeof(Bam.Core.V2.Module), typeof(VSSolutionBuilder.V2.VSSettingsGroup), typeof(string) });
                    convertIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                    convertIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                    convertIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
                    convertIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_3);
                    convertIL.Emit(System.Reflection.Emit.OpCodes.Call, meth);
                }
                convertIL.Emit(System.Reflection.Emit.OpCodes.Ret);

                var type = tb.CreateType();

                var sharedSettings = C.V2.SettingsBase.SharedSettings(list, sharedInterfaces, type);
                (sharedSettings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, compilerGroup);

                foreach (var objFile in list)
                {
                    var deltaSettings = (objFile.Settings as C.V2.SettingsBase).Delta2(sharedSettings, objFile);
                    config.AddSourceFile(objFile, deltaSettings);
                }
            }
            else
            {
                (list[0].Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, compilerGroup);
                foreach (var objFile in list)
                {
                    config.AddSourceFile(objFile, null);
                }
            }

#if true
#else
            var commonObjectFile = (objectFiles[0] is Bam.Core.V2.IModuleGroup) ? objectFiles[0].Children[0] : objectFiles[0];

            foreach (var input in objectFiles)
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

            foreach (var input in libraries)
            {
                if (null != input.MetaData)
                {
                    if ((input is C.V2.StaticLibrary) || (input is C.V2.DynamicLibrary))
                    {
                        project.LinkAgainstProject(solution.EnsureProjectExists(input));
                    }
                    else if ((input is C.V2.CSDKModule) || (input is C.V2.HeaderLibrary))
                    {
                        continue;
                    }
                    else if (input is ExternalFramework)
                    {
                        throw new Bam.Core.Exception("Frameworks are not supported on Windows: {0}", input.ToString());
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Don't know how to handle this buildable library module, {0}", input.ToString());
                    }
                }
                else
                {
                    if (input is C.V2.StaticLibrary)
                    {
                        // TODO: probably a simplification of the DLL codepath
                        throw new System.NotImplementedException();
                    }
                    else if (input is C.V2.DynamicLibrary)
                    {
                        var linker = sender.Settings as C.V2.ICommonLinkerOptions;
                        if (sender.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            var libraryPath = input.GeneratedPaths[C.V2.DynamicLibrary.ImportLibraryKey].Parse();
                            var libraryDir = System.IO.Path.GetDirectoryName(libraryPath);
                            var libraryName = System.IO.Path.GetFileName(libraryPath);
                            linker.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(libraryDir, null));
                            linker.Libraries.AddUnique(libraryName);
                        }
                        else
                        {
                            var libraryPath = input.GeneratedPaths[C.V2.DynamicLibrary.Key].Parse();
                            var libraryDir = System.IO.Path.GetDirectoryName(libraryPath);
                            linker.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(libraryDir, null));
                            if ((sender.Tool as C.V2.LinkerTool).UseLPrefixLibraryPaths)
                            {
                                var libName = System.IO.Path.GetFileNameWithoutExtension(libraryPath);
                                libName = libName.Substring(3); // trim off lib prefix
                                linker.Libraries.AddUnique(System.String.Format("-l{0}", libName));
                            }
                            else
                            {
                                var libraryName = System.IO.Path.GetFileName(libraryPath);
                                linker.Libraries.AddUnique(libraryName);
                            }
                        }
                    }
                    else if ((input is C.V2.CSDKModule) || (input is C.V2.HeaderLibrary))
                    {
                        continue;
                    }
                    else if (input is ExternalFramework)
                    {
                        throw new Bam.Core.Exception("Frameworks are not supported on Windows: {0}", input.ToString());
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Don't know how to handle this prebuilt library module, {0}", input.ToString());
                    }
                }
            }

            var linkerGroup = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.Linker);
            (sender.Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, linkerGroup);

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
            // inspect prebuilt library dependencies, which are added as part of the linker options
            foreach (var input in libraries)
            {
                if (input.MetaData != null)
                {
                    continue;
                }

                if (input is C.V2.StaticLibrary)
                {
                    // TODO: probably a simplification of the DLL codepath
                    throw new System.NotImplementedException();
                }
                else if (input is C.V2.DynamicLibrary)
                {
                    var linker = sender.Settings as C.V2.ICommonLinkerOptions;
                    if (sender.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var libraryPath = input.GeneratedPaths[C.V2.DynamicLibrary.ImportLibraryKey].Parse();
                        var libraryDir = System.IO.Path.GetDirectoryName(libraryPath);
                        var libraryName = System.IO.Path.GetFileName(libraryPath);
                        linker.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(libraryDir, null));
                        linker.Libraries.AddUnique(libraryName);
                    }
                    else
                    {
                        var libraryPath = input.GeneratedPaths[C.V2.DynamicLibrary.Key].Parse();
                        var libraryDir = System.IO.Path.GetDirectoryName(libraryPath);
                        linker.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(libraryDir, null));
                        if ((sender.Tool as C.V2.LinkerTool).UseLPrefixLibraryPaths)
                        {
                            var libName = System.IO.Path.GetFileNameWithoutExtension(libraryPath);
                            libName = libName.Substring(3); // trim off lib prefix
                            linker.Libraries.AddUnique(System.String.Format("-l{0}", libName));
                        }
                        else
                        {
                            var libraryName = System.IO.Path.GetFileName(libraryPath);
                            linker.Libraries.AddUnique(libraryName);
                        }
                    }
                }
                else if (input is C.V2.CSDKModule)
                {
                    // do nothing for SDKs
                    continue;
                }
                else if (input is C.V2.HeaderLibrary)
                {
                    // no library
                    continue;
                }
                else if (input is ExternalFramework)
                {
                    throw new Bam.Core.Exception("Frameworks are not supported on Windows: {0}", input.ToString());
                }
                else
                {
                    throw new Bam.Core.Exception("Don't know how to handle this library module, {0}", input.ToString());
                }
            }

            var platform = sender.Linker is VisualC.V2.Linker64 ? VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.SixtyFour : VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.ThirtyTwo;
            VSSolutionBuilder.V2.VSCommonLinkableProject application = null;
            if (sender is DynamicLibrary)
            {
                application = new VSSolutionBuilder.V2.VSProjectDynamicLibrary(sender, executablePath, platform);
            }
            else
            {
                application = new VSSolutionBuilder.V2.VSProjectProgram(sender, executablePath, platform);
            }
            var commonObject = objectFiles[0];
            application.SetCommonCompilationOptions(commonObject, commonObject.Settings);
            foreach (var input in objectFiles)
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
                        application.AddObjectFile(child, patchSettings);
                    }
                }
                else
                {
                    application.AddObjectFile(input, deltaSettings);
                }
            }

            foreach (var header in headers)
            {
                if (header is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in header.Children)
                    {
                        application.AddHeaderFile(child as HeaderFile);
                    }
                }
                else
                {
                    application.AddHeaderFile(header as HeaderFile);
                }
            }

            // loop over libraries that have been built in projects, and which require a dependency set up
            foreach (var input in libraries)
            {
                if (null == input.MetaData)
                {
                    // prebuilts handled earlier
                    continue;
                }

                if (input is C.V2.StaticLibrary)
                {
                    application.AddStaticLibrary(input.MetaData as VSSolutionBuilder.V2.VSProjectStaticLibrary);
                }
                else if (input is C.V2.DynamicLibrary)
                {
                    application.AddDynamicLibrary(input.MetaData as VSSolutionBuilder.V2.VSProjectDynamicLibrary);
                }
                else if (input is C.V2.CSDKModule)
                {
                    // do nothing for SDKs
                    continue;
                }
                else if (input is C.V2.CSDKModule)
                {
                    // no library
                    continue;
                }
                else
                {
                    throw new Bam.Core.Exception("Don't know how to handle this library module, {0}", input.ToString());
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
            C.Application moduleToBuild,
            out bool success)
        {
            var applicationModule = moduleToBuild as Bam.Core.BaseModule;
            var node = applicationModule.OwningNode;
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

            if (null != node.ExternalDependents)
            {
                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (dependentNode.ModuleName != moduleName)
                    {
                        // TODO: want to remove this
                        lock (this.solutionFile.ProjectDictionary)
                        {
                            if (this.solutionFile.ProjectDictionary.ContainsKey(dependentNode.ModuleName))
                            {
                                var dependentProject = this.solutionFile.ProjectDictionary[dependentNode.ModuleName];
                                projectData.DependentProjects.Add(dependentProject);
                            }
                        }
                    }
                }
            }

            // these do get added as project dependencies, so if these are dynamic libaries, ensure they are C.Plugin modules
            // to avoid a link step
            if (null != node.RequiredDependents)
            {
                foreach (var dependentNode in node.RequiredDependents)
                {
                    if (dependentNode.ModuleName != moduleName)
                    {
                        // TODO: want to remove this
                        lock (this.solutionFile.ProjectDictionary)
                        {
                            if (this.solutionFile.ProjectDictionary.ContainsKey(dependentNode.ModuleName))
                            {
                                var dependentProject = this.solutionFile.ProjectDictionary[dependentNode.ModuleName];
                                projectData.DependentProjects.Add(dependentProject);
                            }
                        }
                    }
                }
            }

            var applicationOptions = applicationModule.Options;

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
#if false
                    C.ILinkerOptions linkerOptions = applicationOptions as C.ILinkerOptions;
                    C.IToolchainOptions toolchainOptions = linkerOptions.ToolchainOptionCollection as C.IToolchainOptions;
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
                    // TODO: replace with 'var'
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

            configuration.Type = EProjectConfigurationType.Application;

            var toolName = "VCLinkerTool";
            var vcCLLinkerTool = configuration.GetTool(toolName);
            if (null == vcCLLinkerTool)
            {
                vcCLLinkerTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLLinkerTool);

                var linkerOptions = applicationOptions as C.LinkerOptionCollection;
                configuration.OutputDirectory = moduleToBuild.Locations[C.Application.OutputDir];
                configuration.TargetName = linkerOptions.OutputName;

                if (applicationOptions is VisualStudioProcessor.IVisualStudioSupport)
                {
                    var visualStudioProjectOption = applicationOptions as VisualStudioProcessor.IVisualStudioSupport;
                    var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (var setting in settingsDictionary)
                    {
                        vcCLLinkerTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Bam.Core.Exception("Linker options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}
