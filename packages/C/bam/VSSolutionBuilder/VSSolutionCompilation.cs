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
namespace C
{
    /// <summary>
    /// Utility class offering support for VisualStudio project generation
    /// </summary>
    public static partial class VSSolutionSupport
    {
        /// <summary>
        /// Create VisualStudio project data for compiling a source file
        /// </summary>
        /// <param name="module">Module containing the ObjectFile</param>
        public static void
        Compile(
            ObjectFileBase module)
        {
            var encapsulating = module.EncapsulatingModule;

            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
            var project = solution.EnsureProjectExists(encapsulating);
            var config = project.GetConfiguration(encapsulating);

            VSSolutionBuilder.VSSettingsGroup.ESettingsGroup group;
            if (module is C.WinResource)
            {
                group = VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Resource;
            }
            else if (module is C.AssembledObjectFile)
            {
                group = VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Assembler;
            }
            else if (module is C.ObjectFile)
            {
                group = VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Compiler;
            }
            else
            {
                throw new Bam.Core.Exception($"Unknown settings group for module {module.ToString()}");
            }

            var settingsGroup = config.GetSettingsGroup(
                group,
                include: (module as C.IRequiresSourceModule).Source.GeneratedPaths[C.SourceFile.SourceFileKey],
                uniqueToProject: true
            );

#if false
            // must specify an ObjectFileName, in case two source files have the same leafname
            // which would then map to the same output file without intervention
            // note that this doesn't seem to work for assembly files - results in weird errors
            if (!(module is AssembledObjectFile))
            {
                var intDir = module.CreateTokenizedString(
                    "@trimstart(@relativeto($(0),$(packagebuilddir)/$(moduleoutputdir)),../)",
                    module.GeneratedPaths[C.ObjectFileBase.Key]
                );
                intDir.Parse();
                settingsGroup.AddSetting(
                    "ObjectFileName",
                    "$(IntDir)" + intDir.ToString()
                );
            }
#endif

            if (!module.PerformCompilation)
            {
                settingsGroup.AddSetting(
                    "ExcludedFromBuild",
                    true
                );
            }
            module.MetaData = settingsGroup;

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
                if (dependent.MetaData is VSSolutionBuilder.VSProject dependentProject)
                {
                    config.RequiresProject(dependentProject);
                }
            }
        }
    }
}
