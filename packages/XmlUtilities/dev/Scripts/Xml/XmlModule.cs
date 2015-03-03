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
namespace XmlUtilities
{
    [Bam.Core.ModuleToolAssignment(typeof(IXmlWriterTool))]
    public class XmlModule :
        Bam.Core.BaseModule
    {
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("XmlFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("XmlFileDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        public
        XmlModule()
        {
            this.Document = new System.Xml.XmlDocument();
            this.Document.XmlResolver = null; // do not attempt to resolve any URLs
        }

        public System.Xml.XmlDocument Document
        {
            get;
            private set;
        }
    }
}
