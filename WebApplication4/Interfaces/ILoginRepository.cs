using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication4.Models.LoginUsers;

namespace WebApplication4.Interfaces
{
    internal interface ILoginRepository
    {
        bool CheckEmail(string userEmail);

        void AddUser(User user);

        User GetUserByEmail(string imePrivremeno);

        UserRole GetUserByRoleId(int roleId);

        User GetUserById(int? userId);

        bool LoginUser(Login login);

        void EditUser(User user);

        void ChangePassword(int userId, string userPassNew);

        IEnumerable<User> GetAllUsers();

        string GetAdminDataEmail();

        void EditUserRole(User user);

        void DeleteUser(int id);

        void AdminEmailChange(string email);
    }
}
