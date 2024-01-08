using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace WebApplication4.Models.TicketModel
{
    public class TicketModel : DbContext
    {
        public TicketModel()
            : base("name=tiketiModel")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public virtual DbSet<Kategorija> Kategorijas { get; set; }
        public virtual DbSet<Komentar> Komentars { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<tbl_file> tbl_file { get; set; }
        public virtual DbSet<Tiket> Tikets { get; set; }
        public virtual DbSet<Relation> Relations { get; set; }

        public System.Data.Entity.DbSet<WebApplication4.Models.LoginUsers.User> Users { get; set; }

        public System.Data.Entity.DbSet<WebApplication4.Models.LoginUsers.UserRole> UserRoles { get; set; }



    }
}