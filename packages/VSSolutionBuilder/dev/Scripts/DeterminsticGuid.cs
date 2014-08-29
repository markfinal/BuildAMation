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
namespace VSSolutionBuilder
{
    public class DeterministicGuid
    {
        public
        DeterministicGuid(
            string input)
        {
            // ref: http://geekswithblogs.net/EltonStoneman/archive/2008/06/26/generating-deterministic-guids.aspx

            // use MD5 hash to get a 16-byte hash of the string
            var provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var inputBytes = System.Text.Encoding.Default.GetBytes(input);
            var hashBytes = provider.ComputeHash(inputBytes);

            // generate a guid from the hash
            var hashGuid = new System.Guid(hashBytes);

            this.Guid = hashGuid;
        }

        public System.Guid Guid
        {
            get;
            private set;
        }
    }
}
