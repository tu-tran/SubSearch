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

        /// <summary>
        /// Compares the test.
        /// </summary>
        [Test]
        public void Compare2Test()
        {
            var winner = "Supernatural.S12E01.1080p.HDTV.X264-DIMENSION.en_lyrics";
            var data = new[]
            {
                "Supernatural.S12E01.HDTV.x264-LOL",
                winner,
                "Supernatural.S12E01.1080p.HDTV.X264-DIMENSION.en_hi",
                "Supernatural.S12E01.1080p.HDTV.X264-DIMENSION.en",
                "Harry Potter and the Deathly Hallows Part 2 (1080p x265 10bit Joy) ( 1.85GB , 6Ch , 10Bit )",
                "Star Wars Episode I The Phantom Menace 1999 (1080p x265 10bit Joy) ( 1.92GB , 8Ch )"
            };

            var target = new ItemDataComparer("Supernatural S12E01 Keep Calm and Carry On  (1080p x265 10bit Joy).mkv");
            var points = data.Select(n => target.GetMatchesPoints(n)).ToArray();
            Assert.AreEqual(points.Max(), target.GetMatchesPoints(winner));
        }

        /// <summary>
        /// Compares the test.
        /// </summary>
        [Test]
        public void Compare3Test()
        {
            var winner = "Supernatural.S02E09.720p.BluRay.x264-SiNNERS-HI";
            var data = new[]
            {
                "The Last p S02E09 Uneasy Lies the Head (1080p x265 Joy) ( 5.1 Audio - 329MB )",
                "The Last p S02E12 Cry Havoc (1080p x265 Joy) ( 5.1 Audio - 329MB )",
                "Supernatural.S02E09.DVDrip.XviD-SAiNTS-eng",
                winner,
                "Supernatural.S02E09.720p.BluRay.x264-SiNNERS"
            };

            var target = new ItemDataComparer("Supernatural S02E09 Croatoan (1080p x265 Joy).mkv");
            var points = data.Select(n => target.GetMatchesPoints(n)).ToArray();
            Assert.AreEqual(points.Max(), target.GetMatchesPoints(winner));
        }
    }
}
