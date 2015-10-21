using Bam.Core;
using System.Linq;
namespace DeltaSettingsTest1
{
    sealed class Test :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/source/*.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("D_COMMON");
                });

            source.Children.Where(item => (item as C.ObjectFile).InputPath.Parse().Contains("c.c")).ToList().ForEach(item =>
                item.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.TargetLanguage = C.ETargetLanguage.Cxx;
                        compiler.PreprocessorDefines.Remove("D_COMMON");
                    }));
        }
    }
}
