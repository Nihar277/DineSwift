using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Model;

namespace Repository.Service
{
    public interface IRatingServices
    {
        public Task<bool> AddRating(t_rating rating);
        public Task<List<t_rating>> GetAllRating();
        public Task<bool> DeleteRating(int id);
        
    }
}