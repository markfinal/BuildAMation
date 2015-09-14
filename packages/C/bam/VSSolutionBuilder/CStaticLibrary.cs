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
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers)
        {
            if (0 == objectFiles.Count)
            {
                return;
            }

            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.V2.VSSolution;
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

            var compilerGroup = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.Compiler);
            if (objectFiles.Count > 1)
            {
                var vsConvertParameterTypes = new Bam.Core.TypeArray
                {
                    typeof(Bam.Core.Module),
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
        }
    }
}
}
