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
                        (module.Tool as C.LinkerTool).ProcessLibraryDependency(
                            module as CModule,
                            input as CModule
                        );
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
            System.Collections.Generic.IEnumerable<Bam.Core.Module> headerFiles)
        {
            // early out
            if (!module.InputModules.Any())
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
            config.EnableIntermediatePath();

            foreach (var header in headerFiles)
            {
                config.AddHeaderFile(header as HeaderFile);
            }

            var compilerGroup = config.GetSettingsGroup(VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Compiler);

            // add real C/C++ source files to the project
            var realObjectFiles = module.InputModules.Select(item => item.Value).Where(item => item is ObjectFile);
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
                    config.AddSourceFile(objFile, deltaSettings);
                }
            }

            // add windows resource files
            foreach (var winResObj in module.InputModules.Select(item => item.Value).Where(item => item is WinResource))
            {
                config.AddResourceFile(winResObj as WinResource, winResObj.Settings);
            }

            // add assembly files
            foreach (var asmObj in module.InputModules.Select(item => item.Value).Where(item => item is AssembledObjectFile))
            {
                config.AddAssemblyFile(asmObj as AssembledObjectFile, asmObj.Settings);
            }
        }

        public static void
        AddOrderOnlyDependentProjects(
            C.CModule module,
            VSSolutionBuilder.VSProjectConfiguration config)
        {
            var order_only_projects =
                module.OrderOnlyDependents().
                Distinct().
                Where(item => item.MetaData != null && item.MetaData is VSSolutionBuilder.VSProject).
                Select(item => item.MetaData as VSSolutionBuilder.VSProject);
            foreach (var proj in order_only_projects)
            {
                config.RequiresProject(proj);
            }
        }
    }
#endif
}
