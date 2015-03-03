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
    public class MSBuildItem :
        MSBuildBaseElement
    {
        private string include = null;

        public
        MSBuildItem(
            System.Xml.XmlDocument document,
            string name,
            string include) : base(document, name)
        {
            // null is only valid when used in an item definition group
            if (null != include)
            {
                this.include = include;
                this.XmlElement.SetAttribute("Include", include);
            }
        }

        public string Include
        {
            get
            {
                return this.include;
            }
        }

        public MSBuildMetaData
        CreateMetaData(
            string name,
            string value)
        {
            var metaData = new MSBuildMetaData(this.XmlDocument, name, value);
            this.AppendChild(metaData);
            return metaData;
        }
    }
}
