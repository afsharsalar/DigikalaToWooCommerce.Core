using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigikalaToWooCommerce.Core.Model;
using HtmlAgilityPack;
using Newtonsoft.Json;

using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace DigikalaToWooCommerce.Core.Service
{
    public class DigiKala
    {

        private readonly RestAPI _restApi;
        private SettingViewModel _setting { get; }
        private List<ProductCategory> _categories;
        public DigiKala(SettingViewModel setting)
        {
            _setting = setting;
            _restApi = new RestAPI(setting.WooCommerceApiUrl, setting.WooCommerceApiKey, setting.WooCommerceApiPassword, false);
          
        }


        public async Task<ulong?> AddToWoocommerce(int dkp)
        {
            _categories =await GetAllCategory();
            var data =await Crawl(dkp);
            var wc = new WCObject(_restApi);
            var product = await wc.Product.Add(data);
            return product.id;
        }

        public async Task<Product> Crawl(int dkp)
        {
            using (WebClient client = new WebClient { Encoding = Encoding.UTF8 })
            {


                var url = $"https://www.digikala.com/product/dkp-{dkp}";
                var content = client.DownloadString(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);



                var variantsNodeFromScript = doc.DocumentNode.SelectNodes("//script")[9];
                if (!variantsNodeFromScript.InnerText.Contains("variants"))
                    throw new ArgumentException("script not found");
                var checkstartIndex = variantsNodeFromScript.InnerText.IndexOf("variants", variantsNodeFromScript.InnerText.IndexOf("variants") + 1);
                if (checkstartIndex < 0)
                {
                    throw new ArgumentException("crawl failed");
                }
                if (!variantsNodeFromScript.InnerText.Contains("var variants"))
                {
                    throw new ArgumentException("crawl failed");
                }




                var startIndex = variantsNodeFromScript.InnerText.IndexOf("{");
                var lastIndex = variantsNodeFromScript.InnerText.IndexOf("}};") + 2;
                var variantsScript = variantsNodeFromScript.InnerText.Substring(startIndex, lastIndex - startIndex);

                string encoded = DecodeEncodedNonAsciiCharacters(variantsScript);


                var variantNode = doc.DocumentNode.SelectNodes("//div[contains(@class,'c-table-suppliers__row js-supplier')]");
                var variants = new List<int>();
                if (variantNode == null)
                    throw new Exception("Not found");
                foreach (var item in variantNode)
                {
                    var id = Convert.ToInt32(item.Attributes["data-variant"].Value);
                    variants.Add(id);
                    encoded = encoded.Replace($"\"{id}\":", "");
                }
                encoded = "[" + encoded.Substring(1, encoded.Length - 2) + "]";

                encoded = encoded.Replace("\"color\":[],", "").Replace("\"size\":[],", "");
                var data = JsonConvert.DeserializeObject<List<VariantModel>>(encoded);


                var picUrls = new List<DigiKalaGalleryModel>();

                var title = doc.DocumentNode.SelectSingleNode("//h1[@class='c-product__title']");
                var description = doc.DocumentNode.SelectSingleNode("//div[@class='c-mask__text c-mask__text--product-summary js-mask__text']");

                var titleVal = title.InnerText.Trim();
                var pics = doc.DocumentNode.SelectNodes("//li[@class='js-product-thumb-img']");


                if (pics != null)
                {
                    for (var index = 0; index < pics.Count; index++)
                    {
                        var item = pics[index];
                        HtmlDocument imgDoc = new HtmlDocument();
                        imgDoc.LoadHtml(item.InnerHtml);

                        var img = imgDoc.DocumentNode.SelectSingleNode("//img");
                        var file = img.Attributes["data-src"].Value.Split('?')[0];

                        picUrls.Add(new DigiKalaGalleryModel { Position = index + 1, Url = file });
                    }
                }

                var singleImg = doc.DocumentNode.SelectSingleNode("//img[@class='js-gallery-img']");
                if (singleImg != null)
                {
                    var file = singleImg.Attributes["data-src"].Value.Split('?')[0];
                    picUrls.Add(new DigiKalaGalleryModel { Position = 0, Url = file });


                }

                var attributes = new List<ProductAttributeLine>();
                var attrSection = doc.DocumentNode.SelectSingleNode("//article[@class='c-params__border-bottom']");
                if (attrSection != null)
                {
                    HtmlDocument attrDoc = new HtmlDocument();
                    attrDoc.LoadHtml(attrSection.InnerHtml);
                    var attrs = attrDoc.DocumentNode.SelectNodes("//li");
                    if (attrs != null)
                    {
                        foreach (var item in attrs)
                        {
                            HtmlDocument liDoc = new HtmlDocument();
                            liDoc.LoadHtml(item.InnerHtml);

                            var name = liDoc.DocumentNode.SelectNodes("//span");
                            if (name != null && name.Count == 2)
                            {
                                var linkName = name[0].InnerText;
                                attributes.Add(new ProductAttributeLine
                                {
                                    visible = true,
                                    name = linkName,
                                    options = name[1].InnerText.Split(',').Select(p => p.Replace("/n", "").Trim()).ToList(),

                                });
                            }
                            else if (name != null && name.Count == 1)
                            {
                                var others = name[0].InnerText.Split(',').Select(p => p.Replace("/n", "").Trim()).ToList();
                                attributes.Last().options.AddRange(others);
                            }
                        }
                    }

                }


                var categories = new List<ProductCategoryLine>();

                var breadCrumbs = doc.DocumentNode.SelectSingleNode("//ul[@class='c-breadcrumb']");

                if (breadCrumbs != null)
                {
                    HtmlDocument categroyDoc = new HtmlDocument();
                    categroyDoc.LoadHtml(breadCrumbs.InnerHtml);
                    var cats = categroyDoc.DocumentNode.SelectNodes("//li");
                    Regex trimmer = new Regex(@"\s\s+");

                    for (var index = 1; index < cats.Count - 1; index++)
                    {
                        var item = cats[index];


                        categories.Add(new ProductCategoryLine
                        {
                            name = trimmer.Replace(item.InnerText.Trim(), " "), slug = item.InnerText.Trim()
                        });
                    }
                }

                var brand = doc.DocumentNode.SelectNodes("//a[@class='c-product__title-container--brand-link']");
                var brandVal = "متفرقه";
                var subCatVal = "";
                if (brand != null)
                {
                    brandVal = brand[0].InnerText;
                    subCatVal = brand[1].InnerText;
                }


                var variable = false;

                if (data.Any(p => p.marketplace_seller.id == _setting.SellerId))
                {

                    var colors = data.Where(p =>p.marketplace_seller.id== _setting.SellerId &&    p.color != null).GroupBy(p => p.color.title).ToList();
                    if (colors.Any())
                    {
                        attributes.Add(new ProductAttributeLine
                        {
                            id = 1,
                            name = "رنگ",
                            options = colors.Select(p => p.Key).ToList(),
                            visible = false,
                            variation = true
                        });
                        variable = true;
                    }
                    var sizes = data.Where(p => p.marketplace_seller.id == _setting.SellerId && p.size !=null).GroupBy(p => p.size.title).ToList();
                    if (sizes.Any())
                    {
                        attributes.Add(new ProductAttributeLine
                        {
                            id = 2,
                            name = "سایز",
                            options = sizes.Select(p => p.Key).ToList(),
                            variation = true,
                            visible = false
                        });
                        variable = true;
                    }

                    var warranties = data.Where(p => p.marketplace_seller.id == _setting.SellerId && p.warranty != null).GroupBy(p => p.warranty.title).ToList();
                    if (warranties.Any())
                    {
                        attributes.Add(new ProductAttributeLine
                        {
                            id = 3,
                            name = "گارانتی",
                            options = warranties.Select(p => p.Key).ToList(),
                            variation = true,
                            visible = false
                        });
                        variable = true;
                    }

                }



                var model = new Product
                {
                    attributes = attributes,                    
                    images = picUrls.Select(p => new ProductImage
                    {
                        src = p.Url,
                        name = titleVal,
                        alt = titleVal,

                    }).ToList(),
                    name = titleVal,
                    description = description != null ? description.InnerText : "",
                    sku = dkp.ToString(),
                    type = variable ? "variable" : "simple",
                };

                if (!variable)
                {
                    var seller = data.FirstOrDefault(p => p.marketplace_seller.id == _setting.SellerId);
                    if (seller != null)
                    {
                        var salePrice = seller.price_list.selling_price;
                        if (_setting.Discount > 0)
                        {
                            var discount = (double)salePrice * ((double)_setting.Discount / 100);
                            salePrice = salePrice - (int)discount;
                            salePrice = salePrice - (salePrice % 1000);
                        }
                        model.price = salePrice / 10;
                        model.regular_price = salePrice / 10;
                        model.sale_price = salePrice / 10;
                    }
                    else
                    {
                        model.stock_quantity = 0;
                        model.stock_status = "outofstock";
                    }

                }
                else
                {
                    var variations = data.Where(p => p.marketplace_seller.id == _setting.SellerId);
                    foreach (var item in variations)
                    {
                        
                    }
                }

                ulong? parentId = null;
                foreach (var item in categories)
                {

                    var count = _categories.Count(p => p.name == item.name);
                    if (count <= 1)
                    {
                        var check = _categories.SingleOrDefault(p => p.name == item.name);
                        if (check == null)
                        {
                            parentId = await AddToCategory(new ProductCategory
                            {
                                name = item.name,
                                parent = parentId,
                                slug = item.name
                            });
                        }
                        else
                        {
                            parentId = check.id;
                        }
                    }

                }
               


                return model;


            }
        }

        #region GetAllCateCategory
        public async Task<List<ProductCategory>> GetAllCategory()
        {

            var wc = new WooCommerceNET.WooCommerce.v3.WCObject(_restApi);


            Dictionary<string, string> dic = new Dictionary<string, string> { { "per_page", "100" } };
            int pageNumber = 1;
            dic.Add("page", pageNumber.ToString());
            var list = new List<ProductCategory>();
            bool endWhile = false;

            while (!endWhile)
            {
                var temp = await wc.Category.GetAll(dic);

                if (temp.Count > 0)
                {
                    list.AddRange(temp);
                    pageNumber++;
                    dic["page"] = pageNumber.ToString();
                }
                else
                {
                    endWhile = true;
                }
            }
            return list;
        }
        #endregion

        #region AddToCategory

        private async Task<ulong?> AddToCategory(ProductCategory model)
        {            
            var wc = new WooCommerceNET.WooCommerce.v3.WCObject(_restApi);
            var data = await wc.Category.Add(model);
            return data.id;
        }

        #endregion

        #region DecodeEncodedNonAsciiCharacters

        private string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        #endregion

    }
}
