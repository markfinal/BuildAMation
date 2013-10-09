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
            var mainPackage = Opus.Core.State.PackageInfo[0];
            var projectFilename = "project.pbxproj";
            var rootDirectory = System.IO.Path.Combine(Opus.Core.State.BuildRoot, mainPackage.Name) + ".xcodeproj";
            this.ProjectRootUri = new System.Uri(rootDirectory, System.UriKind.Absolute);
            var projectPath = System.IO.Path.Combine(rootDirectory, projectFilename);
            this.ProjectPath = projectPath;

            this.Project = new PBXProject(mainPackage.Name);

            // create a main group
            var mainGroup = this.Project.Groups.Get(string.Empty);
            mainGroup.SourceTree = "<group>";
            this.Project.MainGroup = mainGroup;

            // create a products group
            var productsGroup = this.Project.Groups.Get("Products");
            productsGroup.SourceTree = "<group>";
            this.Project.ProductsGroup = productsGroup;

            mainGroup.Children.Add(productsGroup);

            // create common build configurations for all targets
            // these settings are overriden by per-target build configurations
            var projectConfigurationList = this.Project.ConfigurationLists.Get(this.Project);
            this.Project.BuildConfigurationList = projectConfigurationList;
            foreach (var config in Opus.Core.State.BuildConfigurations)
            {
                var genericBuildConfiguration = this.Project.BuildConfigurations.Get(config.ToString(), "Generic");
                genericBuildConfiguration.Options["SYMROOT"].AddUnique(Opus.Core.State.BuildRoot);
                if (config == Opus.Core.EConfiguration.Debug)
                {
                    // Xcode 5 wants this setting for build performance in debug
                    genericBuildConfiguration.Options["ONLY_ACTIVE_ARCH"].AddUnique("YES");
                }
                projectConfigurationList.AddUnique(genericBuildConfiguration);
            }
        }

#endregion
    }
}
