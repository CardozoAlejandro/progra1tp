using Cine_TP.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Data.Interfaces
{
    public interface IFuncionRepository
    {
        Task<List<Funcion>> GetFuncionesAsync(DateTime desde, DateTime hasta);
        Task<Funcion?> GetFuncionByIdAsync(int id);
        Task<int> CountEntradasVendidasAsync(int idFuncion);
        Task<List<Funcion>> GetFuncionesPeliculasAsync(int idPelicula, DateTime desde);

    }
}
