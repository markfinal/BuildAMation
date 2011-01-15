// <copyright file="ArgumentProcessor.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.ArgumentProcessor(typeof(CSharp.ArgumentProcessor))]

namespace CSharp
{
    public sealed class ArgumentProcessor : Opus.Core.IArgumentProcessor
    {
        private readonly string ToolchainSwitch = "-CSharp.toolchain";

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
                Opus.Core.Log.DebugMessage("CSharp toolchain is '{0}'", split[1]);

                if (!Opus.Core.State.HasCategory("Toolchains"))
                {
                    Opus.Core.State.AddCategory("Toolchains");
                }

                if (Opus.Core.State.Has("Toolchains", "CSharp"))
                {
                    throw new Opus.Core.Exception(System.String.Format("Toolchain for 'CSharp' has already been defined as '{0}'", Opus.Core.State.Get("Toolchains", "CSharp") as string));
                }

                Opus.Core.State.Add<string>("Toolchains", "CSharp", split[1]);

                return true;
            }

            return false;
        }
    }
}