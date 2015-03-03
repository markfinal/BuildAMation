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
    public class ProxyModulePath
    {
        private StringArray pathSegments;

        public
        ProxyModulePath()
        {}

        public
        ProxyModulePath(
            params string[] segments)
        {
            this.pathSegments = new StringArray(segments);
        }

        public bool Empty
        {
            get
            {
                return this.pathSegments == null;
            }
        }

        public void
        Assign(
            params string[] segments)
        {
            this.pathSegments = new StringArray(segments);
        }

        public void
        Assign(
            ProxyModulePath proxy)
        {
            if (null == proxy.pathSegments)
            {
                return;
            }

            this.pathSegments = new StringArray(proxy.pathSegments);
        }

        public DirectoryLocation
        Combine(
            Location baseLocation)
        {
            if (null == this.pathSegments)
            {
                return baseLocation as DirectoryLocation;
            }

            var offset = this.pathSegments.ToString(System.IO.Path.DirectorySeparatorChar);
            var basePath = baseLocation.AbsolutePath;
            var combined = System.IO.Path.Combine(basePath, offset);
            return DirectoryLocation.Get(System.IO.Path.GetFullPath(combined));
        }
    }
}
