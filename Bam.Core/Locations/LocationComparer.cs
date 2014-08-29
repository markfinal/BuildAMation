#region License
// Copyright 2010-2014 Mark Final
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
    public class LocationComparer :
        System.Collections.Generic.IEqualityComparer<Location>
    {
        #region IEqualityComparer<Location> Members

        bool
        System.Collections.Generic.IEqualityComparer<Location>.Equals(
            Location x,
            Location y)
        {
            var xLocations = x.GetLocations();
            var yLocations = y.GetLocations();
            if (xLocations.Count != yLocations.Count)
            {
                return false;
            }
            for (int i = 0; i < xLocations.Count; ++i)
            {
                bool equals = (xLocations[i].AbsolutePath.Equals(yLocations[i].AbsolutePath));
                if (!equals)
                {
                    return false;
                }
            }
            return true;
        }

        int
        System.Collections.Generic.IEqualityComparer<Location>.GetHashCode(
            Location obj)
        {
            var locations = obj.GetLocations();
            var combinedPaths = new System.Text.StringBuilder();
            foreach (var location in locations)
            {
                combinedPaths.Append(location.AbsolutePath);
            }
            return combinedPaths.ToString().GetHashCode();
        }

        #endregion
    }
}
