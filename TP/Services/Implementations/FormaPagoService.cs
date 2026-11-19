using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Implementations
{
    public class FormaPagoService : IFormaPagoService
    {
        private readonly IFormaPagoRepository _formapagoRepository;
        public FormaPagoService(IFormaPagoRepository formaPagoRepository)
        {
            _formapagoRepository = formaPagoRepository;
        }

        public Task<List<FormaPago>> ObtenerFormasPagoAsync()
        {
            return _formapagoRepository.GetFormasPagoAsync();
        }
    }
}
