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
#endregion // License
namespace VisualCCommon
{
    // TODO: this does not implement any options interface
    public sealed partial class Win32ResourceCompilerOptionCollection :
        C.Win32ResourceCompilerOptionCollection,
        VisualStudioProcessor.IVisualStudioSupport
    {
        public
        Win32ResourceCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        VisualStudioProcessor.ToolAttributeDictionary
        VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(
            Bam.Core.Target target)
        {
            var vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
            case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
            case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                break;

            default:
                throw new Bam.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            var dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}
