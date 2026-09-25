using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMANC_Inventory.Interfaces
{
    public interface IEmailService
    {
        Task<bool> EnviarCodigoConfirmacionAsync(string correoDestino, string codigo);
        Task<bool> EnviarCodigoRestablecimientoAsync(string correoDestino, string codigo);
    }
}