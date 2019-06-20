﻿using Microsoft.EntityFrameworkCore.Storage;
using Mix.Cms.Lib.Models.Cms;
using Mix.Cms.Lib.Services;
using Mix.Common.Helper;
using Mix.Domain.Core.ViewModels;
using Mix.Domain.Data.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static Mix.Cms.Lib.MixEnums;

namespace Mix.Cms.Lib.ViewModels.MixPages
{
    public class ReadMvcViewModel : ViewModelBase<MixCmsContext, MixPage, ReadMvcViewModel>
    {
        #region Properties

        #region Models

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("fields")]
        public string Fields { get; set; }

        [JsonProperty("type")]
        public MixEnums.MixPageType Type { get; set; }

        [JsonProperty("status")]
        public MixEnums.MixContentStatus Status { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("cssClass")]
        public string CssClass { get; set; }

        [JsonProperty("layout")]
        public string Layout { get; set; }

        [JsonProperty("staticUrl")]
        public string StaticUrl { get; set; }

        [JsonProperty("excerpt")]
        public string Excerpt { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("views")]
        public int? Views { get; set; }

        [JsonProperty("seoName")]
        public string SeoName { get; set; }

        [JsonProperty("seoTitle")]
        public string SeoTitle { get; set; }

        [JsonProperty("seoDescription")]
        public string SeoDescription { get; set; }

        [JsonProperty("seoKeywords")]
        public string SeoKeywords { get; set; }

        [JsonProperty("level")]
        public int? Level { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("updatedDateTime")]
        public DateTime? UpdatedDateTime { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }
        #endregion Models

        #region Views

        [JsonProperty("details")]
        public string DetailsUrl { get; set; }

        [JsonProperty("domain")]
        public string Domain { get { return MixService.GetConfig<string>("Domain"); } }

        [JsonProperty("imageUrl")]
        public string ImageUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(Image) && (Image.IndexOf("http") == -1) && Image[0] != '/')
                {
                    return CommonHelper.GetFullPath(new string[] {
                    Domain,  Image
                });
                }
                else
                {
                    return Image;
                }
            }
        }
        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl
        {
            get
            {
                if (Thumbnail != null && Thumbnail.IndexOf("http") == -1 && Thumbnail[0] != '/')
                {
                    return CommonHelper.GetFullPath(new string[] {
                    Domain,  Thumbnail
                });
                }
                else
                {
                    return string.IsNullOrEmpty(Thumbnail) ? ImageUrl : Thumbnail;
                }
            }
        }
        [JsonProperty("view")]
        public MixTemplates.ReadListItemViewModel View { get; set; }

        [JsonProperty("articles")]
        public PaginationModel<MixPageArticles.ReadViewModel> Articles { get; set; } = new PaginationModel<MixPageArticles.ReadViewModel>();

        [JsonProperty("modules")]
        public List<MixPageModules.ReadMvcViewModel> Modules { get; set; } = new List<MixPageModules.ReadMvcViewModel>(); // Get All Module

        public string TemplatePath
        {
            get
            {
                return $"/{MixConstants.Folder.TemplatesFolder}/{MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.ThemeFolder, Specificulture)}/{Template}";                
            }
        }

        #endregion Views

        #endregion Properties

        #region Contructors

        public ReadMvcViewModel() : base()
        {
        }

        public ReadMvcViewModel(MixPage model, MixCmsContext _context = null, IDbContextTransaction _transaction = null) : base(model, _context, _transaction)
        {
        }

        #endregion Contructors

        #region Overrides

        public override void ExpandView(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            this.View = MixTemplates.ReadListItemViewModel.GetTemplateByPath(Template, Specificulture, _context, _transaction).Data;
            if (View != null)
            {
                GetSubModules(_context, _transaction);
                //switch (Type)
                //{
                //    case MixPageType.Home:
                //    case MixPageType.Blank:
                //    case MixPageType.Article:
                //    case MixPageType.Modules:
                //        break;

                //    case MixPageType.ListArticle:
                //        GetSubArticles(_context, _transaction);
                //        break;

                //    case MixPageType.ListProduct:
                //        GetSubProducts(_context, _transaction);
                //        break;

                //    default:
                //        break;
                //}
            }
        }

        #endregion Overrides

        #region Expands

        #region Sync
        public void LoadData(int? pageSize = null, int? pageIndex = null
            , MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<MixCmsContext>.InitTransaction(_context, _transaction, out MixCmsContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                pageSize = pageSize > 0 ? pageSize : PageSize;
                pageIndex = pageIndex ?? 0;
                Expression<Func<MixPageModule, bool>> dataExp = null;
                Expression<Func<MixPageArticle, bool>> articleExp = null;
                foreach (var item in Modules)
                {
                    item.Module.LoadData(pageSize: pageSize, pageIndex: pageIndex, _context: context, _transaction: transaction);
                }
                switch (Type)
                {
                    case MixPageType.ListArticle:
                        articleExp = n => n.CategoryId == Id && n.Specificulture == Specificulture;
                        break;
                    default:
                        dataExp = m => m.CategoryId == Id && m.Specificulture == Specificulture;
                        articleExp = n => n.CategoryId == Id && n.Specificulture == Specificulture;
                        break;
                }

                if (articleExp != null)
                {
                    var getArticles = MixPageArticles.ReadViewModel.Repository
                    .GetModelListBy(articleExp
                    , MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.OrderBy), 0
                    , pageSize, pageIndex
                    , _context: context, _transaction: transaction);
                    if (getArticles.IsSucceed)
                    {
                        Articles = getArticles.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                UnitOfWorkHelper<MixCmsContext>.HandleException<PaginationModel<ReadMvcViewModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        public void LoadDataByTag(string tagName
            , string orderBy, int orderDirection
            , int? pageSize = null, int? pageIndex = null            
            , MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<MixCmsContext>.InitTransaction(_context, _transaction, out MixCmsContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                pageSize = pageSize > 0 ? pageSize : PageSize;
                pageIndex = pageIndex ?? 0;
                Expression<Func<MixArticle, bool>> articleExp = null;
                JObject obj = new JObject(new JProperty("text", tagName));

                articleExp = n => n.Tags.Contains(obj.ToString(Newtonsoft.Json.Formatting.None)) && n.Specificulture == Specificulture;

                if (articleExp != null)
                {
                    var getArticles = MixArticles.ReadListItemViewModel.Repository
                    .GetModelListBy(articleExp
                    , MixService.GetConfig<string>(orderBy), 0
                    , pageSize, pageIndex
                    , _context: context, _transaction: transaction);
                    if (getArticles.IsSucceed)
                    {
                        Articles.Items = new List<MixPageArticles.ReadViewModel>();
                        Articles.PageIndex = getArticles.Data.PageIndex;
                        Articles.PageSize = getArticles.Data.PageSize;
                        Articles.TotalItems = getArticles.Data.TotalItems;
                        Articles.TotalPage = getArticles.Data.TotalPage;
                        foreach (var article in getArticles.Data.Items)
                        {
                            Articles.Items.Add(new MixPageArticles.ReadViewModel()
                            {
                                CategoryId = Id,
                                ArticleId = article.Id,
                                Article = article
                            });
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                UnitOfWorkHelper<MixCmsContext>.HandleException<PaginationModel<ReadMvcViewModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }
         public void LoadDataByKeyword(string keyword
            , string orderBy, int orderDirection
            , int? pageSize = null, int? pageIndex = null            
            , MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<MixCmsContext>.InitTransaction(_context, _transaction, out MixCmsContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                pageSize = pageSize > 0 ? pageSize : PageSize;
                pageIndex = pageIndex ?? 0;
                Expression<Func<MixArticle, bool>> articleExp = null;

                articleExp = n => n.Title.Contains(keyword) && n.Specificulture == Specificulture;

                if (articleExp != null)
                {
                    var getArticles = MixArticles.ReadListItemViewModel.Repository
                    .GetModelListBy(articleExp
                    , MixService.GetConfig<string>(orderBy), 0
                    , pageSize, pageIndex
                    , _context: context, _transaction: transaction);
                    if (getArticles.IsSucceed)
                    {
                        Articles.Items = new List<MixPageArticles.ReadViewModel>();
                        Articles.PageIndex = getArticles.Data.PageIndex;
                        Articles.PageSize = getArticles.Data.PageSize;
                        Articles.TotalItems = getArticles.Data.TotalItems;
                        Articles.TotalPage = getArticles.Data.TotalPage;
                        foreach (var article in getArticles.Data.Items)
                        {
                            Articles.Items.Add(new MixPageArticles.ReadViewModel()
                            {
                                CategoryId = Id,
                                ArticleId = article.Id,
                                Article = article
                            });
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                UnitOfWorkHelper<MixCmsContext>.HandleException<PaginationModel<ReadMvcViewModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        private void GetSubModules(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var getNavs = MixPageModules.ReadMvcViewModel.Repository.GetModelListBy(
                m => m.CategoryId == Id && m.Specificulture == Specificulture
                , _context, _transaction);
            if (getNavs.IsSucceed)
            {
                Modules = getNavs.Data;
                StringBuilder scripts = new StringBuilder();
                StringBuilder styles = new StringBuilder();
                foreach (var nav in getNavs.Data.OrderBy(n => n.Priority).ToList())
                {
                    scripts.Append(nav.Module.View?.Scripts);
                    styles.Append(nav.Module.View?.Styles);
                }
                View.Scripts += scripts.ToString();
                View.Styles += styles.ToString();
            }
        }

        private void GetSubArticles(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var getArticles = MixPageArticles.ReadViewModel.Repository.GetModelListBy(
                n => n.CategoryId == Id && n.Specificulture == Specificulture,
                MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.OrderBy), 0
                , 4, 0
               , _context: _context, _transaction: _transaction
               );
            if (getArticles.IsSucceed)
            {
                Articles = getArticles.Data;
            }
        }

        #endregion Sync

        public MixModules.ReadMvcViewModel GetModule(string name)
        {
            return Modules.FirstOrDefault(m => m.Module.Name == name)?.Module;
        }
        #endregion Expands
    }
}
