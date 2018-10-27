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
        /// <param name="name">TODO</param>
        /// <param name="type">TODO</param>
        /// <param name="path">TODO</param>
        /// <param name="subdir">TODO</param>
        public PackageSource(
            string name,
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
            this.RemotePath = path;
            this.SubdirectoryAsPackageDir = subdir;

            var config = UserConfiguration.Configuration;
            foreach (var i in config.AsEnumerable())
            {
                Log.MessageAll($"{i.Key}={i.Value}");
            }
            var sourcesDir = $"{config[UserConfiguration.SourcesDir]}";
            var packageSourcesDir = System.IO.Path.Combine(sourcesDir, name);
            if (!System.IO.Directory.Exists(packageSourcesDir))
            {
                // doesn't need to be locked, synchronous
                System.IO.Directory.CreateDirectory(packageSourcesDir);
            }

            var leafname = System.IO.Path.GetFileName(this.RemotePath);
            this.ArchivePath = System.IO.Path.Combine(packageSourcesDir, leafname);
            this.ExtractTo = this.ArchivePath.Substring(0, this.ArchivePath.LastIndexOf('.'));
            this.ExtractedPackageDir = this.ExtractTo;
            if (this.SubdirectoryAsPackageDir != null)
            {
                this.ExtractedPackageDir = System.IO.Path.Combine(this.ExtractedPackageDir, this.SubdirectoryAsPackageDir);
            }
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

        /// <summary>
        ///
        /// </summary>
        public string ArchivePath
        {
            get;
            private set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public string ExtractedPackageDir
        {
            get;
            private set;
        }

        private string ExtractTo
        {
            get;
            set;
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

        private string RemotePath
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
            Execute()
            {
                var client = new System.Net.Http.HttpClient();
                client.BaseAddress = new System.Uri(this.RemotePath);
                client.DefaultRequestHeaders.Accept.Clear();

                var getTask = client.GetAsync(client.BaseAddress);
                Graph.Instance.ProcessState.AppendPreBuildTask(getTask);
                Log.Info($"Downloading {this.RemotePath}...");
                var response = await getTask;

                Log.MessageAll(response.Content.Headers.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var stream = new System.IO.FileStream(
                        this.ArchivePath,
                        System.IO.FileMode.Create,
                        System.IO.FileAccess.Write,
                        System.IO.FileShare.None
                    );
                    var copyTask = response.Content.CopyToAsync(stream);
                    Graph.Instance.ProcessState.AppendPreBuildTask(copyTask);
                    await copyTask;
                    stream.Close();

                    if (!System.IO.Directory.Exists(this.ExtractTo))
                    {
                        System.IO.Directory.CreateDirectory(this.ExtractTo);
                    }

                    using (var readerStream = System.IO.File.OpenRead(this.ArchivePath))
                    using (var reader = SharpCompress.Readers.ReaderFactory.Open(readerStream))
                    {
                        while (reader.MoveToNextEntry())
                        {
                            if (!reader.Entry.IsDirectory)
                            {
                                Log.MessageAll(reader.Entry.Key);
                                reader.WriteEntryToDirectory(this.ExtractTo, new SharpCompress.Common.ExtractionOptions()
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception($"Failed to download {this} because {response.ReasonPhrase}");
                }
            }

            if (System.IO.File.Exists(this.ArchivePath))
            {
                return;
            }

            Log.MessageAll($"Need to download '{this}' to '{this.ArchivePath}' and extract to '{this.ExtractTo}'");
            Execute();
        }
    }
}
