using DocumentApprovalSystem.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace DocumentApprovalSystem.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(IOptionsSnapshot<EmailSettings> emailSettings, ILogger<EmailNotificationService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> IsConfiguredAsync()
        {
            return !string.IsNullOrEmpty(_emailSettings.Host) &&
                   !string.IsNullOrEmpty(_emailSettings.Username) &&
                   !string.IsNullOrEmpty(_emailSettings.FromEmail);
        }

        public async Task SendNewDocumentNotificationAsync(DocumentRequest request, List<User> approvers)
        {
            if (!await IsConfiguredAsync())
            {
                _logger.LogWarning("Email not configured, skipping notification");
                return;
            }

            foreach (var approver in approvers)
            {
                if (string.IsNullOrEmpty(approver.Email)) continue;

                var subject = $"Nuevo documento para aprobar: {request.Title}";
                var body = GetNewDocumentEmailBody(request, approver);

                await SendEmailAsync(approver.Email, subject, body);
            }
        }

        public async Task SendApprovalStatusNotificationAsync(DocumentRequest request, User requester)
        {
            if (!await IsConfiguredAsync() || string.IsNullOrEmpty(requester.Email))
            {
                _logger.LogWarning("Email not configured or requester has no email");
                return;
            }

            var subject = $"Resultado de aprobación: {request.Title}";
            var body = GetApprovalStatusEmailBody(request, requester);

            await SendEmailAsync(requester.Email, subject, body);
        }

        public async Task SendVoteReminderAsync(DocumentRequest request, User approver)
        {
            if (!await IsConfiguredAsync() || string.IsNullOrEmpty(approver.Email))
            {
                _logger.LogWarning("Email not configured or approver has no email");
                return;
            }

            var subject = $"Recordatorio: Documento pendiente de aprobación - {request.Title}";
            var body = GetVoteReminderEmailBody(request, approver);

            await SendEmailAsync(approver.Email, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                // Accept all SSL certificates (in case of self-signed, though Gmail is valid)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Timeout = 5000; // 5 seconds timeout to avoid hanging UI

                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent to {to}: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
            }
        }

        private string GetNewDocumentEmailBody(DocumentRequest request, User approver)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #0066cc;'>Nuevo Documento para Aprobar</h2>
                    <p>Hola <strong>{approver.FullName}</strong>,</p>
                    <p>Se ha creado un nuevo documento que requiere tu aprobación:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-left: 4px solid #0066cc; margin: 20px 0;'>
                        <p><strong>Título:</strong> {request.Title}</p>
                        <p><strong>Descripción:</strong> {request.Description}</p>
                        <p><strong>Solicitante:</strong> {request.RequestedByUser?.FullName}</p>
                        <p><strong>Fecha:</strong> {request.CreatedDate:dd/MM/yyyy HH:mm}</p>
                    </div>
                    <p>Por favor, revisa y vota en el sistema lo antes posible.</p>
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        Este es un mensaje automático del Sistema de Aprobación de Documentos.
                    </p>
                </body>
                </html>";
        }

        private string GetApprovalStatusEmailBody(DocumentRequest request, User requester)
        {
            var status = request.Status == "Approved" ? "APROBADO" : "RECHAZADO";
            var color = request.Status == "Approved" ? "#28a745" : "#dc3545";

            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: {color};'>Documento {status}</h2>
                    <p>Hola <strong>{requester.FullName}</strong>,</p>
                    <p>Tu solicitud de documento ha sido <strong>{status.ToLower()}</strong>:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-left: 4px solid {color}; margin: 20px 0;'>
                        <p><strong>Título:</strong> {request.Title}</p>
                        <p><strong>Estado:</strong> {status}</p>
                        <p><strong>Fecha de resultado:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                    </div>
                    <p>Puedes ver los detalles completos en el sistema.</p>
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        Este es un mensaje automático del Sistema de Aprobación de Documentos.
                    </p>
                </body>
                </html>";
        }

        private string GetVoteReminderEmailBody(DocumentRequest request, User approver)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #ff9800;'>Recordatorio: Documento Pendiente</h2>
                    <p>Hola <strong>{approver.FullName}</strong>,</p>
                    <p>Te recordamos que tienes un documento pendiente de votación:</p>
                    <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ff9800; margin: 20px 0;'>
                        <p><strong>Título:</strong> {request.Title}</p>
                        <p><strong>Solicitante:</strong> {request.RequestedByUser?.FullName}</p>
                        <p><strong>Fecha de creación:</strong> {request.CreatedDate:dd/MM/yyyy}</p>
                    </div>
                    <p>Por favor, revisa y emite tu voto cuando sea posible.</p>
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        Este es un mensaje automático del Sistema de Aprobación de Documentos.
                    </p>
                </body>
                </html>";
        }
    }
}
