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
            FileProcessQueue = new DependencyQueue<Data>();

            System.Threading.ParameterizedThreadStart threadStart = new System.Threading.ParameterizedThreadStart(ProcessFileQueue);
            dependencyThread = new System.Threading.Thread(threadStart);
            dependencyThread.Start(FileProcessQueue);
        }

        public static string HeaderDependencyPathName(string sourceFile, string outputDirectory)
        {
            string depPathName = System.IO.Path.Combine(outputDirectory, System.IO.Path.GetFileNameWithoutExtension(sourceFile)) + ".d";
            return depPathName;
        }

        private static void GenerateDepFile(Data entry, Style style)
        {
            System.Collections.Generic.Queue<string> filesToSearch = new System.Collections.Generic.Queue<string>();
            filesToSearch.Enqueue(entry.sourcePath);

            Opus.Core.StringArray headerPathsFound = new Opus.Core.StringArray();

            while (filesToSearch.Count > 0)
            {
                string fileToSearch = filesToSearch.Dequeue();
#if OPUS_ENABLE_FILE_HASHING
                FileHashGeneration.FileProcessQueue.Enqueue(fileToSearch);
#endif

                string fileContents = null;
                using (System.IO.TextReader reader = new System.IO.StreamReader(fileToSearch))
                {
                    fileContents = reader.ReadToEnd();
                }

                System.Text.RegularExpressions.MatchCollection matches =
                    System.Text.RegularExpressions.Regex.Matches(fileContents,
                                                                 "^\\s*#include \"(.*)\"",
                                                                 System.Text.RegularExpressions.RegexOptions.Multiline);

                // no #includes to check - move onto the next file
                if (0 == matches.Count)
                {
                    continue;
                }

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    bool exists = false;
                    // search for the file on the include paths the compiler uses
                    foreach (string includePath in entry.includePaths)
                    {
                        try
                        {
                            string potentialPath = System.IO.Path.Combine(includePath, match.Groups[1].Value);
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
                        foreach (string headerPath in headerPathsFound)
                        {
                            depWriter.WriteLine(headerPath);
                        }
                    }
                    else
                    {
                        depWriter.WriteLine("{0}:", entry.sourcePath);
                        foreach (string headerPath in headerPathsFound)
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
            Opus.Core.BuildManager buildManager = Opus.Core.State.BuildManager;
            buildManager.AdditionalThreadCompletionEvents.Add(completedEvent);

            DependencyQueue<Data> data = obj as DependencyQueue<Data>;
            for (; ; )
            {
                // wake up when there is data to be processed
                int waitResult = System.Threading.WaitHandle.WaitAny(new System.Threading.WaitHandle[] { data.IsAlive, buildManager.Finished }, -1);

                if (1 == waitResult)
                {
                    break;
                }

                // do work while it's available
                do
                {
                    Data entry = data.Dequeue();
                    GenerateDepFile(entry, Style.Opus);
                }
                while (System.Threading.WaitHandle.WaitAll(new System.Threading.WaitHandle[] { data.IsAlive }, 0));
            }

            // signal complete so that the BuildManager no longer waits
            completedEvent.Set();
        }
    }
}
