
using Cine_TP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Interfaces
{
    public interface ICatalogoService
    {
        Task<List<Genero>> ObtenerGenerosAsync();
        Task<List<Clasificacion>> ObtenerClasificacionesAsync();
    }
}
