using System.Linq;
using NUnit.Framework;
using SubSearch.Data;

namespace SubSearch.Tests
{
    /// <summary>
    /// Tests the <see cref="ItemDataComparer"/>
    /// </summary>
    [TestFixture]
    internal class ItemDataComparerTests
    {
        /// <summary>
        /// Compares the test.
        /// </summary>
        [Test]
        public void CompareTest()
        {
            var winner = "2.Broke.Girls.S01E01.1080p.WEB-DL.x265.HEVC-zsewdc";
            var data = new[]
            {
                "2 Broke Girls - 1X01 - Pilot",
                "2.Broke.Girls.S01E01.REPACK.720p.HDTV.x264-IMMERSE-eng",
                winner,
                "2.Broke.Girls.S01E01.REPACK.720p.HDTV.x264-IMMERSE",
                "2.Broke.Girls.S01E01.720p.bluray.x264-psychd_ENG"
            };            

            var target = new ItemDataComparer("2.Broke.Girls.S01E01.1080p.WEB-DL.x265.HEVC-zsewdc");
            var points = data.Select(n => target.GetMatchesPoints(n)).ToArray();
            Assert.AreEqual(points.Max(), target.GetMatchesPoints(winner));
        }
    }
}
