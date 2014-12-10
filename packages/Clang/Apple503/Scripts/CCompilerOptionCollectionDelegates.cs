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
//     -i=../../../C/dev/Scripts/ICCompilerOptionsOSX.cs
//     -n=Clang
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=ClangCommon.PrivateData
//     -e
#endregion // BamOptionGenerator
namespace Clang
{
    public partial class CCompilerOptionCollection
    {
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
        private static void
        DeploymentTargetCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var deploymentTargetOption = option as Bam.Core.ReferenceTypeOption<string>;
            var deploymentTarget = System.String.Format("-mmacosx-version-min={0}", deploymentTargetOption.Value);
            commandLineBuilder.Add(deploymentTarget);
        }
        private static void
        DeploymentTargetXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var deploymentTargetOption = option as Bam.Core.ReferenceTypeOption<string>;
            configuration.Options["MACOSX_DEPLOYMENT_TARGET"].AddUnique(deploymentTargetOption.Value);
        }
        private static void
        SupportedPlatformCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // don't think there is a command line for this
        }
        private static void
        SupportedPlatformXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var supportedPlatformOption = option as Bam.Core.ValueTypeOption<C.EOSXPlatform>;
            switch (supportedPlatformOption.Value)
            {
            case C.EOSXPlatform.MacOSX:
                configuration.Options["SUPPORTED_PLATFORMS"].AddUnique("macosx");
                break;
            default:
                throw new Bam.Core.Exception("Unsupported OSX platform, '{0}'", supportedPlatformOption.Value.ToString());
            }
        }
        private static void
        CompilerNameCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            // no action required
        }
        private static void
        CompilerNameXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var compilerNameOption = option as Bam.Core.ReferenceTypeOption<string>;
            if (!System.String.IsNullOrEmpty(compilerNameOption.Value))
            {
                configuration.Options["GCC_VERSION"].AddUnique(compilerNameOption.Value);
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["FrameworkSearchDirectories"].PrivateData = new ClangCommon.PrivateData(FrameworkSearchDirectoriesCommandLineProcessor,FrameworkSearchDirectoriesXcodeProjectProcessor);
            this["SDKVersion"].PrivateData = new ClangCommon.PrivateData(SDKVersionCommandLineProcessor,SDKVersionXcodeProjectProcessor);
            this["DeploymentTarget"].PrivateData = new ClangCommon.PrivateData(DeploymentTargetCommandLineProcessor,DeploymentTargetXcodeProjectProcessor);
            this["SupportedPlatform"].PrivateData = new ClangCommon.PrivateData(SupportedPlatformCommandLineProcessor,SupportedPlatformXcodeProjectProcessor);
            this["CompilerName"].PrivateData = new ClangCommon.PrivateData(CompilerNameCommandLineProcessor,CompilerNameXcodeProjectProcessor);
        }
    }
}
