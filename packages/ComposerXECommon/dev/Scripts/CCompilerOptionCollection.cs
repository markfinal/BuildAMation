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
namespace ComposerXECommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            var compilerInterface = this as ICCompilerOptions;
            compilerInterface.AllWarnings = true;
            compilerInterface.StrictDiagnostics = true;
            compilerInterface.EnableRemarks = true;

            base.SetDefaultOptionValues(node);

            var cOptions = this as C.ICCompilerOptions;

            // there is too much of a headache with include paths to enable this!
            cOptions.IgnoreStandardIncludePaths = false;

            var target = node.Target;
            compilerInterface.SixtyFourBit = Bam.Core.OSUtilities.Is64Bit(target);

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                compilerInterface.StrictAliasing = false;
                compilerInterface.InlineFunctions = false;
            }
            else
            {
                compilerInterface.StrictAliasing = true;
                compilerInterface.InlineFunctions = true;
            }

            compilerInterface.PositionIndependentCode = false;

            var compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            cOptions.SystemIncludePaths.AddRange(compilerTool.IncludePaths((Bam.Core.BaseTarget)target));

            cOptions.TargetLanguage = C.ETargetLanguage.C;
        }

        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
