using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication4.Helper_Code.Objects
{
    public class ImgObj
    {
        #region Properties  

        /// <summary>  
        /// Gets or sets Image ID.  
        /// </summary>  
        public int FileId { get; set; }

        /// <summary>  
        /// Gets or sets Image name.  
        /// </summary>  
        public string FileName { get; set; }


        //MOZDA PROMENITI NAZIV DA NIJE ISTI KAO U KLASI MODELA **tbl_file**

        /// <summary>  
        /// Gets or sets Image name.  
        /// </summary>  
        public int IDTiket { get; set; }

        /// <summary>  
        /// Gets or sets Image extension.  
        /// </summary>  
        public string FileContentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? IDUser { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }

        public DateTime? Datum { get; set; }
        #endregion
    }
}