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
    public sealed class XCConfigurationList :
        XcodeNodeData,
        IWriteableNode
    {
        public
        XCConfigurationList(
            XcodeNodeData owner) : base(owner.Name)
        {
            this.Owner = owner;
            this.BuildConfigurations = new Bam.Core.Array<XCBuildConfiguration>();
        }

        public XcodeNodeData Owner
        {
            get;
            private set;
        }

        public void
        AddUnique(
            XCBuildConfiguration configuration)
        {
            lock (this.BuildConfigurations)
            {
                if (!this.BuildConfigurations.Contains(configuration))
                {
                    this.BuildConfigurations.Add(configuration);
                }
            }
        }

        public Bam.Core.Array<XCBuildConfiguration> BuildConfigurations
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(System.IO.TextWriter writer)
        {
            if (null == this.Owner)
            {
                throw new Bam.Core.Exception("Owner of this configuration list has not been set");
            }

            writer.WriteLine("\t\t{0} /* Build configuration list for {1} \"{2}\" */ = {{", this.UUID, this.Owner.GetType().Name, this.Owner.Name);
            writer.WriteLine("\t\t\tisa = XCConfigurationList;");
            writer.WriteLine("\t\t\tbuildConfigurations = (");
            foreach (var configuration in this.BuildConfigurations)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", configuration.UUID, configuration.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tdefaultConfigurationIsVisible = 0;");
            writer.WriteLine("\t\t\tdefaultConfigurationName = {0};", this.BuildConfigurations[0].Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
