#region License
// Copyright 2010-2015 Mark Final
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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator.
// Command line arguments:
//     -i=ICCompilerOptions.cs
//     -n=LLVMGcc
//     -c=ObjCCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=GccCommon.PrivateData
//     -e
#endregion // BamOptionGenerator
namespace LLVMGcc
{
    public partial class ObjCCompilerOptionCollection
    {
        #region ICCompilerOptions Option delegates
        private static void
        VisibilityCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // requires gcc 4.0
            var enumOption = option as Bam.Core.ValueTypeOption<EVisibility>;
            switch (enumOption.Value)
            {
                case EVisibility.Default:
                commandLineBuilder.Add("-fvisibility=default");
                break;
                case EVisibility.Hidden:
                commandLineBuilder.Add("-fvisibility=hidden");
                break;
                case EVisibility.Internal:
                commandLineBuilder.Add("-fvisibility=internal");
                break;
                case EVisibility.Protected:
                commandLineBuilder.Add("-fvisibility=protected");
                break;
                default:
                throw new Bam.Core.Exception("Unrecognized visibility option");
            }
        }
        private static void
        VisibilityXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var visibility = option as Bam.Core.ValueTypeOption<EVisibility>;
            var visibilityOption = configuration.Options["GCC_SYMBOLS_PRIVATE_EXTERN"];
            if (visibility.Value == EVisibility.Default)
            {
                visibilityOption.AddUnique("NO");
            }
            else
            {
                visibilityOption.AddUnique("YES");
            }
            if (visibilityOption.Count != 1)
            {
                throw new Bam.Core.Exception("More than one visibility option has been set");
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["Visibility"].PrivateData = new GccCommon.PrivateData(VisibilityCommandLineProcessor,VisibilityXcodeProjectProcessor);
        }
    }
}
