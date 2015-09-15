#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    internal class XmlResolver :
        System.Xml.XmlResolver
    {
        internal XmlResolver() :
            base()
        {}

        public override System.Net.ICredentials Credentials
        {
            set
            {
                throw new System.NotImplementedException();
            }
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

            if (relativeUri == State.PackageDefinitionSchemaRelativePath)
            {
                // we've got a relative path match, so build an absolute path from the executable directory
                var absolutePath = System.IO.Path.Combine(State.ExecutableDirectory, relativeUri);
                return new System.Uri(absolutePath);
            }

            throw new System.Xml.Schema.XmlSchemaException(System.String.Format("Schema '{0}' cannot be located. Please re-run 'bam' and force a definition file update", relativeUri));
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
