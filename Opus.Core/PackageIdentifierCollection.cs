// <copyright file="PackageIdentifierCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class PackageIdentifierCollection : Array<PackageIdentifier>
    {
        public override void Add(PackageIdentifier item)
        {
            if (this.Contains(item))
            {
                throw new Exception(System.String.Format("Package '{0}' already present in the collection", item.ToString()), false);
            }

            base.Add(item);
        }

        public override bool Contains(PackageIdentifier item)
        {
            bool baseContainsIt = base.Contains(item);
            if (baseContainsIt)
            {
                return true;
            }

            foreach (PackageIdentifier id in this.list)
            {
                if (id.Match(item, true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}