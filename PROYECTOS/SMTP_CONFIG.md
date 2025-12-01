# Instrucciones para Configurar SMTP

Para que el sistema envíe emails automáticos, necesitas configurar tus credenciales SMTP en `appsettings.json`:

## Opción 1: Gmail (Recomendado para pruebas)

1. **Activa la verificación en 2 pasos** en tu cuenta de Gmail
2. **Genera una Contraseña de Aplicación**:
   - Ve a: https://myaccount.google.com/apppasswords
   - Selecciona "Correo" y "Otro (nombre personalizado)"
   - Copia la contraseña generada (16 caracteres sin espacios)

3. **Edita `appsettings.json`**:
```json
"EmailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "tu-email@gmail.com",
  "Password": "xxxx xxxx xxxx xxxx",  // La contraseña de aplicación
  "FromEmail": "tu-email@gmail.com",
  "FromName": "Sistema de Aprobación",
  "EnableSsl": true
}
```

## Opción 2: Outlook/Hotmail

```json
"EmailSettings": {
  "Host": "smtp.office365.com",
  "Port": 587,
  "Username": "tu-email@outlook.com",
  "Password": "tu-contraseña",
  "FromEmail": "tu-email@outlook.com",
  "FromName": "Sistema de Aprobación",
  "EnableSsl": true
}
```

## Opción 3: SendGrid (Producción)

1. Crea cuenta en https://sendgrid.com
2. Genera API Key
3. Configura:
```json
"EmailSettings": {
  "Host": "smtp.sendgrid.net",
  "Port": 587,
  "Username": "apikey",
  "Password": "TU_API_KEY_AQUI",
  "FromEmail": "noreply@tudominio.com",
  "FromName": "Sistema de Aprobación",
  "EnableSsl": true
}
```

## Verificar Logs

Después de configurar, revisa los logs de la consola al aprobar un documento. Deberías ver:
```
Attempting to send email notification for request X. Status: Approved
Sending email to usuario@example.com
Email sent successfully
```

Si ves errores, revisa:
- ✅ Credenciales correctas
- ✅ Sin espacios extra
- ✅ Contraseña de aplicación (no la contraseña normal de Gmail)
