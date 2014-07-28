// <copyright file="ICProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public interface ICProject :
        IProject
    {
        ProjectFileCollection HeaderFiles
        {
            get;
        }

        ProjectFileCollection ResourceFiles
        {
            get;
        }
    }
}
