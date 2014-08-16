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

        public
        QMakeData(
            Bam.Core.DependencyNode node)
        {
            this.OwningNode = node;
            this.OSXApplicationBundle = false;

            this.CCFlags = new Bam.Core.StringArray();
            this.CustomPathVariables = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            this.CustomRules = null;
            this.CXXFlags = new Bam.Core.StringArray();
            this.Defines = new Bam.Core.StringArray();
            this.DestDir = null;
            this.Headers = new Bam.Core.StringArray();
            this.IncludePaths = new Bam.Core.StringArray();
            this.Libraries = new Bam.Core.LocationArray();
            this.ExternalLibraries = new Bam.Core.StringArray();
            this.LinkFlags = new Bam.Core.StringArray();
            this.Merged = false;
            this.MocDir = null;
            this.ObjectiveSources = new Bam.Core.StringArray();
            this.ObjectsDir = null;
            this.Output = OutputType.Undefined;
            this.PostLink = new Bam.Core.StringArray();
            this.PriPaths = new Bam.Core.StringArray();
            this.RPathDir = new Bam.Core.StringArray();
            this.QtModules = new Bam.Core.StringArray();
            this.Sources = new Bam.Core.StringArray();
            this.Target = string.Empty;
            this.VersionMajor = string.Empty;
            this.VersionMinor = string.Empty;
            this.VersionPatch = string.Empty;
            this.WinRCFiles = new Bam.Core.StringArray();
        }

        public Bam.Core.DependencyNode OwningNode
        {
            get;
            private set;
        }

        public bool
        OSXApplicationBundle
        {
            get;
            set;
        }

        public Bam.Core.StringArray CCFlags
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> CustomPathVariables
        {
            get;
            private set;
        }

        public Bam.Core.StringArray CustomRules
        {
            get;
            set;
        }

        public Bam.Core.StringArray CXXFlags
        {
            get;
            private set;
        }

        public Bam.Core.StringArray Defines
        {
            get;
            private set;
        }

        public Bam.Core.Location DestDir
        {
            get;
            set;
        }

        public Bam.Core.StringArray Headers
        {
            get;
            private set;
        }

        public Bam.Core.StringArray IncludePaths
        {
            get;
            private set;
        }

        public Bam.Core.LocationArray Libraries
        {
            get;
            private set;
        }

        public Bam.Core.StringArray ExternalLibraries
        {
            get;
            private set;
        }

        public Bam.Core.StringArray LinkFlags
        {
            get;
            private set;
        }

        private bool Merged
        {
            get;
            set;
        }

        public Bam.Core.Location MocDir
        {
            get;
            set;
        }

        public Bam.Core.StringArray ObjectiveSources
        {
            get;
            private set;
        }

        public Bam.Core.Location ObjectsDir
        {
            get;
            set;
        }

        public string ProFilePath
        {
            get;
            private set;
        }

        public Bam.Core.StringArray PostLink
        {
            get;
            private set;
        }

        public Bam.Core.StringArray PriPaths
        {
            get;
            private set;
        }

        public Bam.Core.StringArray RPathDir
        {
            get;
            private set;
        }

        public Bam.Core.StringArray QtModules
        {
            get;
            private set;
        }

        public Bam.Core.StringArray Sources
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

        public string VersionMajor
        {
            get;
            set;
        }

        public string VersionMinor
        {
            get;
            set;
        }

        public string VersionPatch
        {
            get;
            set;
        }

        public Bam.Core.StringArray WinRCFiles
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

        private static string
        FormatPath(
            string path,
            string proFilePath)
        {
            return FormatPath(path, proFilePath, false);
        }

        private static string
        FormatPath(
            string path,
            string proFilePath,
            bool verbose)
        {
            // make the path relative to the .pro
            var newPath = (null != proFilePath) ? Bam.Core.RelativePathUtilities.GetPath(path, proFilePath) : path;

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

        private static string
        PathListToString(
            Bam.Core.StringArray pathList,
            string proFilePath)
        {
            var escapedPathList = new Bam.Core.StringArray();
            foreach (var path in pathList)
            {
                escapedPathList.Add(FormatPath(path, proFilePath));
            }

            return escapedPathList.ToString("\\\n\t");
        }

        private static void
        WriteTemplate(
            Bam.Core.Array<QMakeData> array,
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
                    throw new Bam.Core.Exception("Should not be writing out .pro files for outputs of type '{0}' : {1}", array[0].Output.ToString(), proFilePath);
            }
        }

        private static void
        WriteConfig(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            var config = string.Empty;
            if (array.Count == 1)
            {
                if (array[0].OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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
                qtModules = new Bam.Core.StringArray(qtModules.Union(array[1].QtModules));
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
            if (Bam.Core.OSUtilities.IsOSXHosting)
            {
                if (array[0].Output == OutputType.Application && !array[0].OSXApplicationBundle)
                {
                    writer.WriteLine("CONFIG -= app_bundle");
                }
            }
        }

        private static void
        WriteSources(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Sources, "SOURCES+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteObjectiveSources(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].ObjectiveSources, "OBJECTIVE_SOURCES+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.ObjectiveSources;
                    }
                    else
                    {
                        values.Release = data.ObjectiveSources;
                    }
                }

                WriteStringArrays(values, "OBJECTIVE_SOURCES+=", proFilePath, writer);
            }
        }

        private static void
        WriteHeaders(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Headers, "HEADERS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteTarget(
            Bam.Core.Array<QMakeData> array,
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
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteDestDir(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                if (null == array[0].DestDir)
                {
                    return;
                }
                WriteString(array[0].DestDir.GetSinglePath(), "DESTDIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (null == data.DestDir)
                    {
                        continue;
                    }
                    var destDir = data.DestDir.GetSinglePath();
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteMocDir(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                if (null == array[0].MocDir)
                {
                    return;
                }
                WriteString(array[0].MocDir.GetSinglePath(), "MOC_DIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (null == data.MocDir)
                    {
                        continue;
                    }
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.MocDir.GetSinglePath();
                    }
                    else
                    {
                        values.Release = data.MocDir.GetSinglePath();
                    }
                }

                WriteString(values, "MOC_DIR=", null, writer);
            }
        }

        private static void
        WriteObjectsDir(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                if (array[0].ObjectsDir == null)
                {
                    return;
                }

                WriteString(array[0].ObjectsDir.GetSinglePath(), "OBJECTS_DIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (data.ObjectsDir == null)
                    {
                        continue;
                    }
                    var objDir = data.ObjectsDir.GetSinglePath();
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteIncludePaths(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].IncludePaths, "INCLUDEPATH+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteDefines(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Defines, "DEFINES+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteCCFlags(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].CCFlags, "QMAKE_CFLAGS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteCXXFlags(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].CXXFlags, "QMAKE_CXXFLAGS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteString(
            string value,
            string format,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            WriteString(value, format, proFilePath, false, writer);
        }

        private static void
        WriteString(
            string value,
            string format,
            string proFilePath,
            bool verbose,
            System.IO.StreamWriter writer)
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

        private static void
        WriteVerboseString(
            string value,
            string format,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (0 == value.Length)
            {
                return;
            }
            if (!format.Contains("{0}"))
            {
                format += "{0}";
            }
            writer.WriteLine(format, value);
        }

        private static void
        WriteStringArray(
            Bam.Core.StringArray stringArray,
            string format,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            WriteStringArray(stringArray, format, proFilePath, false, true, false, writer);
        }

        private static void
        WriteStringArray(
            Bam.Core.StringArray stringArray,
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
                var builder = new System.Text.StringBuilder();
                if (useContinuation)
                {
                    builder.Append(format);
                    foreach (var value in stringArray)
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
                    foreach (var value in stringArray)
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

        private static void
        WriteLocationArray(
            Bam.Core.LocationArray locArray,
            string format,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            WriteLocationArray(locArray, format, proFilePath, false, true, false, writer);
        }

        private static void
        WriteLocationArray(
            Bam.Core.LocationArray locArray,
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

        private static void
        WriteString(
            Values<string> values,
            string format,
            string proFilePath,
            System.IO.StreamWriter writer)
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

        private static void
        WriteStringArrays(
            Values<Bam.Core.StringArray> values,
            string format,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            WriteStringArrays(values, format, proFilePath, false, true, false, writer);
        }

        private static void
        WriteStringArrays(
            Values<Bam.Core.StringArray> values,
            string format,
            string proFilePath,
            bool verbose,
            bool useContinuation,
            bool escaped,
            System.IO.StreamWriter writer)
        {
            var intersect = new Bam.Core.StringArray(values.Debug.Intersect(values.Release));
            WriteStringArray(intersect, format, proFilePath, verbose, useContinuation, escaped, writer);

            // see the following for an explanation of this syntax
            // http://qt-project.org/faq/answer/what_does_the_syntax_configdebugdebugrelease_mean_what_does_the_1st_argumen
            if (intersect.Count != values.Debug.Count)
            {
                var debugOnly = new Bam.Core.StringArray(values.Debug.Complement(intersect));
                WriteStringArray(debugOnly, "CONFIG(debug,debug|release):" + format, proFilePath, verbose, useContinuation, escaped, writer);
            }
            if (intersect.Count != values.Release.Count)
            {
                var releaseOnly = new Bam.Core.StringArray(values.Release.Complement(intersect));
                WriteStringArray(releaseOnly, "CONFIG(release,debug|release):" + format, proFilePath, verbose, useContinuation, escaped, writer);
            }
        }

        private static void
        WriteLocationArrays(
            Values<Bam.Core.LocationArray> values,
            string format,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            WriteLocationArrays(values, format, proFilePath, false, true, false, writer);
        }

        private static void
        WriteLocationArrays(
            Values<Bam.Core.LocationArray> values,
            string format,
            string proFilePath,
            bool verbose,
            bool useContinuation,
            bool escaped,
            System.IO.StreamWriter writer)
        {
            var intersect = new Bam.Core.LocationArray(values.Debug.Intersect(values.Release));
            WriteLocationArray(intersect, format, proFilePath, verbose, useContinuation, escaped, writer);

            // see the following for an explanation of this syntax
            // http://qt-project.org/faq/answer/what_does_the_syntax_configdebugdebugrelease_mean_what_does_the_1st_argumen
            if (intersect.Count != values.Debug.Count)
            {
                var debugOnly = new Bam.Core.LocationArray(values.Debug.Complement(intersect));
                WriteLocationArray(debugOnly, "CONFIG(debug,debug|release):" + format, proFilePath, verbose, useContinuation, escaped, writer);
            }
            if (intersect.Count != values.Release.Count)
            {
                var releaseOnly = new Bam.Core.LocationArray(values.Release.Complement(intersect));
                WriteLocationArray(releaseOnly, "CONFIG(release,debug|release):" + format, proFilePath, verbose, useContinuation, escaped, writer);
            }
        }

        private static void
        WritePriPaths(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].PriPaths, @"include({0})", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteWinRCFiles(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].WinRCFiles, "RC_FILE+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteLibraries(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteLocationArray(array[0].Libraries, "LIBS+=", null, writer);
            }
            else
            {
                var values = new Values<Bam.Core.LocationArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteLinkFlags(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].LinkFlags, "QMAKE_LFLAGS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteRPathDir(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].RPathDir, "QMAKE_RPATHDIR+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.RPathDir;
                    }
                    else
                    {
                        values.Release = data.RPathDir;
                    }
                }

                WriteStringArrays(values, "QMAKE_RPATHDIR+=", proFilePath, writer);
            }
        }

        private static void
        WritePostLinkCommands(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].PostLink, "QMAKE_POST_LINK+=", proFilePath, true, false, true, writer);
            }
            else
            {
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
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

        private static void
        WriteCustomPathVariables(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                foreach (var customPath in array[0].CustomPathVariables)
                {
                    WriteStringArray(customPath.Value, customPath.Key + "+=", proFilePath, writer);
                }
            }
            else
            {
                // TODO
                throw new System.NotImplementedException();
            }
        }

        private static void
        WriteCustomRules(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                if (array[0].CustomRules == null)
                {
                    return;
                }
                foreach (var customRule in array[0].CustomRules)
                {
                    // split on the first = only
                    var split = customRule.Split(new [] {'='}, 2);
                    split[0] = split[0].Trim();
                    split[1] = split[1].Trim();
                    split[0] += '=';
                    WriteVerboseString(split[1], split[0], proFilePath, writer);
                }
            }
            else
            {
                // TODO: how to handle this for splitting on the equals sign?
                var values = new Values<Bam.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.CustomRules == null)
                    {
                        continue;
                    }

                    if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.CustomRules;
                    }
                    else
                    {
                        values.Release = data.CustomRules;
                    }
                }

                // TODO: this might product incorrectly formatted pro files
                WriteStringArrays(values, string.Empty, proFilePath, true, false, true, writer);
            }
        }

        private static void
        WriteMajorVersionNumber(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                if (!string.IsNullOrEmpty(array[0].VersionMajor))
                {
                    WriteString(array[0].VersionMajor, "VER_MAJ=", proFilePath, writer);
                }
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (!string.IsNullOrEmpty(data.VersionMajor))
                    {
                        if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                        {
                            values.Debug = data.VersionMajor;
                        }
                        else
                        {
                            values.Release = data.VersionMajor;
                        }
                    }
                }

                WriteString(values, "VER_MAJ=", proFilePath, writer);
            }
        }

        private static void
        WriteMinorVersionNumber(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                if (!string.IsNullOrEmpty(array[0].VersionMinor))
                {
                    WriteString(array[0].VersionMinor, "VER_MIN=", proFilePath, writer);
                }
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (!string.IsNullOrEmpty(data.VersionMinor))
                    {
                        if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                        {
                            values.Debug = data.VersionMinor;
                        }
                        else
                        {
                            values.Release = data.VersionMinor;
                        }
                    }
                }

                WriteString(values, "VER_MIN=", proFilePath, writer);
            }
        }

        private static void
        WritePatchVersionNumber(
            Bam.Core.Array<QMakeData> array,
            string proFilePath,
            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                if (!string.IsNullOrEmpty(array[0].VersionPatch))
                {
                    WriteString(array[0].VersionPatch, "VER_PAT=", proFilePath, writer);
                }
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (!string.IsNullOrEmpty(data.VersionPatch))
                    {
                        if (data.OwningNode.Target.HasConfiguration(Bam.Core.EConfiguration.Debug))
                        {
                            values.Debug = data.VersionPatch;
                        }
                        else
                        {
                            values.Release = data.VersionPatch;
                        }
                    }
                }

                WriteString(values, "VER_PAT=", proFilePath, writer);
            }
        }

        public void
        Merge(
            QMakeData data)
        {
            this.Merge(data, OutputType.None);
        }

        public void
        Merge(
            QMakeData data,
            OutputType excludeType)
        {
            if (0 != (data.Output & excludeType))
            {
                return;
            }

            var baseTargetLHS = (Bam.Core.BaseTarget)(this.OwningNode.Target);
            var baseTargetRHS = (Bam.Core.BaseTarget)(data.OwningNode.Target);
            if (baseTargetLHS != baseTargetRHS)
            {
                throw new Bam.Core.Exception("Cannot merge data from different Bam.Core.BaseTargets: {0} vs {1}", baseTargetLHS.ToString(), baseTargetRHS.ToString());
            }

            this.CCFlags.AddRangeUnique(data.CCFlags);
            foreach (var customPath in data.CustomPathVariables)
            {
                if (this.CustomPathVariables.ContainsKey(customPath.Key))
                {
                    this.CustomPathVariables[customPath.Key].AddRangeUnique(customPath.Value);
                }
                else
                {
                    this.CustomPathVariables[customPath.Key] = new Bam.Core.StringArray(customPath.Value);
                }
            }
            if (null != data.CustomRules)
            {
                if (null == this.CustomRules)
                {
                    this.CustomRules = new Bam.Core.StringArray();
                }
                this.CustomRules.AddRangeUnique(data.CustomRules);
            }
            this.CXXFlags.AddRangeUnique(data.CXXFlags);
            this.Defines.AddRangeUnique(data.Defines);
            if ((null != data.DestDir) && data.DestDir.IsValid)
            {
                this.DestDir = data.DestDir;
            }
            this.Headers.AddRangeUnique(data.Headers);
            this.IncludePaths.AddRangeUnique(data.IncludePaths);
            this.Libraries.AddRangeUnique(data.Libraries);
            if ((null != data.MocDir) && data.MocDir.IsValid)
            {
                this.MocDir = data.MocDir;
            }
            if ((null != data.ObjectsDir) && data.ObjectsDir.IsValid)
            {
                this.ObjectsDir = data.ObjectsDir;
            }
            this.PostLink.AddRangeUnique(data.PostLink);
            this.PriPaths.AddRangeUnique(data.PriPaths);
            this.RPathDir.AddRangeUnique(data.RPathDir);
            this.QtModules.AddRangeUnique(data.QtModules);
            this.Sources.AddRangeUnique(data.Sources);
            this.ObjectiveSources.AddRangeUnique(data.ObjectiveSources);
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

        public static void
        Write(
            Bam.Core.Array<QMakeData> array)
        {
            var consistentMergeState = true;
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
                throw new Bam.Core.Exception("Data is inconsistently merged");
            }
            if (array[0].Merged)
            {
                Bam.Core.Log.DebugMessage("Not writing a pro file as qmake data is merged");
                return;
            }

            var node = array[0].OwningNode;
            var proFileDirectory = node.GetModuleBuildDirectoryLocation().GetSingleRawPath();
            var proFilePath = System.IO.Path.Combine(proFileDirectory, System.String.Format("{0}.pro", node.ModuleName));
            if (array[0].Target.Length > 0)
            {
                proFilePath = proFilePath.Replace(node.ModuleName, array[0].Target);
                proFileDirectory = System.IO.Path.GetDirectoryName(proFilePath);
            }
            Bam.Core.Log.DebugMessage("QMake Pro File for node '{0}': '{1}'", node.UniqueModuleName, proFilePath);
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
                WriteObjectiveSources(array, proFilePath, proWriter);
                WriteHeaders(array, proFilePath, proWriter);
                WriteWinRCFiles(array, proFilePath, proWriter);
                WriteLibraries(array, proFilePath, proWriter);
                // TODO: WriteExternalLibraries since they have been separated from built libraries by Locations
                WriteLinkFlags(array, proFilePath, proWriter);
                WriteRPathDir(array, proFilePath, proWriter);
                WriteMajorVersionNumber(array, proFilePath, proWriter);
                WriteMinorVersionNumber(array, proFilePath, proWriter);
                WritePatchVersionNumber(array, proFilePath, proWriter);
                WritePostLinkCommands(array, proFilePath, proWriter);
                WriteCustomPathVariables(array, proFilePath, proWriter);
                WriteCustomRules(array, proFilePath, proWriter);
            }
        }

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendFormat("QMakeData: Type {0} node '{1}'", this.Output, this.OwningNode.UniqueModuleName);
            return builder.ToString();
        }
    }
}
