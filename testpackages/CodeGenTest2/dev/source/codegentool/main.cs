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
                file.WriteLine("\tprintf(\"Hello world\\n\");");
                file.WriteLine(@"}");
                file.WriteLine("");

                System.Console.WriteLine("Generated source file written to '{0}'", path);
            }

            return 0;
        }
    }
}
