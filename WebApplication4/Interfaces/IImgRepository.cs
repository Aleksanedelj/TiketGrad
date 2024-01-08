using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication4.Helper_Code.Objects;
//using WebApplication4.Models;
using WebApplication4.Models.TicketModel;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Helper_Code.ViewModels;

namespace WebApplication4.Interfaces
{
    internal interface IImgRepository
    {
        void CreateImg(ImgViewModel model, int IDTiket, User user);

        string GetFilesDetailsFileBase6(int fileId);

        string GetFilesDetailsFileExt(int fileId);

        FileResult GetFile(string fileContent, string fileContentType);
    }
}
