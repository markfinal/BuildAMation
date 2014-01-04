// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=../../../C/dev/Scripts/ICCompilerOptionsOSX.cs -n=Clang -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs:../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs -pv=ClangCommon.PrivateData -e
namespace Clang
{
    public partial class CCompilerOptionCollection
    {
        #region C.ICCompilerOptionsOSX Option properties
        Opus.Core.DirectoryCollection C.ICCompilerOptionsOSX.FrameworkSearchDirectories
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("FrameworkSearchDirectories", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("FrameworkSearchDirectories", value);
                this.ProcessNamedSetHandler("FrameworkSearchDirectoriesSetHandler", this["FrameworkSearchDirectories"]);
            }
        }
        #endregion
    }
}
