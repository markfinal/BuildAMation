#region License
// Copyright (c) 2010-2015, Mark Final
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
    public static class RelativePathUtilities
    {
        public static bool
        IsPathAbsolute(
            string path)
        {
            var pathUri = new System.Uri(path, System.UriKind.RelativeOrAbsolute);
            var isAbsolute = pathUri.IsAbsoluteUri;
            return isAbsolute;
        }

        public static string
        GetPath(
            System.Uri pathUri,
            System.Uri relativeToUri)
        {
            return GetPath(pathUri, relativeToUri, null);
        }

        public static string
        GetPath(
            System.Uri pathUri,
            System.Uri relativeToUri,
            string relativePrefix)
        {
            var relativePathUri = pathUri.IsAbsoluteUri ? relativeToUri.MakeRelativeUri(pathUri) : pathUri;
            if (relativePathUri.IsAbsoluteUri || System.IO.Path.IsPathRooted(relativePathUri.ToString()))
            {
                // should return the path that pathUri was constructed with
                return pathUri.LocalPath;
            }
            else
            {
                var relativePath = relativePathUri.ToString();
                relativePath = System.Uri.UnescapeDataString(relativePath);
                if (null != relativePrefix)
                {
                    relativePath = System.IO.Path.Combine(relativePrefix, relativePath);
                }
                if (OSUtilities.IsWindowsHosting)
                {
                    relativePath = relativePath.Replace('/', '\\');
                }
                return relativePath;
            }
        }

        public static string
        GetPath(
            string path,
            System.Uri relativeToUri,
            string relativePrefix)
        {
            if (null == path)
            {
                Log.DebugMessage("Null relative path requested");
                return path;
            }

            // special cases
            if ("." == path || ".." == path)
            {
                return path;
            }

            var pathUri = new System.Uri(path, System.UriKind.RelativeOrAbsolute);
            return GetPath(pathUri, relativeToUri, relativePrefix);
        }

        public static string
        GetPath(
            string path,
            System.Uri relativeToUri)
        {
            return GetPath(path, relativeToUri, null);
        }

        public static string
        GetPath(
            string path,
            string relativeToString,
            string relativePrefix)
        {
            var relativeToUri = new System.Uri(relativeToString, System.UriKind.RelativeOrAbsolute);
            var relativePath = GetPath(path, relativeToUri, relativePrefix);
            return relativePath;
        }

        public static string
        GetPath(
            string path,
            string relativeToString)
        {
            var relativeToUri = new System.Uri(relativeToString, System.UriKind.RelativeOrAbsolute);
            var relativePath = GetPath(path, relativeToUri);
            return relativePath;
        }

        public static string
        MakeRelativePathAbsoluteToWorkingDir(
            string relativePath)
        {
            return MakeRelativePathAbsoluteTo(relativePath, Graph.Instance.ProcessState.WorkingDirectory);
        }

        public static string
        MakeRelativePathAbsoluteTo(
            string relativePath,
            string basePath)
        {
            var relativePathUri = new System.Uri(relativePath, System.UriKind.RelativeOrAbsolute);
            if (!relativePathUri.IsAbsoluteUri)
            {
                relativePathUri = new System.Uri(System.IO.Path.Combine(basePath, relativePath));
            }

            var absolutePath = relativePathUri.AbsolutePath;
            absolutePath = System.IO.Path.GetFullPath(absolutePath);
            absolutePath = System.Uri.UnescapeDataString(absolutePath);
            if (OSUtilities.IsWindowsHosting)
            {
                absolutePath = absolutePath.Replace('/', '\\');
            }

            return absolutePath;
        }

        public static string
        GetCommonRoot(
            string path1,
            string path2)
        {
            var path1Parts = path1.Split(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
            var path2Parts = path2.Split(new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });

            string commonRoot = null;
            for (int i = 0; i < path1Parts.Length; ++i)
            {
                if (i >= path2Parts.Length)
                {
                    break;
                }

                if (!path1Parts[i].Equals(path2Parts[i]))
                {
                    break;
                }

                if (null != commonRoot)
                {
                    commonRoot = System.IO.Path.Combine(commonRoot, path1Parts[i]);
                }
                else
                {
                    commonRoot = path1Parts[i];
                    if (commonRoot.EndsWith(System.IO.Path.VolumeSeparatorChar.ToString()))
                    {
                        commonRoot += System.IO.Path.DirectorySeparatorChar;
                    }
                }
            }

            return commonRoot;
        }
    }
}
