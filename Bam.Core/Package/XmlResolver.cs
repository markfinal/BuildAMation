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
#endregion
namespace Bam.Core
{
    internal class XmlResolver :
        System.Xml.XmlResolver
    {
        internal
        XmlResolver() : base()
        {}

        public override System.Net.ICredentials Credentials
        {
            set { throw new System.NotImplementedException(); }
        }

        public override System.Uri
        ResolveUri(
            System.Uri baseUri,
            string relativeUri)
        {
            if (baseUri == null)
            {
                return base.ResolveUri(baseUri, relativeUri);
            }

            if (!relativeUri.EndsWith(".xsd"))
            {
                throw new System.Xml.XmlException(System.String.Format("Don't know how to resolve URIs such as '{0}'", relativeUri));
            }

            if (relativeUri == State.PackageDefinitionSchemaRelativePathNameV3)
            {
                // we've got a relative path match, so build an absolute path from the executable directory
                var absolutePath = System.IO.Path.Combine(State.ExecutableDirectory, relativeUri);
                return new System.Uri(absolutePath);
            }
            else if (relativeUri == State.PackageDefinitionSchemaRelativePathNameV2) // NEW style definition files (0.50 onward)
            {
                // we've got a local match, so use the version of the Schema that is next to the application binary
                return new System.Uri(State.PackageDefinitionSchemaPathV2);
            }
            else
            {
                // OLD style definition files (pre 0.50)
                if (System.IO.File.Exists(relativeUri))
                {
                    // absolute pathname to a schema that exists on disk!
                    return base.ResolveUri(baseUri, relativeUri);
                }
                else
                {
                    // absolute pathname to a schema that doesn't exist on disk!
                    throw new System.Xml.Schema.XmlSchemaException(System.String.Format("Schema '{0}' cannot be located. Please re-run 'bam' and force a definition file update", relativeUri));
                }
            }
        }

        public override object
        GetEntity(
            System.Uri absoluteUri,
            string role,
            System.Type ofObjectToReturn)
        {
            if (absoluteUri.IsFile)
            {
                var reader = new System.IO.StreamReader(absoluteUri.LocalPath);
                return reader.BaseStream;
            }

            throw new System.Xml.XmlException(System.String.Format("Did not understand non-file URI '{0}'", absoluteUri.ToString()));
        }
    }
}
