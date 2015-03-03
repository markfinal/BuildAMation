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
namespace XcodeBuilder
{
    public sealed class XCConfigurationList :
        XcodeNodeData,
        IWriteableNode
    {
        public
        XCConfigurationList(
            XcodeNodeData owner) : base(owner.Name)
        {
            this.Owner = owner;
            this.BuildConfigurations = new Bam.Core.Array<XCBuildConfiguration>();
        }

        public XcodeNodeData Owner
        {
            get;
            private set;
        }

        public void
        AddUnique(
            XCBuildConfiguration configuration)
        {
            lock (this.BuildConfigurations)
            {
                if (!this.BuildConfigurations.Contains(configuration))
                {
                    this.BuildConfigurations.Add(configuration);
                }
            }
        }

        public Bam.Core.Array<XCBuildConfiguration> BuildConfigurations
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (null == this.Owner)
            {
                throw new Bam.Core.Exception("Owner of this configuration list has not been set");
            }

            writer.WriteLine("\t\t{0} /* Build configuration list for {1} \"{2}\" */ = {{", this.UUID, this.Owner.GetType().Name, this.Owner.Name);
            writer.WriteLine("\t\t\tisa = XCConfigurationList;");
            writer.WriteLine("\t\t\tbuildConfigurations = (");
            foreach (var configuration in this.BuildConfigurations)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", configuration.UUID, configuration.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tdefaultConfigurationIsVisible = 0;");
            writer.WriteLine("\t\t\tdefaultConfigurationName = {0};", this.BuildConfigurations[0].Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
