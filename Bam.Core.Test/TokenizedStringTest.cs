namespace Bam.Core.Test
{
    [NUnit.Framework.TestFixture(Author="Mark Final")]
    [NUnit.Framework.TestOf(typeof(Bam.Core.TokenizedString))]
    public class TokenizedStringTest
    {
        private Bam.Core.Graph graph;

        [NUnit.Framework.SetUp]
        public void
        Setup()
        {
            // because there is no BAM package, this can be set up incorrectly
            // see
            // - BamState.ExecutableDirectory (will be null)
            // - BamState.WorkingDirectory (will be null)
            // - Graph.PackageRepositories (will be empty)
            this.graph = Bam.Core.Graph.Instance;
            Bam.Core.TokenizedString.reset();
        }

        [NUnit.Framework.TearDown]
        public void
        Teardown()
        {
            this.graph = null;
        }

        [NUnit.Framework.Test]
        public void
        InlineStringThrowsIfParsed()
        {
            var inline = Bam.Core.TokenizedString.CreateInline("Hello World");
            NUnit.Framework.Assert.That(() => inline.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                Message.Contains("Inline TokenizedString cannot be parsed"));
        }

        [NUnit.Framework.Test]
        public void
        VerbatimStringIsAlreadyParsed()
        {
            var verbatim = Bam.Core.TokenizedString.CreateVerbatim("Hello World");
            NUnit.Framework.Assert.IsTrue(verbatim.IsParsed);
        }

        [NUnit.Framework.Test]
        public void
        ParsingAParsedStringThrows()
        {
            var verbatim = Bam.Core.TokenizedString.CreateVerbatim("Hello World");
            // verbatim strings do not require parsing
            NUnit.Framework.Assert.That(() => verbatim.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                Message.Contains("is already parsed"));

            var str = Bam.Core.TokenizedString.Create("Hello World", null, null);
            NUnit.Framework.Assert.IsFalse(object.ReferenceEquals(verbatim, str));
            // first parse
            str.Parse();
            // second parse
            NUnit.Framework.Assert.That(() => str.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                Message.Contains("is already parsed"));
        }

        [NUnit.Framework.Test]
        public void
        ReferencedButNullPositionalArgumentList()
        {
            var str = Bam.Core.TokenizedString.Create("$(0)", null, null);
            NUnit.Framework.Assert.That(() => str.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                InnerException.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void
        ReferencedButMissingPositionalArgumentList()
        {
            var one = Bam.Core.TokenizedString.CreateVerbatim("Hello");
            var str = Bam.Core.TokenizedString.Create("$(0) $(1)", null, new TokenizedStringArray{one});
            NUnit.Framework.Assert.That(() => str.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                InnerException.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void
        UnknownPostFunction()
        {
            var str = Bam.Core.TokenizedString.Create("@failunittest()", null, null);
            NUnit.Framework.Assert.That(() => str.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                Message.Contains("Unknown post-function 'failunittest'"));
        }

        [NUnit.Framework.Test]
        public void
        UnknownPreFunction()
        {
            var str = Bam.Core.TokenizedString.Create("#failunittest()", null, null);
            NUnit.Framework.Assert.That(() => str.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                Message.Contains("Unknown pre-function 'failunittest'"));
        }
    }
}
