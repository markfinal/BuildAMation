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
namespace VisualStudioProcessor
{
#if BAM_V2
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class EnumAttribute :
        System.Attribute
    {
        public EnumAttribute(
            object key,
            string value)
        {
            this.Key = key as System.Enum;
            this.Value = value;
        }

        public System.Enum Key
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }
    }

    public static class VSSolutionCompile
    {
        public static void
        Execute(
            Bam.Core.Module module)
        {
            var encapsulating = module.GetEncapsulatingReferencedModule();

            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
            var project = solution.EnsureProjectExists(encapsulating);
            var config = project.GetConfiguration(encapsulating);

            var group = (module is C.WinResource) ?
                VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Resource :
                VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.Compiler;

            var settingsGroup = config.GetSettingsGroup(
                group,
                include: (module as C.IRequiresSourceModule).Source.GeneratedPaths[C.SourceFile.Key],
                uniqueToProject: true
            );

            var intDir = module.CreateTokenizedString(
                "@trimstart(@relativeto($(0),$(packagebuilddir)/$(moduleoutputdir)),../)",
                module.GeneratedPaths[C.ObjectFileBase.Key]
            );
            intDir.Parse();
            settingsGroup.AddSetting("ObjectFileName", "$(IntDir)" + intDir.ToString());
            if (!(module as C.ObjectFileBase).PerformCompilation)
            {
                settingsGroup.AddSetting("ExcludedFromBuild", true);
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
                var dependentProject = dependent.MetaData as VSSolutionBuilder.VSProject;
                if (null != dependentProject)
                {
                    config.RequiresProject(dependentProject);
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
            Bam.Core.Settings settings,
            Bam.Core.Module module,
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition)
        {
            var moduleType = typeof(Bam.Core.Module);
            var vsSettingsGroupType = typeof(VSSolutionBuilder.VSSettingsGroup);
            var stringType = typeof(string);
            foreach (var i in settings.Interfaces())
            {
                var method = conversionClass.GetMethod("Convert", new[] { i, moduleType, vsSettingsGroupType, stringType });
                if (null == method)
                {
                    throw new Bam.Core.Exception("Unable to locate method {0}.Convert({1}, {2}, {3})",
                        conversionClass.ToString(),
                        i.ToString(),
                        moduleType,
                        vsSettingsGroupType,
                        stringType);
                }
                try
                {
                    method.Invoke(null, new object[] { settings, module, vsSettingsGroup, condition });
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    throw new Bam.Core.Exception(exception.InnerException, "VisualStudio conversion error:");
                }
            }
        }
    }
}
