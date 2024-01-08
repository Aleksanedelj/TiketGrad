using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
//using WebApplication4.Models;
using WebApplication4.Models.LoginUsers;
using WebApplication4.Models.TicketModel;


namespace WebApplication4.Interfaces
{
    public interface ITicketRepository
    {
        IEnumerable<Tiket> GetAllTickets(int userRoleId, int userId);

        IEnumerable<Tiket> GetAllTicketsByParameters(int? IDStatus, int? IDKat, DateTime? dateNameFrom, DateTime? dateNameTo, int? selectedState, string textNote, 
            int? ticketNumber, int userRoleId, int userId);

        IEnumerable<Tiket> GetAllTicketsByUsersParameters(int? IDUserCreate, int? IDUserAssignedTo, int userRoleId, int userId);

        Tiket FindTicket(int? id);

        void EditStatusTicket(int idtiket, int idstatus);

        void EditCategoryTicket(int idtiket, int idkat);

        void EditAssignetTo(int idtiket, int? idAssignTo, string userNameAssignedTo);

        int CheckTicketRelation(int? idTicketRel, int? relatedId);

        void InsertRelations(int? idTiketRel, int? relatedId);

        void CreateTicket(Tiket tiket, HttpPostedFileBase[] FileAttach, User user, string adminEmail);

        IEnumerable<Tiket> SortingTickets(string sortOrder, IEnumerable<Tiket> tiketsSorted, IEnumerable<Tiket> tikets);

        IEnumerable<tbl_file> GetAllFilesByTicket(int ticketId);
    }
}
