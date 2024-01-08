using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication4.Interfaces;
//using WebApplication4.Models;
using System.Data.Entity;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Models.TicketModel;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace WebApplication4.Repository
{
    public class CommentRepository : ICommentRepository
    {
        //private probaZaTiketeEntities _context;
        private TicketModel _context;

        public CommentRepository(TicketModel context)
        {
            _context = context;
        }

        public void CreateComment(Komentar komentar, HttpPostedFileBase[] FileAttach, User user)
        {
            komentar.IDUser = user.Id;
            komentar.UserName = user.Name;
            komentar.Datum = DateTime.Now;

            _context.Komentars.Add(komentar);
            //ovde procedura da se u tabeli Tiket za id tiket updateuje DateUpdated na danasnji
            //_context.ChangeDateUpdatedTicketTable(komentar.IDTiket, DateTime.Now);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangeDateUpdatedTicketTable", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDTiketa", komentar.IDTiket);
                    cmd.Parameters.AddWithValue("@DateUpdated", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

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

                        //int lastTiketId = db.Tikets.Max(item => item.IDTiket);

                        int lastTiketId = komentar.IDTiket;

                        // Converting to bytes.  
                        byte[] uploadedFile = new byte[file.ContentLength];
                        file.InputStream.Read(uploadedFile, 0, uploadedFile.Length);

                        // Initialization.  
                        fileContent = Convert.ToBase64String(uploadedFile);
                        fileContentType = file.ContentType;

                        // Saving info.  
                        //_context.sp_insert_file_forComment(file.FileName, fileContentType, lastTiketId, fileContent, user.Id, user.Name, komentar.IDKomentar, DateTime.Now);

                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
                        {
                            using (SqlCommand cmd = new SqlCommand("sp_insert_file_forComment", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@file_name", file.FileName);
                                cmd.Parameters.AddWithValue("@file_ext", fileContentType);
                                cmd.Parameters.AddWithValue("@IDTiket", lastTiketId);
                                cmd.Parameters.AddWithValue("@file_base64", fileContent);
                                cmd.Parameters.AddWithValue("@IDUser", user.Id);
                                cmd.Parameters.AddWithValue("@UserName", user.Name);
                                cmd.Parameters.AddWithValue("@IDKomentar", komentar.IDKomentar);
                                cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }
                        }

                        //ovde se dodaje dokument uz tiket kraj
                    }
                }
            }
        }
    }
}