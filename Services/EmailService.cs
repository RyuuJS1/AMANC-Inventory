using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AMANC_Inventory.Config;
using AMANC_Inventory.Interfaces;

namespace AMANC_Inventory.Services
{
    public class EmailService : IEmailService
    {
        public async Task<bool> EnviarCodigoConfirmacionAsync(string correoDestino, string codigo)
        {
            string titulo = "Confirmación de Cuenta";
            string descripcion = "Ingresa el siguiente código de verificación en el sistema para completar tu registro:";
            string notaFooter = "Este código es de un solo uso. Si no solicitaste este registro, puedes ignorar este mensaje de forma segura.";

            string htmlBody = GenerarPlantillaHtml(titulo, descripcion, codigo, notaFooter);
            return await EnviarCorreoAsync(correoDestino, "Código de Confirmación de un solo uso para Registro de Cuenta", htmlBody);
        }

        public async Task<bool> EnviarCodigoRestablecimientoAsync(string correoDestino, string codigo)
        {
            string titulo = "Restablecer Contraseña";
            string descripcion = "Has solicitado cambiar tu contraseña. Ingresa el siguiente código de verificación en el sistema:";
            string notaFooter = "Si no solicitaste este cambio, ignora este mensaje y tu contraseña permanecerá sin cambios.";

            string htmlBody = GenerarPlantillaHtml(titulo, descripcion, codigo, notaFooter);
            return await EnviarCorreoAsync(correoDestino, "Código de Verificación para Restablecer Contraseña", htmlBody);
        }

        private async Task<bool> EnviarCorreoAsync(string correoDestino, string asunto, string cuerpoHtml)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(EmailConfig.RemitenteEmail, EmailConfig.NombreRemitente),
                    Subject = asunto,
                    Body = cuerpoHtml,
                    IsBodyHtml = true
                };

                mail.To.Add(correoDestino);

                using (var smtp = new SmtpClient(EmailConfig.SmtpServer, EmailConfig.SmtpPort))
                {
                    smtp.Credentials = new NetworkCredential(EmailConfig.RemitenteEmail, EmailConfig.RemitentePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GenerarPlantillaHtml(string titulo, string descripcion, string codigo, string notaFooter)
        {
            return $@"
            <div style=""background-color: #F4F6F9; padding: 40px 10px; font-family: Arial, sans-serif;"">
                <div style=""max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.08); border: 1px solid #E2E8F0;"">
                    <div style=""background-color: #183059; padding: 25px; text-align: center;"">
                        <h1 style=""color: #ffffff; margin: 0; font-size: 20px; letter-spacing: 2px; font-weight: bold;"">AMANC INVENTORY</h1>
                    </div>
                    <div style=""padding: 30px; text-align: center; color: #334155;"">
                        <h2 style=""color: #183059; margin-top: 0; font-size: 18px;"">{titulo}</h2>
                        <p style=""font-size: 14px; color: #64748B; line-height: 1.5; margin-bottom: 25px;"">
                            {descripcion}
                        </p>
                        <div style=""background-color: #F0F4F8; border: 2px dashed #183059; border-radius: 6px; padding: 15px; display: inline-block; margin-bottom: 25px; width: 80%;"">
                            <span style=""font-family: 'Courier New', monospace; font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #183059;"">{codigo}</span>
                        </div>
                        <p style=""font-size: 12px; color: #94A3B8; margin-bottom: 0;"">
                            {notaFooter}
                        </p>
                    </div>
                    <div style=""background-color: #F8FAFC; padding: 15px; text-align: center; border-top: 1px solid #E2E8F0;"">
                        <p style=""margin: 0; font-size: 11px; color: #94A3B8;"">
                            © AMANC — Sistema de Control de Inventarios
                        </p>
                    </div>
                </div>
            </div>";
        }
    }
}