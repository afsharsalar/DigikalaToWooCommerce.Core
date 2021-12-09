using DigikalaToWooCommerce.Core.Service;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
using System.Threading.Tasks;

namespace DigikalaToWooCommerce.Test
{
    [TestClass]
    public class DigiKalaTest
    {
        private readonly DigiKala _digikala;

        public DigiKalaTest()
        {
            _digikala = new DigiKala(
                new Core.Model.SettingViewModel 
                {
                    Discount=10,
                    WooCommerceApiUrl= "",
                    WooCommerceApiKey= "",
                    WooCommerceApiPassword= "",
                    SellerId= 432254
                }
            );
        }


        [TestMethod]
        public async Task GetCategoryTest()
        {
            var result=await _digikala.GetAllCategory();
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task CrawlTest()
        {
            var result = await _digikala.Crawl(5373091);
            Assert.IsTrue(result != null);
        }
    }
}
