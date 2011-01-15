// <copyright file="ArgumentProcessor.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.ArgumentProcessor(typeof(VisualCCommon.ArgumentProcessor))]

namespace VisualCCommon
{
    public sealed class ArgumentProcessor : Opus.Core.IArgumentProcessor
    {
        private readonly string InstallPathSwitch = "-visualc.installpath";

        public bool Process(string argument)
        {
            string[] split = argument.Split(new char[] { '=' });
            if (split.Length == 2)
            {
                if (split[0] == InstallPathSwitch)
                {
                    Opus.Core.State.AddCategory("VisualC");
                    Opus.Core.State.Set("VisualC", "InstallPath", split[1]);

                    return true;
                }
            }

            return false;
        }
    }
}