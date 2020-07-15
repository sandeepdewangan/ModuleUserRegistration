using ModuleUserRegistration.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleUserRegistration.Models
{
    public class DivisionRepository : IDivisionRepository
    {
        private readonly ApplicationDbContext _db;

        public DivisionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Division> GetAll()
        {
            var all_div = _db.Division.ToList();
            return all_div;
        }
    }
}
