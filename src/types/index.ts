export interface Cliente {
  id: number;
  descripcion: string;
}

export interface Sistema {
  id: number;
  descripcion: string;
}

export interface Usuario {
  id: string;
  nombre: string;
  apellidos: string;
  iniciales: string;
}

export interface Estado {
  id: number;
  descripcion: string;
}

export interface OrdenTrabajo {
  nroOrdenTrabajo: number;
  cliente: Cliente;
  sistema: Sistema;
  modulo: string;
  asunto: string;
  fechaSolicitud: Date;
  fechaFinalizacion?: Date;
  cantidadHorasEstimadas: number;
  cantidadHorasConsumidas: number;
  estado: Estado;
  porcentajeAvance: number;
  usuarioSolicitante: Usuario;
  usuarioResponsable: Usuario;
  descripcion: string;
  observaciones: string;
  prioridad: number;
  proyecto: string;
  dependeDe?: number;
  fechaVencimiento?: Date;
  solicitadoPor: string;
}

export interface AvanceTrabajo {
  id: number;
  fecha: Date;
  usuario: Usuario;
  horasAvance: number;
  descripcion: string;
}

export interface HistorialEstado {
  id: number;
  secuencia: number;
  estado: Estado;
  fechaAlta: Date;
  usuario: Usuario;
}

export interface ArchivoAdjunto {
  id: number;
  nombreArchivo: string;
  fechaSubida: Date;
  usuario: Usuario;
  tamanio: number;
}

export interface FormularioModificado {
  tipo: 'js' | 'html' | 'css' | 'tsx' | 'ts' | 'sql' | 'cs';
  ruta: string;
  descripcion?: string;
}

export interface ModificacionBaseDatos {
  tipo: 'tabla' | 'sp' | 'funcion' | 'vista';
  nombre: string;
  script: string;
  descripcion?: string;
}

export interface PuestaProduccion {
  id: number;
  fecha: Date;
  usuario: Usuario;
  formulariosModificados: FormularioModificado[];
  modificacionesBaseDatos: ModificacionBaseDatos[];
}
