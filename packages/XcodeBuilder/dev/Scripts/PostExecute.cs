// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder : Opus.Core.IBuilderPostExecute
    {
#region IBuilderPostExecute Members

        void Opus.Core.IBuilderPostExecute.PostExecute(Opus.Core.DependencyNodeCollection executedNodes)
        {
            if (0 == executedNodes.Count)
            {
                Opus.Core.Log.Info("No Xcode project written as there were no targets generated");
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

                Opus.Core.Log.DebugMessage("Xcode project written to '{0}'", project.RootUri.AbsolutePath);
            }

            System.IO.Directory.CreateDirectory(this.Workspace.BundlePath);
            using (var workspaceWriter = new System.IO.StreamWriter(this.Workspace.WorkspaceDataPath, false, encoding))
            {
                (this.Workspace as IWriteableNode).Write(workspaceWriter);
            }
            var workspaceSettings = new WorkspaceSettings(this.Workspace);
            workspaceSettings.Serialize();

            Opus.Core.Log.MessageAll("Xcode workspace written to '{0}'", this.Workspace.BundlePath);
        }

#endregion
    }
}
