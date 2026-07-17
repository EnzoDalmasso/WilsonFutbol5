# Wilson Futbol 5

Aplicacion web para gestionar reservas de cancha, panel del dueño, turnos fijos, feriados, vacaciones, precios y datos de transferencia.

## Estructura

- `Wilson Futbol 5/`: backend ASP.NET Core con Entity Framework Core.
- `frontend/`: frontend React con Vite.

## Desarrollo local

Backend:

```powershell
dotnet run --project "Wilson Futbol 5\Wilson Futbol 5.csproj"
```

Frontend:

```powershell
cd frontend
npm.cmd install
npm.cmd run dev
```

## Variables del backend

En local, las claves sensibles pueden guardarse con User Secrets:

```powershell
dotnet user-secrets set "SeguridadAdmin:ClaveInicial" "clave-inicial-admin" --project "Wilson Futbol 5\Wilson Futbol 5.csproj"
dotnet user-secrets set "SeguridadAdmin:ClaveSoporte" "clave-soporte-segura" --project "Wilson Futbol 5\Wilson Futbol 5.csproj"
```

En produccion, configurar estas variables en el hosting:

```txt
ConnectionStrings__WilsonDb=connection-string-produccion
SeguridadAdmin__ClaveInicial=clave-inicial-admin
SeguridadAdmin__ClaveSoporte=clave-soporte-segura
Cors__OrigenesPermitidos__0=https://url-del-frontend
```

Notas:

- `SeguridadAdmin__ClaveInicial` solo se usa si la base todavia no tiene credencial admin.
- `SeguridadAdmin__ClaveSoporte` permite resetear la contraseña del dueño desde soporte.
- `Cors__OrigenesPermitidos__0` debe coincidir con la URL real del frontend publicado.

## Variables del frontend

En `frontend/.env` para desarrollo local:

```env
VITE_API_URL=https://localhost:7094/api
```

En Vercel:

```env
VITE_API_URL=https://url-del-backend/api
```

## Base de datos

Aplicar migraciones:

```powershell
dotnet ef database update --project "Wilson Futbol 5\Wilson Futbol 5.csproj" --startup-project "Wilson Futbol 5\Wilson Futbol 5.csproj"
```

## Docker backend

El backend puede publicarse como contenedor usando el `Dockerfile` de la raiz.

Build local:

```powershell
docker build -t wilson-futbol5-api .
```

Run local:

```powershell
docker run --rm -p 8080:8080 `
  -e ConnectionStrings__WilsonDb="connection-string-produccion-o-prueba" `
  -e SeguridadAdmin__ClaveInicial="clave-inicial-admin" `
  -e SeguridadAdmin__ClaveSoporte="clave-soporte-segura" `
  -e Cors__OrigenesPermitidos__0="http://localhost:5173" `
  wilson-futbol5-api
```

En Render u otro hosting con Docker, configurar las mismas variables de entorno.

## Checklist antes de publicar

- Configurar `VITE_API_URL` en Vercel.
- Configurar `ConnectionStrings__WilsonDb` en el hosting del backend.
- Configurar `SeguridadAdmin__ClaveInicial`.
- Configurar `SeguridadAdmin__ClaveSoporte`.
- Configurar `Cors__OrigenesPermitidos__0` con la URL de Vercel.
- Ejecutar migraciones en la base de produccion.
- Probar reserva cliente.
- Probar login admin.
- Probar confirmar/rechazar reserva.
- Probar turnos fijos.
- Probar feriados/vacaciones.
