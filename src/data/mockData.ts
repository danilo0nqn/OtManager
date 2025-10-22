import { Cliente, Sistema, Usuario, Estado, OrdenTrabajo, AvanceTrabajo, HistorialEstado, ArchivoAdjunto, PuestaProduccion } from '../types';

export const clientes: Cliente[] = [
  { id: 1, descripcion: 'EFECTIVAMENTE' },
  { id: 2, descripcion: 'B1DEV' },
  { id: 3, descripcion: 'SUMARI GRUSE CODBAP EN' },
];

export const sistemas: Sistema[] = [
  { id: 1, descripcion: 'NAHUEL LVS AIVO' },
  { id: 2, descripcion: 'SAP' },
  { id: 3, descripcion: 'OILONE' },
  { id: 4, descripcion: 'GENERAL' },
];

export const usuarios: Usuario[] = [
  { id: 'DDELCOLLAD', nombre: 'Diego', apellidos: 'Del Collado', iniciales: 'DC' },
  { id: 'SSORIA', nombre: 'Santiago', apellidos: 'Soria', iniciales: 'SS' },
  { id: 'JMARTINEZ', nombre: 'Juan', apellidos: 'Martínez', iniciales: 'JM' },
  { id: 'MLOPEZ', nombre: 'María', apellidos: 'López', iniciales: 'ML' },
];

export const estados: Estado[] = [
  { id: 1, descripcion: 'PENDIENTE' },
  { id: 2, descripcion: 'EN DESARROLLO' },
  { id: 3, descripcion: 'EN PRODUCCION' },
  { id: 4, descripcion: 'TERMINADO' },
  { id: 5, descripcion: 'CANCELADO' },
];

export const ordenesTrabajo: OrdenTrabajo[] = [
  {
    nroOrdenTrabajo: 18768,
    cliente: clientes[0],
    sistema: sistemas[0],
    modulo: '',
    asunto: 'MVP parciales de nuevo flujo para AIVO',
    fechaSolicitud: new Date('2025-05-29'),
    fechaFinalizacion: undefined,
    cantidadHorasEstimadas: 0,
    cantidadHorasConsumidas: 445.5,
    estado: estados[3],
    porcentajeAvance: 60,
    usuarioSolicitante: usuarios[0],
    usuarioResponsable: usuarios[0],
    descripcion: 'Presupuesto 18.712',
    observaciones: 'Desarrollo de MVP para flujo de parciales con integración AIVO',
    prioridad: 1,
    proyecto: '60',
    dependeDe: 18712,
    solicitadoPor: 'EFECTIVAMENTE',
  },
  {
    nroOrdenTrabajo: 18750,
    cliente: clientes[1],
    sistema: sistemas[1],
    modulo: 'EAMONE',
    asunto: 'Análisis y armado de funcionalidad',
    fechaSolicitud: new Date('2025-05-15'),
    fechaFinalizacion: undefined,
    cantidadHorasEstimadas: 120,
    cantidadHorasConsumidas: 45,
    estado: estados[1],
    porcentajeAvance: 35,
    usuarioSolicitante: usuarios[1],
    usuarioResponsable: usuarios[1],
    descripcion: 'Análisis de requerimientos para módulo EAMONE',
    observaciones: '',
    prioridad: 2,
    proyecto: '',
    solicitadoPor: 'B1DEV',
  },
  {
    nroOrdenTrabajo: 18745,
    cliente: clientes[1],
    sistema: sistemas[2],
    modulo: 'GENERAL',
    asunto: 'Importación de datos legacy',
    fechaSolicitud: new Date('2025-05-10'),
    fechaFinalizacion: undefined,
    cantidadHorasEstimadas: 80,
    cantidadHorasConsumidas: 80,
    estado: estados[2],
    porcentajeAvance: 100,
    usuarioSolicitante: usuarios[1],
    usuarioResponsable: usuarios[1],
    descripcion: 'Migración de datos desde sistema legacy',
    observaciones: 'Proceso completado exitosamente',
    prioridad: 1,
    proyecto: '',
    solicitadoPor: 'B1DEV',
  },
  {
    nroOrdenTrabajo: 18740,
    cliente: clientes[1],
    sistema: sistemas[1],
    modulo: '',
    asunto: 'Deploy en ambiente productivo',
    fechaSolicitud: new Date('2025-05-08'),
    fechaFinalizacion: new Date('2025-05-20'),
    cantidadHorasEstimadas: 40,
    cantidadHorasConsumidas: 38,
    estado: estados[3],
    porcentajeAvance: 100,
    usuarioSolicitante: usuarios[1],
    usuarioResponsable: usuarios[1],
    descripcion: 'Despliegue final en producción',
    observaciones: 'Completado sin incidencias',
    prioridad: 1,
    proyecto: '',
    solicitadoPor: 'B1DEV',
  },
  {
    nroOrdenTrabajo: 18735,
    cliente: clientes[1],
    sistema: sistemas[1],
    modulo: 'EAMONE',
    asunto: 'Sincronización de datos',
    fechaSolicitud: new Date('2025-05-05'),
    fechaFinalizacion: undefined,
    cantidadHorasEstimadas: 60,
    cantidadHorasConsumidas: 42,
    estado: estados[1],
    porcentajeAvance: 70,
    usuarioSolicitante: usuarios[1],
    usuarioResponsable: usuarios[1],
    descripcion: 'Implementación de sincronización automática',
    observaciones: '',
    prioridad: 2,
    proyecto: '',
    solicitadoPor: 'B1DEV',
  },
];

export const avancesTrabajo: { [key: number]: AvanceTrabajo[] } = {
  18768: [
    {
      id: 1,
      fecha: new Date('2025-05-29'),
      usuario: usuarios[0],
      horasAvance: 120,
      descripcion: 'Análisis inicial de requerimientos y diseño de arquitectura',
    },
    {
      id: 2,
      fecha: new Date('2025-06-05'),
      usuario: usuarios[0],
      horasAvance: 180,
      descripcion: 'Desarrollo de componentes principales del flujo',
    },
    {
      id: 3,
      fecha: new Date('2025-06-15'),
      usuario: usuarios[0],
      horasAvance: 145.5,
      descripcion: 'Integración con API de AIVO y pruebas funcionales',
    },
  ],
  18750: [
    {
      id: 1,
      fecha: new Date('2025-05-16'),
      usuario: usuarios[1],
      horasAvance: 25,
      descripcion: 'Análisis de documentación técnica',
    },
    {
      id: 2,
      fecha: new Date('2025-05-20'),
      usuario: usuarios[1],
      horasAvance: 20,
      descripcion: 'Prototipado de solución',
    },
  ],
};

export const historialEstados: { [key: number]: HistorialEstado[] } = {
  18768: [
    {
      id: 1,
      secuencia: 1,
      estado: estados[0],
      fechaAlta: new Date('2025-05-29'),
      usuario: usuarios[0],
    },
    {
      id: 2,
      secuencia: 2,
      estado: estados[1],
      fechaAlta: new Date('2025-05-30'),
      usuario: usuarios[0],
    },
    {
      id: 3,
      secuencia: 3,
      estado: estados[2],
      fechaAlta: new Date('2025-06-10'),
      usuario: usuarios[0],
    },
    {
      id: 4,
      secuencia: 4,
      estado: estados[3],
      fechaAlta: new Date('2025-06-20'),
      usuario: usuarios[0],
    },
  ],
};

export const archivosAdjuntos: { [key: number]: ArchivoAdjunto[] } = {
  18768: [
    {
      id: 1,
      nombreArchivo: 'especificaciones_tecnicas.pdf',
      fechaSubida: new Date('2025-05-29'),
      usuario: usuarios[0],
      tamanio: 2458000,
    },
    {
      id: 2,
      nombreArchivo: 'diagrama_flujo.png',
      fechaSubida: new Date('2025-06-01'),
      usuario: usuarios[0],
      tamanio: 845000,
    },
    {
      id: 3,
      nombreArchivo: 'manual_usuario.docx',
      fechaSubida: new Date('2025-06-15'),
      usuario: usuarios[0],
      tamanio: 1250000,
    },
  ],
};

export const puestasProduccion: { [key: number]: PuestaProduccion[] } = {
  18768: [
    {
      id: 1,
      fecha: new Date('2025-06-20'),
      usuario: usuarios[0],
      formulariosModificados: [
        {
          tipo: 'js',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql/Scripts',
        },
        {
          tipo: 'sql',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql',
          descripcion: '(Rev 3218)',
        },
        {
          tipo: 'js',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql/Scripts',
        },
        {
          tipo: 'js',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql/Scripts/INSERT',
        },
        {
          tipo: 'sql',
          ruta: 'Cuentas_Bancarias_Validaciones.sql',
        },
        {
          tipo: 'js',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql/Scripts/Tables ConversacionesAivo',
        },
        {
          tipo: 'sql',
          ruta: 'Estados_ConversacionesAivo.sql',
        },
        {
          tipo: 'sql',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql/sp_WSAIVO_ValidarCodigoOTP.sql',
        },
        {
          tipo: 'sql',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql/sp_WSAivo_ActualizarConversacion.sql',
        },
        {
          tipo: 'sql',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql/sp_WSAivo_EjecutarTransferencia.sql',
        },
        {
          tipo: 'sql',
          ruta: 'https://192.168.110.5/svn/ITSur/WS_AIVO/branches/OT_18768/TSql/sp_WSAivo_EnviarResumenOperacion.sql',
        },
      ],
      modificacionesBaseDatos: [
        {
          tipo: 'tabla',
          nombre: 'ConversacionesAivo',
          script: 'CREATE TABLE ConversacionesAivo (id INT, conversacionId VARCHAR(100), estado INT)',
          descripcion: 'Tabla para almacenar conversaciones de AIVO',
        },
        {
          tipo: 'sp',
          nombre: 'sp_WSAIVO_ValidarCodigoOTP',
          script: 'CREATE PROCEDURE sp_WSAIVO_ValidarCodigoOTP @codigo VARCHAR(10) AS BEGIN ... END',
          descripcion: 'Stored procedure para validar códigos OTP',
        },
        {
          tipo: 'sp',
          nombre: 'sp_WSAivo_ActualizarConversacion',
          script: 'CREATE PROCEDURE sp_WSAivo_ActualizarConversacion @conversacionId VARCHAR(100) AS BEGIN ... END',
        },
        {
          tipo: 'tabla',
          nombre: 'Estados_ConversacionesAivo',
          script: 'CREATE TABLE Estados_ConversacionesAivo (id INT, descripcion VARCHAR(50))',
        },
        {
          tipo: 'sp',
          nombre: 'sp_WSAivo_EjecutarTransferencia',
          script: 'CREATE PROCEDURE sp_WSAivo_EjecutarTransferencia AS BEGIN ... END',
        },
      ],
    },
  ],
};

export const currentUser: Usuario = usuarios[0];
