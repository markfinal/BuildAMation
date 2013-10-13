// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder : Opus.Core.IBuilderPreExecute
    {
#region IBuilderPreExecute Members

        void Opus.Core.IBuilderPreExecute.PreExecute()
        {
            this.Workspace = new Workspace();

            var mainPackage = Opus.Core.State.PackageInfo[0];
            var projectFilename = "project.pbxproj";
            var rootDirectory = System.IO.Path.Combine(Opus.Core.State.BuildRoot, mainPackage.FullName);
            rootDirectory = System.IO.Path.Combine(rootDirectory, mainPackage.Name) + ".xcodeproj";
            var projectRootUri = new System.Uri(rootDirectory, System.UriKind.Absolute);
            var projectPath = System.IO.Path.Combine(rootDirectory, projectFilename);

            var project = new PBXProject(mainPackage.Name, projectRootUri, projectPath);
            this.Workspace.Projects.Add(project);
        }

#endregion
    }
}
