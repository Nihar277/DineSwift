using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Model;

namespace Repository.Service
{
    public interface IAuthService
    {
        public Task<t_customer> Login(login_vm login_vm);

        public Task<int> checkEmailExists(string email);
        
    }
}