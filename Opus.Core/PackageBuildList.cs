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

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}: Package '{1}' with {2} versions", base.ToString(), this.Name, this.Versions.Count);
            return builder.ToString();
        }
    }

    public class PackageBuildList : UniqueList<PackageBuild>
    {
        public PackageBuild GetPackage(string name)
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