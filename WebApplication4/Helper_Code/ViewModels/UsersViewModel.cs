using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication4.Models.LoginUsers;

namespace WebApplication4.Helper_Code.ViewModels
{
    public class UsersViewModel
    {
        public User Users { get; set; }
        public AdminData AdminData { get; set; }
    }
}