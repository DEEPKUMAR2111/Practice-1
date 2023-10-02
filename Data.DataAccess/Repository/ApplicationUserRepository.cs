using Data.DataAccess.Data;
using Data.DataAccess.Repository.IRepository;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private CategoryDbContext _db;
        public ApplicationUserRepository(CategoryDbContext db) : base(db)
        {
            _db = db;
        }

    
    }
}
