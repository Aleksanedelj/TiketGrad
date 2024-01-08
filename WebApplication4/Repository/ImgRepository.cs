using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Helper_Code.Objects;
using WebApplication4.Interfaces;
//using WebApplication4.Models;
using WebApplication4.Models.TicketModel;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Helper_Code.ViewModels;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace WebApplication4.Repository
{
    public class ImgRepository : IImgRepository
    {
        private TicketModel _context;

        public ImgRepository(TicketModel context)
        {
            _context = context;
        }

        public void CreateImg(ImgViewModel model, int IDTiket, User user)
        {
            // Initialization.  
            string fileContent = string.Empty;
            string fileContentType = string.Empty;

            // Converting to bytes.  
            byte[] uploadedFile = new byte[model.FileAttach.InputStream.Length];
            model.FileAttach.InputStream.Read(uploadedFile, 0, uploadedFile.Length);

            // Initialization.  
            fileContent = Convert.ToBase64String(uploadedFile);
            fileContentType = model.FileAttach.ContentType;

            // Saving info.  
            //_context.sp_insert_file(model.FileAttach.FileName, fileContentType, IDTiket, fileContent, user.Id, user.Name, DateTime.Now);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_insert_file", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@file_name", model.FileAttach.FileName);
                    cmd.Parameters.AddWithValue("@file_ext", fileContentType);
                    cmd.Parameters.AddWithValue("@IDTiket", IDTiket);
                    cmd.Parameters.AddWithValue("@file_base64", fileContent);
                    cmd.Parameters.AddWithValue("@IDUser", user.Id);
                    cmd.Parameters.AddWithValue("@UserName", user.Name);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            //ovde procedura da se u tabeli Tiket za id tiket updateuje DateUpdated na danasnji
            //_context.ChangeDateUpdatedTicketTable(IDTiket, DateTime.Now);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ChangeDateUpdatedTicketTable", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDTiketa", IDTiket);
                    cmd.Parameters.AddWithValue("@DateUpdated", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            _context.SaveChanges();

            // Settings.  
            //model.ImgLst = this._context.sp_get_all_files().Select(p => new ImgObj
            //{
            //    FileId = p.file_id,
            //    FileName = p.file_name,
            //    IDTiket = p.IDTiket,
            //    FileContentType = p.file_ext,
            //    IDUser = p.IDUser,
            //    UserName = p.UserName
            //}).ToList();
        }

        public string GetFilesDetailsFileBase6(int fileId)
        {
            string obj = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = "exec sp_get_file_details @file_id";
                command.Parameters.AddWithValue("@file_id", fileId);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    obj = reader["file_base6"].ToString();
                }
                reader.Close();
                command.Dispose();
                conn.Close();
            }

            return obj;

        }

        public string GetFilesDetailsFileExt(int fileId)
        {
            string obj = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tiketiModel"].ConnectionString))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = "exec sp_get_file_details @file_id";
                command.Parameters.AddWithValue("@file_id", fileId);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    obj = reader["file_ext"].ToString();
                }
                reader.Close();
                command.Dispose();
                conn.Close();
            }

            return obj;

        }

        public FileResult GetFile(string fileContent, string fileContentType)
        {
            // Initialization.  
            FileResult file = null;

            try
            {
                // Get file.  
                byte[] byteContent = Convert.FromBase64String(fileContent);
                //file = File(byteContent, fileContentType);
                FileContentResult fcr = new FileContentResult(byteContent, fileContentType);
                file = fcr;
            }
            catch (Exception ex)
            {
                // Info.  
                throw ex;
            }

            // info.  
            return file;
        }
    }
}