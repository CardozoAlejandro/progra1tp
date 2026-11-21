using Cine_TP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Data.Interfaces
{
    public interface ICatalogoRepository
    {
        Task<List<Genero>> GetAllGenresAsync();
        Task<List<Clasificacion>> GetAllClassificationsAsync();
    }
}
