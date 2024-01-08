using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication4.Models.TicketModel
{
    public partial class tbl_file
    {
        [Key]
        public int file_id { get; set; }
        public string file_name { get; set; }
        public string file_ext { get; set; }
        public int IDTiket { get; set; }
        public string file_base6 { get; set; }
        public Nullable<int> IDUser { get; set; }
        public string UserName { get; set; }
        public Nullable<int> IDKomentar { get; set; }
        public Nullable<System.DateTime> Datum { get; set; }

        public virtual Tiket Tiket { get; set; }
        public virtual Komentar Komentar { get; set; }
    }
}