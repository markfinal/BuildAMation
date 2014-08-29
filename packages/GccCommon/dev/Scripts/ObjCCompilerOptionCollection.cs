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
namespace GccCommon
{
    public class ObjCCompilerOptionCollection :
        CCompilerOptionCollection
    {
        public static void
        ExportedDefaults(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.DependencyNode node)
        {
            var cInterfaceOptions = options as C.ICCompilerOptions;
            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.ObjectiveC;
            var gccInterfaceOptions = options as ICCompilerOptions;
            gccInterfaceOptions.StrictAliasing = false; // causes type-punning warnings with 'super' in 4.0
        }

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);
            ExportedDefaults(this, node);
        }

        public
        ObjCCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
