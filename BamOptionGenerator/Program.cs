#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace BamOptionGenerator
{
    class Program
    {
        private static readonly string MajorHorizontalRule = new string('=', 80);
        private static readonly string MinorHorizontalRule = new string('-', 80);

        private static char CommandSeparator;
        private static char HeaderReplacementCommandSeparator;
        static Program()
        {
            var isWindows = System.IO.Path.DirectorySeparatorChar == '\\';
            if (isWindows)
            {
                CommandSeparator = ';';
            }
            else
            {
                CommandSeparator = ':';
            }
            HeaderReplacementCommandSeparator = '&';
        }

        private static void
        Log(
            string format,
            params string[] args)
        {
            var message = new System.Text.StringBuilder();
            message.AppendFormat(format, args);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(message.ToString());
            }
            else
            {
                System.Console.WriteLine(message.ToString());
                System.Console.Out.Flush();
            }
        }

        private static void
        PrintHelp()
        {
            Log("BamOptionGenerator [options]");
            Log("Options:");
            Log("-i=<interface filenames, using path separators>");
            Log("-n=<namespace>");
            Log("-c=<generated class name>");
            Log("-pv=<delegate private data class name>");
            Log("-p [generate properties file]");
            Log("-dd=<delegate type definition files, using path separators>");
            Log("-d [generate delegates file]");
            Log("-s [only write to stdout]");
            Log("-f [force write to file]");
            Log("-e [delegates extend an existing implementation]");
            Log("-b [generated delegates class is a base class]");
            Log("-uh [updated header of generated files if needed]");
            Log("-ih [ignore any updates to updates of generated files]");
            Log("--- Obsolete options ---");
            Log("-o=<output properties filename> [use -p]");
        }

        private static Parameters
        ProcessArgs(
            string[] args)
        {
            if (0 == args.Length)
            {
                PrintHelp();
                throw new Exception("No arguments were passed");
            }

            var parameters = new Parameters();
            parameters.args = args;
            parameters.mode = 0;

            foreach (var arg in args)
            {
                if (arg.StartsWith("-o="))
                {
                    parameters.mode |= Parameters.Mode.GenerateProperties;
                    // redundant, ignore
                }
                else if (arg.StartsWith("-i="))
                {
                    var split = arg.Split(new char[] { '=' });
                    var inputFilesString = split[1];
                    parameters.inputPathNames = inputFilesString.Split(new char[] { System.IO.Path.PathSeparator });
                }
                else if (arg.StartsWith("-n="))
                {
                    var split = arg.Split(new char[] { '=' });
                    parameters.outputNamespace = split[1];
                }
                else if (arg.StartsWith("-c="))
                {
                    var split = arg.Split(new char[] { '=' });
                    parameters.outputClassName = split[1];
                }
                else if (arg.StartsWith("-pv="))
                {
                    var split = arg.Split(new char[] { '=' });
                    parameters.privateDataClassName = split[1];
                }
                else if (arg.StartsWith("-p"))
                {
                    parameters.mode |= Parameters.Mode.GenerateProperties;
                }
                else if (arg.StartsWith("-dd="))
                {
                    var split = arg.Split(new char[] { '=' });
                    var inputFilesString = split[1];
                    parameters.inputDelegates = inputFilesString.Split(new char[] { System.IO.Path.PathSeparator });
                }
                else if (arg.StartsWith("-d"))
                {
                    parameters.mode |= Parameters.Mode.GenerateDelegates;
                }
                else if (arg.StartsWith("-s"))
                {
                    parameters.toStdOut = true;
                }
                else if (arg.StartsWith("-f"))
                {
                    parameters.forceWrite = true;
                }
                else if (arg.StartsWith("-e"))
                {
                    parameters.extendedDelegates = true;
                }
                else if (arg.StartsWith("-b"))
                {
                    parameters.isBaseClass = true;
                }
                else if (arg.StartsWith("-uh"))
                {
                    parameters.updateHeader = true;
                }
                else if (arg.StartsWith("-ih"))
                {
                    parameters.ignoreHeaderUpdates = true;
                }
                else if (arg.StartsWith("-l="))
                {
                    var split = arg.Split(new char[] { '=' });
                    parameters.licenseFile = split[1];
                }
                else
                {
                    throw new Exception("Unrecognized argument '{0}'", arg);
                }
            }

            return parameters;
        }

        private static void
        Validate(
            Parameters parameters)
        {
            if (null == parameters.inputPathNames)
            {
                throw new Exception("No input files provided");
            }

            if (null == parameters.outputNamespace)
            {
                throw new Exception("No output namespace provided");
            }

            if (null == parameters.outputClassName)
            {
                throw new Exception("No output class name provided");
            }

            parameters.outputPropertiesPathName = parameters.outputClassName + "Properties.cs";
            parameters.outputDelegatesPathName = parameters.outputClassName + "Delegates.cs";
        }

        private static string
        ReadLine(
            System.IO.TextReader reader,
            bool skipComments,
            out int prefixSpaces,
            out bool valueOnly)
        {
            var numPrefixSpaces = 0;
            var valueOnlyProxy = false;
            string line = null;
            do
            {
                line = reader.ReadLine();
                // TODO: make this optional
                //Log("[{0}]", line);
                if (null == line)
                {
                    numPrefixSpaces = 0;
                    break;
                }
                int originalLineLength = line.Length;
                line = line.TrimStart();
                numPrefixSpaces = originalLineLength - line.Length;
                line = line.TrimEnd();

                if (skipComments)
                {
                    if (line.StartsWith("//"))
                    {
                        line = ""; // just to get through the while test
                        continue;
                    }
                    else if (line.StartsWith("[Bam.Core.ValueOnlyOption]"))
                    {
                        line = "";
                        valueOnlyProxy = true;
                        continue;
                    }
                }
            }
            while (0 == line.Length);
            prefixSpaces = numPrefixSpaces;
            valueOnly = valueOnlyProxy;
            return line;
        }

        private static string
        ReadLine(
            System.IO.TextReader reader)
        {
            int prefixSpaces = 0;
            bool valueOnly = false;
            return ReadLine(reader, false, out prefixSpaces, out valueOnly);
        }

        private static string
        ReadLine(
            System.IO.TextReader reader,
            bool skipComments)
        {
            int prefixSpaces = 0;
            bool valueOnly = false;
            return ReadLine(reader, skipComments, out prefixSpaces, out valueOnly);
        }

        private static string
        ReadLine(
            System.IO.TextReader reader,
            out int prefixSpaces)
        {
            bool valueOnly = false;
            return ReadLine(reader, false, out prefixSpaces, out valueOnly);
        }

        private static string
        ReadLine(
            System.IO.TextReader reader,
            bool skipComments,
            out bool valueOnly)
        {
            int prefixSpaces = 0;
            return ReadLine(reader, skipComments, out prefixSpaces, out valueOnly);
        }

        private static System.Collections.Generic.List<string>
        ReadLicense(
            System.IO.TextReader reader,
            bool returnToCaller)
        {
            var line = reader.ReadLine();
            if (!line.StartsWith("#region License"))
            {
                return null;
            }
            System.Collections.Generic.List<string> licenseText = returnToCaller ? new System.Collections.Generic.List<string>() : null;
            if (returnToCaller)
            {
                licenseText.Add(line);
            }
            for (;;)
            {
                line = reader.ReadLine();
                if (null == line)
                {
                    throw new Exception("End of file reached before the end of the license was found");
                }
                if (returnToCaller)
                {
                    licenseText.Add(line);
                }
                if (line.StartsWith("#endregion // License"))
                {
                    break;
                }
            }
            return licenseText;
        }

        private static System.Collections.Generic.List<string>
        ReadBamOptionGeneratorHeader(
            System.IO.TextReader reader,
            bool returnToCaller)
        {
            var line = reader.ReadLine();
            if (!line.StartsWith("#region BamOptionGenerator"))
            {
                return null;
            }
            System.Collections.Generic.List<string> bamText = returnToCaller ? new System.Collections.Generic.List<string>() : null;
            for (;;)
            {
                line = reader.ReadLine();
                if (line.StartsWith("#endregion // BamOptionGenerator"))
                {
                    break;
                }
                if (returnToCaller)
                {
                    bamText.Add(line);
                }
            }
            return bamText;
        }

        private static void
        WriteNewLicense(
            System.IO.TextWriter writer,
            Parameters parameters)
        {
            if (parameters.licenseFile == null)
            {
                throw new Exception("No license file specified");
            }
            writer.WriteLine("#region License");
            using (System.IO.TextReader licenseReader = new System.IO.StreamReader(parameters.licenseFile))
            {
                for (;;)
                {
                    var line = licenseReader.ReadLine();
                    if (null == line)
                    {
                        break;
                    }

                    if (!line.StartsWith("//"))
                    {
                        if (System.String.IsNullOrEmpty(line))
                        {
                            writer.WriteLine("//");
                        }
                        else
                        {
                            writer.WriteLine(System.String.Format("// {0}", line));
                        }
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            writer.WriteLine("#endregion // License");
        }

        private static void
        Write(
            System.IO.TextWriter writer,
            int tabCount,
            string format,
            params string[] args)
        {
            for (int i = 0; i < tabCount; ++i)
            {
                writer.Write("    ");
            }
            if (0 == args.Length)
            {
                writer.Write(format);
            }
            else
            {
                writer.Write(format, args);
            }
        }

        private static void
        Write(
            System.Text.StringBuilder builder,
            int tabCount,
            string format,
            params string[] args)
        {
            builder.Append(new string('\t', tabCount));
            builder.AppendFormat(format, args);
        }

        private static void
        WriteLine(
            System.IO.TextWriter writer,
            int tabCount,
            string format,
            params string[] args)
        {
            Write(writer, tabCount, format, args);
            writer.Write(writer.NewLine);
        }

        private static void
        WriteLine(
            System.Text.StringBuilder builder,
            int tabCount,
            string format,
            params string[] args)
        {
            Write(builder, tabCount, format, args);
            builder.Append("\n");
        }

        private static DelegateSignature
        ReadDelegateDefinitionFile(
            string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                throw new Exception("File '{0}' does not exist", filename);
            }
            Log(MinorHorizontalRule);
            Log("Reading delegate signature file: '{0}'", filename);

            var signature = new DelegateSignature();

            using (var reader = new System.IO.StreamReader(filename))
            {
                ReadLicense(reader, false);

                string line;

                line = ReadLine(reader);
                if (null == line)
                {
                    throw new Exception("Interface file is empty");
                }
                // namespace
                var namespaceStrings = line.Split(new char[] { ' ' });
                if (!namespaceStrings[0].Equals("namespace"))
                {
                    throw new Exception("Interface file '{0}' does not start with namespace or comments; instead starts with '{1}'", filename, namespaceStrings[0]);
                }
                var namespaceName = namespaceStrings[1];
                Log("\tNamespace: '{0}'", namespaceName);

                // opening namespace scope
                line = ReadLine(reader);
                if (!line.StartsWith("{"))
                {
                    throw new Exception("No scope opened after namespace");
                }

                // delegate
                line = ReadLine(reader, true);
                while (!line.EndsWith(");"))
                {
                    var newLine = ReadLine(reader);
                    line += " "; // add a space as buffers around joining lines up
                    line += newLine;
                }
                var delegateStrings = line.Split(new char[] { ' ' });
                if ("public" != delegateStrings[0] || "delegate" != delegateStrings[1])
                {
                    throw new Exception("No public delegate found in '{0}'", filename);
                }
                Log("\tDelegate: '{0}'", line);
                var returnType = delegateStrings[2];
                if ("void" != returnType)
                {
                    returnType = namespaceName + "." + returnType;
                }
                int firstParenthesis = line.IndexOf('(', 0);
                var argumentsWithoutParentheses = line.Substring(firstParenthesis + 1, line.Length - firstParenthesis - 3);
                var argumentList = argumentsWithoutParentheses.Split(',');

                signature.InNamespace = namespaceName;
                signature.ReturnType = returnType;
                signature.Arguments = new System.Collections.Generic.List<string>(argumentList);
                if (signature.ReturnType == "void")
                {
                    signature.Body = null;
                }
                else
                {
                    signature.Body = new System.Collections.Specialized.StringCollection();
                    signature.Body.Add(System.String.Format("var returnVal = new {0}();", returnType));
                    signature.Body.Add("return returnVal;");
                }
            }

            return signature;
        }

        private static void
        Execute(
            Parameters parameters)
        {
            var propertyMap = new PropertySignatureMap();

            // TODO:
            // * handle anything before an interface, e.g. enum
            foreach (var inputPath in parameters.inputPathNames)
            {
                if (!System.IO.File.Exists(inputPath))
                {
                    throw new Exception("Input file '{0}' does not exist", inputPath);
                }
                Log(MajorHorizontalRule);
                Log("Reading interface file '{0}'", inputPath);

                using (var reader = new System.IO.StreamReader(inputPath))
                {
                    ReadLicense(reader, false);

                    string line;

                    line = ReadLine(reader);
                    if (null == line)
                    {
                        throw new Exception("Interface file is empty");
                    }

                    // namespace
                    var namespaceStrings = line.Split(new char[] { ' ' });
                    if (!namespaceStrings[0].Equals("namespace"))
                    {
                        throw new Exception("Interface file '{0}' does not start with namespace or comments; instead starts with '{1}'", inputPath, namespaceStrings[0]);
                    }
                    var namespaceName = namespaceStrings[1];
                    Log("\tNamespace: '{0}'", namespaceName);

                    // opening namespace scope
                    line = ReadLine(reader);
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("No scope opened after namespace");
                    }

                    // interface
                    line = ReadLine(reader, true);
                    var interfaceStrings = line.Split(new char[] { ' ' });
                    if ("public" != interfaceStrings[0] || "interface" != interfaceStrings[1])
                    {
                        throw new Exception("No public interface found: '{0}'", line);
                    }
                    var interfaceName = interfaceStrings[2];
                    Log("\tInterface: '{0}'", interfaceName);

                    // opening interface scope
                    line = ReadLine(reader);
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("No scope opened after interface");
                    }

                    var propertyList = propertyMap[inputPath] = new PropertySignatureList();
                    if (parameters.outputNamespace != namespaceName)
                    {
                        propertyList.InterfaceName = namespaceName + "." + interfaceName;
                    }
                    else
                    {
                        propertyList.InterfaceName = interfaceName;
                    }

                    Log(MinorHorizontalRule);
                    do
                    {
                        bool valueOnly = false;
                        line = ReadLine(reader, true, out valueOnly);
                        if (line.StartsWith("}"))
                        {
                            break;
                        }

                        var propertyStrings = line.Split(new char[] { ' ' });
                        if (propertyStrings.Length != 2)
                        {
                            throw new Exception("Property invalid at line '{0}'", line);
                        }
                        Log("Property '{0}' with return type '{1}':", propertyStrings[1], propertyStrings[0]);

                        var property = new PropertySignature();
                        property.Name = propertyStrings[1];
                        property.Type = propertyStrings[0];

                        // determine if the type is value or reference
                        var typeSplit = property.Type.Split(new char[] { '.' });
                        var trueType = typeSplit[typeSplit.Length - 1];
                        if (property.Type == "bool" ||
                            property.Type == "int" ||
                            (trueType.StartsWith("E") && System.Char.IsUpper(trueType[1])) // an BuildAMation enumeration
                            )
                        {
                            property.IsValueType = true;
                            Log("\tIs ValueType");
                        }
                        else
                        {
                            property.IsValueType = false;
                            Log("\tIs ReferenceType");
                        }

                        if (valueOnly)
                        {
                            Log("\tOnly has value. No delegate functions attached.");
                            property.ValueOnly = true;
                        }
                        else
                        {
                            Log("\tMaps to delegate functions in the OptionCollection");
                            property.ValueOnly = false;
                        }

                        // opening property scope
                        line = ReadLine(reader);
                        if (!line.StartsWith("{"))
                        {
                            throw new Exception("No scope opened after property");
                        }

                        line = ReadLine(reader);
                        if (line.Equals("get;"))
                        {
                            Log("\t1. Has getter");
                            property.HasGet = true;
                        }
                        else if (line.Equals("set;"))
                        {
                            Log("\t1. Has setter");
                            property.HasSet = true;
                        }
                        else
                        {
                            throw new Exception("Unexpected string '{0}'", line);
                        }

                        line = ReadLine(reader);
                        if (line.Equals("get;"))
                        {
                            Log("\t2. Has getter");
                            property.HasGet = true;
                        }
                        else if (line.Equals("set;"))
                        {
                            Log("\t2. Has setter");
                            property.HasSet = true;
                        }
                        else
                        {
                            throw new Exception("Unexpected string '{0}'", line);
                        }

                        // closing property scope
                        line = ReadLine(reader);
                        if (!line.StartsWith("}"))
                        {
                            throw new Exception("No scope closed after property");
                        }

                        propertyList.Add(property);
                    }
                    while (true);
                }
            }

            var delegateSignatures = new System.Collections.Generic.List<DelegateSignature>();
            if (null != parameters.inputDelegates)
            {
                foreach (var path in parameters.inputDelegates)
                {
                    delegateSignatures.Add(ReadDelegateDefinitionFile(path));
                }
            }

            if (Parameters.Mode.GenerateProperties == (parameters.mode & Parameters.Mode.GenerateProperties))
            {
                Log(MinorHorizontalRule);
                Log("Generating properties...");
                WritePropertiesFile(parameters, propertyMap);
            }
            if (Parameters.Mode.GenerateDelegates == (parameters.mode & Parameters.Mode.GenerateDelegates))
            {
                Log(MinorHorizontalRule);
                Log("Generating delegates...");
                WriteDelegatesFile(parameters, propertyMap, delegateSignatures);
            }
        }

        private static void
        WritePropertiesFile(
            Parameters parameters,
            PropertySignatureMap propertyMap)
        {
            // write out C# file containing the properties
            if (parameters.toStdOut)
            {
                Log("Would have written file '{0}'", parameters.outputPropertiesPathName);
            }
            using (var writer = !parameters.toStdOut ? new System.IO.StreamWriter(parameters.outputPropertiesPathName) : System.Console.Out)
            {
                writer.NewLine = "\n";

                WriteNewLicense(writer, parameters);
                WriteLine(writer, 0, "#region BamOptionGenerator");
                WriteLine(writer, 0, "// Automatically generated file from BamOptionGenerator. DO NOT EDIT.");
                WriteLine(writer, 0, "// Command line arguments:");
                foreach (var arg in parameters.args)
                {
                    if (ArgumentIsExcluded(arg, Parameters.excludedFlagsFromHeaders))
                    {
                        continue;
                    }
                    WriteLine(writer, 0, "//     {0}", arg.Replace(CommandSeparator, HeaderReplacementCommandSeparator));
                }
                WriteLine(writer, 0, "#endregion // BamOptionGenerator");
                WriteLine(writer, 0, "namespace {0}", parameters.outputNamespace);
                WriteLine(writer, 0, "{");
                WriteLine(writer, 1, "public partial class {0}", parameters.outputClassName);
                WriteLine(writer, 1, "{");

                foreach (var interfacePath in propertyMap.Keys)
                {
                    var propertyList = propertyMap[interfacePath];
                    WriteLine(writer, 2, "#region {0} Option properties", propertyList.InterfaceName);
                    foreach (var property in propertyList)
                    {
                        var typeString = property.IsValueType ? "ValueType" : "ReferenceType";
                        WriteLine(writer, 2, "{0} {1}.{2}", property.Type, propertyList.InterfaceName, property.Name);
                        WriteLine(writer, 2, "{");
                        if (property.HasGet)
                        {
                            WriteLine(writer, 3, "get");
                            WriteLine(writer, 3, "{");
                            WriteLine(writer, 4, "return this.Get{0}Option<{1}>(\"{2}\", this.SuperSetOptionCollection);", typeString, property.Type, property.Name);
                            WriteLine(writer, 3, "}");
                        }
                        if (property.HasSet)
                        {
                            WriteLine(writer, 3, "set");
                            WriteLine(writer, 3, "{");
                            WriteLine(writer, 4, "this.Set{0}Option<{1}>(\"{2}\", value);", typeString, property.Type, property.Name);
                            WriteLine(writer, 4, "this.ProcessNamedSetHandler(\"{0}SetHandler\", this[\"{0}\"]);", property.Name);
                            WriteLine(writer, 3, "}");
                        }
                        WriteLine(writer, 2, "}");
                    }
                    WriteLine(writer, 2, "#endregion");
                }

                WriteLine(writer, 1, "}");
                WriteLine(writer, 0, "}");
            }
            // flush to disk
            if (!parameters.toStdOut)
            {
                if (null != parameters.outputPropertiesPathName)
                {
                    Log("\tWrote file '{0}'", parameters.outputPropertiesPathName);
                }
            }
        }

        class IndentedString
        {
            public
            IndentedString(
                int numSpaces,
                string line)
            {
                this.NumSpaces = numSpaces;
                this.Line = line;
            }

            public int NumSpaces
            {
                get;
                private set;
            }

            public string Line
            {
                get;
                private set;
            }
        }

        class DelegateFileLayout
        {
            public System.Collections.Generic.List<string> license = null;
            public System.Collections.Generic.List<string> header = null;
            public string namespaceName = null;
            public string className = null;
            public System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<IndentedString>> functions = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<IndentedString>>();
        }

        private static DelegateFileLayout
        ReadAndParseDelegatesImplementation(
            Parameters parameters)
        {
            if (!System.IO.File.Exists(parameters.outputDelegatesPathName))
            {
                return null;
            }

            Log(MinorHorizontalRule);
            Log("Reading existing delegate implementation from '{0}'", parameters.outputDelegatesPathName);

            var layout = new DelegateFileLayout();
            using (var reader = new System.IO.StreamReader(parameters.outputDelegatesPathName))
            {
                layout.license = ReadLicense(reader, true);
                layout.header = ReadBamOptionGeneratorHeader(reader, true);

                string line = ReadLine(reader, true);
                // read the namespace
                if (!line.StartsWith("namespace"))
                {
                    throw new Exception("Expected 'namespace'; found '{0}'", line);
                }
                layout.namespaceName = line.Split(' ')[1];
                line = ReadLine(reader);
                if (!line.StartsWith("{"))
                {
                    throw new Exception("Expected namespace opening scope '{'; found '{0}'", line);
                }
                line = ReadLine(reader);
                while (string.IsNullOrEmpty(line))
                {
                    line = ReadLine(reader);
                }

                // read the class
                if (!line.Contains("class"))
                {
                    throw new Exception("Expected a class; found '{0}'", line);
                }
                layout.className = line.Split(' ')[3];
                line = ReadLine(reader);
                if (!line.StartsWith("{"))
                {
                    throw new Exception("Expected namespace opening scope '{'; found '{0}'", line);
                }
                line = ReadLine(reader);
                while (string.IsNullOrEmpty(line))
                {
                    line = ReadLine(reader);
                }

                // find functions
                for (;;)
                {
                    // done reading - it's the end of the class
                    if (line.StartsWith("}"))
                    {
                        break;
                    }

                    Log("\tDelegate source line: '{0}'", line);

                    // ignored regions
                    if (line.StartsWith("#region") || line.StartsWith("#endregion"))
                    {
                        Log("\t\tIgnored preprocessor line");
                        line = ReadLine(reader);
                        continue;
                    }

                    // look for function signatures
                    var functionSignature = new System.Text.StringBuilder();
                    for (;;)
                    {
                        functionSignature.AppendFormat("{0} ", line);
                        if (line.EndsWith(")"))
                        {
                            break;
                        }
                        line = ReadLine(reader);
                    }

                    // use a key that will not change if the signature changes
                    var funcSigString = functionSignature.ToString();
                    var functionName = funcSigString.Substring(0, funcSigString.IndexOf('('));
                    functionName = functionName.Substring(functionName.LastIndexOf(' ') + 1);
                    var body = layout.functions[functionName] = new System.Collections.Generic.List<IndentedString>();
                    int braceCount = 0;
                    int baselineIndentation = -1;
                    for (;;)
                    {
                        int indentation;
                        line = ReadLine(reader, out indentation);
                        if (baselineIndentation == -1)
                        {
                            baselineIndentation = indentation;
                        }
                        int openBraces = line.Split('{').Length - 1;
                        int closeBraces = line.Split('}').Length - 1;
                        //Log("Found {0} open braces and {1} close braces", openBraces, closeBraces);

                        braceCount += openBraces;
                        if (braceCount > 0)
                        {
                            body.Add(new IndentedString(indentation - baselineIndentation, line));
                        }

                        braceCount -= closeBraces;
                        if (0 == braceCount)
                        {
                            break;
                        }
                    }
                    Log("\t\tRead body of function '{0}'", functionName);

                    line = ReadLine(reader);
                }
            }

            return layout;
        }

        private static bool
        ArgumentIsExcluded(
            string arg,
            System.Collections.Specialized.StringCollection excludedList)
        {
            if (Parameters.excludedFlagsFromHeaders.Contains(arg))
            {
                return true;
            }
            foreach (var excluded in Parameters.excludedFlagsFromHeaders)
            {
                if (arg.StartsWith(excluded))
                {
                    return true;
                }
            }
            return false;
        }

        private static void
        WriteDelegatesFile(
            Parameters parameters,
            PropertySignatureMap propertyMap,
            System.Collections.Generic.List<DelegateSignature> delegateSignatures)
        {
            // write out C# file containing the delegates
            if (parameters.toStdOut)
            {
                Log("Would have written file '{0}'", parameters.outputDelegatesPathName);
            }

            var layout = ReadAndParseDelegatesImplementation(parameters);

            var memStream = new System.IO.MemoryStream();
            using (var writer = !parameters.toStdOut ? new System.IO.StreamWriter(memStream) : System.Console.Out)
            {
                writer.NewLine = "\n";

                // write license
                if (null != layout && null != layout.license)
                {
                    foreach (var line in layout.license)
                    {
                        WriteLine(writer, 0, line);
                    }
                }
                else
                {
                    WriteNewLicense(writer, parameters);
                }

                var headerBuilder = new System.Collections.Generic.List<string>();
                bool writeToDisk = (null == layout);

                // write header
                headerBuilder.Add("// Automatically generated file from BamOptionGenerator.");
                headerBuilder.Add("// Command line arguments:");
                foreach (var arg in parameters.args)
                {
                    if (ArgumentIsExcluded(arg, Parameters.excludedFlagsFromHeaders))
                    {
                        continue;
                    }
                    headerBuilder.Add(System.String.Format("//     {0}", arg.Replace(CommandSeparator, HeaderReplacementCommandSeparator)));
                }
                if (null != layout &&
                    null != layout.header &&
                    layout.header.Count != 0)
                {
                    var big = System.Math.Min(headerBuilder.Count, layout.header.Count);
                    var differenceIndices = new System.Collections.Generic.List<int>();
                    int index = 0;
                    for (; index < big; ++index)
                    {
                        if (headerBuilder[index] != layout.header[index])
                        {
                            differenceIndices.Add(index);
                        }
                    }
                    if (headerBuilder.Count > big)
                    {
                        for (; index < headerBuilder.Count; ++index)
                        {
                            if (!string.IsNullOrEmpty(headerBuilder[index]))
                            {
                                differenceIndices.Add(index);
                            }
                        }
                    }
                    if (layout.header.Count > big)
                    {
                        for (; index < layout.header.Count; ++index)
                        {
                            if (!string.IsNullOrEmpty(layout.header[index]))
                            {
                                differenceIndices.Add(index);
                            }
                        }
                    }

                    var useOriginalHeader = false;
                    if (differenceIndices.Count > 0)
                    {
                        if (parameters.ignoreHeaderUpdates)
                        {
                            useOriginalHeader = true;
                        }
                        else
                        {
                            var message = new System.Text.StringBuilder();
                            message.AppendFormat("Headers are different:\n");
                            foreach (int diff in differenceIndices)
                            {
                                var lhs = diff >= headerBuilder.Count ? "Undefined" : headerBuilder[diff];
                                var rhs = diff >= layout.header.Count ? "Undefined" : layout.header[diff];
                                message.AppendFormat("{0}: {1} {2}", diff, lhs, rhs);
                            }

                            if (parameters.forceWrite || parameters.updateHeader)
                            {
                                Log(message.ToString());
                                Log("**** FORCE WRITE ALLOWING THIS TO CONTINUE\n");
                                writeToDisk = true;
                            }
                            else
                            {
                                throw new Exception(message.ToString());
                            }
                        }
                    }
                    else
                    {
                        useOriginalHeader = true;
                    }

                    if (useOriginalHeader)
                    {
                        headerBuilder = layout.header;
                    }
                }
                else
                {
                    writeToDisk = true;
                }
                WriteLine(writer, 0, "#region BamOptionGenerator");
                foreach (var line in headerBuilder)
                {
                    WriteLine(writer, 0, line);
                }
                WriteLine(writer, 0, "#endregion // BamOptionGenerator");

                // open namespace
                if (null != layout && layout.namespaceName != parameters.outputNamespace)
                {
                    throw new Exception("Namespaces are different:\nFile:\n'{0}'\nNew:\n'{1}'", layout.namespaceName, parameters.outputNamespace);
                }
                WriteLine(writer, 0, "namespace {0}", parameters.outputNamespace);
                WriteLine(writer, 0, "{");

                // open OptionCollection partial class
                if (null != layout && layout.className != parameters.outputClassName)
                {
                    throw new Exception("Classes are different:\nFile:\n'{0}'\nNew:\n'{1}'", layout.className, parameters.outputClassName);
                }
                WriteLine(writer, 1, "public partial class {0}", parameters.outputClassName);
                WriteLine(writer, 1, "{");

                var delegatesToRegister = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();

                // write delegates for each property of the interface
                var functionCount = 1; // start with one for the SetDelegates function
                foreach (var interfacePath in propertyMap.Keys)
                {
                    WriteLine(writer, 2, "#region {0} Option delegates", propertyMap[interfacePath].InterfaceName);
                    foreach (var property in propertyMap[interfacePath])
                    {
                        if (property.ValueOnly)
                        {
                            delegatesToRegister[property.Name] = null;
                            continue;
                        }
                        else
                        {
                            delegatesToRegister[property.Name] = new System.Collections.Generic.List<string>();
                        }

                        foreach (var delegateSig in delegateSignatures)
                        {
                            Log(MinorHorizontalRule);

                            var delegateName = property.Name + delegateSig.InNamespace;
                            delegatesToRegister[property.Name].Add(delegateName);

                            // write function declaration
                            WriteLine(writer, 2, "{0} static {1}", parameters.isBaseClass ? "public" : "private", delegateSig.ReturnType);
                            WriteLine(writer, 2, "{0}(", delegateName);
                            for (int argIndex = 0; argIndex < delegateSig.Arguments.Count; ++argIndex)
                            {
                                if (argIndex == delegateSig.Arguments.Count - 1)
                                {
                                    WriteLine(writer, 3, "{0})", delegateSig.Arguments[argIndex]);
                                }
                                else
                                {
                                    WriteLine(writer, 3, "{0},", delegateSig.Arguments[argIndex]);
                                }
                            }

                            // write function body
                            if (null != layout && layout.functions.ContainsKey(delegateName))
                            {
                                Log("\tFunction '{0}' reusing from file", delegateName);
                                foreach (var line in layout.functions[delegateName])
                                {
                                    // TODO: magic number
                                    WriteLine(writer, 2 + line.NumSpaces / 4, line.Line);
                                }
                            }
                            else
                            {
                                Log("\tFunction '{0}' generating empty code for", delegateName);
                                WriteLine(writer, 2, "{");
                                if (null != delegateSig.Body)
                                {
                                    foreach (var line in delegateSig.Body)
                                    {
                                        WriteLine(writer, 3, line);
                                    }
                                }
                                WriteLine(writer, 2, "}");
                                writeToDisk = true;
                            }

                            ++functionCount;
                        }
                    }
                    WriteLine(writer, 2, "#endregion");
                }

                if (null != layout && (functionCount != layout.functions.Count))
                {
                    writeToDisk = true;
                }

                Log(MinorHorizontalRule);

                // write the SetDelegates function that assigns the accumulated delegates above to their named properties
                var setDelegatesFunctionName = "SetDelegates";
                WriteLine(writer, 2, "protected override void");
                WriteLine(writer, 2, "{0}(", setDelegatesFunctionName);
                WriteLine(writer, 3, "Bam.Core.DependencyNode node)");
                if (!writeToDisk && null != layout && layout.functions.ContainsKey(setDelegatesFunctionName))
                {
                    Log("\tReusing existing code for function: '{0}'", setDelegatesFunctionName);
                    foreach (var line in layout.functions[setDelegatesFunctionName])
                    {
                        // TOOD: magic number
                        WriteLine(writer, 2 + line.NumSpaces/4, line.Line);
                    }
                }
                else
                {
                    if (null == parameters.privateDataClassName)
                    {
                        throw new Exception("Private data class name was not provided");
                    }

                    Log("\tGenerating new code for the function: '{0}'", setDelegatesFunctionName);
                    WriteLine(writer, 2, "{");
                    if (parameters.extendedDelegates)
                    {
                        WriteLine(writer, 3, "base.{0}(node);", setDelegatesFunctionName);
                    }
                    foreach (var propertyName in delegatesToRegister.Keys)
                    {
                        var registration = new System.Text.StringBuilder();
                        if (null != delegatesToRegister[propertyName])
                        {
                            registration.AppendFormat("this[\"{0}\"].PrivateData = new {1}(", propertyName, parameters.privateDataClassName);
                            if (delegatesToRegister[propertyName].Count > 0)
                            {
                                foreach (var delegateToRegister in delegatesToRegister[propertyName])
                                {
                                    registration.AppendFormat("{0},", delegateToRegister);
                                }
                                registration.Remove(registration.Length - 1, 1); // last comma
                            }
                            registration.Append(");");
                        }
                        else
                        {
                            registration.AppendFormat("// Property '{0}' is value only - no delegates", propertyName);
                        }
                        WriteLine(writer, 3, registration.ToString());
                    }
                    WriteLine(writer, 2, "}");
                    writeToDisk = true;
                }

                // close the class
                WriteLine(writer, 1, "}");

                // close the namespace
                WriteLine(writer, 0, "}");

                // flush to disk
                if (!parameters.toStdOut)
                {
                    if (!writeToDisk && !parameters.forceWrite)
                    {
                        Log("\tFile '{0}' already up-to-date", parameters.outputDelegatesPathName);
                    }
                    else
                    {
                        writer.Flush();

                        using (var finalWriter = new System.IO.StreamWriter(parameters.outputDelegatesPathName))
                        {
                            memStream.WriteTo(finalWriter.BaseStream);
                        }

                        Log("\tWrote file '{0}'", parameters.outputDelegatesPathName);
                    }
                }
            }
        }

        static int
        Main(
            string[] args)
        {
            try
            {
                var parameters = ProcessArgs(args);
                Validate(parameters);
                Execute(parameters);
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Local exception");
                System.Console.Error.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
                return -1;
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine("System exception");
                System.Console.Error.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
                return -2;
            }

            return 0;
        }
    }
}
