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
    public class CatalogoService : ICatalogoService
    {
        private readonly ICatalogoRepository _catalogoRepository;
        public CatalogoService(ICatalogoRepository catalogoRepository)
        {
            _catalogoRepository = catalogoRepository;
        }

        public Task<List<Genero>> ObtenerGenerosAsync()
        {
            return _catalogoRepository.GetAllGenresAsync();
        }

        public Task<List<Clasificacion>> ObtenerClasificacionesAsync()
        {
            return _catalogoRepository.GetAllClassificationsAsync();
        }
    }
}
