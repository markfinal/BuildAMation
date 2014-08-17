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
