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
using System.Linq;
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
        /// <param name="version">TODO</param>
        /// <param name="type">TODO</param>
        /// <param name="path">TODO</param>
        /// <param name="subdir">TODO</param>
        /// <param name="extractto">TODO</param>
        /// <param name="downloadRequired">TODO</param>
        public PackageSource(
            string name,
            string version,
            string type,
            string path,
            string subdir,
            string extractto,
            bool downloadRequired)
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

            this.PackageName = name;
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

            var leafname = System.IO.Path.GetFileName(this.RemotePath);
            if (leafname.LastIndexOf('.') < 0)
            {
                // if the original download link doesn't have an extension (e.g. SourceForge)
                // make one up, so it's versioned
                leafname = $"{name}-{version}.archive";
            }
            this.ArchivePath = System.IO.Path.Combine(packageSourcesDir, leafname);
            this.ExtractedSourceChecksum = $"{this.ArchivePath}.md5";
            if (null != extractto)
            {
                this.ExtractTo = System.IO.Path.Combine(packageSourcesDir, extractto);
            }
            else
            {
                this.ExtractTo = this.ArchivePath.Substring(0, this.ArchivePath.LastIndexOf('.'));
            }
            this.ExtractedPackageDir = this.ExtractTo;
            if (this.SubdirectoryAsPackageDir != null)
            {
                this.ExtractedPackageDir = System.IO.Path.Combine(this.ExtractedPackageDir, this.SubdirectoryAsPackageDir);
            }

            // bypass any downloads
            if (!downloadRequired)
            {
                this.AlreadyFetched = true;
                return;
            }

            // now check if there has already been an extraction
            if (System.IO.File.Exists(this.ArchivePath) &&
                System.IO.File.Exists(this.ExtractedSourceChecksum))
            {
                // TODO: this could be quite expensive, so put it onto a command line switch
                var checksum = this.GenerateMD5Hash();
                var old = System.IO.File.ReadAllBytes(this.ExtractedSourceChecksum);
                if (!checksum.Hash.SequenceEqual(old))
                {
                    throw new Exception($"MD5 checksum comparison failed for package {this.PackageName}");
                }
                this.AlreadyFetched = true;
            }
            else
            {
                this.AlreadyFetched = false;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="packageName"></param>
        public System.Threading.Tasks.Task
        Fetch(
            string packageName)
        {
            if (this.AlreadyFetched)
            {
                return null;
            }
            switch (this.Type)
            {
                case PackageSource.EType.Http:
                    return this.DownloadAndExtractPackageViaHTTP(packageName);
            }
            throw new Exception($"Unhandled package source type, '{this.Type.ToString()}'");
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

        private string ExtractedSourceChecksum
        {
            get;
            set;
        }

        private string PackageName
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

        private bool AlreadyFetched { get; set; } = false;

        private System.Security.Cryptography.MD5
        GenerateMD5Hash()
        {
            // regenerate filelist from the original archive, and then MD5 the actual files
            // on disk (if they exist)
            var filelist = new StringArray();
            using (var readerStream = System.IO.File.OpenRead(this.ArchivePath))
            using (var reader = SharpCompress.Readers.ReaderFactory.Open(readerStream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.IsDirectory)
                    {
                        continue;
                    }

                    filelist.Add(System.IO.Path.Combine(this.ExtractTo, reader.Entry.ToString()));
                }
            }

            var hash = System.Security.Cryptography.MD5.Create();
            for (int i = 0; i < filelist.Count; ++i)
            {
                var filepath = filelist[i];
                if (!System.IO.File.Exists(filepath))
                {
                    continue;
                }
                try
                {
                    var bytes = System.IO.File.ReadAllBytes(filepath); // TODO: ouch
                    if (i < filelist.Count - 1)
                    {
                        hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                    }
                    else
                    {
                        hash.TransformFinalBlock(bytes, 0, bytes.Length);
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    continue; // some archives have symlinks that go to nowhere
                }
            }
            Log.MessageAll($"{string.Concat(hash.Hash.Select(x => x.ToString("X2")))}");
            return hash;
        }

        private System.Threading.Tasks.Task
        DownloadAndExtractPackageViaHTTP(
            string packageName)
        {
            var client = new System.Net.Http.HttpClient
            {
                BaseAddress = new System.Uri(this.RemotePath),
                Timeout = System.TimeSpan.FromMilliseconds(-1) // infinite
            };
            client.DefaultRequestHeaders.Accept.Clear();

            var downloadTask = client.GetAsync(client.BaseAddress);
            Log.Info($"Downloading {this.RemotePath}...");

            var savingTask = downloadTask.ContinueWith(t =>
            {
                Log.Info($"Saving {this.RemotePath} to {this.ArchivePath}...");

                if (!t.Result.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to download {this.RemotePath} because {t.Result.ReasonPhrase}");
                }

                var parentDir = System.IO.Path.GetDirectoryName(this.ArchivePath);
                if (!System.IO.Directory.Exists(parentDir))
                {
                    System.IO.Directory.CreateDirectory(parentDir);
                }

                using (var stream = new System.IO.FileStream(
                            this.ArchivePath,
                            System.IO.FileMode.Create,
                            System.IO.FileAccess.Write,
                            System.IO.FileShare.None
                            ))
                {
                    t.Result.Content.CopyToAsync(stream).Wait(); // waiting since it's already in a task
                }
            });

            var extractingTask = savingTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    throw t.Exception;
                }
                Log.Info($"Extracting {this.ArchivePath} to {this.ExtractTo}...");
                using (var readerStream = System.IO.File.OpenRead(this.ArchivePath))
                using (var reader = SharpCompress.Readers.ReaderFactory.Open(readerStream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (reader.Entry.IsDirectory)
                        {
                            continue;
                        }

                        reader.WriteEntryToDirectory(this.ExtractTo, new SharpCompress.Common.ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }

                // write the MD5 checksum to disk
                Log.Info($"Generating checksum of extracted {this.ArchivePath}...");
                var checksum = this.GenerateMD5Hash();
                Log.Info($"Writing checksum of extracted {this.ArchivePath} to {this.ExtractedSourceChecksum}...");
                System.IO.File.WriteAllBytes(this.ExtractedSourceChecksum, checksum.Hash);
            });

            return extractingTask;
        }
    }
}
