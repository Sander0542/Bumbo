﻿using Bumbo.Data.Models;
using Bumbo.Data.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Bumbo.Data.Repositories
{
    public class BranchForecastStandardRepository : RepositoryBase<BranchForecastStandard>
    {
        public BranchForecastStandardRepository(ApplicationDbContext context) : base(context)
        {
        }

        protected override IQueryable<BranchForecastStandard> GetQueryBase()
        {
            return base.GetQueryBase().Include(f => f.Branch);
        }
    }
}
