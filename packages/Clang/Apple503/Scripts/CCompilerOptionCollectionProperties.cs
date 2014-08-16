// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
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
        #region C.ICCompilerOptionsOSX Option properties
        Bam.Core.DirectoryCollection C.ICCompilerOptionsOSX.FrameworkSearchDirectories
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.DirectoryCollection>("FrameworkSearchDirectories", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.DirectoryCollection>("FrameworkSearchDirectories", value);
                this.ProcessNamedSetHandler("FrameworkSearchDirectoriesSetHandler", this["FrameworkSearchDirectories"]);
            }
        }
        #endregion
    }
}
