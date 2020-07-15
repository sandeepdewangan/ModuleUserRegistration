﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleUserRegistration.Models
{
    public interface IDepartmentRepository
    {
        IEnumerable<Department> GetAll();
    }
}
