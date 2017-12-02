namespace Bam.Core.Test
{
    [NUnit.Framework.TestFixture]
    public class TokenizedStringTest
    {
        [NUnit.Framework.Test]
        public void
        InlineStringThrowsIfParsed()
        {
            var inline = Bam.Core.TokenizedString.CreateInline("Hello World");
            NUnit.Framework.Assert.That(() => inline.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>());
        }
    }
}
