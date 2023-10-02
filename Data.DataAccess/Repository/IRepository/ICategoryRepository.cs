﻿using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository:IRepository<CategoryModel> 
    {
        void Update(CategoryModel category);
     
    }
}
