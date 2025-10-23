# OT Manager MVC

Migración completa de la interfaz **OT Manager** desde React + Vite hacia una arquitectura **ASP.NET Core MVC (.NET 8)** con Razor Views. El objetivo es preservar la experiencia visual original, la navegación entre vistas y las interacciones de gestión de órdenes de trabajo.

## 🚀 Características principales

- **Autenticación real** contra `https://OTManager.itsur.com.ar/api/` con persistencia del token JWT en sesión.
- **Layout responsive** con barra lateral colapsable, encabezado contextual y acciones rápidas.
- **Listado de órdenes** con filtros avanzados por número, cliente, sistema, estado, asunto y rangos de fechas consultados vía API.
- **Detalle enriquecido** de la orden: ficha de datos maestros, tabs para descripción, avances, puestas en producción, historial y archivos adjuntos obtenidos del servicio remoto.
- **Historial reciente** sincronizado con la API para mostrar el progreso de cada orden asignada al usuario autenticado.
- **Toasts informativos** para reforzar acciones clave manteniendo la experiencia visual del diseño original.

## 🗂️ Estructura del proyecto

```
OtManager.Web/
├── Controllers/
│   ├── AccountController.cs
│   ├── HomeController.cs
│   └── WorkOrdersController.cs
├── Models/
│   ├── ErrorViewModel.cs
│   ├── SessionKeys.cs
│   ├── WorkOrderModels.cs
│   └── ViewModels/
│       ├── LoginViewModel.cs
│       ├── RecentHistoryViewModel.cs
│       ├── WorkOrderDetailViewModel.cs
│       ├── WorkOrderFiltersViewModel.cs
│       ├── WorkOrderUpdateInputModel.cs
│       └── WorkOrdersListViewModel.cs
├── Services/
│   └── WorkOrderService.cs
├── Views/
│   ├── Account/Login.cshtml
│   ├── Shared/_Layout.cshtml
│   ├── Shared/Error.cshtml
│   └── WorkOrders/
│       ├── Details.cshtml
│       ├── History.cshtml
│       └── Index.cshtml
├── wwwroot/
│   ├── css/site.css
│   └── js/site.js
├── appsettings.json
├── Program.cs
└── OtManager.Web.csproj
```

## 🧰 Requisitos

- .NET SDK **8.0** o superior
- Opcional: Visual Studio 2022, Rider o VS Code con extensión C#

## 🔧 Configuración de API

1. Define la URL base de la API en `appsettings.json` y `appsettings.Development.json` dentro de la clave `ApiSettings:BaseUrl`.
2. Ejecuta el sitio y utiliza credenciales válidas del backend OT Manager para iniciar sesión.
3. El token JWT emitido se almacena en la sesión del servidor y se reenvía automáticamente en cada llamada mediante `HttpClientFactory`.

## ▶️ Ejecución

```bash
# Restaurar dependencias
dotnet restore OtManager.Web/OtManager.Web.csproj

# Compilar
dotnet build OtManager.Web/OtManager.Web.csproj

# Ejecutar (desde la raíz del repo)
dotnet run --project OtManager.Web/OtManager.Web.csproj
```

El sitio quedará disponible en `https://localhost:7155` o `http://localhost:5155` según `launchSettings.json`.

## 🛠️ Publicación

```bash
dotnet publish OtManager.Web/OtManager.Web.csproj -c Release -o build
```

Se generará una carpeta `build/` lista para desplegar en IIS, Azure App Service, contenedores o cualquier hosting compatible con ASP.NET Core.

## 📝 Notas técnicas

- Se emplea **Tailwind CDN** para disponer de las utilidades usadas por el diseño original sin proceso de build adicional.
- `AuthService` consume el endpoint `auth/login` y almacena el token JWT en sesión junto a los datos del usuario.
- `WorkOrderService` está construido sobre `HttpClientFactory`, mapea las respuestas JSON de la API y maneja errores devolviendo colecciones seguras para las vistas.
- `Session` mantiene el token y la información del usuario autenticado mientras dure la sesión del navegador.
- Los toasts, tabs y la lógica de tema se implementan con JavaScript liviano en `wwwroot/js/site.js`.
- El layout centraliza la barra lateral, encabezado, toasts y scripts compartidos.

## 🔄 Diferencias vs. SPA React

| React SPA | ASP.NET Core MVC |
|-----------|------------------|
| Estado global en componentes (`useState`, `useMemo`, `useEffect`) | Records y ViewModels tipados + filtrado server-side |
| Navegación `react-router-dom` | Controladores + `MapControllerRoute` con acciones específicas |
| Componentes UI (Radix, shadcn) | Vistas Razor con clases Tailwind y helpers HTML |
| Mock data en TypeScript | Servicios C# con HttpClient y token JWT |
| Toaster Sonner | Toasts propios renderizados en Razor + JS |

## 🌱 Mejoras futuras sugeridas

- Implementar refresh tokens o expiración automática para renovar la sesión.
- Añadir pruebas automatizadas (unitarias e integradas) que validen la integración con la API OT Manager.
- Internacionalización con recursos `.resx`.
- Componentizar vistas parciales (`_Sidebar`, `_Header`) y usar ViewComponents.
- Evolucionar hacia Razor Components/Blazor Server cuando se requiera interactividad en tiempo real.

## ✅ Plan de commits sugerido

1. `feat: create ASP.NET Core MVC base project`
2. `feat: migrate Home and Navbar components`
3. `feat: add routing and controllers`
4. `feat: implement HttpClient services`
5. `feat: finalize layout and styling`

> El repositorio ya incorpora todos los cambios necesarios en un único commit migratorio.

---

¡Listo! La aplicación está preparada para abrirse en Visual Studio o ejecutarse vía `dotnet run`, conservando la estética y funcionalidad del diseño original construido en React.
