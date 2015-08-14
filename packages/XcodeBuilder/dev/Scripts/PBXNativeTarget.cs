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
    public sealed class PBXNativeTarget :
        XcodeNodeData,
        IWriteableNode
    {
        public enum EType
        {
            ApplicationBundle,
            Executable,
            DynamicLibrary,
            Plugin,
            StaticLibrary
        }

        public
        PBXNativeTarget(
            string name,
            EType type,
            PBXProject project) : base (name)
        {
            this.Type = this.OriginalType = type;
            this.BuildPhases = new Bam.Core.Array<BuildPhase>();
            this.Dependencies = new Bam.Core.Array<PBXTargetDependency>();
            this.SourceFilesToBuild = new Bam.Core.Array<PBXBuildFile>();
            this.Project = project;
            this.RequiredTargets = new Bam.Core.Array<PBXNativeTarget>();
        }

        public EType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the original type set against the native target. This is here only because the type can be changed
        /// in some circumstances, e.g. change from Executable to ApplicationBundle
        /// </summary>
        /// <value>The original type of the target.</value>
        public EType OriginalType
        {
            get;
            private set;
        }

        public PBXFileReference ProductReference
        {
            get;
            set;
        }

        public XCConfigurationList BuildConfigurationList
        {
            get;
            set;
        }

        public Bam.Core.Array<BuildPhase> BuildPhases
        {
            get;
            private set;
        }

        public Bam.Core.Array<PBXTargetDependency> Dependencies
        {
            get;
            private set;
        }

        public Bam.Core.Array<PBXBuildFile> SourceFilesToBuild
        {
            get;
            private set;
        }

        public PBXProject Project
        {
            get;
            private set;
        }

        public PBXGroup Group
        {
            get;
            set;
        }

        public Bam.Core.Array<PBXNativeTarget> RequiredTargets
        {
            get;
            private set;
        }

        public void
        ChangeType(EType newType)
        {
            this.Type = newType;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXNativeTarget;");
            writer.WriteLine("\t\t\tbuildConfigurationList = {0} /* Build configuration list for PBXNativeTarget \"{1}\" */;", this.BuildConfigurationList.UUID, this.Name);
            writer.WriteLine("\t\t\tbuildPhases = (");
            foreach (var buildPhase in this.BuildPhases)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", buildPhase.UUID, buildPhase.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tbuildRules = (");
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tdependencies = (");
            foreach (var dependency in this.Dependencies)
            {
                writer.WriteLine("\t\t\t\t{0} /* {1} */,", dependency.UUID, dependency.GetType().Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t\tproductName = \"$(TARGET_NAME)\";");
            writer.WriteLine("\t\t\tproductReference = {0} /* {1} */;", this.ProductReference.UUID, this.ProductReference.ShortPath);
            switch (this.Type)
            {
            case EType.ApplicationBundle:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.application\";");
                break;

            case EType.Executable:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.tool\";");
                break;

            case EType.DynamicLibrary:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.library.dynamic\";");
                break;

            case EType.Plugin:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.bundle\";");
                break;

            case EType.StaticLibrary:
                writer.WriteLine("\t\t\tproductType = \"com.apple.product-type.library.static\";");
                break;

            default:
                throw new Bam.Core.Exception("Unknown product type");
            }
            writer.WriteLine("\t\t};");
        }
#endregion
    }
}
