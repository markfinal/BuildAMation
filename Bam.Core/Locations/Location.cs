// <copyright file="Location.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    /// <summary>
    /// Location is the abstract base class that can represent any of the higher form of Locations.
    /// </summary>
    public abstract class Location
    {
        public enum EExists
        {
            Exists,
            WillExist
        }

        public abstract Location
        SubDirectory(
            string subDirName);

        public abstract Location
        SubDirectory(
            string subDirName,
            EExists exists);

        public virtual string AbsolutePath
        {
            get;
            protected set;
        }

        public abstract LocationArray
        GetLocations();

        public abstract bool IsValid
        {
            get;
        }

        public EExists Exists
        {
            get;
            protected set;
        }

        /// <summary>
        /// This is a special case of GetLocations. It requires a Location resolves to a single path, which is returned.
        /// If the path contains a space, it will be returned quoted.
        /// </summary>
        /// <returns>Single path of the resolved Location</returns>
        public string
        GetSinglePath()
        {
            var path = this.GetSingleRawPath();
            if (path.Contains(" "))
            {
                path = System.String.Format("\"{0}\"", path);
            }
            return path;
        }

        /// <summary>
        /// This is a special case of GetLocations. It requires a Location resolves to a single path, which is returned.
        /// No checking whether the path contains spaces is performed.
        /// </summary>
        /// <returns>Single path of the resolved Location</returns>
        public string
        GetSingleRawPath()
        {
            var resolvedLocations = this.GetLocations();
            if (resolvedLocations.Count != 1)
            {
                throw new Exception("Location '{0}' did not resolve just one path, but {1}", this.ToString(), resolvedLocations.Count);
            }
            var path = resolvedLocations[0].AbsolutePath;
            return path;
        }
    }
}
