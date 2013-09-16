// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed partial class XCodeBuilder : Opus.Core.IBuilderPreExecute
    {
#region IBuilderPreExecute Members

        void Opus.Core.IBuilderPreExecute.PreExecute()
        {
            var mainPackage = Opus.Core.State.PackageInfo[0];
            var projectFilename = "project.pbxproj";
            var rootDirectory = System.IO.Path.Combine(Opus.Core.State.BuildRoot, mainPackage.Name) + ".xcodeproj";
            this.ProjectRootUri = new System.Uri(rootDirectory, System.UriKind.Absolute);
            System.IO.Directory.CreateDirectory(rootDirectory);
            var projectPath = System.IO.Path.Combine(rootDirectory, projectFilename);
            this.ProjectPath = projectPath;

            this.Project = new PBXProject(mainPackage.Name);

            // create a products group
            var productsGroup = new PBXGroup("Products");
            productsGroup.SourceTree = "<group>";
            this.Project.ProductsGroup = productsGroup;

            // create a main group
            var mainGroup = new PBXGroup(string.Empty);
            mainGroup.SourceTree = "<group>";
            mainGroup.Children.Add(productsGroup);
            this.Project.MainGroup = mainGroup;

            // add them in this order
            this.Project.Groups.Add(mainGroup);
            this.Project.Groups.Add(productsGroup);
        }

#endregion
    }
}
