using Cine_TP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Data.Interfaces
{
    public interface IFormaPagoRepository
    {
        Task<List<FormaPago>> GetFormasPagoAsync();
    }
}
