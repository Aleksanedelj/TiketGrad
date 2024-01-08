using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication4.Models.LoginUsers
{
    public class User
    {
        public int Id { get; set; }
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Ime može sadržati između 3 i 20 karaktera.")]
        public string Name { get; set; }

        [Display(Name = "Email adresa")]
        [Required(ErrorMessage = "Zahteva se unos email adrese u polju 'Email adresa'.")]
        [EmailAddress(ErrorMessage = "Nepravilna Email adresa. Pravilan oblik Email adrese je 'testtestovic@gmail.com'.")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public UserRole Role { get; set; }

        [StringLength(20, MinimumLength = 3, ErrorMessage = "Prezime može sadržati između 3 i 20 karaktera.")]
        public string Prezime { get; set; }

        [Required(ErrorMessage = "Morate uneti broj telefona u polju 'Telefon'.")]
        [Display(Name = "Telefon")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Broj mobilnog telefona je nepravilan.")]
        public string Telefon { get; set; }

        //public User()
        //{
        //    RoleId = 5;
            
        //}
    }
}