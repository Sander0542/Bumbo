﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bumbo.Data.Models;
using Bumbo.Data.Models.Enums;
using Bumbo.Data.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Bumbo.Data.Repositories
{
    public class UserRepository : RepositoryBase<User>
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<User>> GetUsersAndShifts(Branch branch, int year, int week, Department department)
        {
            var startTime = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);

            return await Context.Users
                .Include(user => user.Shifts
                    .Where(shift => shift.BranchId == branch.Id)
                    .Where(shift => shift.Department == department)
                    .Where(shift => shift.StartTime >= startTime)
                    .Where(shift => shift.StartTime < startTime.AddDays(7))
                )
                .Include(user => user.UserAvailabilities)
                .Include(user => user.UserAdditionalWorks)
                .Include(user => user.Branches
                    .Where(userBranch => userBranch.BranchId == branch.Id)
                )
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}