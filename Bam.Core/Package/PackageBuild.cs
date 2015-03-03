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
    public class PackageBuild
    {
        public
        PackageBuild(
            PackageIdentifier id)
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

        public override string
        ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendFormat("{0}: Package '{1}' with {2} versions", base.ToString(), this.Name, this.Versions.Count);
            return builder.ToString();
        }
    }
}
