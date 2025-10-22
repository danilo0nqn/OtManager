# OT Manager MVC

Migración completa de la interfaz **OT Manager** desde React + Vite hacia una arquitectura **ASP.NET Core MVC (.NET 8)** con Razor Views. El objetivo es preservar la experiencia visual original, la navegación entre vistas y las interacciones de gestión de órdenes de trabajo.

## 🚀 Características principales

- **Inicio de sesión temático** con selector claro/oscuro persistido en `localStorage`.
- **Layout responsive** con barra lateral colapsable, encabezado contextual y acciones rápidas.
- **Listado de órdenes** con filtros avanzados por número, cliente, sistema, estado, asunto y rangos de fechas.
- **Detalle enriquecido** de la orden: ficha de datos maestros, edición simulada, tabs para descripción, avances, puestas en producción, historial y archivos adjuntos.
- **Historial reciente** con métricas y progreso visual por orden.
- **Toasts informativos** y acciones demo para alta/edición/eliminación (sin persistencia al estar basados en datos mockeados).
- **Servicios en memoria** que reimplementan la estructura `mockData.ts` original, tipada mediante records C#.

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

> ℹ️ El login acepta cualquier combinación de usuario/contraseña, replicando el comportamiento mock original.

## 🛠️ Publicación

```bash
dotnet publish OtManager.Web/OtManager.Web.csproj -c Release -o build
```

Se generará una carpeta `build/` lista para desplegar en IIS, Azure App Service, contenedores o cualquier hosting compatible con ASP.NET Core.

## 📝 Notas técnicas

- Se emplea **Tailwind CDN** para disponer de las utilidades usadas por el diseño original sin proceso de build adicional.
- Los datos provienen de `WorkOrderService` que emula los `mockData` de React; puede sustituirse fácilmente por repositorios o `HttpClient` contra APIs reales.
- `Session` se utiliza para flag de autenticación simple y para recordar el usuario logueado.
- Los toasts, tabs y la lógica de tema se implementan con JavaScript liviano en `wwwroot/js/site.js`.
- El layout centraliza la barra lateral, encabezado, toasts y scripts compartidos.

## 🔄 Diferencias vs. SPA React

| React SPA | ASP.NET Core MVC |
|-----------|------------------|
| Estado global en componentes (`useState`, `useMemo`, `useEffect`) | Records y ViewModels tipados + filtrado server-side |
| Navegación `react-router-dom` | Controladores + `MapControllerRoute` con acciones específicas |
| Componentes UI (Radix, shadcn) | Vistas Razor con clases Tailwind y helpers HTML |
| Mock data en TypeScript | Servicio C# singleton con datos in-memory |
| Toaster Sonner | Toasts propios renderizados en Razor + JS |

## 🌱 Mejoras futuras sugeridas

- Integrar autenticación real (Identity o proveedor externo).
- Reemplazar datos mock por repositorios/servicios persistentes.
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
