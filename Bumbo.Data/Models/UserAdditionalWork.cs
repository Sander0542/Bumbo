using System;
using System.ComponentModel.DataAnnotations;
using Bumbo.Data.Models.Common;

namespace Bumbo.Data.Models
{
    public class UserAdditionalWork : IEntity
    {
        public int UserId { get; set; }

        public DayOfWeek Day { get; set; }

        [Range(0, 24)]
        public double Hours { get; set; }


        public User User { get; set; }
    }
}