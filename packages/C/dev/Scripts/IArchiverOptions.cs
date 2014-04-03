// <copyright file="IArchiverOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface IArchiverOptions
    {
        /// <summary>
        /// The output type of the archiving operation
        /// </summary>
        C.EArchiverOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Additional options passed to the archiver
        /// </summary>
        string AdditionalOptions
        {
            get;
            set;
        }
    }
}
