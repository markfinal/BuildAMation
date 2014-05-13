// <copyright file="QMakeNodeData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QMakeBuilder package</summary>
// <author>Mark Final</author>

namespace QMakeBuilder
{
    public class QMakeData
    {
        [System.Flags]
        public enum OutputType
        {
            None           = 0,
            Undefined      = (1 << 0),
            ObjectFile     = (1 << 1),
            WinResource    = (1 << 2),
            MocFile        = (1 << 3),
            StaticLibrary  = (1 << 4),
            DynamicLibrary = (1 << 5),
            Application    = (1 << 6),
            HeaderLibrary  = (1 << 7)
        }

        public QMakeData(Opus.Core.DependencyNode node)
        {
            this.OwningNode = node;

            this.CCFlags = new Opus.Core.StringArray();
            this.CXXFlags = new Opus.Core.StringArray();
            this.Defines = new Opus.Core.StringArray();
            this.DestDir = null;
            this.Headers = new Opus.Core.StringArray();
            this.IncludePaths = new Opus.Core.StringArray();
            this.Libraries = new Opus.Core.LocationArray();
            this.ExternalLibraries = new Opus.Core.StringArray();
            this.LinkFlags = new Opus.Core.StringArray();
            this.Merged = false;
            this.MocDir = string.Empty;
            this.ObjectsDir = null;
            this.Output = OutputType.Undefined;
            this.PostLink = new Opus.Core.StringArray();
            this.PriPaths = new Opus.Core.StringArray();
            this.QtModules = new Opus.Core.StringArray();
            this.Sources = new Opus.Core.StringArray();
            this.Target = string.Empty;
            this.WinRCFiles = new Opus.Core.StringArray();
        }

        public Opus.Core.DependencyNode OwningNode
        {
            get;
            private set;
        }

        public Opus.Core.StringArray CCFlags
        {
            get;
            private set;
        }

        public Opus.Core.StringArray CXXFlags
        {
            get;
            private set;
        }

        public Opus.Core.StringArray Defines
        {
            get;
            private set;
        }

#if true
        public Opus.Core.Location DestDir
        {
            get;
            set;
        }
#else
        public string DestDir
        {
            get;
            set;
        }
#endif

        public Opus.Core.StringArray Headers
        {
            get;
            private set;
        }

        public Opus.Core.StringArray IncludePaths
        {
            get;
            private set;
        }

        public Opus.Core.LocationArray Libraries
        {
            get;
            private set;
        }

        public Opus.Core.StringArray ExternalLibraries
        {
            get;
            private set;
        }

        public Opus.Core.StringArray LinkFlags
        {
            get;
            private set;
        }

        private bool Merged
        {
            get;
            set;
        }

        public string MocDir
        {
            get;
            set;
        }

        public Opus.Core.Location ObjectsDir
        {
            get;
            set;
        }

        public string ProFilePath
        {
            get;
            private set;
        }

        public Opus.Core.StringArray PostLink
        {
            get;
            private set;
        }

        public Opus.Core.StringArray PriPaths
        {
            get;
            private set;
        }

        public Opus.Core.StringArray QtModules
        {
            get;
            private set;
        }

        public Opus.Core.StringArray Sources
        {
            get;
            private set;
        }

        public OutputType Output
        {
            get;
            set;
        }

        public string Target
        {
            get;
            set;
        }

        public Opus.Core.StringArray WinRCFiles
        {
            get;
            private set;
        }

        // TODO: need to decide what to do with this
        class Values<T>
        {
            public T Debug
            {
                get;
                set;
            }

            public T Release
            {
                get;
                set;
            }
        }

        private static string FormatPath(string path, string proFilePath)
        {
            return FormatPath(path, proFilePath, false);
        }

        private static string FormatPath(string path, string proFilePath, bool verbose)
        {
            // make the path relative to the .pro
            var newPath = (null != proFilePath) ? Opus.Core.RelativePathUtilities.GetPath(path, proFilePath) : path;

            if (!verbose)
            {
                // QMake warning: unescaped backslashes are deprecated
                newPath = newPath.Replace("\\", "/");
            }

            // spaces in paths need to be quoted
            if (newPath.Contains(" ") && !newPath.Contains("$$quote"))
            {
                newPath = System.String.Format("$$quote({0})", newPath);
            }

            return newPath;
        }

        private static string PathListToString(Opus.Core.StringArray pathList, string proFilePath)
        {
            var escapedPathList = new Opus.Core.StringArray();
            foreach (var path in pathList)
            {
                escapedPathList.Add(FormatPath(path, proFilePath));
            }

            return escapedPathList.ToString("\\\n\t");
        }

        private static void WriteTemplate(Opus.Core.Array<QMakeData> array,
                                          string proFilePath,
                                          System.IO.StreamWriter writer)
        {
            switch (array[0].Output)
            {
                case OutputType.Application:
                    writer.WriteLine("TEMPLATE = app");
                    break;

                case OutputType.StaticLibrary:
                case OutputType.DynamicLibrary:
                    writer.WriteLine("TEMPLATE = lib");
                    break;

                case OutputType.ObjectFile:
                    // special case - a static library which has the least side-effects
                    writer.WriteLine("TEMPLATE = lib");
                    break;

                case OutputType.HeaderLibrary:
                    writer.WriteLine("TEMPLATE = subdirs");
                    break;

                default:
                    throw new Opus.Core.Exception("Should not be writing out .pro files for outputs of type '{0}' : {1}", array[0].Output.ToString(), proFilePath);
            }
        }

        private static void WriteConfig(Opus.Core.Array<QMakeData> array,
                                        string proFilePath,
                                        System.IO.StreamWriter writer)
        {
            string config = string.Empty;
            if (array.Count == 1)
            {
                if (array[0].OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                {
                    config += " debug";
                }
                else
                {
                    config += " release";
                }
            }
            else
            {
                config += " debug_and_release";
            }

            var qtModules = array[0].QtModules;
            if (array.Count > 1)
            {
                qtModules = new Opus.Core.StringArray(qtModules.Union(array[1].QtModules));
            }
            if (qtModules.Count > 0)
            {
                config += " qt";
            }

            // special case of an object file creating a static lib too
            if (0 != (array[0].Output & (OutputType.StaticLibrary | OutputType.ObjectFile)))
            {
                config += " staticlib";
            }
            else if (array[0].Output == OutputType.DynamicLibrary)
            {
                config += " shared";
            }
            writer.WriteLine("CONFIG +={0}", config);
            if (qtModules.Count > 0)
            {
                writer.WriteLine("QT = {0}", qtModules.ToString());
            }
        }

        private static void WriteSources(Opus.Core.Array<QMakeData> array,
                                         string proFilePath,
                                         System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Sources, "SOURCES+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Sources;
                    }
                    else
                    {
                        values.Release = data.Sources;
                    }
                }

                WriteStringArrays(values, "SOURCES+=", proFilePath, writer);
            }
        }

        private static void WriteHeaders(Opus.Core.Array<QMakeData> array,
                                         string proFilePath,
                                         System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Headers, "HEADERS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Headers;
                    }
                    else
                    {
                        values.Release = data.Headers;
                    }
                }

                WriteStringArrays(values, "HEADERS+=", proFilePath, writer);
            }
        }

        private static void WriteTarget(Opus.Core.Array<QMakeData> array,
                                        string proFilePath,
                                        System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteString(array[0].Target, "TARGET=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Target;
                    }
                    else
                    {
                        values.Release = data.Target;
                    }
                }

                WriteString(values, "TARGET=", null, writer);
            }
        }

        private static void WriteDestDir(Opus.Core.Array<QMakeData> array,
                                         string proFilePath,
                                         System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteString(array[0].DestDir.GetSinglePath(), "DESTDIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    var destDir = data.DestDir.GetSinglePath();
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = destDir;
                    }
                    else
                    {
                        values.Release = destDir;
                    }
                }

                WriteString(values, "DESTDIR=", null, writer);
            }
        }

        private static void WriteMocDir(Opus.Core.Array<QMakeData> array,
                                        string proFilePath,
                                        System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteString(array[0].MocDir, "MOC_DIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.MocDir;
                    }
                    else
                    {
                        values.Release = data.MocDir;
                    }
                }

                WriteString(values, "MOC_DIR=", null, writer);
            }
        }

        private static void WriteObjectsDir(Opus.Core.Array<QMakeData> array,
                                            string proFilePath,
                                            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteString(array[0].ObjectsDir.GetSinglePath(), "OBJECTS_DIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    var objDir = data.ObjectsDir.GetSinglePath();
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = objDir;
                    }
                    else
                    {
                        values.Release = objDir;
                    }
                }

                WriteString(values, "OBJECTS_DIR=", null, writer);
            }
        }

        private static void WriteIncludePaths(Opus.Core.Array<QMakeData> array, string proFilePath, System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].IncludePaths, "INCLUDEPATH+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.IncludePaths;
                    }
                    else
                    {
                        values.Release = data.IncludePaths;
                    }
                }

                WriteStringArrays(values, "INCLUDEPATH+=", proFilePath, writer);
            }
        }

        private static void WriteDefines(Opus.Core.Array<QMakeData> array, string proFilePath, System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Defines, "DEFINES+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Defines;
                    }
                    else
                    {
                        values.Release = data.Defines;
                    }
                }

                WriteStringArrays(values, "DEFINES+=", proFilePath, writer);
            }
        }

        private static void WriteCCFlags(Opus.Core.Array<QMakeData> array, string proFilePath, System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].CCFlags, "QMAKE_CFLAGS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.CCFlags;
                    }
                    else
                    {
                        values.Release = data.CCFlags;
                    }
                }

                WriteStringArrays(values, "QMAKE_CFLAGS+=", proFilePath, writer);
            }
        }

        private static void WriteCXXFlags(Opus.Core.Array<QMakeData> array, string proFilePath, System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].CXXFlags, "QMAKE_CXXFLAGS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.CXXFlags;
                    }
                    else
                    {
                        values.Release = data.CXXFlags;
                    }
                }

                WriteStringArrays(values, "QMAKE_CXXFLAGS+=", proFilePath, writer);
            }
        }

        private static void WriteString(string value, string format, string proFilePath, System.IO.StreamWriter writer)
        {
            WriteString(value, format, proFilePath, false, writer);
        }

        private static void WriteString(string value, string format, string proFilePath, bool verbose, System.IO.StreamWriter writer)
        {
            if (0 == value.Length)
            {
                return;
            }
            if (!format.Contains("{0}"))
            {
                format += "{0}";
            }
            writer.WriteLine(format, FormatPath(value, proFilePath, verbose));
        }

        private static void WriteStringArray(Opus.Core.StringArray stringArray,
                                             string format,
                                             string proFilePath,
                                             System.IO.StreamWriter writer)
        {
            WriteStringArray(stringArray, format, proFilePath, false, true, false, writer);
        }

        private static void WriteStringArray(Opus.Core.StringArray stringArray,
                                             string format,
                                             string proFilePath,
                                             bool verbose,
                                             bool useContinuation,
                                             bool escaped,
                                             System.IO.StreamWriter writer)
        {
            if (0 == stringArray.Count)
            {
                return;
            }
            else if (1 == stringArray.Count)
            {
                WriteString(stringArray[0], format, proFilePath, verbose, writer);
            }
            else
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                if (useContinuation)
                {
                    builder.Append(format);
                    foreach (string value in stringArray)
                    {
                        if (escaped)
                        {
                            builder.Append(@"$$escape_expand(\n\t)");
                        }
                        builder.AppendFormat("\\\n\t{0}", FormatPath(value, proFilePath, verbose));
                    }
                }
                else
                {
                    foreach (string value in stringArray)
                    {
                        builder.AppendFormat("{0}{1}", format, FormatPath(value, proFilePath, verbose));
                        if (escaped)
                        {
                            builder.Append(@"$$escape_expand(\n\t)");
                        }
                        builder.Append("\n");
                    }
                }
                writer.WriteLine(builder.ToString());
            }
        }

        private static void WriteLocationArray(Opus.Core.LocationArray locArray,
                                               string format,
                                               string proFilePath,
                                               System.IO.StreamWriter writer)
        {
            WriteLocationArray(locArray, format, proFilePath, false, true, false, writer);
        }

        private static void WriteLocationArray(Opus.Core.LocationArray locArray,
                                               string format,
                                               string proFilePath,
                                               bool verbose,
                                               bool useContinuation,
                                               bool escaped,
                                               System.IO.StreamWriter writer)
        {
            if (0 == locArray.Count)
            {
                return;
            }
            else if (1 == locArray.Count)
            {
                WriteString(locArray[0].GetSinglePath(), format, proFilePath, verbose, writer);
            }
            else
            {
                var builder = new System.Text.StringBuilder();
                if (useContinuation)
                {
                    builder.Append(format);
                    foreach (var value in locArray)
                    {
                        if (escaped)
                        {
                            builder.Append(@"$$escape_expand(\n\t)");
                        }
                        builder.AppendFormat("\\\n\t{0}", FormatPath(value.GetSinglePath(), proFilePath, verbose));
                    }
                }
                else
                {
                    foreach (var value in locArray)
                    {
                        builder.AppendFormat("{0}{1}", format, FormatPath(value.GetSinglePath(), proFilePath, verbose));
                        if (escaped)
                        {
                            builder.Append(@"$$escape_expand(\n\t)");
                        }
                        builder.Append("\n");
                    }
                }
                writer.WriteLine(builder.ToString());
            }
        }

        private static void WriteString(Values<string> values, string format, string proFilePath, System.IO.StreamWriter writer)
        {
            if (values.Debug == values.Release)
            {
                WriteString(values.Debug, format, proFilePath, writer);
            }
            else
            {
                // see the following for an explanation of this syntax
                // http://qt-project.org/faq/answer/what_does_the_syntax_configdebugdebugrelease_mean_what_does_the_1st_argumen
                WriteString(values.Debug, "CONFIG(debug,debug|release):" + format, proFilePath, writer);
                WriteString(values.Release, "CONFIG(release,debug|release):" + format, proFilePath, writer);
            }
        }

        private static void WriteStringArrays(Values<Opus.Core.StringArray> values, string format, string proFilePath, System.IO.StreamWriter writer)
        {
            WriteStringArrays(values, format, proFilePath, false, true, false, writer);
        }

        private static void WriteStringArrays(Values<Opus.Core.StringArray> values,
                                              string format,
                                              string proFilePath,
                                              bool verbose,
                                              bool useContinuation,
                                              bool escaped,
                                              System.IO.StreamWriter writer)
        {
            var intersect = new Opus.Core.StringArray(values.Debug.Intersect(values.Release));
            WriteStringArray(intersect, format, proFilePath, verbose, useContinuation, escaped, writer);

            // see the following for an explanation of this syntax
            // http://qt-project.org/faq/answer/what_does_the_syntax_configdebugdebugrelease_mean_what_does_the_1st_argumen
            if (intersect.Count != values.Debug.Count)
            {
                var debugOnly = new Opus.Core.StringArray(values.Debug.Complement(intersect));
                WriteStringArray(debugOnly, "CONFIG(debug,debug|release):" + format, proFilePath, verbose, useContinuation, escaped, writer);
            }
            if (intersect.Count != values.Release.Count)
            {
                var releaseOnly = new Opus.Core.StringArray(values.Release.Complement(intersect));
                WriteStringArray(releaseOnly, "CONFIG(release,debug|release):" + format, proFilePath, verbose, useContinuation, escaped, writer);
            }
        }

        private static void WriteLocationArrays(Values<Opus.Core.LocationArray> values, string format, string proFilePath, System.IO.StreamWriter writer)
        {
            WriteLocationArrays(values, format, proFilePath, false, true, false, writer);
        }

        private static void WriteLocationArrays(Values<Opus.Core.LocationArray> values,
                                                string format,
                                                string proFilePath,
                                                bool verbose,
                                                bool useContinuation,
                                                bool escaped,
                                                System.IO.StreamWriter writer)
        {
            var intersect = new Opus.Core.LocationArray(values.Debug.Intersect(values.Release));
            WriteLocationArray(intersect, format, proFilePath, verbose, useContinuation, escaped, writer);

            // see the following for an explanation of this syntax
            // http://qt-project.org/faq/answer/what_does_the_syntax_configdebugdebugrelease_mean_what_does_the_1st_argumen
            if (intersect.Count != values.Debug.Count)
            {
                var debugOnly = new Opus.Core.LocationArray(values.Debug.Complement(intersect));
                WriteLocationArray(debugOnly, "CONFIG(debug,debug|release):" + format, proFilePath, verbose, useContinuation, escaped, writer);
            }
            if (intersect.Count != values.Release.Count)
            {
                var releaseOnly = new Opus.Core.LocationArray(values.Release.Complement(intersect));
                WriteLocationArray(releaseOnly, "CONFIG(release,debug|release):" + format, proFilePath, verbose, useContinuation, escaped, writer);
            }
        }

        private static void WritePriPaths(Opus.Core.Array<QMakeData> array,
                                          string proFilePath,
                                          System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].PriPaths, @"include({0})", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.PriPaths;
                    }
                    else
                    {
                        values.Release = data.PriPaths;
                    }
                }

                WriteStringArrays(values, @"include({0})", proFilePath, writer);
            }
        }

        private static void WriteWinRCFiles(Opus.Core.Array<QMakeData> array,
                                            string proFilePath,
                                            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].WinRCFiles, "RC_FILE+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.WinRCFiles;
                    }
                    else
                    {
                        values.Release = data.WinRCFiles;
                    }
                }

                WriteStringArrays(values, "RC_FILE+=", proFilePath, writer);
            }
        }

        private static void WriteLibraries(Opus.Core.Array<QMakeData> array,
                                           string proFilePath,
                                           System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteLocationArray(array[0].Libraries, "LIBS+=", null, writer);
            }
            else
            {
                var values = new Values<Opus.Core.LocationArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Libraries;
                    }
                    else
                    {
                        values.Release = data.Libraries;
                    }
                }

                WriteLocationArrays(values, "LIBS+=", null, writer);
            }
        }

        private static void WriteLinkFlags(Opus.Core.Array<QMakeData> array, string proFilePath, System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].LinkFlags, "QMAKE_LFLAGS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.LinkFlags;
                    }
                    else
                    {
                        values.Release = data.LinkFlags;
                    }
                }

                WriteStringArrays(values, "QMAKE_LFLAGS+=", proFilePath, writer);
            }
        }

        private static void WritePostLinkCommands(Opus.Core.Array<QMakeData> array, string proFilePath, System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].PostLink, "QMAKE_POST_LINK+=", proFilePath, true, false, true, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.PostLink;
                    }
                    else
                    {
                        values.Release = data.PostLink;
                    }
                }

                WriteStringArrays(values, "QMAKE_POST_LINK+=", proFilePath, true, false, true, writer);
            }
        }

        public void Merge(QMakeData data)
        {
            this.Merge(data, OutputType.None);
        }

        public void Merge(QMakeData data, OutputType excludeType)
        {
            if (0 != (data.Output & excludeType))
            {
                return;
            }

            var baseTargetLHS = (Opus.Core.BaseTarget)(this.OwningNode.Target);
            var baseTargetRHS = (Opus.Core.BaseTarget)(data.OwningNode.Target);
            if (baseTargetLHS != baseTargetRHS)
            {
                throw new Opus.Core.Exception("Cannot merge data from different Opus.Core.BaseTargets: {0} vs {1}", baseTargetLHS.ToString(), baseTargetRHS.ToString());
            }

            this.CCFlags.AddRangeUnique(data.CCFlags);
            this.CXXFlags.AddRangeUnique(data.CXXFlags);
            this.Defines.AddRangeUnique(data.Defines);
            if ((null != data.DestDir) && data.DestDir.IsValid)
            {
                this.DestDir = data.DestDir;
            }
            this.Headers.AddRangeUnique(data.Headers);
            this.IncludePaths.AddRangeUnique(data.IncludePaths);
            this.Libraries.AddRangeUnique(data.Libraries);
            if (data.MocDir.Length > 0)
            {
                this.MocDir = data.MocDir;
            }
            if ((null != data.ObjectsDir) && data.ObjectsDir.IsValid)
            {
                this.ObjectsDir = data.ObjectsDir;
            }
            this.PostLink.AddRangeUnique(data.PostLink);
            this.PriPaths.AddRangeUnique(data.PriPaths);
            this.QtModules.AddRangeUnique(data.QtModules);
            this.Sources.AddRangeUnique(data.Sources);
            if (data.Output != OutputType.Undefined)
            {
                this.Output = data.Output;
            }
            if (this.Target.Length == 0)
            {
                this.Target = data.Target;
            }
            this.WinRCFiles.AddRangeUnique(data.WinRCFiles);

            data.Merged = true;
        }

        public static void Write(Opus.Core.Array<QMakeData> array)
        {
            bool consistentMergeState = true;
            if (array.Count > 1)
            {
                foreach (var data in array)
                {
                    if (data.Merged != array[0].Merged)
                    {
                        consistentMergeState = false;
                    }
                }
            }
            if (!consistentMergeState)
            {
                throw new Opus.Core.Exception("Data is inconsistently merged");
            }
            if (array[0].Merged)
            {
                Opus.Core.Log.DebugMessage("Not writing a pro file as qmake data is merged");
                return;
            }

            var node = array[0].OwningNode;
            string proFileDirectory = node.GetModuleBuildDirectory();
            string proFilePath = System.IO.Path.Combine(proFileDirectory, System.String.Format("{0}.pro", node.ModuleName));
            if (array[0].Target.Length > 0)
            {
                proFilePath = proFilePath.Replace(node.ModuleName, array[0].Target);
                proFileDirectory = System.IO.Path.GetDirectoryName(proFilePath);
            }
            Opus.Core.Log.DebugMessage("QMake Pro File for node '{0}': '{1}'", node.UniqueModuleName, proFilePath);
            foreach (var data in array)
            {
                data.ProFilePath = proFilePath;
            }

            // TODO: this might be temporary
            System.IO.Directory.CreateDirectory(proFileDirectory);

            using (var proWriter = new System.IO.StreamWriter(proFilePath))
            {
                WritePriPaths(array, proFilePath, proWriter);
                WriteTemplate(array, proFilePath, proWriter);
                WriteConfig(array, proFilePath, proWriter);
                WriteTarget(array, proFilePath, proWriter);
                WriteDestDir(array, proFilePath, proWriter);
                WriteMocDir(array, proFilePath, proWriter);
                WriteObjectsDir(array, proFilePath, proWriter);
                WriteIncludePaths(array, proFilePath, proWriter);
                WriteDefines(array, proFilePath, proWriter);
                WriteCCFlags(array, proFilePath, proWriter);
                WriteCXXFlags(array, proFilePath, proWriter);
                WriteSources(array, proFilePath, proWriter);
                WriteHeaders(array, proFilePath, proWriter);
                WriteWinRCFiles(array, proFilePath, proWriter);
                WriteLibraries(array, proFilePath, proWriter);
                // TODO: WriteExternalLibraries
                WriteLinkFlags(array, proFilePath, proWriter);
                WritePostLinkCommands(array, proFilePath, proWriter);
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("QMakeData: Type {0} node '{1}'", this.Output, this.OwningNode.UniqueModuleName);
            return builder.ToString();
        }
    }
}
