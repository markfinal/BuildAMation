#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace XcodeBuilder
{
    /// <summary>
    /// Class representing the user data in the workspace
    /// </summary>
    sealed class UserWorkspaceSettings :
        WorkspaceSettings
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="workspaceDir">Directory for the workspace</param>
        public
        UserWorkspaceSettings(
            string workspaceDir)
        {
            this.Path = workspaceDir;
            this.Path = System.IO.Path.Combine(this.Path, "xcuserdata");
            this.Path = System.IO.Path.Combine(this.Path, $"{System.Environment.GetEnvironmentVariable("USER")}.xcuserdatad");
            this.Path = System.IO.Path.Combine(this.Path, "WorkspaceSettings.xcsettings");
            this.CreatePlist();
        }

        /// <summary>
        /// Create a PList
        /// </summary>
        protected override void
        CreatePlist()
        {
            var doc = new System.Xml.XmlDocument();
            // don't resolve any URLs, or if there is no internet, the process will pause for some time
            doc.XmlResolver = null;

            {
                var type = doc.CreateDocumentType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
                doc.AppendChild(type);
            }
            var plistEl = doc.CreateElement("plist");
            {
                var versionAttr = doc.CreateAttribute("version");
                versionAttr.Value = "1.0";
                plistEl.Attributes.Append(versionAttr);
            }

            var dictEl = doc.CreateElement("dict");
            plistEl.AppendChild(dictEl);
            doc.AppendChild(plistEl);

            // this appears to be the only way to make Xcode save out in
            // in a BAM-like directory hierarchy
            CreateKeyValuePair(doc, dictEl, "BuildLocationStyle", "UseTargetSettings");

            this.Document = doc;
        }
    }
}
