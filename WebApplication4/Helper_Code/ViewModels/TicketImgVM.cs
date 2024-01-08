using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using WebApplication4.Models;
using WebApplication4.Models.TicketModel;

namespace WebApplication4.Helper_Code.ViewModels
{
    public class TicketImgVM
    {
        
        public Tiket Tiketi { get; set; }

        public ImgViewModel Files { get; set; }

        public Relation Relations { get; set; }
        

    }
}