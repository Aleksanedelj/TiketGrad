using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Interfaces;
//using WebApplication4.Models;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Repository;
using WebApplication4.Models.TicketModel;

namespace WebApplication4.Controllers
{
    public class HomeController : Controller
    {
        DB dbUsers = new DB();

        private ITicketRepository _ticketRepository;

        private ILoginRepository _loginRepositor;

        public HomeController()
        {
            //this._ticketRepository = new TicketRepository(new probaZaTiketeEntities());
            this._ticketRepository = new TicketRepository(new TicketModel());
            this._loginRepositor = new LoginRepository(new DB());
        }

        // GET: Home
        [Authorize(Roles = "user,admin,headadmin")]
        public ActionResult Index()
        {
            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = _loginRepositor.GetUserByEmail(imePrivremeno);

            IEnumerable<Tiket> tikets = new List<Tiket>();

            //dodati za datum i to sve smestiti u repository
            if(user != null)
            {
                tikets = _ticketRepository.GetAllTickets(user.RoleId, user.Id);

                return View(tikets);
            }
            else
            { 
                return View();
            }
        }
    }
}