#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
namespace QtCommon
{
    public interface IMocOptions
    {
        Bam.Core.DirectoryCollection IncludePaths
        {
            get;
            set;
        }

        C.DefineCollection Defines
        {
            get;
            set;
        }

        bool DoNotGenerateIncludeStatement
        {
            get;
            set;
        }

        bool DoNotDisplayWarnings
        {
            get;
            set;
        }

        string PathPrefix
        {
            get;
            set;
        }
    }
}
