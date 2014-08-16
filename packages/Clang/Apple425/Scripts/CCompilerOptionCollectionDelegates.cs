// Automatically generated file from OpusOptionCodeGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ICCompilerOptionsOSX.cs
//     -n=Clang
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=ClangCommon.PrivateData
//     -e

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
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["FrameworkSearchDirectories"].PrivateData = new ClangCommon.PrivateData(FrameworkSearchDirectoriesCommandLineProcessor,FrameworkSearchDirectoriesXcodeProjectProcessor);
        }
    }
}
