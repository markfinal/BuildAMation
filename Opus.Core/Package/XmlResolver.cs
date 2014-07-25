// <copyright file="XmlResolver.cs" company="Mark Final">
//  Opus.Core
// </copyright>
// <summary>Opus package definition XML file</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    internal class XmlResolver :
        System.Xml.XmlResolver
    {
        internal
        XmlResolver() :
        base()
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

            // NEW style definition files (0.50 onward)
            if (relativeUri == State.OpusPackageDependencySchemaRelativePathNameV2)
            {
                // we've got a local match, so use the version of the Schema that is next to the Opus binary
                return new System.Uri(State.OpusPackageDependencySchemaPathNameV2);
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
                    throw new System.Xml.Schema.XmlSchemaException(System.String.Format("Schema '{0}' cannot be located. Please re-run the Opus command and force a definition file update", relativeUri));
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
            else
            {
                if ("http://code.google.com/p/opus" == absoluteUri.ToString())
                {
                    var reader = new System.IO.StreamReader(State.OpusPackageDependencySchemaPathNameV2);
                    return reader.BaseStream;
                }

                throw new System.Xml.XmlException(System.String.Format("Did not understand non-file URI '{0}'", absoluteUri.ToString()));
            }
        }
    }
}
