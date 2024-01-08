using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using WebApplication4.Interfaces;
//using WebApplication4.Models;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Models.TicketModel;

namespace WebApplication4.Repository
{
    public class TicketRepository : ITicketRepository
    {
        //private probaZaTiketeEntities _context;
        private TicketModel _context;

        //public TicketRepository(probaZaTiketeEntities context)
        //{
        //    _context = context;
        //}

        public TicketRepository(TicketModel context)
        {
            _context = context;
        }

        public int CheckTicketRelation(int? idTicketRel, int? relatedId)
        {
            string provera = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = "exec [dbo].[ProveraPovezanihTiketa] @RelatedID, @RelatedToId";
                command.Parameters.AddWithValue("@RelatedID", idTicketRel);
                command.Parameters.AddWithValue("@RelatedToId", relatedId);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    provera = reader["provera"].ToString();
                }
                reader.Close();
                command.Dispose();
                conn.Close();
            }

            return Convert.ToInt32(provera);
        }

        public void CreateTicket(Tiket tiket, HttpPostedFileBase[] FileAttach, User user, string adminEmail)
        {
            tiket.IDUser = user.Id;
            tiket.UserName = user.Name;

            tiket.Datum = DateTime.Now;
            tiket.DateUpdated = DateTime.Now;

            _context.Tikets.Add(tiket);
            _context.SaveChanges();
            
            if (FileAttach != null)
            {
                foreach (HttpPostedFileBase file in FileAttach)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        //ovde se dodaje dokument uz tiket --pocetak
                        // Initialization.  
                        string fileContent = string.Empty;
                        string fileContentType = string.Empty;

                        int lastTiketId = _context.Tikets.Max(item => item.IDTiket);

                        // Converting to bytes.  
                        byte[] uploadedFile = new byte[file.ContentLength];
                        file.InputStream.Read(uploadedFile, 0, uploadedFile.Length);

                        // Initialization.  
                        fileContent = Convert.ToBase64String(uploadedFile);
                        fileContentType = file.ContentType;

                        // Saving info.  
                       // _context.sp_insert_file(file.FileName, fileContentType, lastTiketId, fileContent, user.Id, user.Name, DateTime.Now);

                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
                        {
                            using (SqlCommand cmd = new SqlCommand("sp_insert_file", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@file_name", file.FileName);
                                cmd.Parameters.AddWithValue("@file_ext", fileContentType);
                                cmd.Parameters.AddWithValue("@IDTiket", lastTiketId);
                                cmd.Parameters.AddWithValue("@file_base64", fileContent);
                                cmd.Parameters.AddWithValue("@IDUser", user.Id);
                                cmd.Parameters.AddWithValue("@UserName", user.Name);
                                cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }
                        }
                        //ovde se dodaje dokument uz tiket kraj
                    }
                }
            }

            //ovo je mail onoga ko unosi tiket
            SendResetPasswordEmail(user.Email, tiket);

            //TO DO: ovde mail od admin sluzbe
            SendResetPasswordEmail(adminEmail, tiket);

        }

        private void SendResetPasswordEmail(string email, Tiket tiket)
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
            mailMessage.Body = "utekst tiketa: " + tiket.Opis;

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

        public void EditCategoryTicket(int idtiket, int idkat)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangeTicketCategory", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDTiketa", idtiket);
                    cmd.Parameters.AddWithValue("@KategorijaTiketa", idkat);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangeDateUpdatedTicketTable", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDTiketa", idtiket);
                    cmd.Parameters.AddWithValue("@DateUpdated", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            _context.SaveChanges();
        }

        public void EditStatusTicket(int idtiket, int idstatus)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangeTicketStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDTiketa", idtiket);
                    cmd.Parameters.AddWithValue("@StatusTiketa", idstatus);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangeDateUpdatedTicketTable", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDTiketa", idtiket);
                    cmd.Parameters.AddWithValue("@DateUpdated", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            _context.SaveChanges();
        }

        public Tiket FindTicket(int? id)
        {
            return _context.Tikets.Find(id);
        }

        public IEnumerable<Tiket> GetAllTickets(int userRoleId, int userId)
        {
            if (userRoleId == 4 || userRoleId == 7)
            {
                return _context.Tikets.Include(t => t.Kategorija).Include(t => t.Status).ToList();
            }
            else
            {
                return _context.Tikets.Where(t => t.IDUser == userId).Include(t => t.Kategorija).Include(t => t.Status).ToList();
            }
        }

        public IEnumerable<Tiket> GetAllTicketsByParameters(int? IDStatus, int? IDKat, DateTime? dateNameFrom, DateTime? dateNameTo, int? selectedState, string textNote, 
            int? ticketNumber, int userRoleId, int userId)
        {
            IEnumerable<Tiket> tikets = new List<Tiket>();

            tikets = GetAllTickets(userRoleId, userId);

            //              01.11 -- null
            //              null  -- 01.11

            if(dateNameFrom != null && dateNameTo == null)
            {
                dateNameTo = DateTime.Now;
            }

            if(dateNameTo != null && dateNameFrom == null)
            {
                dateNameFrom = DateTime.MinValue;
            }



            if (selectedState == 1)
            {
                tikets = GetAllTickets(userRoleId, userId);
            }

            //svi parametri
            else if (IDStatus != null && IDKat != null && (dateNameFrom != null && dateNameTo != null) && (textNote != "" && textNote != null) && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId)
                    .Where(s => s.IDStatus == IDStatus && s.IDKat == IDKat && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo) && s.Naslov.Contains(textNote) && s.IDTiket == ticketNumber);
            }

            //kombinacije sa 4 parametra
            else if (IDKat != null && (dateNameFrom != null && dateNameTo != null) && (textNote != "" && textNote != null) && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo) && s.Naslov.Contains(textNote)
                && s.IDTiket == ticketNumber);
            }
            else if (IDStatus != null && (dateNameFrom != null && dateNameTo != null) && (textNote != "" && textNote != null) && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo) && s.Naslov.Contains(textNote)
                && s.IDTiket == ticketNumber);
            }
            else if (IDKat != null && IDStatus != null && (textNote != "" && textNote != null) && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDStatus == IDStatus && s.Naslov.Contains(textNote)
                && s.IDTiket == ticketNumber);
            }
            else if (IDKat != null && IDStatus != null && (dateNameFrom != null && dateNameTo != null) && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDStatus == IDStatus && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo)
                && s.IDTiket == ticketNumber);
            }
            else if (IDKat != null && IDStatus != null && (dateNameFrom != null && dateNameTo != null) && (textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDStatus == IDStatus && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo)
                && s.Naslov.Contains(textNote));
            }

            //kombinacije sa 3 parametra
            else if (IDKat != null && IDStatus != null && (textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDStatus == IDStatus && s.Naslov.Contains(textNote));
            }
            else if (IDKat != null && IDStatus != null && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDStatus == IDStatus && s.IDTiket == ticketNumber);
            }
            else if (IDKat != null && IDStatus != null && (dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDStatus == IDStatus && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if (IDKat != null && (textNote != "" && textNote != null) && (dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.Naslov.Contains(textNote) && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if (IDKat != null && ticketNumber != null && (dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDTiket == ticketNumber && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if (IDKat != null && ticketNumber != null && (textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDTiket == ticketNumber && s.Naslov.Contains(textNote));
            }
            else if (IDStatus != null && (textNote != "" && textNote != null) && (dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus && s.Naslov.Contains(textNote) && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if (IDStatus != null && ticketNumber != null && (dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus && s.IDTiket == ticketNumber && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if (IDStatus != null && ticketNumber != null && (textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus && s.IDTiket == ticketNumber && s.Naslov.Contains(textNote));
            }
            else if ((dateNameFrom != null && dateNameTo != null) && ticketNumber != null && (textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => (s.Datum >= dateNameFrom && s.Datum <= dateNameTo) && s.IDTiket == ticketNumber && s.Naslov.Contains(textNote));
            }

            //kombinacije sa 2 parametra
            else if (IDStatus != null && IDKat != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus && s.IDKat == IDKat);
            }
            else if (IDStatus != null && (dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if (IDStatus != null && (textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus && s.Naslov.Contains(textNote));
            }
            else if (IDStatus != null && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus && s.IDTiket == ticketNumber);
            }
            else if (IDKat != null && (dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if (IDKat != null && (dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if (IDKat != null && (textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.Naslov.Contains(textNote));
            }
            else if (IDKat != null && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat && s.IDTiket == ticketNumber);
            }
            else if ((dateNameFrom != null && dateNameTo != null) && (textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => (s.Datum >= dateNameFrom && s.Datum <= dateNameTo) && s.Naslov.Contains(textNote));
            }
            else if ((dateNameFrom != null && dateNameTo != null) && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => (s.Datum >= dateNameFrom && s.Datum <= dateNameTo) && s.IDTiket == ticketNumber);
            }
            else if ((textNote != "" && textNote != null) && ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.Naslov.Contains(textNote) && s.IDTiket == ticketNumber);
            }


            //kombinacije sa jednim parametrom
            else if (IDKat != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDKat == IDKat);
            }
            else if (IDStatus != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDStatus == IDStatus);
            }
            else if ((dateNameFrom != null && dateNameTo != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => (s.Datum >= dateNameFrom && s.Datum <= dateNameTo));
            }
            else if ((textNote != "" && textNote != null))
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.Naslov.Contains(textNote));
            }
            else if (ticketNumber != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDTiket == ticketNumber);
            }

            else
            {
                //nista ako se nista ne odabere
                //prazna lista
                tikets = new List<Tiket>();
            }

            return tikets;
        }

        public IEnumerable<Tiket> GetAllTicketsByUsersParameters(int? IDUserCreate, int? IDUserAssignedTo, int userRoleId, int userId)
        {
            IEnumerable<Tiket> tikets = new List<Tiket>();

            tikets = GetAllTickets(userRoleId, userId);

            if(IDUserCreate != null && IDUserAssignedTo != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDUser == IDUserCreate && s.IDUserAssignedTo == IDUserAssignedTo);
            }
            else if(IDUserCreate != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDUser == IDUserCreate);
            }
            else if (IDUserAssignedTo != null)
            {
                tikets = GetAllTickets(userRoleId, userId).Where(s => s.IDUserAssignedTo == IDUserAssignedTo);
            }

            return tikets;
        }

        public void InsertRelations(int? idTiketRel, int? relatedId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertIntoRelations", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@related", relatedId);
                    cmd.Parameters.AddWithValue("@relatedTo", idTiketRel);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertIntoRelations", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@related", idTiketRel);
                    cmd.Parameters.AddWithValue("@relatedTo", relatedId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            _context.SaveChanges();
        }

        public IEnumerable<Tiket> SortingTickets(string sortOrder, IEnumerable<Tiket> tiketsSorted, IEnumerable<Tiket> tikets)
        {
            switch (sortOrder)
            {
                case "IDTik":
                    tiketsSorted = tikets.OrderBy(s => s.IDTiket);
                    break;
                case "idTik_desc":
                    tiketsSorted = tikets.OrderByDescending(s => s.IDTiket);
                    break;
                case "Date":
                    tiketsSorted = tikets.OrderBy(s => s.Datum);
                    break;
                case "date_desc":
                    tiketsSorted = tikets.OrderByDescending(s => s.Datum);
                    break;
                case "DateUpdated":
                    tiketsSorted = tikets.OrderBy(s => s.DateUpdated);
                    break;
                case "dateUpdated_desc":
                    tiketsSorted = tikets.OrderByDescending(s => s.DateUpdated);
                    break;
                case "StatusSorted":
                    tiketsSorted = tikets.OrderBy(s => s.Status.IDStatus);
                    break;
                case "StatusSorted_desc":
                    tiketsSorted = tikets.OrderByDescending(s => s.Status.IDStatus);
                    break;
                case "CategorySorted":
                    tiketsSorted = tikets.OrderBy(s => s.Kategorija.IDKat);
                    break;
                case "CategorySorted_desc":
                    tiketsSorted = tikets.OrderByDescending(s => s.Kategorija.IDKat);
                    break;
                default:
                    tiketsSorted = tikets.OrderBy(s => s.IDTiket).OrderByDescending(s => s.DateUpdated);
                    break;
            }

            return tiketsSorted;
        }


        public IEnumerable<tbl_file> GetAllFilesByTicket(int ticketId)
        {
            //DataSet ds = new DataSet();
            //using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            //{
            //    using (SqlCommand cmd = new SqlCommand("sp_get_all_files_byTicketId", conn))
            //    {
            //        cmd.Parameters.AddWithValue("@idtiket", ticketId);

            //        cmd.CommandType = CommandType.StoredProcedure;

            //        SqlDataAdapter da = new SqlDataAdapter();
            //        da.SelectCommand = cmd;

            //        da.Fill(ds);
            //    }
            //}

            //return ds;


            return _context.tbl_file.Where(f => f.IDTiket == ticketId).ToList();




        }

        public void EditAssignetTo(int idtiket, int? idAssignTo, string userNameAssignedTo)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangeAssignedTo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDTiketa", idtiket);
                    cmd.Parameters.AddWithValue("@IDAssignedTo", idAssignTo);
                    cmd.Parameters.AddWithValue("@UserNameAssignedTo", userNameAssignedTo);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangeDateUpdatedTicketTable", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDTiketa", idtiket);
                    cmd.Parameters.AddWithValue("@DateUpdated", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            _context.SaveChanges();
        }
    }
}