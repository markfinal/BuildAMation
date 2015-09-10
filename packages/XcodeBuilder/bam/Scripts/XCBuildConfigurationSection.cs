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
