// <copyright file="ArgumentProcessor.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>NativeBuilder package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.ArgumentProcessor(typeof(NativeBuilder.ArgumentProcessor))]

namespace NativeBuilder
{
    public sealed class ArgumentProcessor : Opus.Core.IArgumentProcessor
    {
        private readonly string ForceBuildSwitch = "-forcebuild";

        public ArgumentProcessor()
        {
            Opus.Core.State.AddCategory("NativeBuilder");
            Opus.Core.State.Add<bool>("NativeBuilder", "ForceBuild", false);
        }

        public bool Process(string argument)
        {
            if (Opus.Core.State.BuilderName == "Native")
            {
                if (ForceBuildSwitch == argument)
                {
                    Opus.Core.State.Set("NativeBuilder", "ForceBuild", true);

                    return true;
                }
            }

            return false;
        }
    }
}