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
            this.Project = new PBXProject(mainPackage.Name);

            // create the project configuration lists
            foreach (var configuration in Opus.Core.State.BuildConfigurations)
            {
                var configurationName = configuration.ToString();
                var buildConfiguration = this.Project.BuildConfigurations.Get(configurationName, "PBXProjectRoot");

                var projectConfigurationList = this.Project.ConfigurationLists.Get(configurationName, this.Project);
                projectConfigurationList.AddUnique(buildConfiguration);
                this.Project.BuildConfigurationList = projectConfigurationList;
            }

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
