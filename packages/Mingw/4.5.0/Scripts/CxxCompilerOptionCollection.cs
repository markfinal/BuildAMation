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
namespace Mingw
{
    // this implementation is here because the specific version of the Mingw compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CxxCompilerOptionCollection :
        CCompilerOptionCollection,
        C.ICxxCompilerOptions
    {
        public
        CxxCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // TODO: can this be moved to MingwCommon? (difference in root C folders)
            var target = node.Target;
            var mingwToolset = target.Toolset as MingwCommon.Toolset;

            var cCompilerOptions = this as C.ICCompilerOptions;

            // using [1] as we want the one in the lib folder
            var cppIncludePath = System.IO.Path.Combine(mingwToolset.MingwDetail.IncludePaths[1], "c++");
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath);

            var cppIncludePath2 = System.IO.Path.Combine(cppIncludePath, mingwToolset.MingwDetail.Target);
            cCompilerOptions.SystemIncludePaths.Add(cppIncludePath2);

            MingwCommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }
    }
}
