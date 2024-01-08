using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication4.Models.TicketModel
{
    public class Relation
    {
        [Key]
        public int RelationID { get; set; }
        public int RelatedId { get; set; }
        public int IDTiket { get; set; }

        public virtual Tiket Tiket { get; set; }
    }
}