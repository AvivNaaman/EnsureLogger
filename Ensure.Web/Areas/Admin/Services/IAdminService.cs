using System.Threading.Tasks;
using Ensure.Web.Areas.Admin.Models;

namespace Ensure.Web.Areas.Admin.Services
{
    public interface IAdminService
    {
        Task<AdminHomeModel> GetStatsForHome();
    }
}