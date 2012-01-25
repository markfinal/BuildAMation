// <copyright file="IProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public interface IProject
    {
        string Name
        {
            get;
        }

        string PathName
        {
            get;
        }

        System.Guid Guid
        {
            get;
        }

        System.Collections.Generic.List<string> Platforms
        {
            get;
        }

        ProjectConfigurationCollection Configurations
        {
            get;
        }

        ProjectFileCollection SourceFiles
        {
            get;
        }

        System.Collections.Generic.List<IProject> DependentProjects
        {
            get;
        }

        VisualStudioProcessor.EVisualStudioTarget VSTarget
        {
            get;
        }

        System.Collections.Generic.List<string> References
        {
            get;
        }

        string GroupName
        {
            get;
            set;
        }

        void Serialize();
    }
}
