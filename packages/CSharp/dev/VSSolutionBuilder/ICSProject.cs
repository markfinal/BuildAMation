// <copyright file="ICSProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public interface ICSProject : IProject
    {
        ProjectFile ApplicationDefinition
        {
            get;
            set;
        }

        ProjectFileCollection Pages
        {
            get;
        }
    }
}
