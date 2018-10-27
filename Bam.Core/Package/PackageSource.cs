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
using Microsoft.Extensions.Configuration;
using SharpCompress.Readers;
namespace Bam.Core
{
    /// <summary>
    /// TODO
    /// </summary>
    public class PackageSource
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="type">TODO</param>
        /// <param name="path">TODO</param>
        /// <param name="subdir">TODO</param>
        public PackageSource(
            string type,
            string path,
            string subdir)
        {
            PackageSource.EType
            ToType(
                string typeAsString)
            {
                switch (typeAsString)
                {
                    case "http":
                        return PackageSource.EType.Http;
                }
                throw new Exception($"Invalid package source type, '{typeAsString}'");
            }

            this.Type = ToType(type);
            this.Path = path;
            this.SubdirectoryAsPackageDir = subdir;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="packageName"></param>
        public void
        Execute(
            string packageName)
        {
            switch (this.Type)
            {
                case PackageSource.EType.Http:
                    this.DownloadAndExtractPackageViaHTTP(packageName);
                    break;
            }
        }

        private enum EType
        {
            Http
        }

        private EType Type
        {
            get;
            set;
        }

        private string Path
        {
            get;
            set;
        }

        private string SubdirectoryAsPackageDir
        {
            get;
            set;
        }

        private void
        DownloadAndExtractPackageViaHTTP(
            string packageName)
        {
            async void
            RunMe(
                string path,
                string extractTo)
            {
                var client = new System.Net.Http.HttpClient();
                client.BaseAddress = new System.Uri(this.Path);
                client.DefaultRequestHeaders.Accept.Clear();

                var getTask = client.GetAsync(client.BaseAddress);
                Graph.Instance.ProcessState.AppendPreBuildTask(getTask);
                var response = await getTask;

                Log.MessageAll(response.Content.Headers.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var stream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
                    var copyTask = response.Content.CopyToAsync(stream);
                    Graph.Instance.ProcessState.AppendPreBuildTask(copyTask);
                    await copyTask;
                    stream.Close();

                    if (!System.IO.Directory.Exists(extractTo))
                    {
                        System.IO.Directory.CreateDirectory(extractTo);
                    }

                    using (var readerStream = System.IO.File.OpenRead(path))
                    using (var reader = SharpCompress.Readers.ReaderFactory.Open(readerStream))
                    {
                        while (reader.MoveToNextEntry())
                        {
                            if (!reader.Entry.IsDirectory)
                            {
                                Log.MessageAll(reader.Entry.Key);
                                reader.WriteEntryToDirectory(extractTo, new SharpCompress.Common.ExtractionOptions()
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });
                            }
                        }
                    }

                    if (this.SubdirectoryAsPackageDir != null)
                    {
                        Graph.Instance.ProcessState.AddDownloadedPackageSourceMapping(
                            packageName,
                            System.IO.Path.Combine(extractTo, this.SubdirectoryAsPackageDir)
                        );
                    }
                    else
                    {
                        Graph.Instance.ProcessState.AddDownloadedPackageSourceMapping(
                            packageName,
                            extractTo
                        );
                    }
                }
                else
                {
                    throw new Exception($"Failed to download {this} because {response.ReasonPhrase}");
                }
            }

            var config = UserConfiguration.Configuration;
            foreach (var i in config.AsEnumerable())
            {
                Log.MessageAll($"{i.Key}={i.Value}");
            }
            var sourcesDir = $"{config[UserConfiguration.SourcesDir]}";
            var packageSourcesDir = System.IO.Path.Combine(sourcesDir, packageName);
            if (!System.IO.Directory.Exists(packageSourcesDir))
            {
                // doesn't need to be locked, synchronous
                System.IO.Directory.CreateDirectory(packageSourcesDir);
            }

            var leafname = System.IO.Path.GetFileName(this.Path);
            var packageSourcePath = System.IO.Path.Combine(packageSourcesDir, leafname);
            var packageSourceExtractDir = packageSourcePath.Substring(0, packageSourcePath.LastIndexOf('.'));
            if (!System.IO.File.Exists(packageSourcePath))
            {
                Log.MessageAll($"Need to download '{this}' to '{packageSourcePath}' and extract to '{packageSourceExtractDir}'");
                RunMe(
                    packageSourcePath,
                    packageSourceExtractDir
                );
            }
        }
    }
}
