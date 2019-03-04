﻿using Microsoft.EntityFrameworkCore.Storage;
using Mix.Cms.Lib.Models.Cms;
using Mix.Cms.Lib.Services;
using Mix.Domain.Core.ViewModels;
using Mix.Domain.Data.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Mix.Cms.Lib.MixEnums;

namespace Mix.Cms.Lib.ViewModels.MixCultures
{
    public class UpdateViewModel
      : ViewModelBase<MixCmsContext, MixCulture, UpdateViewModel>
    {
        #region Properties

        #region Models

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("lcid")]
        public string Lcid { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("status")]
        public MixContentStatus Status { get; set; }
        #endregion Models

        #region Views

        [JsonProperty("configurations")]
        public List<MixConfigurations.ReadMvcViewModel> Configurations { get; set; }

        #endregion
        #endregion Properties

        #region Contructors

        public UpdateViewModel() : base()
        {
        }

        public UpdateViewModel(MixCulture model, MixCmsContext _context = null, IDbContextTransaction _transaction = null) : base(model, _context, _transaction)
        {
        }

        #endregion Contructors

        #region Overrides
        public override MixCulture ParseModel(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (Id == 0)
            {
                Id = Repository.Max(m => m.Id).Data + 1;
                CreatedDateTime = DateTime.UtcNow;
            }
            return base.ParseModel(_context, _transaction);
        }
        public override void ExpandView(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var getConfigurations = MixConfigurations.ReadMvcViewModel.Repository.GetModelListBy(c => c.Specificulture == Specificulture, _context, _transaction);
            if (getConfigurations.IsSucceed)
            {
                Configurations = getConfigurations.Data;
            }
        }
        #region Async
        public override async Task<RepositoryResponse<UpdateViewModel>> SaveModelAsync(bool isSaveSubModels = false, MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var result = await base.SaveModelAsync(isSaveSubModels, _context, _transaction);
            if (result.IsSucceed)
            {
                MixService.LoadFromDatabase();
                MixService.Save();
            }
            return result;
        }

        public override async Task<RepositoryResponse<bool>> SaveSubModelsAsync(MixCulture parent, MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var result = new RepositoryResponse<bool>() { IsSucceed = true };
            var getPages = await MixPages.ReadViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
            if (getPages.IsSucceed)
            {
                foreach (var p in getPages.Data)
                {
                    p.Specificulture = Specificulture;
                    p.CreatedDateTime = DateTime.UtcNow;
                    p.LastModified = DateTime.UtcNow;
                    var saveResult = await p.SaveModelAsync(false, _context, _transaction);
                    result.IsSucceed = saveResult.IsSucceed;
                    if (!saveResult.IsSucceed)
                    {
                        result.Errors.AddRange(saveResult.Errors);
                        result.Exception = saveResult.Exception;
                        break;
                    }
                }
            }
            if (result.IsSucceed)
            {
                var getConfigurations = await MixConfigurations.ReadMvcViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
                if (getConfigurations.IsSucceed)
                {
                    foreach (var c in getConfigurations.Data)
                    {
                        c.Specificulture = Specificulture;
                        c.CreatedDateTime = DateTime.UtcNow;
                        var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                        result.IsSucceed = saveResult.IsSucceed;
                        if (!saveResult.IsSucceed)
                        {
                            result.Errors.AddRange(saveResult.Errors);
                            result.Exception = saveResult.Exception;
                            break;
                        }
                    }
                }

            }
            if (result.IsSucceed)
            {
                var getLanguages = await MixLanguages.ReadMvcViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
                if (getLanguages.IsSucceed)
                {
                    foreach (var c in getLanguages.Data)
                    {
                        c.Specificulture = Specificulture;
                        c.CreatedDateTime = DateTime.UtcNow;
                        var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                        result.IsSucceed = saveResult.IsSucceed;
                        if (!saveResult.IsSucceed)
                        {
                            result.Errors.AddRange(saveResult.Errors);
                            result.Exception = saveResult.Exception;
                            break;
                        }
                    }
                }
            }
            // Clone Module from Default culture
            var getModules = await MixModules.ReadListItemViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
            if (getModules.IsSucceed)
            {
                foreach (var c in getModules.Data)
                {
                    c.Specificulture = Specificulture;
                    c.CreatedDateTime = DateTime.UtcNow;
                    c.LastModified = DateTime.UtcNow;
                    var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                    result.IsSucceed = saveResult.IsSucceed;
                    if (!saveResult.IsSucceed)
                    {
                        result.Errors.AddRange(saveResult.Errors);
                        result.Exception = saveResult.Exception;
                        break;
                    }
                }
            }
            // Clone ModuleData from Default culture
            var getModuleDatas = await MixModuleDatas.ReadViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
            if (getModuleDatas.IsSucceed)
            {
                foreach (var c in getModuleDatas.Data)
                {
                    c.Specificulture = Specificulture;
                    c.CreatedDateTime = DateTime.UtcNow;
                    var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                    result.IsSucceed = saveResult.IsSucceed;
                    if (!saveResult.IsSucceed)
                    {
                        result.Errors.AddRange(saveResult.Errors);
                        result.Exception = saveResult.Exception;
                        break;
                    }
                }
            }
            
            // Clone Article from Default culture
            var getArticles = await MixArticles.ReadListItemViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
            if (getArticles.IsSucceed)
            {
                foreach (var c in getArticles.Data)
                {
                    c.Specificulture = Specificulture;
                    c.CreatedDateTime = DateTime.UtcNow;
                    c.LastModified = DateTime.UtcNow;
                    var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                    result.IsSucceed = saveResult.IsSucceed;
                    if (!saveResult.IsSucceed)
                    {
                        result.Errors.AddRange(saveResult.Errors);
                        result.Exception = saveResult.Exception;
                        break;
                    }
                }
            }
            // Clone PageArticle from Default culture
            var getPageArticles = await MixPageArticles.ReadViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
            if (getPageArticles.IsSucceed)
            {
                foreach (var c in getPageArticles.Data)
                {
                    c.Specificulture = Specificulture;
                    var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                    result.IsSucceed = saveResult.IsSucceed;
                    if (!saveResult.IsSucceed)
                    {
                        result.Errors.AddRange(saveResult.Errors);
                        result.Exception = saveResult.Exception;
                        break;
                    }
                }
            }
            // Clone ModuleArticle from Default culture
            var getModuleArticles = await MixModuleArticles.ReadViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
            if (getModuleArticles.IsSucceed)
            {
                foreach (var c in getModuleArticles.Data)
                {
                    c.Specificulture = Specificulture;
                    var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                    result.IsSucceed = saveResult.IsSucceed;
                    if (!saveResult.IsSucceed)
                    {
                        result.Errors.AddRange(saveResult.Errors);
                        result.Exception = saveResult.Exception;
                        break;
                    }
                }
            }
            
            // Clone ArticleArticle from Default culture
            var getArticleArticles = await MixArticleArticles.ReadViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
            if (getArticleArticles.IsSucceed)
            {
                foreach (var c in getArticleArticles.Data)
                {
                    c.Specificulture = Specificulture;
                    var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                    result.IsSucceed = saveResult.IsSucceed;
                    if (!saveResult.IsSucceed)
                    {
                        result.Errors.AddRange(saveResult.Errors);
                        result.Exception = saveResult.Exception;
                        break;
                    }
                }
            }
            
            // Clone ArticleMedia from Default culture
            var getArticleMedias = await MixArticleMedias.ReadViewModel.Repository.GetModelListByAsync(c => c.Specificulture == MixService.GetConfig<string>(MixConstants.ConfigurationKeyword.DefaultCulture), _context, _transaction);
            if (getArticleMedias.IsSucceed)
            {
                foreach (var c in getArticleMedias.Data)
                {
                    c.Specificulture = Specificulture;
                    var saveResult = await c.SaveModelAsync(false, _context, _transaction);
                    result.IsSucceed = saveResult.IsSucceed;
                    if (!saveResult.IsSucceed)
                    {
                        result.Errors.AddRange(saveResult.Errors);
                        result.Exception = saveResult.Exception;
                        break;
                    }
                }
            }
            _context.SaveChanges();
            return result;
        }

        public override async Task<RepositoryResponse<bool>> RemoveRelatedModelsAsync(UpdateViewModel view, MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var result = new RepositoryResponse<bool>() { IsSucceed = true };

            var configs = _context.MixConfiguration.Where(c => c.Specificulture == Specificulture).ToList();
            configs.ForEach(c => _context.Entry(c).State = Microsoft.EntityFrameworkCore.EntityState.Deleted);

            var languages = _context.MixLanguage.Where(l => l.Specificulture == Specificulture).ToList();
            languages.ForEach(l => _context.Entry(l).State = Microsoft.EntityFrameworkCore.EntityState.Deleted);

            var cates = _context.MixPage.Where(c => c.Specificulture == Specificulture).ToList();
            cates.ForEach(c => _context.Entry(c).State = Microsoft.EntityFrameworkCore.EntityState.Deleted);

            var modules = _context.MixModule.Where(c => c.Specificulture == Specificulture).ToList();
            modules.ForEach(c => _context.Entry(c).State = Microsoft.EntityFrameworkCore.EntityState.Deleted);

            var articles = _context.MixArticle.Where(c => c.Specificulture == Specificulture).ToList();
            articles.ForEach(c => _context.Entry(c).State = Microsoft.EntityFrameworkCore.EntityState.Deleted);

            var products = _context.MixProduct.Where(c => c.Specificulture == Specificulture).ToList();
            products.ForEach(c => _context.Entry(c).State = Microsoft.EntityFrameworkCore.EntityState.Deleted);

            await _context.SaveChangesAsync();

            return result;
        }

        public override async Task<RepositoryResponse<MixCulture>> RemoveModelAsync(bool isRemoveRelatedModels = false, MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            var result = await base.RemoveModelAsync(isRemoveRelatedModels, _context, _transaction);
            if (result.IsSucceed)
            {
                if (result.IsSucceed)
                {
                    MixService.LoadFromDatabase();
                    MixService.Save();
                }
            }
            return result;
        }

        #endregion

        #endregion Overrides
    }
}
