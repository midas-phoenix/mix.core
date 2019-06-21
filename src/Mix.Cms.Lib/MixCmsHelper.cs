﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mix.Cms.Lib.Services;
using Mix.Common.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Mix.Cms.Lib.MixEnums;

namespace Mix.Cms.Lib
{
    public class MixCmsHelper
    {
        public static string GetAssetFolder(string culture)
        {
            return $"/{MixConstants.Folder.FileFolder}/{MixConstants.Folder.TemplatesAssetFolder}/{MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.ThemeFolder, culture)}";
        }
        public static List<ViewModels.MixPages.ReadListItemViewModel> GetCategory(IUrlHelper Url, string culture, MixEnums.CatePosition position, string activePath = "")
        {
            var getTopCates = ViewModels.MixPages.ReadListItemViewModel.Repository.GetModelListBy
            (c => c.Specificulture == culture && c.MixPagePosition.Any(
              p => p.PositionId == (int)position)
            );
            var cates = getTopCates.Data ?? new List<ViewModels.MixPages.ReadListItemViewModel>();
            activePath = activePath.ToLower();
            foreach (var cate in cates)
            {
                switch (cate.Type)
                {
                    case MixPageType.Blank:
                        foreach (var child in cate.Childs)
                        {
                            child.Page.DetailsUrl = Url.RouteUrl("Page", new { culture, seoName = child.Page.SeoName });
                        }
                        break;

                    case MixPageType.StaticUrl:
                        cate.DetailsUrl = cate.StaticUrl;
                        break;

                    case MixPageType.Home:
                    case MixPageType.ListArticle:
                    case MixPageType.Article:
                    case MixPageType.Modules:
                    default:
                        cate.DetailsUrl = Url.RouteUrl("Page", new { culture, seoName = cate.SeoName });
                        break;
                }
                cate.IsActived = (cate.DetailsUrl == activePath
                    || (cate.Type == MixPageType.Home && activePath == string.Format("/{0}/home", culture)));
                cate.Childs.ForEach((Action<ViewModels.MixPagePages.ReadViewModel>)(c =>
                {
                    c.IsActived = (
                    c.Page.DetailsUrl == activePath);
                    cate.IsActived = cate.IsActived || c.IsActived;
                }));
            }
            return cates;
        }

        public static List<ViewModels.MixPages.ReadListItemViewModel> GetCategory(IUrlHelper Url, string culture, MixPageType cateType, string activePath = "")
        {
            var getTopCates = ViewModels.MixPages.ReadListItemViewModel.Repository.GetModelListBy
            (c => c.Specificulture == culture && c.Type == (int)cateType
            );
            var cates = getTopCates.Data ?? new List<ViewModels.MixPages.ReadListItemViewModel>();
            activePath = activePath.ToLower();
            foreach (var cate in cates)
            {
                switch (cate.Type)
                {
                    case MixPageType.Blank:
                        foreach (var child in cate.Childs)
                        {
                            child.Page.DetailsUrl = Url.RouteUrl("Page", new { culture, pageName = child.Page.SeoName });
                        }
                        break;

                    case MixPageType.StaticUrl:
                        cate.DetailsUrl = cate.StaticUrl;
                        break;

                    case MixPageType.Home:
                    case MixPageType.ListArticle:
                    case MixPageType.Article:
                    case MixPageType.Modules:
                    default:
                        cate.DetailsUrl = Url.RouteUrl("Page", new { culture, pageName = cate.SeoName });
                        break;
                }

                cate.IsActived = (
                    cate.DetailsUrl == activePath || (cate.Type == MixPageType.Home && activePath == string.Format("/{0}/home", culture))
                    );

                cate.Childs.ForEach((Action<ViewModels.MixPagePages.ReadViewModel>)(c =>
                {
                    c.IsActived = (
                    c.Page.DetailsUrl == activePath);
                    cate.IsActived = cate.IsActived || c.IsActived;
                }));
            }
            return cates;
        }

        public static string GetRouterUrl(string routerName, object routeValues, HttpRequest request, IUrlHelper Url)
        {
            return string.Format("{0}://{1}{2}", request.Scheme, request.Host,
                        Url.RouteUrl(routerName, routeValues)
                        );
        }

        public static string FormatPrice(double? price, string oldPrice = "0")
        {
            string strPrice = price?.ToString();
            if (string.IsNullOrEmpty(strPrice))
            {
                return "0";
            }
            string s1 = strPrice.Replace(",", string.Empty);
            if (CheckIsPrice(s1))
            {
                Regex rgx = new Regex("(\\d+)(\\d{3})");
                while (rgx.IsMatch(s1))
                {
                    s1 = rgx.Replace(s1, "$1" + "," + "$2");
                }
                return s1;
            }
            return oldPrice;
        }

        public static bool CheckIsPrice(string number)
        {
            if (number == null)
            {
                return false;
            }
            number = number.Replace(",", "");

            return double.TryParse(number, out double t);
        }

        public static double ReversePrice(string formatedPrice)
        {
            try
            {
                if (string.IsNullOrEmpty(formatedPrice))
                {
                    return 0;
                }
                return double.Parse(formatedPrice.Replace(",", string.Empty));
            }
            catch
            {
                return 0;
            }
        }

        public static void LogException(Exception ex)
        {
            string fullPath = string.Format($"{Environment.CurrentDirectory}/logs");
            if (!string.IsNullOrEmpty(fullPath) && !Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            string filePath = $"{fullPath}/{DateTime.Now.ToString("YYYYMMDD")}/log_exceptions.json";

            try
            {
                FileInfo file = new FileInfo(filePath);
                string content = "[]";
                if (file.Exists)
                {
                    using (StreamReader s = file.OpenText())
                    {
                        content = s.ReadToEnd();
                    }
                    File.Delete(filePath);
                }

                JArray arrExceptions = JArray.Parse(content);
                JObject jex = new JObject
                {
                    new JProperty("CreatedDateTime", DateTime.UtcNow),
                    new JProperty("Details", JObject.FromObject(ex))
                };
                arrExceptions.Add(jex);
                content = arrExceptions.ToString();

                using (var writer = File.CreateText(filePath))
                {
                    writer.WriteLine(content);
                }
            }
            catch
            {
                // File invalid
            }
        }

        public static async System.Threading.Tasks.Task<ViewModels.MixModules.ReadMvcViewModel> GetModuleAsync (string name, string culture)
        {
            string cachekey = $"module_{culture}_{name}";
            var module = await MixCacheService.GetAsync<Mix.Domain.Core.ViewModels.RepositoryResponse<ViewModels.MixModules.ReadMvcViewModel>>(cachekey);
            if (module==null)
            {
                module = ViewModels.MixModules.ReadMvcViewModel.GetBy(m => m.Name == name && m.Specificulture == culture);
                if (module.IsSucceed)
                {
                    await MixCacheService.SetAsync(cachekey, module);
                }
            }
            return module.Data;
        }

        public static ViewModels.MixModules.ReadMvcViewModel GetModule (string name, string culture)
        {
            var module = ViewModels.MixModules.ReadMvcViewModel.GetBy(m => m.Name == name && m.Specificulture == culture);
            return module.Data;
        }

        public static ViewModels.MixPages.ReadMvcViewModel GetPage(int id, string culture)
        {
            var page = ViewModels.MixPages.ReadMvcViewModel.Repository.GetSingleModel(m => m.Id == id&& m.Specificulture == culture);
            return page.Data;
        }
    }
}
