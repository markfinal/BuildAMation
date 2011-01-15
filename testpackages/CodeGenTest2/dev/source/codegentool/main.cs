namespace CodeGenTool
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("Not enough arguments");
                return -1;
            }

            string path = System.String.Format("{0}{1}{2}.c", args[0], System.IO.Path.DirectorySeparatorChar, args[1]);

            using (System.IO.TextWriter file = new System.IO.StreamWriter(path))
            {
                if (null == file)
                {
                    System.Console.WriteLine("Unable to open '{0}' for writing", path);
                    return -2;
                }

                file.WriteLine(@"#include <stdio.h>");
                file.WriteLine(@"void MyGeneratedFunction()");
                file.WriteLine(@"{");
                file.WriteLine("\tprintf(\"Hello world\");");
                file.WriteLine(@"}");
                file.WriteLine("");

                System.Console.WriteLine("Generated source file written to '{0}'", path);
            }

            return 0;
        }
    }
}
