#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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

            Bam.Core.Log.Info("Successfully created Xcode workspace for package '{0}'\n\t{1}", Bam.Core.State.PackageInfo[0].Name, this.Workspace.BundlePath);
        }

#endregion
    }
}
