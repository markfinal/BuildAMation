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
namespace ComposerXE
{
    // this implementation is here because the specific version of the ComposerXE compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CxxCompilerOptionCollection :
        CCompilerOptionCollection,
        C.ICxxCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);
            ComposerXECommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }

        public
        CxxCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
