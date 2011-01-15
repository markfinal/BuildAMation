// <copyright file="ArgumentProcessor.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.ArgumentProcessor(typeof(MingwCommon.ArgumentProcessor))]

namespace MingwCommon
{
    public sealed class ArgumentProcessor : Opus.Core.IArgumentProcessor
    {
        private readonly string InstallPathSwitch = "-mingw.installpath";

        public bool Process(string argument)
        {
            string[] split = argument.Split(new char[] { '=' });
            if (split.Length == 2)
            {
                if (split[0] == InstallPathSwitch)
                {
                    Opus.Core.State.AddCategory("Mingw");
                    Opus.Core.State.Set("Mingw", "InstallPath", split[1]);

                    return true;
                }
            }

            return false;
        }
    }
}