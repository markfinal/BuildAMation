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
    public sealed class XCConfigurationListSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        XCConfigurationListSection()
        {
            this.ConfigurationLists = new System.Collections.Generic.List<XCConfigurationList>();
        }

        public void
        Add(
            XCConfigurationList configurationList)
        {
            lock (this.ConfigurationLists)
            {
                this.ConfigurationLists.Add(configurationList);
            }
        }

        public XCConfigurationList
        Get(
            XcodeNodeData owner)
        {
            lock (this.ConfigurationLists)
            {
                foreach (var configurationList in this.ConfigurationLists)
                {
                    if (configurationList.Owner == owner)
                    {
                        return configurationList;
                    }
                }

                var newConfigurationList = new XCConfigurationList(owner);
                this.ConfigurationLists.Add(newConfigurationList);
                return newConfigurationList;
            }
        }

        private System.Collections.Generic.List<XCConfigurationList> ConfigurationLists
        {
            get;
            set;
        }

        #region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.ConfigurationLists.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<XCConfigurationList>(this.ConfigurationLists);
            orderedList.Sort(
                delegate(XCConfigurationList p1, XCConfigurationList p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin XCConfigurationList section */");
            foreach (var configurationList in orderedList)
            {
                (configurationList as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End XCConfigurationList section */");
        }
        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.ConfigurationLists.GetEnumerator();
        }

        #endregion
    }
}
