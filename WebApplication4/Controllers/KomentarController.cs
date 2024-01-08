using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Interfaces;
//using WebApplication4.Models;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Models.TicketModel;
using WebApplication4.Repository;

namespace WebApplication4.Controllers
{
    public class KomentarController : Controller
    {
        //private probaZaTiketeEntities db = new probaZaTiketeEntities();

        TicketModel db = new TicketModel();

        DB dbUsers = new DB();

        private ICommentRepository _commentRepositor;
        private ILoginRepository _loginRepositor;

        public KomentarController()
        {
            //this._commentRepositor = new CommentRepository(new probaZaTiketeEntities());
            this._commentRepositor = new CommentRepository(new TicketModel());
            this._loginRepositor = new LoginRepository(new DB());
        }

        // POST: Komentar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDKomentar,Tekst,IDTiket,IDUser,UserName,Datum")] Komentar komentar, HttpPostedFileBase[] FileAttach)
        {
            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = _loginRepositor.GetUserByEmail(imePrivremeno);

            if(komentar.Tekst == null)
            {
                TempData["MessageErrorTicket"] = "Obavezan unos teksta komentara.";
                return RedirectToAction("Details/" + komentar.IDTiket, "Tikets");
            }

            if (ModelState.IsValid)
            {
                _commentRepositor.CreateComment(komentar, FileAttach, user);
                TempData["MessageSuccessTicket"] = user.Name + " je uspešno dodao komentar.";
                return RedirectToAction("Details" + "/" + komentar.IDTiket, "Tikets");
            }

            ViewBag.IDTiket = new SelectList(db.Tikets, "IDTiket", "Naslov", komentar.IDTiket);
            return View(komentar);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
