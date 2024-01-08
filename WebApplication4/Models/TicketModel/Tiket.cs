using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication4.Models.TicketModel
{
    public partial class Tiket
    {
        public Tiket()
        {
            this.Komentars = new HashSet<Komentar>();
            this.tbl_file = new HashSet<tbl_file>();
            this.Relations = new HashSet<Relation>();
        }

        [Key]
        public int IDTiket { get; set; }
        public string Naslov { get; set; }
        [AllowHtml]
        public string Opis { get; set; }

        public System.DateTime Datum { get; set; }
        public int IDStatus { get; set; }
        public int IDKat { get; set; }
        public Nullable<int> IDUser { get; set; }
        public string UserName { get; set; }
        public Nullable<System.DateTime> DateUpdated { get; set; }

        public Nullable<int> IDUserAssignedTo { get; set; }
        public string UserNameAssignedTo { get; set; }

        public virtual Kategorija Kategorija { get; set; }
        public virtual ICollection<Komentar> Komentars { get; set; }
        public virtual Status Status { get; set; }
        public virtual ICollection<tbl_file> tbl_file { get; set; }
        public virtual ICollection<Relation> Relations { get; set; }
    }
}