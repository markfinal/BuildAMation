// <copyright file="File.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class File
    {
        static private readonly string DirectorySeparatorString = new string(System.IO.Path.DirectorySeparatorChar, 1);
        static private readonly string AltDirectorySeparatorString = new string(System.IO.Path.AltDirectorySeparatorChar, 1);

        private static bool ContainsDirectorySeparators(string filePart)
        {
            bool containsSeparators = filePart.Contains(DirectorySeparatorString) || filePart.Contains(AltDirectorySeparatorString);
            return containsSeparators;
        }

        private static void ValidateFilePart(string filePart)
        {
            if (ContainsDirectorySeparators(filePart))
            {
                throw new Exception(System.String.Format("Individual file parts cannot contain directory separators; '{0}'", filePart));
            }
        }

        public File(params string[] fileParts)
        {
            if (1 == fileParts.Length && ContainsDirectorySeparators(fileParts[0]))
            {
                if (RelativePathUtilities.IsPathAbsolute(fileParts[0]))
                {
                    this.RelativePath = fileParts[0];
                }
                else
                {
                    throw new Exception(System.String.Format("Path '{0}' is not absolute but contains directory separators; please use the separated version", fileParts[0]));
                }
            }
            else
            {
                ValidateFilePart(fileParts[0]);
                string fullSourcePath = fileParts[0];
                for (int i = 1; i < fileParts.Length; ++i)
                {
                    ValidateFilePart(fileParts[i]);
                    fullSourcePath = System.IO.Path.Combine(fullSourcePath, fileParts[i]);
                }

                this.RelativePath = fullSourcePath;
            }
        }

        public string RelativePath
        {
            get;
            private set;
        }
    }
}