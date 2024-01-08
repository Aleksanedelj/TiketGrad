using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebApplication4.Common;
using WebApplication4.Interfaces;
using WebApplication4.Models.LoginUsers;

namespace WebApplication4.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private DB _context;

        public LoginRepository(DB context)
        {
            _context = context;
        }

        public bool CheckEmail(string userEmail)
        {
            var userCheckEmail = _context.Users.Where(a => a.Email == userEmail).FirstOrDefault();

            if (userCheckEmail != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddUser(User user)
        {
            using (DB db = new DB())
            {
                Password pass = new Password();

                user.Password = pass.ComputeHash(user.Password, "SHA512");
                user.RoleId = 5;
                db.Users.Add(user);
                db.SaveChanges();
            }
        }

        public User GetUserByEmail(string imePrivremeno)
        {
            return _context.Users.Where(a => a.Email == imePrivremeno).FirstOrDefault();
        }

        public UserRole GetUserByRoleId(int roleId)
        {
            return _context.Roles.Where(a => a.RoleId == roleId).FirstOrDefault();
        }

        public User GetUserById(int? userId)
        {
            return _context.Users.Find(userId);
        }

        public bool LoginUser(Login login)
        {
            using (DB db = new DB())
            {
                Password pass = new Password();
                var userpass = "";
                if (login.Password != null)
                {
                    userpass = pass.ComputeHash(login.Password, "SHA512");
                }
                var user = db.Users.Where(a => a.Email == login.Email && a.Password == userpass).FirstOrDefault();
                if (user != null)
                {
                    var Ticket = new FormsAuthenticationTicket(login.Email, true, 3000);
                    string Encrypt = FormsAuthentication.Encrypt(Ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, Encrypt);
                    cookie.HttpOnly = true;
                    HttpContext.Current.Response.Cookies.Add(cookie);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void EditUser(User user)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("EditUserProfile", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Prezime", user.Prezime);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Telefon", user.Telefon);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ChangePassword(int userId, string userPassNew)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangePassword", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", userId);
                    cmd.Parameters.AddWithValue("@Password", userPassNew);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public string GetAdminDataEmail()
        {
            var adminEmail = _context.AdminData.FirstOrDefault();

            if(adminEmail == null)
            {
                return null;
            }
            else
            {
                return adminEmail.AdminEmail;
            } 
        }

        public void EditUserRole(User user)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("EditUserRole", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", user.Id);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteUser(int id)
        {
            User user = _context.Users.Find(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public void AdminEmailChange(string email)
        {
           var data = _context.AdminData.FirstOrDefault();
                
            if(data != null)
            {

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("ChangeEmailAdminData", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", data.IDAdminData);
                        cmd.Parameters.AddWithValue("@Email", email);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            else
            {

            }
        }
    }
}