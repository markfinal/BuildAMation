// <copyright file="Program.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Option Interface Property Generator</summary>
// <author>Mark Final</author>
namespace OpusOptionInterfacePropertyGenerator
{
    class Exception : System.Exception
    {
        public Exception(string message)
            : base(message)
        {
        }
    }

    class PropertySignature
    {
        public string Type
        {
            get;
            set;
        }

        public bool IsValueType
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Interface
        {
            get;
            set;
        }

        public bool HasGet
        {
            get;
            set;
        }

        public bool HasSet
        {
            get;
            set;
        }
    }

    class DelegateSignature
    {
        public string InNamespace
        {
            get;
            set;
        }

        public string ReturnType
        {
            get;
            set;
        }

        public string ArgumentString
        {
            get;
            set;
        }
    }

    class Parameters
    {
        [System.Flags]
        public enum Mode
        {
            GenerateProperties = (1<<0),
            GenerateDelegates  = (1<<1)
        }

        public string[] args;
        public string outputPropertiesPathName;
        public string outputDelegatesPathName;
        public string[] inputPathNames;
        public string[] inputDelegates;
        public string outputNamespace;
        public string outputClassName;
        public string privateDataClassName;
        public Mode mode;
        public bool toStdOut;
    }

    class Program
    {
        static Parameters ProcessArgs(string[] args)
        {
            if (0 == args.Length)
            {
                throw new Exception("Arguments required");
            }

            Parameters parameters = new Parameters();
            parameters.args = args;
            parameters.mode = 0;

            foreach (string arg in args)
            {
                if (arg.StartsWith("-o="))
                {
                    parameters.mode |= Parameters.Mode.GenerateProperties;
                    // redundant, ignore
                }
                else if (arg.StartsWith("-i="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    string inputFilesString = split[1];
                    parameters.inputPathNames = inputFilesString.Split(new char[] { System.IO.Path.PathSeparator });
                }
                else if (arg.StartsWith("-n="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    parameters.outputNamespace = split[1];
                }
                else if (arg.StartsWith("-c="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    parameters.outputClassName = split[1];
                }
                else if (arg.StartsWith("-pv="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    parameters.privateDataClassName = split[1];
                }
                else if (arg.StartsWith("-p"))
                {
                    parameters.mode |= Parameters.Mode.GenerateProperties;
                }
                else if (arg.StartsWith("-dd="))
                {
                    string[] split = arg.Split(new char[] { '=' });
                    string inputFilesString = split[1];
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
                else
                {
                    throw new Exception(System.String.Format("Unrecognized argument '{0}'", arg));
                }
            }

            return parameters;
        }

        static void Validate(Parameters parameters)
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

        static string ReadLine(System.IO.TextReader reader)
        {
            string line = null;
            do
            {
                line = reader.ReadLine();
                if (null == line)
                {
                    break;
                }
                line = line.Trim();
            }
            while (0 == line.Length);
            return line;
        }

        static void Write(System.IO.TextWriter writer, int tabCount, string format, params string[] args)
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

        static void WriteLine(System.IO.TextWriter writer, int tabCount, string format, params string[] args)
        {
            Write(writer, tabCount, format, args);
            writer.Write(writer.NewLine);
        }

        static DelegateSignature ReadDelegate(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                throw new Exception(System.String.Format("File '{0}' does not exist", filename));
            }
            System.Console.WriteLine("\nDelegate to read: '{0}'", filename);

            DelegateSignature signature = new DelegateSignature();

            using (System.IO.TextReader reader = new System.IO.StreamReader(filename))
            {
                string line;

                line = ReadLine(reader);
                if (null == line)
                {
                    throw new Exception("Interface file is empty");
                }
                // ignore comments
                while (line.StartsWith("//"))
                {
                    line = ReadLine(reader);
                }
                // namespace
                string[] namespaceStrings = line.Split(new char[] { ' ' });
                if (!namespaceStrings[0].Equals("namespace"))
                {
                    throw new Exception(System.String.Format("Interface file does not start with namespace or comments; instead starts with '{0}'", namespaceStrings[0]));
                }
                string namespaceName = namespaceStrings[1];
                System.Console.WriteLine("Namespace found is '{0}'", namespaceName);

                // opening namespace scope
                line = ReadLine(reader);
                if (!line.StartsWith("{"))
                {
                    throw new Exception("No scope opened after namespace");
                }

                // delegate
                line = ReadLine(reader);
                string[] delegateStrings = line.Split(new char[] { ' ' });
                if ("public" != delegateStrings[0] || "delegate" != delegateStrings[1])
                {
                    throw new Exception("No public delegate found");
                }
                System.Console.WriteLine("Delegate found is '{0}'", line);
                string returnType = delegateStrings[2];
                int firstParenthesis = line.IndexOf('(', 0);
                string arguments = line.Substring(firstParenthesis, line.Length - firstParenthesis - 1);

                signature.InNamespace = namespaceName;
                signature.ReturnType = returnType;
                signature.ArgumentString = arguments;
            }

            return signature;
        }

        static void Execute(Parameters parameters)
        {
            System.Collections.Generic.List<PropertySignature> propertyList = new System.Collections.Generic.List<PropertySignature>();

            // TODO:
            // handle anything before an interface, e.g. enum
            // handle comments
            foreach (string inputPath in parameters.inputPathNames)
            {
                if (!System.IO.File.Exists(inputPath))
                {
                    throw new Exception(System.String.Format("Input file '{0}' does not exist", inputPath));
                }
                System.Console.WriteLine("\nInterface to read: '{0}'", inputPath);

                using (System.IO.TextReader reader = new System.IO.StreamReader(inputPath))
                {
                    string line;

                    line = ReadLine(reader);
                    if (null == line)
                    {
                        throw new Exception("Interface file is empty");
                    }
                    // ignore comments
                    while (line.StartsWith("//"))
                    {
                        line = ReadLine(reader);
                    }
                    // namespace
                    string[] namespaceStrings = line.Split(new char[] { ' ' });
                    if (!namespaceStrings[0].Equals("namespace"))
                    {
                        throw new Exception(System.String.Format("Interface file does not start with namespace or comments; instead starts with '{0}'", namespaceStrings[0]));
                    }
                    string namespaceName = namespaceStrings[1];
                    System.Console.WriteLine("Namespace found is '{0}'", namespaceName);

                    // opening namespace scope
                    line = ReadLine(reader);
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("No scope opened after namespace");
                    }

                    // interface
                    line = ReadLine(reader);
                    string[] interfaceStrings = line.Split(new char[] { ' ' });
                    if ("public" != interfaceStrings[0] || "interface" != interfaceStrings[1])
                    {
                        throw new Exception("No public interface found");
                    }
                    string interfaceName = interfaceStrings[2];
                    System.Console.WriteLine("Interface found is '{0}'", interfaceName);

                    // opening interface scope
                    line = ReadLine(reader);
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("No scope opened after interface");
                    }

                    do
                    {
                        line = ReadLine(reader);
                        if (line.StartsWith("}"))
                        {
                            break;
                        }

                        string[] propertyStrings = line.Split(new char[] { ' ' });
                        System.Console.WriteLine("PropertySignature found: type '{0}', name '{1}'", propertyStrings[0], propertyStrings[1]);

                        PropertySignature property = new PropertySignature();
                        property.Name = propertyStrings[1];
                        property.Type = propertyStrings[0];
                        if (parameters.outputNamespace != namespaceName)
                        {
                            property.Interface = namespaceName + "." + interfaceName;
                        }
                        else
                        {
                            property.Interface = interfaceName;
                        }

                        // determine if the type is value or reference
                        string[] typeSplit = property.Type.Split(new char[] { '.' });
                        string trueType = typeSplit[typeSplit.Length - 1];
                        if (property.Type == "bool" || property.Type == "int" || (trueType.StartsWith("E") && System.Char.IsUpper(trueType[1])))
                        {
                            property.IsValueType = true;
                            System.Console.WriteLine("\tIs ValueType");
                        }
                        else
                        {
                            property.IsValueType = false;
                            System.Console.WriteLine("\tIs ReferenceType");
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
                            System.Console.WriteLine("\twith get");
                            property.HasGet = true;
                        }
                        else if (line.Equals("set;"))
                        {
                            System.Console.WriteLine("\twith set");
                            property.HasSet = true;
                        }
                        else
                        {
                            throw new Exception(System.String.Format("Unexpected string '{0}'", line));
                        }

                        line = ReadLine(reader);
                        if (line.Equals("get;"))
                        {
                            System.Console.WriteLine("\twith get");
                            property.HasGet = true;
                        }
                        else if (line.Equals("set;"))
                        {
                            System.Console.WriteLine("\twith set");
                            property.HasSet = true;
                        }
                        else
                        {
                            throw new Exception(System.String.Format("Unexpected string '{0}'", line));
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

                    if (0 == propertyList.Count)
                    {
                        throw new Exception("No properties were found in the interface");
                    }
                }
            }

            System.Collections.Generic.List<DelegateSignature> delegateSignatures = new System.Collections.Generic.List<DelegateSignature>();
            foreach (string path in parameters.inputDelegates)
            {
                delegateSignatures.Add(ReadDelegate(path));
            }

            if (Parameters.Mode.GenerateProperties == (parameters.mode & Parameters.Mode.GenerateProperties))
            {
                System.Console.WriteLine("Generating properties...");
                WritePropertiesFile(parameters, propertyList);
            }
            if (Parameters.Mode.GenerateDelegates == (parameters.mode & Parameters.Mode.GenerateDelegates))
            {
                System.Console.WriteLine ("Generating delegates...");
                WriteDelegatesFile(parameters, propertyList, delegateSignatures);
            }
        }

        private static void WritePropertiesFile(Parameters parameters, System.Collections.Generic.List<PropertySignature> propertyList)
        {
            // write out C# file containing the properties
            if (parameters.toStdOut)
            {
                System.Console.WriteLine ("Would have written file '{0}'", parameters.outputPropertiesPathName);
            }
            using (System.IO.TextWriter writer = !parameters.toStdOut ? new System.IO.StreamWriter(parameters.outputPropertiesPathName) : System.Console.Out)
            {
                WriteLine(writer, 0, "// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.");
                WriteLine(writer, 0, "// Command line:");
                Write(writer, 0, "// ");
                foreach (string arg in parameters.args)
                {
                    Write(writer, 0, "{0} ", arg);
                }
                Write(writer, 0, writer.NewLine);
                WriteLine(writer, 0, "namespace {0}", parameters.outputNamespace);
                WriteLine(writer, 0, "{");
                WriteLine(writer, 1, "public partial class {0}", parameters.outputClassName);
                WriteLine(writer, 1, "{");

                foreach (PropertySignature property in propertyList)
                {
                    WriteLine(writer, 2, "{0} {1}.{2}", property.Type, property.Interface, property.Name);
                    WriteLine(writer, 2, "{");
                    if (property.HasGet)
                    {
                        WriteLine(writer, 3, "get");
                        WriteLine(writer, 3, "{");
                        WriteLine(writer, 4, "return this.Get{0}Option<{1}>(\"{2}\");", property.IsValueType ? "ValueType" : "ReferenceType", property.Type, property.Name);
                        WriteLine(writer, 3, "}");
                    }
                    if (property.HasSet)
                    {
                        WriteLine(writer, 3, "set");
                        WriteLine(writer, 3, "{");
                        WriteLine(writer, 4, "this.Set{0}Option<{1}>(\"{2}\", value);", property.IsValueType ? "ValueType" : "ReferenceType", property.Type, property.Name);
                        WriteLine(writer, 4, "this.ProcessNamedSetHandler(\"{0}SetHandler\", this[\"{0}\"]);", property.Name);
                        WriteLine(writer, 3, "}");
                    }
                    WriteLine(writer, 2, "}");
                }

                WriteLine(writer, 1, "}");
                WriteLine(writer, 0, "}");
            }
            if (null != parameters.outputPropertiesPathName)
            {
                System.Console.WriteLine("Wrote file '{0}'", parameters.outputPropertiesPathName);
            }
        }

        private static void WriteDelegatesFile(Parameters parameters, System.Collections.Generic.List<PropertySignature> propertyList, System.Collections.Generic.List<DelegateSignature> delegateSignatures)
        {
            // write out C# file containing the delegates
            if (parameters.toStdOut)
            {
                System.Console.WriteLine ("Would have written file '{0}'", parameters.outputDelegatesPathName);
            }
            using (System.IO.TextWriter writer = !parameters.toStdOut ? new System.IO.StreamWriter(parameters.outputDelegatesPathName) : System.Console.Out)
            {
                WriteLine(writer, 0, "// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.");
                WriteLine(writer, 0, "// Command line:");
                Write(writer, 0, "// ");
                foreach (string arg in parameters.args)
                {
                    Write(writer, 0, "{0} ", arg);
                }
                Write(writer, 0, writer.NewLine);
                WriteLine(writer, 0, "namespace {0}", parameters.outputNamespace);
                WriteLine(writer, 0, "{");
                WriteLine(writer, 1, "public partial class {0}", parameters.outputClassName);
                WriteLine(writer, 1, "{");

                System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> delegatesToRegister = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();

                foreach (PropertySignature property in propertyList)
                {
                    delegatesToRegister[property.Name] = new System.Collections.Generic.List<string>();

                    foreach (DelegateSignature delegateSig in delegateSignatures)
                    {
                        string delegateName = property.Name + delegateSig.InNamespace;
                        delegatesToRegister[property.Name].Add(delegateName);

                        System.Text.StringBuilder propertyDelegate = new System.Text.StringBuilder();
                        propertyDelegate.AppendFormat("private {0} {1}{2}", delegateSig.ReturnType, delegateName, delegateSig.ArgumentString);
                        WriteLine(writer, 2, propertyDelegate.ToString());
                        WriteLine(writer, 2, "{");
                        WriteLine(writer, 2, "}");
                    }
                }

                WriteLine (writer, 2, "protected override void SetDelegates(Opus.Core.DependencyNode node)");
                WriteLine(writer, 2, "{");
                foreach (string propertyName in delegatesToRegister.Keys)
                {
                    System.Text.StringBuilder registration = new System.Text.StringBuilder();
                    registration.AppendFormat("this[\"{0}\"].PrivateData = new {1}(", propertyName, parameters.privateDataClassName);
                    foreach (string delegateToRegister in delegatesToRegister[propertyName])
                    {
                        registration.Append(delegateToRegister);
                    }
                    registration.Append(");");
                    WriteLine(writer, 3, registration.ToString());
                }
                WriteLine(writer, 2, "}");

                WriteLine(writer, 1, "}");
                WriteLine(writer, 0, "}");
            }
            if (null != parameters.outputPropertiesPathName)
            {
                System.Console.WriteLine("Wrote file '{0}'", parameters.outputPropertiesPathName);
            }
        }

        static int Main(string[] args)
        {
            try
            {
                Parameters parameters = ProcessArgs(args);
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
