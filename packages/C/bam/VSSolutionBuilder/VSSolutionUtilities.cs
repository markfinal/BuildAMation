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
namespace C
{
#if BAM_V2
    public static partial class VSSolutionSupport
    {
        public static void
        ProcessLibraryDependencies(
            ConsoleApplication module,
            VSSolutionBuilder.VSProjectConfiguration config,
            VSSolutionBuilder.VSSolution solution)
        {
            foreach (var input in module.Libraries)
            {
                if ((null != input.MetaData) && VSSolutionBuilder.VSProject.IsBuildable(input))
                {
                    if ((input is C.StaticLibrary) || (input is C.IDynamicLibrary))
                    {
                        config.LinkAgainstProject(solution.EnsureProjectExists(input));
                    }
                    else if ((input is C.CSDKModule) || (input is C.HeaderLibrary))
                    {
                        continue;
                    }
                    else if (input is OSXFramework)
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
                    if (input is C.StaticLibrary)
                    {
                        // TODO: probably a simplification of the DLL codepath
                        throw new System.NotImplementedException();
                    }
                    else if (input is C.IDynamicLibrary)
                    {
                        // TODO: this might be able to shift out of the conditional
                        (module.Tool as C.LinkerTool).ProcessLibraryDependency(
                            module as CModule,
                            input as CModule
                        );
                    }
                    else if ((input is C.CSDKModule) || (input is C.HeaderLibrary))
                    {
                        continue;
                    }
                    else if (input is OSXFramework)
                    {
                        throw new Bam.Core.Exception("Frameworks are not supported on Windows: {0}", input.ToString());
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Don't know how to handle this prebuilt library module, {0}", input.ToString());
                    }
                }
            }
        }

        public static void
        LinkOrArchive(
            out VSSolutionBuilder.VSSolution solution,
            out VSSolutionBuilder.VSProjectConfiguration config,
            CModule module,
            VSSolutionBuilder.VSProjectConfiguration.EType type,
            System.Collections.Generic.IEnumerable<Bam.Core.Module> objectFiles,
            System.Collections.Generic.IEnumerable<Bam.Core.Module> headerFiles)
        {
            // early out
            if (!objectFiles.Any())
            {
                solution = null;
                config = null;
                return;
            }

            solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
            var project = solution.EnsureProjectExists(module);
            config = project.GetConfiguration(module);

            // ensure the project type is accurate
            config.SetType(type);
            if (module.Settings is ICommonHasOutputPath)
            {
                config.SetOutputPath((module.Settings as ICommonHasOutputPath).OutputPath);
            }
            config.EnableIntermediatePath();

            foreach (var header in headerFiles)
            {
                config.AddHeaderFile(header as HeaderFile);
            }

            var compilerGroup = config.GetSettingsGroup(VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Compiler);

            // add real C/C++ source files to the project
            var realObjectFiles = objectFiles.Where(item => item is ObjectFile);
            if (realObjectFiles.Any())
            {
                var sharedSettings = C.SettingsBase.SharedSettings(
                    realObjectFiles);
                VisualStudioProcessor.VSSolutionConversion.Convert(
                    sharedSettings,
                    realObjectFiles.First().Settings.GetType(),
                    module,
                    compilerGroup,
                    config
                );

                foreach (var objFile in realObjectFiles)
                {
                    var deltaSettings = (objFile.Settings as C.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    config.AddSourceFile(config, objFile, deltaSettings);
                }
            }

            // add windows resource files
            foreach (var winResObj in objectFiles.Where(item => item is WinResource))
            {
                config.AddResourceFile(config, winResObj as WinResource, winResObj.Settings);
            }

            // add assembly files
            foreach (var asmObj in objectFiles.Where(item => item is AssembledObjectFile))
            {
                config.AddAssemblyFile(config, asmObj as AssembledObjectFile, asmObj.Settings);
            }

            return;
        }

        public static void
        AddOrderOnlyDependentProjects(
            Bam.Core.Module module,
            VSSolutionBuilder.VSProjectConfiguration config)
        {
            var required_projects = new System.Collections.Generic.HashSet<VSSolutionBuilder.VSProject>();
            // order only dependencies - recurse into each, so that all layers
            // of order only dependencies are included
            var queue = new System.Collections.Generic.Queue<Bam.Core.Module>(module.Requirements);
            while (queue.Count > 0)
            {
                var required = queue.Dequeue();
                foreach (var additional in required.Requirements)
                {
                    queue.Enqueue(additional);
                }

                if (null == required.MetaData)
                {
                    continue;
                }

                var requiredProject = required.MetaData as VSSolutionBuilder.VSProject;
                if (null != requiredProject)
                {
                    required_projects.Add(requiredProject);
                }
            }
            // any non-C module projects should be order-only dependencies
            foreach (var dependent in module.Dependents)
            {
                if (null == dependent.MetaData)
                {
                    continue;
                }
                if (dependent is C.CModule)
                {
                    continue;
                }
                var dependentProject = dependent.MetaData as VSSolutionBuilder.VSProject;
                if (null != dependentProject)
                {
                    required_projects.Add(dependentProject);
                }
            }
            // however, there may be forwarded libraries, and these are useful order only dependents
            if (module is IForwardedLibraries)
            {
                foreach (var dependent in (module as IForwardedLibraries).ForwardedLibraries)
                {
                    if (null == dependent.MetaData)
                    {
                        continue;
                    }
                    var dependentProject = dependent.MetaData as VSSolutionBuilder.VSProject;
                    if (null != dependentProject)
                    {
                        required_projects.Add(dependentProject);
                    }
                }
            }
            foreach (var proj in required_projects)
            {
                config.RequiresProject(proj);
            }
        }
    }
#endif
}
