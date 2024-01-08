using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Helper_Code.Objects;
using WebApplication4.Helper_Code.ViewModels;
using WebApplication4.Interfaces;
using WebApplication4.Repository;
//using WebApplication4.Models;
using WebApplication4.Models.LoginUsers;
using FastReport;
using PagedList;
using System.Configuration;
using System.Data.SqlClient;
using WebApplication4.Models.TicketModel;
using System.Data.Entity.Core.Objects;
using System.Globalization;

namespace WebApplication4.Controllers
{
    public class TiketsController : Controller
    {
        //private probaZaTiketeEntities db = new probaZaTiketeEntities();


        DB dbUsers = new DB();

        TicketModel db = new TicketModel();

        private ITicketRepository _ticketRepository;
        private ILoginRepository _loginRepositor;

        public TiketsController()
        {
            //this._ticketRepository = new TicketRepository(new probaZaTiketeEntities());
            this._ticketRepository = new TicketRepository(new TicketModel());
            this._loginRepositor = new LoginRepository(new DB());
        }

        // GET: Tikets
        [Authorize(Roles = "user,admin,headadmin")]
        public ActionResult Index(string sortOrder, int? page, int? IDUserCreate, int? IDUserAssignedTo)
        {
            ViewBag.CurrentSort = sortOrder;

            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = _loginRepositor.GetUserByEmail(imePrivremeno);

            IEnumerable<Tiket> tikets = _ticketRepository.GetAllTickets(user.RoleId, user.Id);

            //sortiranje po ID-ju tiketa
            ViewBag.IDTikSort = sortOrder == "IDTik" ? "idTik_desc" : "IDTik";

            //sortiranje datuma
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.DateUpdatedSortParm = sortOrder == "DateUpdated" ? "dateUpdated_desc" : "DateUpdated";

            //sortiranje statusa
            ViewBag.StatusSort = sortOrder == "StatusSorted" ? "StatusSorted_desc" : "StatusSorted";

            //sortiranje kategorije
            ViewBag.CategorySort = sortOrder == "CategorySorted" ? "CategorySorted_desc" : "CategorySorted";

            var tiketsSorted = from s in tikets
                               select s;

            var ticketsSortedAfter = _ticketRepository.SortingTickets(sortOrder, tiketsSorted, tikets);

            List<User> users = new List<User>();

            if (user.RoleId == 4 || user.RoleId == 7)
            {
                users = dbUsers.Users.ToList();
                users.Remove(user);
                user.Email = "Moji Tiketi";
                users.Add(user);
            }
            else
            {
                users = dbUsers.Users.Where(u => u.RoleId == 4 || u.RoleId == 7).ToList();
                user.Email = "Moji Tiketi";
                users.Add(user);
            }



            ViewBag.IDUserCreate = new SelectList(users, "ID", "Email");
            ViewBag.IDUserAssignedTo = new SelectList(users, "ID", "Email");

            //proba za pretragu po korisniku

            if (IDUserCreate != null || IDUserAssignedTo != null)
            {
                var ticketsAfterSearch = _ticketRepository.GetAllTicketsByUsersParameters(IDUserCreate, IDUserAssignedTo, user.RoleId, user.Id);

                ticketsSortedAfter = ticketsAfterSearch;
            }

            //proba za pretragu po korisniku

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(ticketsSortedAfter.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "user,admin,headadmin")]
        public ActionResult GetTicketListByStatus(int? IDStatus, int? IDKat, DateTime? dateNameFrom, DateTime? dateNameTo, int? selectedState, string textNote, int? ticketNumber, int? page)
        {
            ViewBag.IDStatus = new SelectList(db.Status, "IDStatus", "Naziv");
            ViewBag.IDKat = new SelectList(db.Kategorijas, "IDKat", "Naziv");

            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = dbUsers.Users.Where(a => a.Email == imePrivremeno).FirstOrDefault();
            
            int pageSize = 5;
            int pageNumber = (page ?? 1);

            IEnumerable<Tiket> paging = new List<Tiket>();

            if (page != null)
            {
                paging = (IEnumerable<Tiket>)TempData["Paging"];
                TempData["Paging"] = paging;
                return View(paging.ToPagedList(pageNumber, pageSize));
            }

            IEnumerable<Tiket> tiketsAfterFilters = new List<Tiket>();

            tiketsAfterFilters = _ticketRepository.GetAllTicketsByParameters(IDStatus, IDKat, dateNameFrom, dateNameTo, selectedState, textNote, ticketNumber, user.RoleId, user.Id)
                .OrderBy(t => t.Datum); //reseni //
            
            TempData["Things"] = tiketsAfterFilters; 

            TempData["Paging"] = tiketsAfterFilters;

            return View(tiketsAfterFilters.ToPagedList(pageNumber, pageSize));

            //return View(tiketsAfterFilters);
        }

        [HttpPost]
        [Authorize(Roles = "user,admin,headadmin")]
        public ActionResult GetTicketListByUsers(int? IDStatus, int? IDKat)
        {
            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = dbUsers.Users.Where(a => a.Email == imePrivremeno).FirstOrDefault();

            List<User> users = new List<User>();

            users = dbUsers.Users.Where(u => u.RoleId == 4).ToList();
            users.Add(user);

            ViewBag.IDUserAssignedTo = new SelectList(users, "ID", "Email");

            IEnumerable<Tiket> tiketsAfterFilters = new List<Tiket>();

            return View(tiketsAfterFilters);
        }

        public FileResult Generate(int? IDStatus, int? IDKat, DateTime? dateName, int? selectedState, string textNote, int? ticketNumber)
        {
            FastReport.Utils.Config.WebMode = true;
            Report rep = new Report();
            string path = Server.MapPath("~/Reports/TicketsReport.frx");
            rep.Load(path);

            IEnumerable<Tiket> things = (IEnumerable<Tiket>)TempData["Things"];

            if (things == null)
            {
                things = (IEnumerable<Tiket>)TempData["proba"];
            }

            var numberOfTickets = things.Count();

            var categoryOne = things.Where(t => t.IDKat == 1).Count();
            var categoryTwo = things.Where(t => t.IDKat == 2).Count();
            var categoryThree = things.Where(t => t.IDKat == 3).Count();
            var categoryFour = things.Where(t => t.IDKat == 4).Count();
            var categoryFive = things.Where(t => t.IDKat == 5).Count();

            var statusOne = things.Where(t => t.IDStatus == 1).Count();
            var statusTwo = things.Where(t => t.IDStatus == 2).Count();
            var statusThree = things.Where(t => t.IDStatus == 3).Count();

            IEnumerable<Kategorija> categoryStuff = things.Select(t => t.Kategorija).ToList();

            IEnumerable<Status> statusStuff = things.Select(t => t.Status).ToList();

            rep.RegisterData(things, "TicketRef");
            rep.RegisterData(categoryStuff, "TicketDetailsCategoryRef");
            rep.RegisterData(statusStuff, "TicketDetailsStatusRef");

            rep.SetParameterValue("Category1", categoryOne);
            rep.SetParameterValue("Category2", categoryTwo);
            rep.SetParameterValue("Category3", categoryThree);
            rep.SetParameterValue("Category4", categoryFour);
            rep.SetParameterValue("Category5", categoryFive);

            rep.SetParameterValue("Status1", statusOne);
            rep.SetParameterValue("Status2", statusTwo);
            rep.SetParameterValue("Status3", statusThree);

            rep.SetParameterValue("NumberOfTickets", numberOfTickets);

            if (rep.Report.Prepare())
            {
                FastReport.Export.PdfSimple.PDFSimpleExport pdfExport = new FastReport.Export.PdfSimple.PDFSimpleExport();
                pdfExport.ShowProgress = false;
                pdfExport.Subject = "Subject Report";
                pdfExport.Title = "Report Title";
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                rep.Report.Export(pdfExport, ms);
                rep.Dispose();
                pdfExport.Dispose();
                ms.Position = 0;

                TempData["proba"] = things;

                return File(ms, "application/pdf", "myreport.pdf");
            }
            else
            {
                return null;
            }
        }

        // GET: Tikets/Details/5
        [Authorize(Roles = "user,admin,headadmin")]
        public ActionResult Details(int? id)
        {
            Tiket tiket = _ticketRepository.FindTicket(id);

            ViewBag.IDStatus = new SelectList(db.Status, "IDStatus", "Naziv");
            ViewBag.IDKat = new SelectList(db.Kategorijas, "IDKat", "Naziv");

            if (id == null || tiket == null)
            {
                TempData["MessageErrorTicketIndex"] = "Tiket sa ovim rednim brojem ne postoji u sistemu.";

                return RedirectToAction("Index");
            }

            List<User> users = new List<User>();

            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = dbUsers.Users.Where(a => a.Email == imePrivremeno).FirstOrDefault();

            var ticketValues = db.Tikets.Where(t => t.IDTiket == id).FirstOrDefault();

            var ticketOwner = dbUsers.Users.Where(t => t.Id == ticketValues.IDUser).FirstOrDefault();

            users = dbUsers.Users.Where(u => u.RoleId == 4 || u.RoleId == 7).ToList();


            if (user == ticketOwner)
            {
                users.Remove(user);
                ticketOwner.Email = "Moj nalog";
                users.Add(ticketOwner);
            }
            else
            {
                users.Remove(user);
                users.Add(ticketOwner);
                user.Email = "Moj nalog";
                users.Add(user);

            }



            ViewBag.IDUserAssignedTo = new SelectList(users, "ID", "Email");

            if (user.Id != tiket.IDUser && (user.RoleId != 4 && user.RoleId != 7))
            {
                TempData["MessageErrorTicketIndex"] = "Nemate pravo pristupa tiketu broj " + id + ".";
                return RedirectToAction("Index");
            }

            if (tiket == null)
            {
                return HttpNotFound();
            }

            ImgViewModel model = new ImgViewModel { FileAttach = null, ImgLst = new List<ImgObj>() };
            try
            {
                IEnumerable<tbl_file> files = _ticketRepository.GetAllFilesByTicket(tiket.IDTiket);

                foreach (var file in files)
                {
                    ImgObj item = new ImgObj();
                    item.FileId = file.file_id;
                    item.FileName = file.file_name;
                    item.FileContentType = file.file_base6;
                    item.IDTiket = file.IDTiket;
                    item.IDUser = file.IDUser;
                    item.UserName = file.UserName;
                    item.Datum = file.Datum;

                    model.ImgLst.Add(item);
                }
            }
            catch (Exception ex)
            {
                // Info  
                Console.Write(ex);
            }

            TicketImgVM tivm = new TicketImgVM();
            tivm.Tiketi = tiket;
            tivm.Files = model;

            return View(tivm);
        }

        // GET: Tikets/Create
        public ActionResult Create()
        {
            ViewBag.IDKat = new SelectList(db.Kategorijas, "IDKat", "Naziv");
            ViewBag.IDStatus = new SelectList(db.Status, "IDStatus", "Naziv");

            List<User> users = new List<User>();

            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = dbUsers.Users.Where(a => a.Email == imePrivremeno).FirstOrDefault();

            users = dbUsers.Users.Where(u => u.RoleId == 4 || u.RoleId == 7).ToList();

            if (user.RoleId == 4 || user.RoleId == 7)
            {
                users.Remove(user);
            }

            ViewBag.IDUserAssignedTo = new SelectList(users, "ID", "Email");

            return View();
        }

        // POST: Tikets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Naslov,Opis,Datum,DateUpdated,IDStatus,IDKat,IDUser,IDUserAssignedTo,UserNameAssignedTo")] Tiket tiket, HttpPostedFileBase[] FileAttach)
        {
            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = dbUsers.Users.Where(a => a.Email == imePrivremeno).FirstOrDefault();

            var userAssigneTo = dbUsers.Users.Where(u => u.Id == tiket.IDUserAssignedTo).FirstOrDefault();
            tiket.UserNameAssignedTo = userAssigneTo.Name;

            if (tiket.Naslov.Length > 49)
            {
                TempData["MessageNotificationCreate"] = "Naslov tiketa ne može sadržati više od 50 karaktera.";
                return RedirectToAction("Create");
            }

            if (ModelState.IsValid)
            {
                string adminData = _loginRepositor.GetAdminDataEmail();
                //ovde pozvati metodu iz repozitorija
                _ticketRepository.CreateTicket(tiket, FileAttach, user, adminData);
                TempData["MessageSuccessTicketIndex"] = user.Name + " je uspešno napravio tiket!";

                //ovde staviti da ide na detalje kreiranog tiketa
                return RedirectToAction("Index");
            }

            ViewBag.IDKat = new SelectList(db.Kategorijas, "IDKat", "Naziv", tiket.IDKat);
            ViewBag.IDStatus = new SelectList(db.Status, "IDStatus", "Naziv", tiket.IDStatus);

            return View(tiket);
        }

        // POST: Tikets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        // ISPRAVKA STATUSA TIKETA 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStatus(Tiket tiket)
        {
            if (tiket.IDStatus != 0 && tiket.IDKat != 0 && (tiket.IDUserAssignedTo != 0 && tiket.IDUserAssignedTo != null))
            {
                _ticketRepository.EditStatusTicket(tiket.IDTiket, tiket.IDStatus);

                _ticketRepository.EditCategoryTicket(tiket.IDTiket, tiket.IDKat);

                var userAssigneTo = dbUsers.Users.Where(u => u.Id == tiket.IDUserAssignedTo).FirstOrDefault();

                _ticketRepository.EditAssignetTo(tiket.IDTiket, tiket.IDUserAssignedTo, userAssigneTo.Email);

                TempData["MessageSuccessTicket"] = "Uspešno ste izmenili status i kategoriju i korisnika na koga je tiket naslovljen.";
                return RedirectToAction("Details/" + tiket.IDTiket);
            }
            else if (tiket.IDStatus != 0 && tiket.IDKat != 0)
            {
                _ticketRepository.EditStatusTicket(tiket.IDTiket, tiket.IDStatus);

                _ticketRepository.EditCategoryTicket(tiket.IDTiket, tiket.IDKat);

                TempData["MessageSuccessTicket"] = "Uspešno ste izmenili status i kategoriju tiketa.";
                return RedirectToAction("Details/" + tiket.IDTiket);
            }
            else if (tiket.IDStatus != 0 && (tiket.IDUserAssignedTo != 0 && tiket.IDUserAssignedTo != null))
            {
                _ticketRepository.EditStatusTicket(tiket.IDTiket, tiket.IDStatus);

                var userAssigneTo = dbUsers.Users.Where(u => u.Id == tiket.IDUserAssignedTo).FirstOrDefault();

                _ticketRepository.EditAssignetTo(tiket.IDTiket, tiket.IDUserAssignedTo, userAssigneTo.Email);

                TempData["MessageSuccessTicket"] = "Uspešno ste izmenili status tiketa i korisnika na koga je tiket naslovljen.";
                return RedirectToAction("Details/" + tiket.IDTiket);
            }
            else if (tiket.IDKat != 0 && (tiket.IDUserAssignedTo != 0 && tiket.IDUserAssignedTo != null))
            {
                _ticketRepository.EditCategoryTicket(tiket.IDTiket, tiket.IDKat);

                var userAssigneTo = dbUsers.Users.Where(u => u.Id == tiket.IDUserAssignedTo).FirstOrDefault();

                _ticketRepository.EditAssignetTo(tiket.IDTiket, tiket.IDUserAssignedTo, userAssigneTo.Email);

                TempData["MessageSuccessTicket"] = "Uspešno ste izmenili kategoriju tiketa i korisnika na koga je tiket naslovljen.";
                return RedirectToAction("Details/" + tiket.IDTiket);
            }
            else if (tiket.IDStatus != 0)
            {
                _ticketRepository.EditStatusTicket(tiket.IDTiket, tiket.IDStatus);

                TempData["MessageSuccessTicket"] = "Uspešno ste izmenili status tiketa.";
                return RedirectToAction("Details/" + tiket.IDTiket);
            }
            else if (tiket.IDKat != 0)
            {
                _ticketRepository.EditCategoryTicket(tiket.IDTiket, tiket.IDKat);

                TempData["MessageSuccessTicket"] = "Uspešno ste izmenili kategoriju tiketa.";
                return RedirectToAction("Details/" + tiket.IDTiket);
            }
            else if (tiket.IDUserAssignedTo != 0 && tiket.IDUserAssignedTo != null)
            {
                var userAssigneTo = dbUsers.Users.Where(u => u.Id == tiket.IDUserAssignedTo).FirstOrDefault();

                _ticketRepository.EditAssignetTo(tiket.IDTiket, tiket.IDUserAssignedTo, userAssigneTo.Email);

                TempData["MessageSuccessTicket"] = "Uspešno ste izmenili korisnika na koga je tiket naslovljen.";
                return RedirectToAction("Details/" + tiket.IDTiket);
            }
            else
            {
                TempData["MessageErrorTicket"] = "Morate izabrati barem jednu vrednost iz neke od padajućih lista.";
                return RedirectToAction("Details/" + tiket.IDTiket);
            }
        }

        public ActionResult AddRelation(int? IDTiketRel, int? relatedId)
        {
            if (IDTiketRel == relatedId)
            {
                TempData["MessageErrorTicket"] = "Izaberite različit broj tiketa.";
                return RedirectToAction("Details/" + IDTiketRel);
            }

            if (relatedId == null)
            {
                TempData["MessageErrorTicket"] = " Polje 'Broj tiketa' mora biti popunjeno.";
                return RedirectToAction("Details/" + IDTiketRel);
            }

            int provera = _ticketRepository.CheckTicketRelation(IDTiketRel, relatedId);

            if (provera == 1)
            {
                TempData["MessageErrorTicket"] = "Tiketi su vec povezani.";
                return RedirectToAction("Details/" + IDTiketRel);
            }

            Tiket relatedTicket = _ticketRepository.FindTicket(relatedId);

            if (relatedTicket != null)
            {
                if (ModelState.IsValid)
                {
                    _ticketRepository.InsertRelations(IDTiketRel, relatedId);
                    TempData["MessageSuccessTicket"] = "Uspešno povezani tiketi.";
                    return RedirectToAction("Details/" + IDTiketRel);
                }
            }
            else
            {
                TempData["MessageErrorTicket"] = " Ne postoji tiket sa ovim brojem.";
                return RedirectToAction("Details/" + IDTiketRel);
            }

            Tiket tiket = _ticketRepository.FindTicket(IDTiketRel);

            if (tiket == null)
            {
                TempData["MessageErrorTicket"] = " Ne postoji tiket sa ovim brojem.";
                return RedirectToAction("Details/" + IDTiketRel);
            }

            return View(tiket);
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
