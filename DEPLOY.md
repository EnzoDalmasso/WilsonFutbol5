# Deploy Wilson Futbol 5

Arquitectura recomendada para la primera version productiva:

- Frontend: Vercel.
- Backend: hosting con Docker, por ejemplo Render.
- Base de datos: SQL Server en la nube.

## 1. Base de datos SQL Server

Crear una base SQL Server accesible por internet.

Datos que necesitamos guardar:

```txt
Servidor
Base de datos
Usuario
Password
Connection string
```

La variable que va a usar el backend es:

```txt
ConnectionStrings__WilsonDb
```

Ejemplo de formato:

```txt
Server=SERVIDOR;Database=BASE;User Id=USUARIO;Password=PASSWORD;TrustServerCertificate=True;
```

## 2. Backend con Docker

El backend se publica usando el `Dockerfile` de la raiz.

Variables de entorno necesarias:

```txt
ConnectionStrings__WilsonDb=connection-string-sql-server
SeguridadAdmin__ClaveInicial=clave-inicial-admin
SeguridadAdmin__ClaveSoporte=clave-soporte-segura
Cors__OrigenesPermitidos__0=https://url-del-frontend-vercel
ASPNETCORE_ENVIRONMENT=Production
```

Notas:

- `SeguridadAdmin__ClaveInicial` crea la primera contraseña solo si la base no tiene credencial admin.
- `SeguridadAdmin__ClaveSoporte` sirve para resetear la contraseña del dueño desde soporte.
- `Cors__OrigenesPermitidos__0` debe ser exactamente la URL del frontend publicado.

## 3. Migraciones en produccion

Cuando la base productiva ya exista, hay que aplicar migraciones.

Desde una terminal local, usando la connection string productiva como variable:

```powershell
$env:ConnectionStrings__WilsonDb="connection-string-sql-server"
dotnet ef database update --project "Wilson Futbol 5\Wilson Futbol 5.csproj" --startup-project "Wilson Futbol 5\Wilson Futbol 5.csproj"
```

Despues de aplicar migraciones, limpiar la variable local si corresponde:

```powershell
Remove-Item Env:\ConnectionStrings__WilsonDb
```

## 4. Frontend en Vercel

Configurar el proyecto Vercel apuntando a la carpeta:

```txt
frontend
```

Variable de entorno:

```txt
VITE_API_URL=https://url-del-backend/api
```

Comando de build:

```txt
npm run build
```

Carpeta de salida:

```txt
dist
```

## 5. Prueba final

Probar en URL real:

- Consultar disponibilidad como cliente.
- Crear reserva.
- Entrar a `/admin`.
- Ver reserva pendiente.
- Usar boton de WhatsApp manual.
- Confirmar reserva.
- Rechazar otra reserva.
- Crear turno fijo.
- Crear feriado o vacaciones.
- Modificar precio, seña, alias y mensaje de pago.
- Cambiar contraseña admin.
- Resetear contraseña con clave de soporte desde `.http`.

## 6. Orden recomendado

1. Crear base SQL Server en la nube.
2. Aplicar migraciones.
3. Publicar backend Docker.
4. Configurar variables del backend.
5. Publicar frontend en Vercel.
6. Configurar `VITE_API_URL`.
7. Configurar CORS del backend con la URL de Vercel.
8. Probar flujo completo.
