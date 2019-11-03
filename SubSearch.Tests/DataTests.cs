namespace SubSearch.Tests
{
    using NUnit.Framework;

    using SubSearch.Data.Handlers;
    using SubSearch.WPF.Controllers;
    using SubSearch.WPF.Views;

    [TestFixture]
    public class DataTests
    {
        [Test]
        public void TestMethod()
        {
            var title = "The.Forbidden.Girl.2013.1080p.BluRay.x264.DTS-FGT.mkv";
            var controller = new MainViewController(title, new DummyView(), new AggregateDb());
            var result = controller.Query();
        }
    }
}
