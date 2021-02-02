﻿using Microsoft.EntityFrameworkCore.Storage;
using Mix.Cms.Lib.Enums;
using Mix.Cms.Lib.Models.Cms;
using Mix.Cms.Lib.Services;
using Mix.Domain.Core.ViewModels;
using Mix.Domain.Data.ViewModels;
using Mix.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mix.Cms.Lib.ViewModels.MixRelatedAttributeDatas
{
    public class DeleteViewModel
        : ViewModelBase<MixCmsContext, MixRelatedAttributeData, DeleteViewModel>
    {
        #region Properties

        #region Models

        public string Id { get; set; }
        public string Specificulture { get; set; }
        public string DataId { get; set; }
        public string ParentId { get; set; }
        public MixDatabaseParentType ParentType { get; set; }
        public int AttributeSetId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        #endregion Models

        #endregion Properties

        #region Contructors

        public DeleteViewModel() : base()
        {
        }

        public DeleteViewModel(MixRelatedAttributeData model, MixCmsContext _context = null, IDbContextTransaction _transaction = null) : base(model, _context, _transaction)
        {
        }

        #endregion Contructors

        #region Overrides

        public override MixRelatedAttributeData ParseModel(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (CreatedDateTime == default(DateTime))
            {
                CreatedDateTime = DateTime.UtcNow;
            }
            return base.ParseModel(_context, _transaction);
        }
        #endregion Overrides
    }
}