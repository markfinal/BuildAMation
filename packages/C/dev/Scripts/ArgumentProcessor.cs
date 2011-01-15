// <copyright file="ArgumentProcessor.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.ArgumentProcessor(typeof(C.ArgumentProcessor))]

namespace C
{
    public sealed class ArgumentProcessor : Opus.Core.IArgumentProcessor
    {
        private readonly string ToolchainSwitch = "-C.toolchain";

        public ArgumentProcessor()
        {
        }

        public bool Process(string argument)
        {
            string[] split = argument.Split(new char[] { '=' });
            if (split.Length != 2)
            {
                return false;
            }

            if (ToolchainSwitch == split[0])
            {
                Opus.Core.Log.DebugMessage("C toolchain is '{0}'", split[1]);

                if (!Opus.Core.State.HasCategory("Toolchains"))
                {
                    Opus.Core.State.AddCategory("Toolchains");
                }

                if (Opus.Core.State.Has("Toolchains", "C"))
                {
                    throw new Opus.Core.Exception(System.String.Format("Toolchain for 'C' has already been defined as '{0}'", Opus.Core.State.Get("Toolchains", "C") as string));
                }

                Opus.Core.State.Add<string>("Toolchains", "C", split[1]);
                Opus.Core.State.Add<string>("Toolchains", "C.CPlusPlus", split[1]);

                return true;
            }

            return false;
        }
    }
}