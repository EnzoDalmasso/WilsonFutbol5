using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.DTOs.AutenticacionAdmin;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Dominio.Entidades;
using Wilson_Futbol_5.Infraestructura.Persistencia;

namespace Wilson_Futbol_5.Aplicacion.Servicios;

// Contiene la logica de login, tokens temporales y cambio de contraseña del dueño.
public class ServicioAutenticacionAdmin : IServicioAutenticacionAdmin
{
    private const int CantidadBytesHash = 32;
    private const int CantidadBytesSalt = 16;
    private const int IteracionesHash = 100_000;
    private const int HorasDuracionSesion = 12;

    private readonly IConfiguration _configuracion;
    private readonly WilsonDbContext _contexto;

    public ServicioAutenticacionAdmin(WilsonDbContext contexto, IConfiguration configuracion)
    {
        _contexto = contexto;
        _configuracion = configuracion;
    }

    public async Task InicializarCredencialAdminAsync()
    {
        var yaExisteCredencial = await _contexto.CredencialesAdmin.AnyAsync();

        if (yaExisteCredencial)
        {
            return;
        }

        var claveInicial = _configuracion["SeguridadAdmin:ClaveInicial"];

        if (string.IsNullOrWhiteSpace(claveInicial))
        {
            throw new InvalidOperationException(
                "Falta configurar SeguridadAdmin:ClaveInicial para crear el primer acceso del dueño.");
        }

        var salt = GenerarSalt();

        _contexto.CredencialesAdmin.Add(new CredencialAdmin
        {
            HashClave = HashearClave(claveInicial, salt),
            SaltClave = salt,
            FechaActualizacion = DateTime.UtcNow
        });

        await _contexto.SaveChangesAsync();
    }

    public async Task<LoginAdminRespuestaDto> IniciarSesionAsync(LoginAdminDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Clave))
        {
            throw new InvalidOperationException("Tenes que ingresar la contraseña.");
        }

        var credencial = await ObtenerCredencialAdminAsync();

        if (!ClaveValida(dto.Clave, credencial))
        {
            throw new InvalidOperationException("Contraseña incorrecta.");
        }

        await DesactivarSesionesVencidasAsync();

        var token = GenerarToken();
        var fechaAhora = DateTime.UtcNow;
        var fechaExpiracion = fechaAhora.AddHours(HorasDuracionSesion);

        _contexto.SesionesAdmin.Add(new SesionAdmin
        {
            HashToken = HashearToken(token),
            FechaCreacion = fechaAhora,
            FechaExpiracion = fechaExpiracion,
            Activa = true
        });

        await _contexto.SaveChangesAsync();

        return new LoginAdminRespuestaDto
        {
            Token = token,
            FechaExpiracion = fechaExpiracion
        };
    }

    public async Task CambiarClaveAsync(CambiarClaveAdminDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClaveActual))
        {
            throw new InvalidOperationException("Tenes que ingresar la contraseña actual.");
        }

        if (string.IsNullOrWhiteSpace(dto.ClaveNueva) || dto.ClaveNueva.Length < 8)
        {
            throw new InvalidOperationException("La contraseña nueva debe tener al menos 8 caracteres.");
        }

        var credencial = await ObtenerCredencialAdminAsync();

        if (!ClaveValida(dto.ClaveActual, credencial))
        {
            throw new InvalidOperationException("La contraseña actual no es correcta.");
        }

        var saltNuevo = GenerarSalt();

        credencial.HashClave = HashearClave(dto.ClaveNueva, saltNuevo);
        credencial.SaltClave = saltNuevo;
        credencial.FechaActualizacion = DateTime.UtcNow;

        // Al cambiar contraseña cerramos todas las sesiones anteriores.
        // Asi cualquier token viejo deja de servir.
        await _contexto.SesionesAdmin
            .Where(sesion => sesion.Activa)
            .ExecuteUpdateAsync(setters => setters.SetProperty(sesion => sesion.Activa, false));

        await _contexto.SaveChangesAsync();
    }

    public async Task ResetearClaveConSoporteAsync(ResetearClaveAdminSoporteDto dto)
    {
        var claveSoporteConfigurada = _configuracion["SeguridadAdmin:ClaveSoporte"];

        if (string.IsNullOrWhiteSpace(claveSoporteConfigurada))
        {
            throw new InvalidOperationException("La clave de soporte no esta configurada.");
        }

        if (dto.ClaveSoporte != claveSoporteConfigurada)
        {
            throw new InvalidOperationException("La clave de soporte no es correcta.");
        }

        if (string.IsNullOrWhiteSpace(dto.ClaveNueva) || dto.ClaveNueva.Length < 8)
        {
            throw new InvalidOperationException("La contraseña nueva debe tener al menos 8 caracteres.");
        }

        var credencial = await ObtenerCredencialAdminAsync();
        var saltNuevo = GenerarSalt();

        // Reemplazamos la contraseña del dueño sin necesitar saber la contraseña anterior.
        credencial.HashClave = HashearClave(dto.ClaveNueva, saltNuevo);
        credencial.SaltClave = saltNuevo;
        credencial.FechaActualizacion = DateTime.UtcNow;

        // Cerramos todas las sesiones abiertas para obligar a entrar con la contraseña nueva.
        await _contexto.SesionesAdmin
            .Where(sesion => sesion.Activa)
            .ExecuteUpdateAsync(setters => setters.SetProperty(sesion => sesion.Activa, false));

        await _contexto.SaveChangesAsync();
    }

    public async Task<bool> ValidarTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        await DesactivarSesionesVencidasAsync();

        var hashToken = HashearToken(token);
        var fechaAhora = DateTime.UtcNow;

        return await _contexto.SesionesAdmin.AnyAsync(sesion =>
            sesion.HashToken == hashToken &&
            sesion.Activa &&
            sesion.FechaExpiracion > fechaAhora);
    }

    private async Task<CredencialAdmin> ObtenerCredencialAdminAsync()
    {
        return await _contexto.CredencialesAdmin.FirstAsync();
    }

    private async Task DesactivarSesionesVencidasAsync()
    {
        var fechaAhora = DateTime.UtcNow;

        await _contexto.SesionesAdmin
            .Where(sesion => sesion.Activa && sesion.FechaExpiracion <= fechaAhora)
            .ExecuteUpdateAsync(setters => setters.SetProperty(sesion => sesion.Activa, false));
    }

    private static bool ClaveValida(string clave, CredencialAdmin credencial)
    {
        var hashClaveIngresada = HashearClave(clave, credencial.SaltClave);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(hashClaveIngresada),
            Convert.FromBase64String(credencial.HashClave));
    }

    private static string GenerarSalt()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(CantidadBytesSalt));
    }

    private static string GenerarToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }

    private static string HashearClave(string clave, string saltBase64)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            clave,
            salt,
            IteracionesHash,
            HashAlgorithmName.SHA256,
            CantidadBytesHash);

        return Convert.ToBase64String(hash);
    }

    private static string HashearToken(string token)
    {
        var bytesToken = Encoding.UTF8.GetBytes(token);

        return Convert.ToHexString(SHA256.HashData(bytesToken));
    }
}
