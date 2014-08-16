// <copyright file="PackageBuildList.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public class PackageBuildList :
        UniqueList<PackageBuild>
    {
        public PackageBuild
        GetPackage(
            string name)
        {
            foreach (var i in this)
            {
                if (i.Name == name)
                {
                    return i;
                }
            }

            return null;
        }
    }
}
