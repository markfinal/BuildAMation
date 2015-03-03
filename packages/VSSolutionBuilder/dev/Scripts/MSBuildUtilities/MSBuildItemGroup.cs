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
namespace VSSolutionBuilder
{
    public class MSBuildItemGroup :
        MSBuildBaseElement
    {
        public
        MSBuildItemGroup(
            System.Xml.XmlDocument document) : base(document, "ItemGroup")
        {}

        public MSBuildItem
        CreateItem(
            string name,
            string include)
        {
            var item = new MSBuildItem(this.XmlDocument, name, include);
            this.AppendChild(item);
            return item;
        }

        public MSBuildItem
        FindItem(
            string name,
            string include)
        {
            // TODO: convert to var
            foreach (System.Xml.XmlElement element in this.XmlElement.ChildNodes)
            {
                var b = this.childElements[element] as MSBuildItem;
                if ((b.Name == name) && (b.Include == include))
                {
                    return b;
                }
            }
            return null;
        }
    }
}
