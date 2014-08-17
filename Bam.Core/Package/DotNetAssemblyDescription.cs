// <copyright file="DotNetAssemblyDescription.cs" company="Mark Final">
//  Opus.Core
// </copyright>
// <summary>Opus package definition XML file</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public class DotNetAssemblyDescription
    {
        public
        DotNetAssemblyDescription(
            string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            private set;
        }

        public string RequiredTargetFramework
        {
            get;
            set;
        }
    }
}
