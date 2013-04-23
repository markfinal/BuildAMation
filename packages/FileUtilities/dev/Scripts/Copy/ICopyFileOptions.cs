namespace FileUtilities
{
    public interface ICopyFileOptions
    {
        /// <summary>
        /// Gets or sets the destination directory.
        /// </summary>
        /// <value>The destination directory.</value>
        // StateOnly
        string DestinationDirectory
        {
            get;
            set;
        }
    }
}
