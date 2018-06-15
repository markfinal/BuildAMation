#region License
// Copyright (c) 2010-2018, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    /// <summary>
    /// Wrapper around IO functions in order to add value to any exceptions thrown.
    /// </summary>
    public static class IOWrapper
    {
        /// <summary>
        /// Wrapper around System.IO.Directory.CreateDirectory, catching exceptions thrown
        /// and embedding them into a Bam.Core.Exception with more semantic detail.
        /// No checks whether the directory already exists occur.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to create.</param>
        static public void
        CreateDirectory(
            string directoryPath)
        {
            try
            {
                try
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }
                catch (System.ArgumentException)
                {
                    // this can happen, say, if there are enclosing quotes
                    directoryPath = directoryPath.Trim(System.IO.Path.GetInvalidPathChars());
                    System.IO.Directory.CreateDirectory(directoryPath);
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex, "Unable to create directory, {0}", directoryPath);
            }
        }

        /// <summary>
        /// Conditionally create the directory, if it does not exist.
        /// Any exceptions thrown from the directory creation are embedded into a Bam.Core.Exception
        /// with more semantic detail.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to create.</param>
        static public void
        CreateDirectoryIfNotExists(
            string directoryPath)
        {
            if (!System.IO.Directory.Exists(directoryPath))
            {
                CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Create a temporary file.
        /// May throw an exception if there are reasons why temporary files cannot be crated.
        /// </summary>
        /// <returns>Temporary file path</returns>
        static public string
        CreateTemporaryFile()
        {
            try
            {
                var path = System.IO.Path.GetTempFileName();
                return path;
            }
            catch (System.IO.IOException ex)
            {
                throw new Bam.Core.Exception(ex, "Unable to get a temporary path; please delete all temporary files in {0} and try again", System.IO.Path.GetTempPath());
            }
        }

        /// <summary>
        /// If a path contains spaces, enclose it in double quotes. Otherwise return asis.
        /// </summary>
        /// <returns>Quoted path if necessary.</returns>
        /// <param name="path">Path to check.</param>
        static public string
        EncloseSpaceContainingPathWithDoubleQuotes(
            string path)
        {
            if (path.Contains(" "))
            {
                return System.String.Format("\"{0}\"", path);
            }
            return path;
        }

        /// <summary>
        /// If a path contains spaces, escape each occurrence with a back slash.
        /// </summary>
        /// <returns>Escaped path if necessary.</returns>
        /// <param name="path">Path to check.</param>
        static public string
        EscapeSpacesInPath(
            string path)
        {
            if (path.Contains(" "))
            {
                return path.Replace(" ", "\\ ");
            }
            return path;
        }
    }
}
