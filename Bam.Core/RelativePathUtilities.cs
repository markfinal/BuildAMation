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
    /// Static utility class to convert paths to relative paths.
    /// </summary>
    public static class RelativePathUtilities
    {
        /// <summary>
        /// Is the specified path absolute?
        /// </summary>
        /// <returns><c>true</c> if is path absolute the specified path; otherwise, <c>false</c>.</returns>
        /// <param name="path">Path.</param>
        public static bool
        IsPathAbsolute(
            string path) => System.IO.Path.IsPathRooted(path);

        /// <summary>
        /// Generate a relative path from the specified root.
        /// </summary>
        /// <param name="root">Root of the path</param>
        /// <param name="path">Path that, if rooted, needs to be relative.</param>
        /// <returns>The relative path, with root as its base.</returns>
        public static string
        GetRelativePathFromRoot(
            string root,
            string path)
        {
            if (!System.IO.Path.IsPathRooted(path))
            {
                return path;
            }
            return System.IO.Path.GetRelativePath(root, path);
        }

        /// <summary>
        /// Convert a relative path to an absolute, using the specified root.
        /// If an absolute path is provided, it is returned.
        /// </summary>
        /// <param name="root">Root to use to make the path absolute.</param>
        /// <param name="path">Current relative path to convert.</param>
        /// <returns>The resulting absolute path.</returns>
        public static string
        ConvertRelativePathToAbsolute(
            string root,
            string path)
        {
            if (System.IO.Path.IsPathRooted(path))
            {
                return path;
            }
            var absolute = System.IO.Path.Combine(root, path);
            if (OSUtilities.IsWindowsHosting)
            {
                absolute = absolute.Replace('/', '\\');
            }
            absolute = System.IO.Path.GetFullPath(absolute);
            return absolute;
        }
    }
}
