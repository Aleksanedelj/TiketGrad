using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
//using WebApplication4.Models;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Models.TicketModel;

namespace WebApplication4.Interfaces
{
    internal interface ICommentRepository
    {
        void CreateComment(Komentar comment, HttpPostedFileBase[] FileAttach, User user);
    }
}
