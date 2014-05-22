// <copyright file="IncludeDependencyGeneration.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DependencyGenerator package</summary>
// <author>Mark Final</author>
namespace DependencyGenerator
{
    public class IncludeDependencyGeneration
    {
        private static System.Threading.Thread dependencyThread = null;
        private static System.Threading.ManualResetEvent completedEvent = new System.Threading.ManualResetEvent(false);

        public class Data
        {
            public string sourcePath = null;
            public string depFilePath = null;
            public Opus.Core.StringArray includePaths = null;
        }

        public enum Style
        {
            Opus,
            Makefile
        }

        internal static DependencyQueue<Data> FileProcessQueue
        {
            get;
            private set;
        }

        static IncludeDependencyGeneration()
        {
            // TODO: put this on an action
            var isThreaded = true;
            FileProcessQueue = new DependencyQueue<Data>(isThreaded);

            var threadStart = new System.Threading.ParameterizedThreadStart(ProcessFileQueue);
            dependencyThread = new System.Threading.Thread(threadStart);
            dependencyThread.Start(FileProcessQueue);
        }

        public static string HeaderDependencyPathName(string sourceFile, string outputDirectory)
        {
            var depPathName = System.IO.Path.Combine(outputDirectory, System.IO.Path.GetFileNameWithoutExtension(sourceFile)) + ".d";
            return depPathName;
        }

        public static string HeaderDependencyPathName(string filename, Opus.Core.Location directory)
        {
            var depLeafname = System.IO.Path.GetFileNameWithoutExtension(filename) + ".d";
            var headerDependencyLocation = new Opus.Core.ScaffoldLocation(directory, depLeafname, Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.WillExist);
            return headerDependencyLocation.GetSinglePath();
        }

        public static void GenerateDepFile(Data entry, Style style)
        {
            var filesToSearch = new System.Collections.Generic.Queue<string>();
            filesToSearch.Enqueue(entry.sourcePath);

            var headerPathsFound = new Opus.Core.StringArray();

            while (filesToSearch.Count > 0)
            {
                var fileToSearch = filesToSearch.Dequeue();
#if OPUS_ENABLE_FILE_HASHING
                FileHashGeneration.FileProcessQueue.Enqueue(fileToSearch);
#endif

                string fileContents = null;
                using (System.IO.TextReader reader = new System.IO.StreamReader(fileToSearch))
                {
                    fileContents = reader.ReadToEnd();
                }

                var matches =
                    System.Text.RegularExpressions.Regex.Matches(fileContents,
                                                                 "^\\s*#include \"(.*)\"",
                                                                 System.Text.RegularExpressions.RegexOptions.Multiline);

                // no #includes to check - move onto the next file
                if (0 == matches.Count)
                {
                    continue;
                }

                // TODO: change to var?
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    bool exists = false;
                    // search for the file on the include paths the compiler uses
                    foreach (var includePath in entry.includePaths)
                    {
                        try
                        {
                            var potentialPath = System.IO.Path.Combine(includePath, match.Groups[1].Value);
                            if (System.IO.File.Exists(potentialPath))
                            {
                                potentialPath = System.IO.Path.GetFullPath(potentialPath);
                                if (!headerPathsFound.Contains(potentialPath))
                                {
                                    headerPathsFound.Add(potentialPath);
                                    filesToSearch.Enqueue(potentialPath);
                                }
                                exists = true;
                                break;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Opus.Core.Log.MessageAll("IncludeDependency Exception: Cannot locate '{0}' on '{1}' due to {2}", match.Groups[1].Value, includePath, ex.Message);
                        }
                    }

                    if (!exists)
                    {
                        Opus.Core.Log.DebugMessage("***** Could not locate '{0}' on any include search path, included from {1}:\n{2}",
                                                   match.Groups[1],
                                                   fileToSearch,
                                                   entry.includePaths.ToString('\n'));
                    }
                }
            }

            if (headerPathsFound.Count > 0)
            {
                using (System.IO.TextWriter depWriter = new System.IO.StreamWriter(entry.depFilePath))
                {
                    if (Style.Opus == style)
                    {
                        foreach (var headerPath in headerPathsFound)
                        {
                            depWriter.WriteLine(headerPath);
                        }
                    }
                    else
                    {
                        depWriter.WriteLine("{0}:", entry.sourcePath);
                        foreach (var headerPath in headerPathsFound)
                        {
                            depWriter.WriteLine("\t{0}", headerPath);
                        }
                    }
                }
            }
        }

        internal static void ProcessFileQueue(object obj)
        {
            // wait for the build to start
            System.Threading.WaitHandle.WaitAll(new System.Threading.WaitHandle[] { Opus.Core.State.BuildStartedEvent }, -1);
            var buildManager = Opus.Core.State.BuildManager;
            buildManager.AdditionalThreadCompletionEvents.Add(completedEvent);

            var data = obj as DependencyQueue<Data>;
            for (; ; )
            {
                // wake up when there is data to be processed
                var waitResult = System.Threading.WaitHandle.WaitAny(new System.Threading.WaitHandle[] { data.IsAlive, buildManager.Finished }, -1);
                if (1 == waitResult)
                {
                    break;
                }

                // do work while it's available
                do
                {
                    var entry = data.Dequeue();
                    GenerateDepFile(entry, Style.Opus);
                }
                while (System.Threading.WaitHandle.WaitAll(new System.Threading.WaitHandle[] { data.IsAlive }, 0));
            }

            // signal complete so that the BuildManager no longer waits
            completedEvent.Set();
        }
    }
}
