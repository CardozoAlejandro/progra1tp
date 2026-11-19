using Cine_TP.Data.Interfaces;
using Cine_TP.Domain;
using Cine_TP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cine_TP.Services.Implementations
{
    public class FuncionService : IFuncionService
    {
        private readonly IFuncionRepository _funcionRepository;
        public FuncionService(IFuncionRepository funcionRepository)
        {
            _funcionRepository = funcionRepository;
        }
        public Task<List<Funcion>> ObtenerFuncionesAsync(DateTime desde, DateTime hasta)
        {
            return _funcionRepository.GetFuncionesAsync(desde, hasta);
        }

        public Task<List<Funcion>> ObtenerFuncionesPorPeliculaAsync(int idPelicula, DateTime desde)
        {
            if(idPelicula <= 0)
            {
                return null;
            }
            return _funcionRepository.GetFuncionesPeliculasAsync(idPelicula, desde);
        }
    }
}
