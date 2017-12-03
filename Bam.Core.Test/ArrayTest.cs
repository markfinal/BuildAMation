namespace Bam.Core.Test
{
    [NUnit.Framework.TestFixture(Author = "Mark Final")]
    [NUnit.Framework.TestOf(typeof(Bam.Core.Array<>))]
    public class ArrayTest
    {
        Bam.Core.Array<int> intArray;

        [NUnit.Framework.SetUp]
        public void
        Setup()
        {
            this.intArray = new Bam.Core.Array<int>();
        }

        [NUnit.Framework.TearDown]
        public void
        TearDown()
        {
            this.intArray = null;
        }

        [NUnit.Framework.Test]
        public void
        NewIsEmpty()
        {
            NUnit.Framework.Assert.IsTrue(0 == this.intArray.Count);
        }
    }
}
