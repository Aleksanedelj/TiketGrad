using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication4.Models.TicketModel
{
    public partial class Kategorija
    {
        
        public Kategorija()
        {
            this.Tikets = new HashSet<Tiket>();
        }
        [Key]
        public int IDKat { get; set; }
        public string Naziv { get; set; }
        public virtual ICollection<Tiket> Tikets { get; set; }
    }
}