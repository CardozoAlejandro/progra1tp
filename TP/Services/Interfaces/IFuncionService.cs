using Cine_TP.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Interfaces
{
    public interface IFuncionService
    {
        Task<List<Funcion>> ObtenerFuncionesAsync(DateTime desde, DateTime hasta);
        Task<List<Funcion>> ObtenerFuncionesPorPeliculaAsync(int idPelicula, DateTime desde);
    }
}
