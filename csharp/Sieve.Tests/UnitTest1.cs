namespace Sieve.Tests

{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestNthPrime()
        {
            ISieve sieve = new SieveImplementation();
            Assert.AreEqual(2, sieve.NthPrime(0));
            Assert.AreEqual(71, sieve.NthPrime(19));
            Assert.AreEqual(541, sieve.NthPrime(99));

            // Bounds Checks
            Assert.ThrowsException<ArgumentException>( () => sieve.NthPrime(-1));

            // Extra tests to validate 229 switchover point.
            Assert.AreEqual(1447, sieve.NthPrime(228));
            Assert.AreEqual(1451, sieve.NthPrime(229));
            Assert.AreEqual(1453, sieve.NthPrime(230));

            Assert.AreEqual(3581, sieve.NthPrime(500));
            Assert.AreEqual(7793, sieve.NthPrime(986));
            Assert.AreEqual(17393, sieve.NthPrime(2000));
            Assert.AreEqual(15485867, sieve.NthPrime(1000000));
            Assert.AreEqual(179424691, sieve.NthPrime(10000000));
            // This works but takes about a minute.
            //Assert.AreEqual(2038074751, sieve.NthPrime(100000000)); //not required, just a fun challenge
        }
    }
}