using OtManager.Web.Models;
using OtManager.Web.Models.ViewModels;

namespace OtManager.Web.Services;

public sealed class WorkOrderService
{
    private readonly IReadOnlyList<Cliente> _clientes;
    private readonly IReadOnlyList<Sistema> _sistemas;
    private readonly IReadOnlyList<Usuario> _usuarios;
    private readonly IReadOnlyList<Estado> _estados;
    private readonly IReadOnlyList<OrdenTrabajo> _ordenesTrabajo;
    private readonly IReadOnlyDictionary<int, IReadOnlyList<AvanceTrabajo>> _avancesPorOrden;
    private readonly IReadOnlyDictionary<int, IReadOnlyList<HistorialEstado>> _historialPorOrden;
    private readonly IReadOnlyDictionary<int, IReadOnlyList<ArchivoAdjunto>> _archivosPorOrden;
    private readonly IReadOnlyDictionary<int, IReadOnlyList<PuestaProduccion>> _puestasPorOrden;

    public WorkOrderService()
    {
        _clientes = new List<Cliente>
        {
            new(1, "EFECTIVAMENTE"),
            new(2, "B1DEV"),
            new(3, "SUMARI GRUSE CODBAP EN"),
        };

        _sistemas = new List<Sistema>
        {
            new(1, "NAHUEL LVS AIVO"),
            new(2, "SAP"),
            new(3, "OILONE"),
            new(4, "GENERAL"),
        };

        _usuarios = new List<Usuario>
        {
            new("DDELCOLLAD", "Diego", "Del Collado", "DC"),
            new("SSORIA", "Santiago", "Soria", "SS"),
            new("JMARTINEZ", "Juan", "Martínez", "JM"),
            new("MLOPEZ", "María", "López", "ML"),
        };

        _estados = new List<Estado>
        {
            new(1, "PENDIENTE"),
            new(2, "EN DESARROLLO"),
            new(3, "EN PRODUCCION"),
            new(4, "TERMINADO"),
            new(5, "CANCELADO"),
        };

        _ordenesTrabajo = new List<OrdenTrabajo>
        {
            new(
                Numero: 18768,
                Cliente: _clientes[0],
                Sistema: _sistemas[0],
                Modulo: string.Empty,
                Asunto: "MVP parciales de nuevo flujo para AIVO",
                FechaSolicitud: new DateTime(2025, 5, 29),
                FechaFinalizacion: null,
                HorasEstimadas: 0,
                HorasConsumidas: 445.5,
                Estado: _estados[3],
                PorcentajeAvance: 60,
                UsuarioSolicitante: _usuarios[0],
                UsuarioResponsable: _usuarios[0],
                Descripcion: "Presupuesto 18.712",
                Observaciones: "Desarrollo de MVP para flujo de parciales con integración AIVO",
                Prioridad: 1,
                Proyecto: "60",
                DependeDe: 18712,
                FechaVencimiento: null,
                SolicitadoPor: "EFECTIVAMENTE"
            ),
            new(
                Numero: 18750,
                Cliente: _clientes[1],
                Sistema: _sistemas[1],
                Modulo: "EAMONE",
                Asunto: "Análisis y armado de funcionalidad",
                FechaSolicitud: new DateTime(2025, 5, 15),
                FechaFinalizacion: null,
                HorasEstimadas: 120,
                HorasConsumidas: 45,
                Estado: _estados[1],
                PorcentajeAvance: 35,
                UsuarioSolicitante: _usuarios[1],
                UsuarioResponsable: _usuarios[1],
                Descripcion: "Análisis de requerimientos para módulo EAMONE",
                Observaciones: string.Empty,
                Prioridad: 2,
                Proyecto: string.Empty,
                DependeDe: null,
                FechaVencimiento: null,
                SolicitadoPor: "B1DEV"
            ),
            new(
                Numero: 18745,
                Cliente: _clientes[1],
                Sistema: _sistemas[2],
                Modulo: "GENERAL",
                Asunto: "Importación de datos legacy",
                FechaSolicitud: new DateTime(2025, 5, 10),
                FechaFinalizacion: null,
                HorasEstimadas: 80,
                HorasConsumidas: 80,
                Estado: _estados[2],
                PorcentajeAvance: 100,
                UsuarioSolicitante: _usuarios[1],
                UsuarioResponsable: _usuarios[1],
                Descripcion: "Migración de datos desde sistema legacy",
                Observaciones: "Proceso completado exitosamente",
                Prioridad: 1,
                Proyecto: string.Empty,
                DependeDe: null,
                FechaVencimiento: null,
                SolicitadoPor: "B1DEV"
            ),
            new(
                Numero: 18740,
                Cliente: _clientes[1],
                Sistema: _sistemas[1],
                Modulo: string.Empty,
                Asunto: "Deploy en ambiente productivo",
                FechaSolicitud: new DateTime(2025, 5, 8),
                FechaFinalizacion: new DateTime(2025, 5, 20),
                HorasEstimadas: 40,
                HorasConsumidas: 38,
                Estado: _estados[3],
                PorcentajeAvance: 100,
                UsuarioSolicitante: _usuarios[1],
                UsuarioResponsable: _usuarios[1],
                Descripcion: "Despliegue final en producción",
                Observaciones: "Completado sin incidencias",
                Prioridad: 1,
                Proyecto: string.Empty,
                DependeDe: null,
                FechaVencimiento: null,
                SolicitadoPor: "B1DEV"
            ),
            new(
                Numero: 18735,
                Cliente: _clientes[1],
                Sistema: _sistemas[1],
                Modulo: "EAMONE",
                Asunto: "Sincronización de datos",
                FechaSolicitud: new DateTime(2025, 5, 5),
                FechaFinalizacion: null,
                HorasEstimadas: 60,
                HorasConsumidas: 42,
                Estado: _estados[1],
                PorcentajeAvance: 70,
                UsuarioSolicitante: _usuarios[1],
                UsuarioResponsable: _usuarios[1],
                Descripcion: "Implementación de sincronización automática",
                Observaciones: string.Empty,
                Prioridad: 2,
                Proyecto: string.Empty,
                DependeDe: null,
                FechaVencimiento: null,
                SolicitadoPor: "B1DEV"
            ),
        };

        _avancesPorOrden = new Dictionary<int, IReadOnlyList<AvanceTrabajo>>
        {
            [18768] = new List<AvanceTrabajo>
            {
                new(1, new DateTime(2025, 5, 29), _usuarios[0], 120, "Análisis inicial de requerimientos y diseño de arquitectura"),
                new(2, new DateTime(2025, 6, 5), _usuarios[0], 180, "Desarrollo de componentes principales del flujo"),
                new(3, new DateTime(2025, 6, 15), _usuarios[0], 145.5, "Integración con API de AIVO y pruebas funcionales"),
            },
            [18750] = new List<AvanceTrabajo>
            {
                new(1, new DateTime(2025, 5, 16), _usuarios[1], 25, "Análisis de documentación técnica"),
                new(2, new DateTime(2025, 5, 20), _usuarios[1], 20, "Prototipado de solución"),
            },
        };

        _historialPorOrden = new Dictionary<int, IReadOnlyList<HistorialEstado>>
        {
            [18768] = new List<HistorialEstado>
            {
                new(1, 1, _estados[0], new DateTime(2025, 5, 29), _usuarios[0]),
                new(2, 2, _estados[1], new DateTime(2025, 5, 30), _usuarios[0]),
                new(3, 3, _estados[2], new DateTime(2025, 6, 10), _usuarios[0]),
                new(4, 4, _estados[3], new DateTime(2025, 6, 28), _usuarios[0]),
            },
            [18750] = new List<HistorialEstado>
            {
                new(5, 1, _estados[0], new DateTime(2025, 5, 15), _usuarios[1]),
                new(6, 2, _estados[1], new DateTime(2025, 5, 18), _usuarios[1]),
            },
        };

        _archivosPorOrden = new Dictionary<int, IReadOnlyList<ArchivoAdjunto>>
        {
            [18768] = new List<ArchivoAdjunto>
            {
                new(1, "AnalisisFuncional.pdf", new DateTime(2025, 5, 29), _usuarios[0], 2_560_000),
                new(2, "DiagramaArquitectura.png", new DateTime(2025, 6, 1), _usuarios[0], 980_000),
                new(3, "CasosPrueba.xlsx", new DateTime(2025, 6, 10), _usuarios[0], 1_250_000),
            },
            [18750] = new List<ArchivoAdjunto>
            {
                new(4, "Requerimientos.docx", new DateTime(2025, 5, 16), _usuarios[1], 840_000),
            },
        };

        _puestasPorOrden = new Dictionary<int, IReadOnlyList<PuestaProduccion>>
        {
            [18768] = new List<PuestaProduccion>
            {
                new(
                    Id: 1,
                    Fecha: new DateTime(2025, 6, 29),
                    Usuario: _usuarios[0],
                    FormulariosModificados: new List<FormularioModificado>
                    {
                        new("tsx", "src/components/WorkOrderDetail.tsx", "Actualización de layout principal"),
                        new("ts", "src/services/workOrders.ts", "Implementación de servicio de integración"),
                        new("sql", "database/scripts/UpdateOrders.sql", "Migración de datos a nuevo esquema"),
                    },
                    ModificacionesBaseDatos: new List<ModificacionBaseDatos>
                    {
                        new("tabla", "WorkOrders", "ALTER TABLE WorkOrders ADD COLUMN IntegrationStatus INT NOT NULL DEFAULT 0;"),
                        new("sp", "sp_UpdateWorkOrderStatus", "CREATE OR ALTER PROCEDURE sp_UpdateWorkOrderStatus @OrderId INT AS BEGIN SET NOCOUNT ON; UPDATE WorkOrders SET StatusId = 4 WHERE Id = @OrderId; END"),
                    }
                )
            },
            [18750] = new List<PuestaProduccion>
            {
                new(
                    Id: 2,
                    Fecha: new DateTime(2025, 5, 22),
                    Usuario: _usuarios[1],
                    FormulariosModificados: new List<FormularioModificado>
                    {
                        new("tsx", "src/components/WorkOrdersList.tsx", "Incorporación de filtros avanzados"),
                        new("css", "src/styles/orders.css", "Ajustes visuales en tarjetas"),
                    },
                    ModificacionesBaseDatos: new List<ModificacionBaseDatos>
                    {
                        new("vista", "vw_WorkOrderSummary", "CREATE VIEW vw_WorkOrderSummary AS SELECT * FROM WorkOrders;"),
                    }
                )
            },
        };
    }

    public Usuario CurrentUser => _usuarios[0];

    public IReadOnlyList<Cliente> GetClientes() => _clientes;

    public IReadOnlyList<Sistema> GetSistemas() => _sistemas;

    public IReadOnlyList<Usuario> GetUsuarios() => _usuarios;

    public IReadOnlyList<Estado> GetEstados() => _estados;

    public IReadOnlyList<OrdenTrabajo> GetWorkOrders() => _ordenesTrabajo;

    public OrdenTrabajo? GetWorkOrder(int numero) => _ordenesTrabajo.FirstOrDefault(o => o.Numero == numero);

    public IReadOnlyList<AvanceTrabajo> GetAvances(int numero) =>
        _avancesPorOrden.TryGetValue(numero, out var avances) ? avances : Array.Empty<AvanceTrabajo>();

    public IReadOnlyList<HistorialEstado> GetHistorial(int numero) =>
        _historialPorOrden.TryGetValue(numero, out var historial) ? historial : Array.Empty<HistorialEstado>();

    public IReadOnlyList<ArchivoAdjunto> GetArchivos(int numero) =>
        _archivosPorOrden.TryGetValue(numero, out var archivos) ? archivos : Array.Empty<ArchivoAdjunto>();

    public IReadOnlyList<PuestaProduccion> GetPuestas(int numero) =>
        _puestasPorOrden.TryGetValue(numero, out var puestas) ? puestas : Array.Empty<PuestaProduccion>();

    public IReadOnlyList<OrdenTrabajo> GetRecentWorkOrders(int take = 10) => _ordenesTrabajo
        .OrderByDescending(o => o.FechaSolicitud)
        .Take(take)
        .ToList();

    public IReadOnlyList<OrdenTrabajo> ApplyFilters(WorkOrderFiltersViewModel filters)
    {
        IEnumerable<OrdenTrabajo> query = _ordenesTrabajo;

        if (!string.IsNullOrWhiteSpace(filters.NumeroOrden))
        {
            var numeroTexto = filters.NumeroOrden!.Trim();
            query = query.Where(o => o.Numero.ToString().Contains(numeroTexto, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.ClienteId) && filters.ClienteId is not "all")
        {
            if (int.TryParse(filters.ClienteId, out var clienteId))
            {
                query = query.Where(o => o.Cliente.Id == clienteId);
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.SistemaId) && filters.SistemaId is not "all")
        {
            if (int.TryParse(filters.SistemaId, out var sistemaId))
            {
                query = query.Where(o => o.Sistema.Id == sistemaId);
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.EstadoId) && filters.EstadoId is not "all")
        {
            if (int.TryParse(filters.EstadoId, out var estadoId))
            {
                query = query.Where(o => o.Estado.Id == estadoId);
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.Asunto))
        {
            query = query.Where(o => o.Asunto.Contains(filters.Asunto!, StringComparison.OrdinalIgnoreCase));
        }

        if (filters.FechaDesde.HasValue)
        {
            query = query.Where(o => o.FechaSolicitud >= filters.FechaDesde.Value);
        }

        if (filters.FechaHasta.HasValue)
        {
            query = query.Where(o => o.FechaSolicitud <= filters.FechaHasta.Value);
        }

        return query
            .OrderByDescending(o => o.FechaSolicitud)
            .ToList();
    }
}
