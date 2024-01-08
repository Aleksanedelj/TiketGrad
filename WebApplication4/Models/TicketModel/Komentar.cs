using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication4.Models.TicketModel
{
    public partial class Komentar
    {
        public Komentar()
        {
            this.tbl_file = new HashSet<tbl_file>();
        }

        [Key]
        public int IDKomentar { get; set; }

        [AllowHtml]
        public string Tekst { get; set; }
        public int IDTiket { get; set; }
        public Nullable<int> IDUser { get; set; }
        public string UserName { get; set; }
        public Nullable<System.DateTime> Datum { get; set; }

        public virtual Tiket Tiket { get; set; }
        public virtual ICollection<tbl_file> tbl_file { get; set; }
    }
}