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
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder :
        Bam.Core.IBuilderPostExecute
    {
#region IBuilderPostExecute Members

        void
        Bam.Core.IBuilderPostExecute.PostExecute(
            Bam.Core.DependencyNodeCollection executedNodes)
        {
            if (0 == executedNodes.Count)
            {
                Bam.Core.Log.Info("No Xcode project written as there were no targets generated");
                return;
            }

            // cannot write a Byte-Ordering-Mark (BOM) into the project file
            var encoding = new System.Text.UTF8Encoding(false);

            foreach (var project in this.Workspace.Projects)
            {
                // handle any source file exclusions by diffing the source files attached to configurations
                // which those in the native target
                foreach (var target in project.NativeTargets)
                {
                    var nativeTarget = target as PBXNativeTarget;
                    var allSource = nativeTarget.SourceFilesToBuild;
                    foreach (var config in nativeTarget.BuildConfigurationList.BuildConfigurations)
                    {
                        var buildConfig = config as XCBuildConfiguration;
                        if (0 == buildConfig.SourceFiles.Count)
                        {
                            continue;
                        }

                        var complement = allSource.Complement(buildConfig.SourceFiles);
                        foreach (var source in complement)
                        {
                            buildConfig.Options["EXCLUDED_SOURCE_FILE_NAMES"].AddUnique(source.FileReference.ShortPath);
                        }
                    }
                }

                if (!System.IO.Directory.Exists(project.RootUri.AbsolutePath))
                {
                    System.IO.Directory.CreateDirectory(project.RootUri.AbsolutePath);
                }

                using (var projectFileWriter = new System.IO.StreamWriter(project.Path, false, encoding) as System.IO.TextWriter)
                {
                    (project as IWriteableNode).Write(projectFileWriter);
                }

                if (project.NativeTargets.Count > 0)
                {
                    if ((bool)Bam.Core.State.Get("XcodeBuilder", "WarmSchemeCache") || true)
                    {
                        var projectSchemeCache = new ProjectSchemeCache(project);
                        projectSchemeCache.Serialize();

                        Bam.Core.Log.DebugMessage("Xcode project scheme caches have been warmed");
                    }
                }

                Bam.Core.Log.DebugMessage("Xcode project written to '{0}'", project.RootUri.AbsolutePath);
            }

            System.IO.Directory.CreateDirectory(this.Workspace.BundlePath);
            using (var workspaceWriter = new System.IO.StreamWriter(this.Workspace.WorkspaceDataPath, false, encoding))
            {
                (this.Workspace as IWriteableNode).Write(workspaceWriter);
            }

            // the workspace settings is always written out, due to the location in which targets are expected
            // to be built
            var workspaceSettings = new WorkspaceSettings(this.Workspace);
            workspaceSettings.Serialize();

#if false
            Bam.Core.Log.Info("Successfully created Xcode workspace for package '{0}'\n\t{1}", Bam.Core.State.PackageInfo[0].Name, this.Workspace.BundlePath);
#endif
        }

#endregion
    }
}
