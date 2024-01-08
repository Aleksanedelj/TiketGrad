using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Helper_Code.Objects;
using WebApplication4.Interfaces;
//using WebApplication4.Models;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Repository;
using WebApplication4.Helper_Code.ViewModels;
using WebApplication4.Models.TicketModel;


namespace WebApplication4.Controllers
{
    public class ImgController : Controller
    {
        #region Private Properties  

        /// <summary>  
        /// Gets or sets database manager property.  
        /// </summary>  
        //private probaZaTiketeEntities databaseManager = new probaZaTiketeEntities();
        private TicketModel databaseManager = new TicketModel();

        DB dbUsers = new DB();

        private IImgRepository _imgRepository;
        private ILoginRepository _loginRepositor;

        public ImgController()
        {
            this._imgRepository = new ImgRepository(new TicketModel());
            this._loginRepositor = new LoginRepository(new DB());
        }

        #endregion

        #region POST: /Img/Index  

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(ImgViewModel model, int IDTiket)
        {
            var imePrivremeno = HttpContext.User.Identity.Name;
            var user = _loginRepositor.GetUserByEmail(imePrivremeno);

            if (ModelState.IsValid)
            {
                try
                {
                    _imgRepository.CreateImg(model, IDTiket, user);
                    TempData["MessageSuccessTicket"] = user.Name + " je uspešno dodao datoteku.";
                    return RedirectToAction("Details" + "/" + IDTiket, "Tikets");
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }
            }

            return this.View(model);
        }

        #endregion

        #region GET: /Img/DownloadFile  

        /// <summary>  
        /// GET: /Img/DownloadFile  
        /// </summary>  
        /// <param name="fileId">File Id parameter</param>  
        /// <returns>Return download file</returns>  
        public ActionResult DownloadFile(int fileId)
        {
            // Model binding.  
            ImgViewModel model = new ImgViewModel { FileAttach = null, ImgLst = new List<ImgObj>() };

            try
            {
                var fileBase6 = _imgRepository.GetFilesDetailsFileBase6(fileId);
                var fileExt = _imgRepository.GetFilesDetailsFileExt(fileId);

                return _imgRepository.GetFile(fileBase6, fileExt);
            }
            catch (Exception ex)
            {
                // Info  
                Console.Write(ex);
            }

            // Info.  
            return this.View(model);
        }

        #endregion
    }
}