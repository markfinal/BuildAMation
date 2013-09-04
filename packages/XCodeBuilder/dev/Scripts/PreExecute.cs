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
        }

#endregion
    }
}
