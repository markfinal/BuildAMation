#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
