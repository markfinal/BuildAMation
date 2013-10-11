// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder : Opus.Core.IBuilderPostExecute
    {
        private void WriteRoot(System.IO.TextWriter writer)
        {
            writer.WriteLine("// !$*UTF8*$!");
            writer.WriteLine("{");
            writer.WriteLine("\tarchiveVersion = 1;");
            writer.WriteLine("\tclasses = {");
            writer.WriteLine("\t};");
            writer.WriteLine("\tobjectVersion = 46;");
            writer.WriteLine("\tobjects = {");
            (this.Project as IWriteableNode).Write(writer);
            writer.WriteLine("\t};");
            writer.WriteLine("\trootObject = {0} /* Project object */;", this.Project.UUID);
            writer.WriteLine("}");
        }

#region IBuilderPostExecute Members

        void Opus.Core.IBuilderPostExecute.PostExecute(Opus.Core.DependencyNodeCollection executedNodes)
        {
            if (0 == executedNodes.Count)
            {
                Opus.Core.Log.Info("No Xcode project written as there were no targets generated");
                return;
            }

            // handle any source file exclusions by diffing the source files attached to configurations
            // which those in the native target
            foreach (var target in this.Project.NativeTargets)
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

            if (!System.IO.Directory.Exists(this.Project.RootUri.AbsolutePath))
            {
                System.IO.Directory.CreateDirectory(this.Project.RootUri.AbsolutePath);
            }

            // cannot write a Byte-Ordering-Mark (BOM) into the project file
            var encoding = new System.Text.UTF8Encoding(false);
            using (var projectFile = new System.IO.StreamWriter(this.Project.Path, false, encoding) as System.IO.TextWriter)
            {
                this.WriteRoot(projectFile);
            }

            Opus.Core.Log.MessageAll("Xcode project written to '{0}'", this.Project.RootUri.AbsolutePath);

            System.IO.Directory.CreateDirectory(this.Workspace.BundlePath);
            using (var workspaceWriter = new System.IO.StreamWriter(this.Workspace.WorkspaceDataPath, false, encoding))
            {
                (this.Workspace as IWriteableNode).Write(workspaceWriter);
            }

            Opus.Core.Log.MessageAll("Xcode workspace written to '{0}'", this.Workspace.BundlePath);
        }

#endregion
    }
}
