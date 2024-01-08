using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication4.Common;
using WebApplication4.Interfaces;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Repository;
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System;
using System.Security.Cryptography;

namespace WebApplication4.Controllers
{
    public class LoginController : Controller
    {
        private ILoginRepository _loginRepository;
        DB dbUsers = new DB();

        private readonly Microsoft.AspNet.Identity.UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public LoginController(Microsoft.AspNet.Identity.UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this._loginRepository = new LoginRepository(new DB());
            this.userManager = userManager;
            this.signInManager = signInManager;
        }




        public LoginController()
        {
            this._loginRepository = new LoginRepository(new DB());
        }

        //public LoginController()
        //{
        //    this._loginRepository = new LoginRepository(new DB());

        //}

        // GET: Login
        public ActionResult Register()
        {
            return View();
        }

        //komentarrrrrrrrrrrrrrrrr
        [HttpGet]
        [Authorize(Roles = "headadmin")]
        public ActionResult Index()
        {
            IEnumerable<User> users = _loginRepository.GetAllUsers();

            string adminData = _loginRepository.GetAdminDataEmail();

            TempData["AdminEmail"] = adminData;

            return View(users);
        }

        [HttpPost]
        public ActionResult Register(User user, string sifraRep)
        {
            //validacije
            bool checkEmail = _loginRepository.CheckEmail(user.Email);


            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMinimum8Chars = new Regex(@".{8,}");

            if (!hasNumber.IsMatch(user.Password) || !hasUpperChar.IsMatch(user.Password) || !hasMinimum8Chars.IsMatch(user.Password))
            {
                TempData["MessageRegister"] = "Šifr" +
                    "a mora sadržati minimun 8 karaktera, jedno veliko slovo i jedan broj.";
                return RedirectToAction("Register", "Login");
            }

            if (checkEmail == true)
            {
                TempData["MessageRegister"] = "Korisnik sa ovim Email-om već postoji.";
                return RedirectToAction("Register", "Login");
            }

            if (user.Password != sifraRep)
            {
                TempData["MessageRegister"] = "Šifre se ne poklapaju.";
                return RedirectToAction("Register", "Login");
            }

            if (ModelState.IsValid)
            {
                //poziv metode za insert korisnika u bazu
                _loginRepository.AddUser(user);
                TempData["MessageRegisterSuccess"] = "Uspešno ste se registrovali na portal.";
            }
            else
            {
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.Count > 0)
                    {
                        foreach (var error in item.Errors)
                        {
                            TempData["MessageRegister"] = error.ErrorMessage;
                        }
                    }
                }
                return RedirectToAction("Register");
            }

            ModelState.Clear();

            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            return View();
        }

        [Authorize(Roles = "user,admin,headadmin")]
        public ActionResult Details()
        {
            var imePrivremeno = HttpContext.User.Identity.Name;
            var userGet = _loginRepository.GetUserByEmail(imePrivremeno);
            var roleName = _loginRepository.GetUserByRoleId(userGet.RoleId);

            User user = _loginRepository.GetUserById(userGet.Id);

            user.Role.RoleName = roleName.RoleName;

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        public ActionResult Login(Login login)
        {
            bool loginUser = _loginRepository.LoginUser(login);

            if (loginUser == true)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["MessageLoginErrorData"] = "Niste uneli dobre podatke za prijavu.";
            }

            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Login");
        }


        // GET: Komentar/Edit/5
        [Authorize(Roles = "user,admin,headadmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User usr = _loginRepository.GetUserById(id);
            if (usr == null)
            {
                return HttpNotFound();
            }
            return View(usr);
        }

        // POST: Komentar/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                _loginRepository.EditUser(user);
                TempData["MessageLoginSuccess"] = "Uspesno promenjeni podaci korisnika.";
            }

            return RedirectToAction("Logout");
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var imePrivremeno = HttpContext.User.Identity.Name;
            var userGet = dbUsers.Users.Where(a => a.Email == imePrivremeno).FirstOrDefault();

            if (model.CurrentPassword == null || model.NewPassword == null)
            {
                TempData["MessageLoginErrorData"] = "Morate popuniti sve podatke na formi.";
                return View(model);
            }

            Password pass = new Password();
            var userpass = pass.ComputeHash(model.CurrentPassword, "SHA512");
            var userpassNew = pass.ComputeHash(model.NewPassword, "SHA512");

            if (userGet.Password != userpass)
            {
                TempData["MessageLoginErrorData"] = "Trenutna šifra nije tačna!";
                return View(model);
            }

            if (userpassNew == userpass)
            {
                TempData["MessageLoginErrorData"] = "Nova šifra mora biti različita u odnosu na trenutnu šifru.";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _loginRepository.ChangePassword(userGet.Id, userpassNew);
                TempData["MessageLoginSuccess"] = userGet.Name + " je uspešno izmenio šifru!";
            }

            return RedirectToAction("Edit" + "/" + userGet.Id, "Login");
        }

        // GET: Login/Edit/5
        [Authorize(Roles = "headadmin")]
        public ActionResult EditUserRole(int? id)
        {
            ViewBag.RoleId = new SelectList(dbUsers.Roles, "RoleId", "RoleName");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = dbUsers.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        //post edituserrole
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult EditUserRole(User user)
        {
            if (user.RoleId != 0)
            {
                _loginRepository.EditUserRole(user);

                TempData["MessageUserRoleChangedSuccess"] = "Uspesno promenjena uloga korisnika.";
            }
            else
            {
                TempData["MessageUserRoleChangedEmpty"] = "Morate izabrati vrednost iz padajuće liste.";
                return RedirectToAction("EditUserRole/" + user.Id);
            }

            return RedirectToAction("Index");
        }

        // POST: Tikets/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _loginRepository.DeleteUser(id);
            TempData["MessageUserRoleChangedSuccess"] = "Uspesno ste obrisali korisnika iz baze.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            var user = _loginRepository.GetUserByEmail(email);

            if (user == null)
            {
                TempData["usernotvalid"] = "nepravilan mail";
                return RedirectToAction("ForgotPassword");

                //ovde da ne postoji korisnik taj poruka , da je nepravilan mail
                //return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = GenerateToken();
            var resetLink = Url.Action("ResetPassword", "Login", new { userId = user.Id, token = token }, Request.Url.Scheme);

            SendResetPasswordEmail(email, resetLink);

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        private void SendResetPasswordEmail(string email, string resetLink)
        {
            // Sender's email address and credentials
            string senderEmail = "testimirovictestimir@gmail.com";
            string senderPassword = "mqjt ivms pnux ogbq";

            // Recipient's email address
            string recipientEmail = email;

            // SMTP server settings
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;

            // Create the email message
            MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail);
            mailMessage.Subject = "Test Email";
            mailMessage.Body = resetLink;

            // Create the SMTP client
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
               SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                // Send the email
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
            finally
            {
                // Dispose of resources
                mailMessage.Dispose();
                smtpClient.Dispose();
            }
        }

        public static string GenerateToken(int length = 32)
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());

            return token;
        }

        [HttpGet]
        public ActionResult ResetPassword(string userId, string token)
        {
            byte[] data = Convert.FromBase64String(token);
            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));

            if (when < DateTime.UtcNow.AddMinutes(-60))
            {
                TempData["tokenmessage"] = "istekao token";
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (model.Password == null || model.ConfirmPassword == null)
            {
                TempData["MessageLoginErrorData"] = "Morate popuniti sve podatke na formi.";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                TempData["PasswordCompare"] = "Šifre se moraju poklapati.";
                return View(model);
            }

            Password pass = new Password();
            var userPassNew = pass.ComputeHash(model.Password, "SHA512");

            if (model.UserId != null && model.Token != null)
            {
                _loginRepository.ChangePassword(Convert.ToInt32(model.UserId), userPassNew);
                TempData["ChangePasswordSuccess"] = "Vaša pifra je uspešno promenjena!";
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmailChange(string AdminEmail)
        {
            _loginRepository.AdminEmailChange(AdminEmail);

            return RedirectToAction("Index");
        }


    }
}

