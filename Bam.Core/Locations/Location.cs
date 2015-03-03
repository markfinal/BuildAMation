#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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
