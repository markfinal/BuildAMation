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
    public sealed class XCBuildConfigurationSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        XCBuildConfigurationSection()
        {
            this.BuildConfigurations = new System.Collections.Generic.List<XCBuildConfiguration>();
        }

        public void
        Add(
            XCBuildConfiguration target)
        {
            lock (this.BuildConfigurations)
            {
                this.BuildConfigurations.Add(target);
            }
        }

        public XCBuildConfiguration
        Get(
            string name,
            string moduleName)
        {
            lock(this.BuildConfigurations)
            {
                foreach (var buildConfiguration in this.BuildConfigurations)
                {
                    if ((buildConfiguration.Name == name) && (buildConfiguration.ModuleName == moduleName))
                    {
                        return buildConfiguration;
                    }
                }

                var newBuildConfiguration = new XCBuildConfiguration(name, moduleName);
                this.Add(newBuildConfiguration);
                return newBuildConfiguration;
            }
        }

        private System.Collections.Generic.List<XCBuildConfiguration> BuildConfigurations
        {
            get;
            set;
        }

        #region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.BuildConfigurations.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<XCBuildConfiguration>(this.BuildConfigurations);
            orderedList.Sort(
                delegate(XCBuildConfiguration p1, XCBuildConfiguration p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin XCBuildConfiguration section */");
            foreach (var buildConfiguration in orderedList)
            {
                (buildConfiguration as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End XCBuildConfiguration section */");
        }
        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.BuildConfigurations.GetEnumerator();
        }

        #endregion
    }
}
