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
namespace Publisher
{
    /// <summary>
    /// Helper function for writing VisualStudio projects with collation
    /// </summary>
    static partial class VSSolutionSupport
    {
        public static void
        CollateObject(
            CollatedObject module)
        {
            if (module.Ignore)
            {
                return;
            }

            var collatedInterface = module as ICollatedObject;
            var collation = collatedInterface.EncapsulatingCollation;

            if (collation is Collation realCollation && realCollation.PublishingType == Collation.EPublishingType.Library)
            {
                var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
                var theproject = solution.EnsureProjectExists(realCollation.GetType(), realCollation.BuildEnvironment);
                var theconfig = theproject.GetConfiguration(realCollation);
                theconfig.SetType(VSSolutionBuilder.VSProjectConfiguration.EType.Utility);

                if (collatedInterface.SourceModule is PreExistingFile)
                {
                    theconfig.AddOtherFile(module.GeneratedPaths[CollatedObject.CopiedFileKey]);
                }

                VSSolutionBuilder.Support.AddPreBuildSteps(
                    module,
                    config: theconfig
                );
            }
            else
            {
                System.Diagnostics.Debug.Assert(null != collatedInterface.SourceModule);
                if (module.IsAnchor && !(collatedInterface.SourceModule is PreExistingObject))
                {
                    // since all dependents are copied _beside_ their anchor, the anchor copy is a no-op
                    // a no-op also applies to stripped executable copies
                    return;
                }

                var projectModule = collatedInterface.SourceModule;
                var arePostBuildCommands = true;
                // check for runtime dependencies that won't have projects, use their anchor
                if (null == projectModule.MetaData)
                {
                    if (null != collatedInterface.Anchor)
                    {
                        projectModule = collatedInterface.Anchor.SourceModule;
                    }
                    else
                    {
                        if (collatedInterface.SourceModule is PreExistingObject)
                        {
                            projectModule = (collatedInterface.SourceModule as PreExistingObject).ParentOfCollationModule;

                            // ensure a project configuration exists, as this collation may be visited prior to
                            // the source which invoked it
                            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
                            var theproject = solution.EnsureProjectExists(projectModule.GetType(), projectModule.BuildEnvironment);
                            theproject.GetConfiguration(projectModule);

                            arePostBuildCommands = false;
                        }
                        else
                        {
                            throw new Bam.Core.Exception(
                                $"No anchor set on '{module.GetType().ToString()}' with source path '{module.SourcePath}'"
                            );
                        }
                    }
                }

                var project = projectModule.MetaData as VSSolutionBuilder.VSProject;
                var config = project.GetConfiguration(projectModule);

                if (config.Type != VSSolutionBuilder.VSProjectConfiguration.EType.Utility && arePostBuildCommands)
                {
                    VSSolutionBuilder.Support.AddPostBuildSteps(
                        module,
                        config: config
                    );
                }
                else
                {
                    VSSolutionBuilder.Support.AddPreBuildSteps(
                        module,
                        config: config
                    );
                }
            }
        }
    }
}
