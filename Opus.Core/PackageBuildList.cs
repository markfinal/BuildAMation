// <copyright file="PackageBuildList.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class PackageBuild
    {
        public PackageBuild(PackageIdentifier id)
        {
            this.Name = id.Name;
            this.Versions = new UniqueList<PackageIdentifier>();
            this.Versions.Add(id);
            this.SelectedVersion = id;
        }

        public string Name
        {
            get;
            private set;
        }

        public UniqueList<PackageIdentifier> Versions
        {
            get;
            private set;
        }

        public PackageIdentifier SelectedVersion
        {
            get;
            set;
        }
    }

    public class PackageBuildList : UniqueList<PackageBuild>
    {
        public PackageBuild GetPackage(string name)
        {
            foreach (PackageBuild i in this)
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