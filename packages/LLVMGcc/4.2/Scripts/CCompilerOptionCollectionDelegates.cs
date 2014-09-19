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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator.
// Command line arguments:
//     -i=ICCompilerOptions.cs&../../../C/dev/Scripts/ICCompilerOptionsOSX.cs
//     -n=LLVMGcc
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=GccCommon.PrivateData
//     -e
#endregion // BamOptionGenerator
namespace LLVMGcc
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option delegates
        private static void
        VisibilityCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
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
        #region C.ICCompilerOptionsOSX Option delegates
        private static void
        FrameworkSearchDirectoriesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var switchPrefix = "-F";
            var frameworkIncludePathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to 'var'
            foreach (string includePath in frameworkIncludePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", switchPrefix, includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("{0}{1}", switchPrefix, includePath));
                }
            }
        }
        private static void
        FrameworkSearchDirectoriesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var frameworkPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            configuration.Options["FRAMEWORK_SEARCH_PATHS"].AddRangeUnique(frameworkPathsOption.Value.ToStringArray());
        }
        private static void
        SDKVersionCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var sdkVersionOption = option as Bam.Core.ReferenceTypeOption<string>;
            var sysroot = System.String.Format("-isysroot /Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX{0}.sdk", sdkVersionOption.Value);
            commandLineBuilder.Add(sysroot);
        }
        private static void
        SDKVersionXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var sdkVersionOption = option as Bam.Core.ReferenceTypeOption<string>;
            configuration.Options["SDKROOT"].AddUnique(System.String.Format("macosx{0}", sdkVersionOption.Value));
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["Visibility"].PrivateData = new GccCommon.PrivateData(VisibilityCommandLineProcessor,VisibilityXcodeProjectProcessor);
            this["FrameworkSearchDirectories"].PrivateData = new GccCommon.PrivateData(FrameworkSearchDirectoriesCommandLineProcessor,FrameworkSearchDirectoriesXcodeProjectProcessor);
            this["SDKVersion"].PrivateData = new GccCommon.PrivateData(SDKVersionCommandLineProcessor,SDKVersionXcodeProjectProcessor);
        }
    }
}
