import { useState } from 'react';
import { OrdenTrabajo, AvanceTrabajo } from '../types';
import { 
  avancesTrabajo, 
  historialEstados, 
  archivosAdjuntos, 
  puestasProduccion,
  clientes,
  sistemas,
  estados,
  usuarios,
  ordenesTrabajo,
  currentUser
} from '../data/mockData';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Textarea } from './ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from './ui/select';
import { Button } from './ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from './ui/tabs';
import { Badge } from './ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from './ui/dialog';
import { 
  FileText, 
  Search,
  Clock,
  Save,
  Download,
  Upload,
  FileIcon,
  History,
  Rocket,
  Code,
  Database,
  Edit,
  Trash2,
  Plus,
  X
} from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { toast } from 'sonner@2.0.3';

interface WorkOrderDetailProps {
  order?: OrdenTrabajo;
}

export function WorkOrderDetail({ order }: WorkOrderDetailProps) {
  const [searchNumber, setSearchNumber] = useState(order?.nroOrdenTrabajo.toString() || '');
  const [selectedOrder, setSelectedOrder] = useState(order);
  const [isEditing, setIsEditing] = useState(false);
  const [editedOrder, setEditedOrder] = useState<OrdenTrabajo | undefined>(order);
  
  // Estado para avances
  const [avanceDialogOpen, setAvanceDialogOpen] = useState(false);
  const [avanceToEdit, setAvanceToEdit] = useState<AvanceTrabajo | null>(null);
  const [avanceForm, setAvanceForm] = useState({
    fecha: format(new Date(), 'yyyy-MM-dd'),
    horasAvance: 0,
    descripcion: ''
  });

  const handleSearch = () => {
    const foundOrder = ordenesTrabajo.find(
      (o) => o.nroOrdenTrabajo.toString() === searchNumber
    );
    if (foundOrder) {
      setSelectedOrder(foundOrder);
      setEditedOrder(foundOrder);
      setIsEditing(false);
    }
  };

  const avances = selectedOrder ? avancesTrabajo[selectedOrder.nroOrdenTrabajo] || [] : [];
  const historial = selectedOrder ? historialEstados[selectedOrder.nroOrdenTrabajo] || [] : [];
  const archivos = selectedOrder ? archivosAdjuntos[selectedOrder.nroOrdenTrabajo] || [] : [];
  const puestas = selectedOrder ? puestasProduccion[selectedOrder.nroOrdenTrabajo] || [] : [];

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  };

  const handleModificar = () => {
    setIsEditing(true);
    setEditedOrder(selectedOrder);
  };

  const handleCancelar = () => {
    setIsEditing(false);
    setEditedOrder(selectedOrder);
  };

  const handleGrabar = () => {
    if (editedOrder) {
      setSelectedOrder(editedOrder);
      setIsEditing(false);
      toast.success('Orden de trabajo actualizada correctamente');
    }
  };

  const handleFieldChange = (field: string, value: string | number) => {
    if (editedOrder) {
      setEditedOrder({ ...editedOrder, [field]: value });
    }
  };

  const handleClienteChange = (clienteId: string) => {
    if (editedOrder) {
      const cliente = clientes.find(c => c.id.toString() === clienteId);
      if (cliente) {
        setEditedOrder({ ...editedOrder, cliente });
      }
    }
  };

  const handleSistemaChange = (sistemaId: string) => {
    if (editedOrder) {
      const sistema = sistemas.find(s => s.id.toString() === sistemaId);
      if (sistema) {
        setEditedOrder({ ...editedOrder, sistema });
      }
    }
  };

  const handleEstadoChange = (estadoId: string) => {
    if (editedOrder) {
      const estado = estados.find(e => e.id.toString() === estadoId);
      if (estado) {
        setEditedOrder({ ...editedOrder, estado });
      }
    }
  };

  const handleUsuarioResponsableChange = (usuarioId: string) => {
    if (editedOrder) {
      const usuario = usuarios.find(u => u.id === usuarioId);
      if (usuario) {
        setEditedOrder({ ...editedOrder, usuarioResponsable: usuario });
      }
    }
  };

  const handleUsuarioSolicitanteChange = (usuarioId: string) => {
    if (editedOrder) {
      const usuario = usuarios.find(u => u.id === usuarioId);
      if (usuario) {
        setEditedOrder({ ...editedOrder, usuarioSolicitante: usuario });
      }
    }
  };

  // Funciones para manejar avances
  const handleOpenAvanceDialog = (avance?: AvanceTrabajo) => {
    if (avance) {
      setAvanceToEdit(avance);
      setAvanceForm({
        fecha: format(avance.fecha, 'yyyy-MM-dd'),
        horasAvance: avance.horasAvance,
        descripcion: avance.descripcion
      });
    } else {
      setAvanceToEdit(null);
      setAvanceForm({
        fecha: format(new Date(), 'yyyy-MM-dd'),
        horasAvance: 0,
        descripcion: ''
      });
    }
    setAvanceDialogOpen(true);
  };

  const handleCloseAvanceDialog = () => {
    setAvanceDialogOpen(false);
    setAvanceToEdit(null);
    setAvanceForm({
      fecha: format(new Date(), 'yyyy-MM-dd'),
      horasAvance: 0,
      descripcion: ''
    });
  };

  const handleSaveAvance = () => {
    if (avanceToEdit) {
      toast.success('Avance actualizado correctamente');
    } else {
      toast.success('Avance agregado correctamente');
    }
    handleCloseAvanceDialog();
  };

  const handleDeleteAvance = (avanceId: number) => {
    if (window.confirm('¿Está seguro de eliminar este avance?')) {
      toast.success('Avance eliminado correctamente');
    }
  };

  const displayOrder = editedOrder || selectedOrder;

  return (
    <div className="p-6 space-y-6 max-w-[1400px] mx-auto">
      {/* Search Card */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Search className="w-5 h-5" />
            Buscar Orden de Trabajo
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4">
            <div className="flex-1 space-y-2">
              <Label htmlFor="searchOrder">Número de Orden</Label>
              <Input
                id="searchOrder"
                placeholder="Ingrese número de orden..."
                value={searchNumber}
                onChange={(e) => setSearchNumber(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
              />
            </div>
            <div className="flex items-end">
              <Button onClick={handleSearch}>
                <Search className="w-4 h-4 mr-2" />
                Buscar
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {displayOrder ? (
        <>
          {/* Header Info Card */}
          <Card>
            <CardContent className="pt-6">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                {/* Orden */}
                <div className="space-y-2">
                  <Label htmlFor="orden">Orden</Label>
                  <Input
                    id="orden"
                    value={displayOrder.nroOrdenTrabajo}
                    readOnly
                    className="bg-slate-50"
                  />
                </div>

                {/* Depende De */}
                <div className="space-y-2">
                  <Label htmlFor="dependeDe">Depende De</Label>
                  <Input
                    id="dependeDe"
                    value={displayOrder.dependeDe || ''}
                    readOnly
                    className="bg-slate-50"
                  />
                </div>

                {/* Fecha Solicitud */}
                <div className="space-y-2">
                  <Label htmlFor="fechaSolicitud">Fecha de Solicitud</Label>
                  <Input
                    id="fechaSolicitud"
                    value={format(displayOrder.fechaSolicitud, 'dd/MM/yyyy')}
                    readOnly
                    className="bg-slate-50"
                  />
                </div>

                {/* Fecha Vencimiento */}
                <div className="space-y-2">
                  <Label htmlFor="fechaVenc">Fecha Vencimiento</Label>
                  <Input
                    id="fechaVenc"
                    value={displayOrder.fechaVencimiento ? format(displayOrder.fechaVencimiento, 'dd/MM/yyyy') : ''}
                    readOnly
                    className="bg-slate-50"
                  />
                </div>

                {/* Cliente */}
                <div className="space-y-2">
                  <Label htmlFor="cliente">Cliente</Label>
                  <Select 
                    value={displayOrder.cliente.id.toString()} 
                    onValueChange={handleClienteChange}
                    disabled={!isEditing}
                  >
                    <SelectTrigger id="cliente">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {clientes.map((c) => (
                        <SelectItem key={c.id} value={c.id.toString()}>
                          {c.descripcion}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Sistema */}
                <div className="space-y-2">
                  <Label htmlFor="sistema">Sistema</Label>
                  <Select 
                    value={displayOrder.sistema.id.toString()}
                    onValueChange={handleSistemaChange}
                    disabled={!isEditing}
                  >
                    <SelectTrigger id="sistema">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {sistemas.map((s) => (
                        <SelectItem key={s.id} value={s.id.toString()}>
                          {s.descripcion}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Estado */}
                <div className="space-y-2">
                  <Label htmlFor="estado">Estado del Trabajo</Label>
                  <Select 
                    value={displayOrder.estado.id.toString()}
                    onValueChange={handleEstadoChange}
                    disabled={!isEditing}
                  >
                    <SelectTrigger id="estado">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {estados.map((e) => (
                        <SelectItem key={e.id} value={e.id.toString()}>
                          {e.descripcion}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Proyecto */}
                <div className="space-y-2">
                  <Label htmlFor="proyecto">Proyecto</Label>
                  <Input
                    id="proyecto"
                    value={displayOrder.proyecto}
                    onChange={(e) => handleFieldChange('proyecto', e.target.value)}
                    readOnly={!isEditing}
                    className={!isEditing ? 'bg-slate-50' : ''}
                  />
                </div>

                {/* Usuario Responsable */}
                <div className="space-y-2">
                  <Label htmlFor="responsable">Usuario Responsable</Label>
                  <Select 
                    value={displayOrder.usuarioResponsable.id}
                    onValueChange={handleUsuarioResponsableChange}
                    disabled={!isEditing}
                  >
                    <SelectTrigger id="responsable">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {usuarios.map((u) => (
                        <SelectItem key={u.id} value={u.id}>
                          {u.nombre} {u.apellidos}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Usuario Solicitante */}
                <div className="space-y-2">
                  <Label htmlFor="solicitante">Usuario Solicitante</Label>
                  <Select 
                    value={displayOrder.usuarioSolicitante.id}
                    onValueChange={handleUsuarioSolicitanteChange}
                    disabled={!isEditing}
                  >
                    <SelectTrigger id="solicitante">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {usuarios.map((u) => (
                        <SelectItem key={u.id} value={u.id}>
                          {u.nombre} {u.apellidos}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Módulo */}
                <div className="space-y-2">
                  <Label htmlFor="modulo">Módulo/Pantallas</Label>
                  <Input
                    id="modulo"
                    value={displayOrder.modulo}
                    onChange={(e) => handleFieldChange('modulo', e.target.value)}
                    readOnly={!isEditing}
                    className={!isEditing ? 'bg-slate-50' : ''}
                  />
                </div>

                {/* Solicitado Por */}
                <div className="space-y-2">
                  <Label htmlFor="solicitadoPor">Solicitado Por</Label>
                  <Input
                    id="solicitadoPor"
                    value={displayOrder.solicitadoPor}
                    onChange={(e) => handleFieldChange('solicitadoPor', e.target.value)}
                    readOnly={!isEditing}
                    className={!isEditing ? 'bg-slate-50' : ''}
                  />
                </div>

                {/* Asunto */}
                <div className="lg:col-span-4 space-y-2">
                  <Label htmlFor="asunto">Asunto</Label>
                  <Input
                    id="asunto"
                    value={displayOrder.asunto}
                    onChange={(e) => handleFieldChange('asunto', e.target.value)}
                    readOnly={!isEditing}
                    className={!isEditing ? 'bg-slate-50' : ''}
                  />
                </div>

                {/* Horas */}
                <div className="space-y-2">
                  <Label htmlFor="horasFacturables">Horas Facturables</Label>
                  <Input
                    id="horasFacturables"
                    type="number"
                    value="0"
                    readOnly
                    className="bg-slate-50"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="avance">% de Avance</Label>
                  <Input
                    id="avance"
                    type="number"
                    value={displayOrder.porcentajeAvance}
                    onChange={(e) => handleFieldChange('porcentajeAvance', parseInt(e.target.value))}
                    readOnly={!isEditing}
                    className={!isEditing ? 'bg-slate-50' : ''}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="horasConsumidas">Horas Consumidas</Label>
                  <Input
                    id="horasConsumidas"
                    type="number"
                    value={displayOrder.cantidadHorasConsumidas}
                    readOnly
                    className="bg-slate-50"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="horasAvances">Horas Avances</Label>
                  <Input
                    id="horasAvances"
                    type="number"
                    value={displayOrder.cantidadHorasConsumidas}
                    readOnly
                    className="bg-slate-50"
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Tabs Section */}
          <Card>
            <CardContent className="pt-6">
              <Tabs defaultValue="descripcion" className="w-full">
                <TabsList className="grid w-full grid-cols-5">
                  <TabsTrigger value="descripcion">
                    <FileText className="w-4 h-4 mr-2" />
                    Descripción
                  </TabsTrigger>
                  <TabsTrigger value="avances">
                    <Clock className="w-4 h-4 mr-2" />
                    Avances del Trabajo
                  </TabsTrigger>
                  <TabsTrigger value="produccion">
                    <Rocket className="w-4 h-4 mr-2" />
                    Puesta en Producción
                  </TabsTrigger>
                  <TabsTrigger value="historial">
                    <History className="w-4 h-4 mr-2" />
                    Historial de Estados
                  </TabsTrigger>
                  <TabsTrigger value="archivos">
                    <Upload className="w-4 h-4 mr-2" />
                    Archivos Adjuntos
                  </TabsTrigger>
                </TabsList>

                {/* Descripción */}
                <TabsContent value="descripcion" className="space-y-4 mt-6">
                  <div className="space-y-2">
                    <Label htmlFor="descripcion">Descripción Detallada</Label>
                    <Textarea
                      id="descripcion"
                      value={displayOrder.descripcion}
                      onChange={(e) => handleFieldChange('descripcion', e.target.value)}
                      rows={10}
                      className="font-mono"
                      readOnly={!isEditing}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="observaciones">Observaciones</Label>
                    <Textarea
                      id="observaciones"
                      value={displayOrder.observaciones}
                      onChange={(e) => handleFieldChange('observaciones', e.target.value)}
                      rows={6}
                      readOnly={!isEditing}
                    />
                  </div>
                </TabsContent>

                {/* Avances del Trabajo */}
                <TabsContent value="avances" className="mt-6">
                  <div className="border rounded-lg overflow-hidden">
                    <Table>
                      <TableHeader>
                        <TableRow className="bg-slate-50">
                          <TableHead>Fecha</TableHead>
                          <TableHead>Usuario</TableHead>
                          <TableHead className="text-right">Horas</TableHead>
                          <TableHead>Descripción</TableHead>
                          <TableHead className="w-24 text-center">Acciones</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {avances.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={5} className="text-center text-gray-500 py-8">
                              No hay avances registrados
                            </TableCell>
                          </TableRow>
                        ) : (
                          avances.map((avance) => (
                            <TableRow key={avance.id}>
                              <TableCell>
                                {format(avance.fecha, 'dd/MM/yyyy', { locale: es })}
                              </TableCell>
                              <TableCell>{avance.usuario.nombre} {avance.usuario.apellidos}</TableCell>
                              <TableCell className="text-right">{avance.horasAvance}</TableCell>
                              <TableCell>{avance.descripcion}</TableCell>
                              <TableCell>
                                <div className="flex gap-1 justify-center">
                                  <Button 
                                    size="sm" 
                                    variant="ghost"
                                    onClick={() => handleOpenAvanceDialog(avance)}
                                  >
                                    <Edit className="w-4 h-4 text-blue-600" />
                                  </Button>
                                  <Button 
                                    size="sm" 
                                    variant="ghost"
                                    onClick={() => handleDeleteAvance(avance.id)}
                                  >
                                    <Trash2 className="w-4 h-4 text-red-600" />
                                  </Button>
                                </div>
                              </TableCell>
                            </TableRow>
                          ))
                        )}
                      </TableBody>
                    </Table>
                  </div>
                  {avances.length > 0 && (
                    <div className="mt-4 p-4 bg-blue-50 rounded-lg flex items-center justify-between">
                      <div className="text-gray-700">
                        <span>Total de Horas Registradas: </span>
                        <span className="text-blue-700">
                          {avances.reduce((sum, a) => sum + a.horasAvance, 0)} horas
                        </span>
                      </div>
                      <Button onClick={() => handleOpenAvanceDialog()}>
                        <Plus className="w-4 h-4 mr-2" />
                        Nuevo Avance
                      </Button>
                    </div>
                  )}
                  {avances.length === 0 && (
                    <div className="mt-4 flex justify-end">
                      <Button onClick={() => handleOpenAvanceDialog()}>
                        <Plus className="w-4 h-4 mr-2" />
                        Nuevo Avance
                      </Button>
                    </div>
                  )}
                </TabsContent>

                {/* Puesta en Producción */}
                <TabsContent value="produccion" className="mt-6">
                  {puestas.length === 0 ? (
                    <div className="text-center text-gray-500 py-8">
                      No hay registros de puesta en producción
                    </div>
                  ) : (
                    <div className="space-y-4">
                      {puestas.map((puesta) => (
                        <div key={puesta.id} className="space-y-4">
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2">
                              <Rocket className="w-5 h-5 text-blue-600" />
                              <span className="text-gray-900">
                                {format(puesta.fecha, 'dd/MM/yyyy HH:mm', { locale: es })}
                              </span>
                            </div>
                            <Badge variant="outline">
                              {puesta.usuario.nombre} {puesta.usuario.apellidos}
                            </Badge>
                          </div>

                          <Tabs defaultValue="formularios" className="w-full">
                            <TabsList className="grid w-full grid-cols-2 bg-cyan-500">
                              <TabsTrigger value="formularios" className="data-[state=active]:bg-white data-[state=inactive]:text-white">
                                <Code className="w-4 h-4 mr-2" />
                                Formularios Modificados
                              </TabsTrigger>
                              <TabsTrigger value="basedatos" className="data-[state=active]:bg-cyan-400 data-[state=inactive]:text-white">
                                <Database className="w-4 h-4 mr-2" />
                                Mod. a la Base de Datos
                              </TabsTrigger>
                            </TabsList>

                            {/* Formularios Modificados */}
                            <TabsContent value="formularios" className="mt-4">
                              <div className="border rounded-lg bg-white">
                                <div className="p-3 border-b bg-slate-50 flex items-center gap-2">
                                  <Code className="w-4 h-4 text-slate-600" />
                                  <span className="text-slate-700">Archivos de Código Modificados</span>
                                </div>
                                <div className="p-4 max-h-[400px] overflow-y-auto">
                                  {puesta.formulariosModificados.length === 0 ? (
                                    <div className="text-center text-gray-500 py-8">
                                      No hay formularios modificados
                                    </div>
                                  ) : (
                                    <div className="space-y-1">
                                      {puesta.formulariosModificados.map((formulario, idx) => (
                                        <div key={idx} className="font-mono text-gray-700 hover:bg-blue-50 px-2 py-1 rounded">
                                          {formulario.ruta}
                                          {formulario.descripcion && (
                                            <span className="text-gray-500"> {formulario.descripcion}</span>
                                          )}
                                        </div>
                                      ))}
                                    </div>
                                  )}
                                </div>
                              </div>
                            </TabsContent>

                            {/* Modificaciones a la Base de Datos */}
                            <TabsContent value="basedatos" className="mt-4">
                              <div className="border rounded-lg bg-white">
                                <div className="p-3 border-b bg-slate-50 flex items-center gap-2">
                                  <Database className="w-4 h-4 text-slate-600" />
                                  <span className="text-slate-700">Scripts y Objetos de Base de Datos</span>
                                </div>
                                <div className="p-4 max-h-[400px] overflow-y-auto">
                                  {puesta.modificacionesBaseDatos.length === 0 ? (
                                    <div className="text-center text-gray-500 py-8">
                                      No hay modificaciones a la base de datos
                                    </div>
                                  ) : (
                                    <div className="space-y-3">
                                      {puesta.modificacionesBaseDatos.map((mod, idx) => (
                                        <div key={idx} className="border-l-4 border-blue-500 pl-4 py-2 bg-slate-50 rounded">
                                          <div className="flex items-center gap-2 mb-1">
                                            <Badge variant="outline" className="bg-blue-100 text-blue-800 border-blue-200">
                                              {mod.tipo.toUpperCase()}
                                            </Badge>
                                            <span className="font-mono text-gray-900">{mod.nombre}</span>
                                          </div>
                                          {mod.descripcion && (
                                            <p className="text-gray-600 mt-1">{mod.descripcion}</p>
                                          )}
                                          <details className="mt-2">
                                            <summary className="text-blue-600 cursor-pointer hover:text-blue-700">
                                              Ver script
                                            </summary>
                                            <pre className="mt-2 p-3 bg-slate-900 text-green-400 rounded text-sm overflow-x-auto">
                                              <code>{mod.script}</code>
                                            </pre>
                                          </details>
                                        </div>
                                      ))}
                                    </div>
                                  )}
                                </div>
                              </div>
                            </TabsContent>
                          </Tabs>
                        </div>
                      ))}
                    </div>
                  )}
                </TabsContent>

                {/* Historial de Estados */}
                <TabsContent value="historial" className="mt-6">
                  <div className="border rounded-lg overflow-hidden">
                    <Table>
                      <TableHeader>
                        <TableRow className="bg-slate-50">
                          <TableHead className="w-24">Secuencia</TableHead>
                          <TableHead>Estado</TableHead>
                          <TableHead>Fecha</TableHead>
                          <TableHead>Usuario</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {historial.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={4} className="text-center text-gray-500 py-8">
                              No hay historial de estados
                            </TableCell>
                          </TableRow>
                        ) : (
                          historial.map((h) => (
                            <TableRow key={h.id}>
                              <TableCell>{h.secuencia}</TableCell>
                              <TableCell>
                                <Badge variant="outline">
                                  {h.estado.descripcion}
                                </Badge>
                              </TableCell>
                              <TableCell>
                                {format(h.fechaAlta, 'dd/MM/yyyy HH:mm', { locale: es })}
                              </TableCell>
                              <TableCell>{h.usuario.nombre} {h.usuario.apellidos}</TableCell>
                            </TableRow>
                          ))
                        )}
                      </TableBody>
                    </Table>
                  </div>
                </TabsContent>

                {/* Archivos Adjuntos */}
                <TabsContent value="archivos" className="mt-6">
                  <div className="space-y-4">
                    <div className="flex justify-end">
                      <Button variant="outline">
                        <Upload className="w-4 h-4 mr-2" />
                        Subir Archivo
                      </Button>
                    </div>
                    <div className="border rounded-lg overflow-hidden">
                      <Table>
                        <TableHeader>
                          <TableRow className="bg-slate-50">
                            <TableHead>Nombre del Archivo</TableHead>
                            <TableHead>Fecha Subida</TableHead>
                            <TableHead>Usuario</TableHead>
                            <TableHead className="text-right">Tamaño</TableHead>
                            <TableHead className="w-20"></TableHead>
                          </TableRow>
                        </TableHeader>
                        <TableBody>
                          {archivos.length === 0 ? (
                            <TableRow>
                              <TableCell colSpan={5} className="text-center text-gray-500 py-8">
                                No hay archivos adjuntos
                              </TableCell>
                            </TableRow>
                          ) : (
                            archivos.map((archivo) => (
                              <TableRow key={archivo.id}>
                                <TableCell className="flex items-center gap-2">
                                  <FileIcon className="w-4 h-4 text-blue-600" />
                                  {archivo.nombreArchivo}
                                </TableCell>
                                <TableCell>
                                  {format(archivo.fechaSubida, 'dd/MM/yyyy HH:mm', { locale: es })}
                                </TableCell>
                                <TableCell>{archivo.usuario.nombre} {archivo.usuario.apellidos}</TableCell>
                                <TableCell className="text-right">{formatFileSize(archivo.tamanio)}</TableCell>
                                <TableCell>
                                  <Button size="sm" variant="ghost">
                                    <Download className="w-4 h-4" />
                                  </Button>
                                </TableCell>
                              </TableRow>
                            ))
                          )}
                        </TableBody>
                      </Table>
                    </div>
                  </div>
                </TabsContent>
              </Tabs>
            </CardContent>
          </Card>

          {/* Action Buttons */}
          <div className="flex justify-end gap-4">
            <Button 
              variant="outline" 
              onClick={handleCancelar}
              disabled={!isEditing}
            >
              <X className="w-4 h-4 mr-2" />
              Cancelar
            </Button>
            <Button 
              variant="outline"
              onClick={handleModificar}
              disabled={isEditing}
            >
              <Edit className="w-4 h-4 mr-2" />
              Modificar
            </Button>
            <Button 
              className="bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800"
              onClick={handleGrabar}
              disabled={!isEditing}
            >
              <Save className="w-4 h-4 mr-2" />
              Grabar
            </Button>
          </div>
        </>
      ) : (
        <Card>
          <CardContent className="py-12">
            <div className="text-center text-gray-500">
              <Search className="w-12 h-12 mx-auto mb-4 text-gray-400" />
              <p>No hay orden de trabajo seleccionada</p>
              <p className="text-sm mt-2">Utilice el buscador para encontrar una orden</p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Dialog para Agregar/Editar Avance */}
      <Dialog open={avanceDialogOpen} onOpenChange={setAvanceDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {avanceToEdit ? 'Editar Avance' : 'Nuevo Avance'}
            </DialogTitle>
            <DialogDescription>
              {avanceToEdit 
                ? 'Modifique los datos del avance de trabajo'
                : 'Complete los datos del nuevo avance de trabajo'
              }
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="avanceFecha">Fecha</Label>
              <Input
                id="avanceFecha"
                type="date"
                value={avanceForm.fecha}
                onChange={(e) => setAvanceForm({ ...avanceForm, fecha: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="avanceHoras">Horas de Avance</Label>
              <Input
                id="avanceHoras"
                type="number"
                step="0.5"
                value={avanceForm.horasAvance}
                onChange={(e) => setAvanceForm({ ...avanceForm, horasAvance: parseFloat(e.target.value) })}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="avanceDescripcion">Descripción</Label>
              <Textarea
                id="avanceDescripcion"
                rows={4}
                value={avanceForm.descripcion}
                onChange={(e) => setAvanceForm({ ...avanceForm, descripcion: e.target.value })}
                placeholder="Descripción del trabajo realizado..."
              />
            </div>
            <div className="space-y-2">
              <Label>Usuario</Label>
              <Input
                value={`${currentUser.nombre} ${currentUser.apellidos}`}
                readOnly
                className="bg-slate-50"
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={handleCloseAvanceDialog}>
              Cancelar
            </Button>
            <Button onClick={handleSaveAvance}>
              <Save className="w-4 h-4 mr-2" />
              Guardar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
