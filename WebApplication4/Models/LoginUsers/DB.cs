﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebApplication4.Models.LoginUsers;

namespace WebApplication4.Models.LoginUsers
{
    public class DB : DbContext
    {
        //rrrrrrrrrrrrrrrrrr
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> Roles { get; set; }

        public DbSet<AdminData> AdminData { get; set; }
    }
}