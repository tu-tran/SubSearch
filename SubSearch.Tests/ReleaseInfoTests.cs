using System.Linq;
using NUnit.Framework;
using SubSearch.Data;

namespace SubSearch.Tests
{
    /// <summary>
    /// Tests the <see cref="ReleaseInfo"/>
    /// </summary>
    [TestFixture]
    internal class ReleaseInfoTests
    {
        /// <summary>
        /// Compares the test.
        /// </summary>
        [Test]
        public void ConstructorTest()
        {
            var target = new ReleaseInfo("Supernatural S02E09 Croatoan (1080p x265 Joy).mkv");
        }
    }
}