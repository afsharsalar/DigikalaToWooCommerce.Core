using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigikalaToWooCommerce.Core.Model;
using HtmlAgilityPack;
using WooCommerceNET.WooCommerce.v3;

namespace DigikalaToWooCommerce.Core.Service
{
    public class DigiKala
    {

        public void Crawl()
        {
            using (WebClient client = new WebClient { Encoding = Encoding.UTF8 })
            {


                var url = $"https://www.digikala.com/product/dkp-{dkp}";
                var content = client.DownloadString(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);

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
                            name = trimmer.Replace(item.InnerText.Trim()., " "), slug = item.InnerText.Trim()
                        });
                    }
                }


            }
        }

    }
}
