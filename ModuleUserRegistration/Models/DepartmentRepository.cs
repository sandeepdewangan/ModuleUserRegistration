using ModuleUserRegistration.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleUserRegistration.Models
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _db;

        public DepartmentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Department> GetAll()
        {
            var all_dept = _db.Department.ToList();
            return all_dept;
        }
    }
}
