using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegistrarAsync(string nombreUsuario, string email, string password);
        Task<string?> LoginAsync(string nombreUsuario, string password);
    }
}
