namespace Bam.Core.Test
{
    class TokenizedStringTestModule :
        Bam.Core.Module
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // this is quite ugly, but you can only use
            // Bam.Core.Graph.FindReferencedModule while modules are being
            // created, and the tests need a handle to the module
            TokenizedStringTest.testModule = this;
        }

        public override void Evaluate()
        {
            throw new System.NotImplementedException();
        }

        protected override void ExecuteInternal(ExecutionContext context)
        {
            throw new System.NotImplementedException();
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }

    [NUnit.Framework.TestFixture(Author="Mark Final")]
    [NUnit.Framework.TestOf(typeof(Bam.Core.TokenizedString))]
    public class TokenizedStringTest
    {
        private Bam.Core.Graph graph;
        public static Bam.Core.Module testModule = null; // workaround for not being able to use Graph.FindReferencedModule

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
            NUnit.Framework.Assert.That(1 == Bam.Core.TokenizedString.Count);
        }

        [NUnit.Framework.Test]
        public void
        VerbatimStringIsAlreadyParsed()
        {
            var verbatim = Bam.Core.TokenizedString.CreateVerbatim("Hello World");
            NUnit.Framework.Assert.IsTrue(verbatim.IsParsed);
            NUnit.Framework.Assert.That(1 == Bam.Core.TokenizedString.Count);
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
            NUnit.Framework.Assert.That(2 == Bam.Core.TokenizedString.Count);
        }

        [NUnit.Framework.Test]
        public void
        ReferencedButNullPositionalArgumentList()
        {
            var str = Bam.Core.TokenizedString.Create("$(0)", null, null);
            NUnit.Framework.Assert.That(() => str.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                InnerException.TypeOf<System.ArgumentOutOfRangeException>());
            NUnit.Framework.Assert.That(1 == Bam.Core.TokenizedString.Count);
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
            NUnit.Framework.Assert.That(2 == Bam.Core.TokenizedString.Count);
        }

        [NUnit.Framework.Test]
        public void
        UnknownPostFunction()
        {
            var str = Bam.Core.TokenizedString.Create("@failunittest()", null, null);
            NUnit.Framework.Assert.That(() => str.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                Message.Contains("Unknown post-function 'failunittest'"));
            NUnit.Framework.Assert.That(1 == Bam.Core.TokenizedString.Count);
        }

        [NUnit.Framework.Test]
        public void
        UnknownPreFunction()
        {
            var str = Bam.Core.TokenizedString.Create("#failunittest()", null, null);
            NUnit.Framework.Assert.That(() => str.Parse(),
                NUnit.Framework.Throws.Exception.TypeOf<Bam.Core.Exception>().And.
                Message.Contains("Unknown pre-function 'failunittest'"));
            NUnit.Framework.Assert.That(1 == Bam.Core.TokenizedString.Count);
        }

        [NUnit.Framework.Test]
        public void
        CanAddVerbatimMacro()
        {
            var env = new Bam.Core.Environment();
            env.Configuration = Bam.Core.EConfiguration.Debug;
            this.graph.CreateTopLevelModuleFromTypes(new [] {typeof(TokenizedStringTestModule)}, env);
            NUnit.Framework.Assert.That(1 == Bam.Core.Module.Count);

            var module = TokenizedStringTest.testModule;
            module.Macros.AddVerbatim("Macro1", "Hello World");

            var str = module.CreateTokenizedString("$(Macro1)");
            str.Parse();
            NUnit.Framework.Assert.That("Hello World" == str.ToString());

            TokenizedStringTest.testModule = null;
        }
    }
}
