import { useState, useMemo } from 'react';
import { ordenesTrabajo, clientes, sistemas, estados } from '../data/mockData';
import { OrdenTrabajo } from '../types';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { Button } from './ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from './ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './ui/table';
import { Badge } from './ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Search, Filter, Eye } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

interface WorkOrdersListProps {
  onSelectOrder: (order: OrdenTrabajo) => void;
}

export function WorkOrdersList({ onSelectOrder }: WorkOrdersListProps) {
  const [filters, setFilters] = useState({
    numeroOrden: '',
    cliente: '',
    sistema: '',
    estado: '',
    fechaDesde: '',
    fechaHasta: '',
    asunto: '',
  });

  const [appliedFilters, setAppliedFilters] = useState({
    numeroOrden: '',
    cliente: '',
    sistema: '',
    estado: '',
    fechaDesde: '',
    fechaHasta: '',
    asunto: '',
  });

  const filteredOrders = useMemo(() => {
    return ordenesTrabajo.filter((orden) => {
      if (appliedFilters.numeroOrden && !orden.nroOrdenTrabajo.toString().includes(appliedFilters.numeroOrden)) {
        return false;
      }
      if (appliedFilters.cliente && orden.cliente.id.toString() !== appliedFilters.cliente) {
        return false;
      }
      if (appliedFilters.sistema && orden.sistema.id.toString() !== appliedFilters.sistema) {
        return false;
      }
      if (appliedFilters.estado && orden.estado.id.toString() !== appliedFilters.estado) {
        return false;
      }
      if (appliedFilters.asunto && !orden.asunto.toLowerCase().includes(appliedFilters.asunto.toLowerCase())) {
        return false;
      }
      if (appliedFilters.fechaDesde && orden.fechaSolicitud < new Date(appliedFilters.fechaDesde)) {
        return false;
      }
      if (appliedFilters.fechaHasta && orden.fechaSolicitud > new Date(appliedFilters.fechaHasta)) {
        return false;
      }
      return true;
    });
  }, [appliedFilters]);

  const handleSearch = () => {
    setAppliedFilters(filters);
  };

  const handleClearFilters = () => {
    const emptyFilters = {
      numeroOrden: '',
      cliente: '',
      sistema: '',
      estado: '',
      fechaDesde: '',
      fechaHasta: '',
      asunto: '',
    };
    setFilters(emptyFilters);
    setAppliedFilters(emptyFilters);
  };

  const getEstadoColor = (estadoId: number) => {
    switch (estadoId) {
      case 1: return 'bg-yellow-100 text-yellow-800 border-yellow-200 dark:bg-yellow-900/30 dark:text-yellow-300 dark:border-yellow-700';
      case 2: return 'bg-blue-100 text-blue-800 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-700';
      case 3: return 'bg-orange-100 text-orange-800 border-orange-200 dark:bg-orange-900/30 dark:text-orange-300 dark:border-orange-700';
      case 4: return 'bg-green-100 text-green-800 border-green-200 dark:bg-green-900/30 dark:text-green-300 dark:border-green-700';
      case 5: return 'bg-red-100 text-red-800 border-red-200 dark:bg-red-900/30 dark:text-red-300 dark:border-red-700';
      default: return 'bg-gray-100 text-gray-800 border-gray-200 dark:bg-gray-800/30 dark:text-gray-300 dark:border-gray-600';
    }
  };

  return (
    <div className="p-6 space-y-6">
      {/* Filters Card */}
      <Card className="shadow-sm">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="w-5 h-5" />
            Filtros de Búsqueda
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="space-y-2">
              <Label htmlFor="numeroOrden">Número de Orden</Label>
              <Input
                id="numeroOrden"
                placeholder="Buscar por número..."
                value={filters.numeroOrden}
                onChange={(e) => setFilters({ ...filters, numeroOrden: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="cliente">Cliente</Label>
              <Select
                value={filters.cliente}
                onValueChange={(value) => setFilters({ ...filters, cliente: value })}
              >
                <SelectTrigger id="cliente">
                  <SelectValue placeholder="Todos los clientes" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los clientes</SelectItem>
                  {clientes.map((cliente) => (
                    <SelectItem key={cliente.id} value={cliente.id.toString()}>
                      {cliente.descripcion}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="sistema">Sistema</Label>
              <Select
                value={filters.sistema}
                onValueChange={(value) => setFilters({ ...filters, sistema: value })}
              >
                <SelectTrigger id="sistema">
                  <SelectValue placeholder="Todos los sistemas" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los sistemas</SelectItem>
                  {sistemas.map((sistema) => (
                    <SelectItem key={sistema.id} value={sistema.id.toString()}>
                      {sistema.descripcion}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="estado">Estado</Label>
              <Select
                value={filters.estado}
                onValueChange={(value) => setFilters({ ...filters, estado: value })}
              >
                <SelectTrigger id="estado">
                  <SelectValue placeholder="Todos los estados" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los estados</SelectItem>
                  {estados.map((estado) => (
                    <SelectItem key={estado.id} value={estado.id.toString()}>
                      {estado.descripcion}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="fechaDesde">Fecha Desde</Label>
              <Input
                id="fechaDesde"
                type="date"
                value={filters.fechaDesde}
                onChange={(e) => setFilters({ ...filters, fechaDesde: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="fechaHasta">Fecha Hasta</Label>
              <Input
                id="fechaHasta"
                type="date"
                value={filters.fechaHasta}
                onChange={(e) => setFilters({ ...filters, fechaHasta: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="asunto">Asunto</Label>
              <Input
                id="asunto"
                placeholder="Buscar en asunto..."
                value={filters.asunto}
                onChange={(e) => setFilters({ ...filters, asunto: e.target.value })}
              />
            </div>

            <div className="flex items-end gap-2 lg:col-span-1">
              <Button 
                onClick={handleSearch}
                className="flex-1 bg-blue-600 hover:bg-blue-700 text-white dark:bg-blue-600 dark:hover:bg-blue-700"
              >
                <Search className="w-4 h-4 mr-2" />
                Buscar
              </Button>
              <Button 
                variant="outline" 
                onClick={handleClearFilters}
                className="flex-1"
              >
                Limpiar Filtros
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Results Card */}
      <Card className="shadow-sm">
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Search className="w-5 h-5" />
              Resultados
            </div>
            <Badge variant="secondary">
              {filteredOrders.length} {filteredOrders.length === 1 ? 'orden' : 'órdenes'}
            </Badge>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="border dark:border-slate-600 rounded-lg overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow className="bg-slate-50 dark:bg-slate-800/50">
                  <TableHead className="w-24">Orden</TableHead>
                  <TableHead>Cliente</TableHead>
                  <TableHead>Sistema</TableHead>
                  <TableHead>Asunto</TableHead>
                  <TableHead>Responsable</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead className="w-32">Fecha Solicitud</TableHead>
                  <TableHead className="text-right w-32">Horas</TableHead>
                  <TableHead className="w-20"></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredOrders.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={9} className="text-center text-gray-500 dark:text-gray-400 py-8">
                      No se encontraron órdenes de trabajo con los filtros aplicados
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredOrders.map((orden) => (
                    <TableRow 
                      key={orden.nroOrdenTrabajo}
                      className="hover:bg-blue-50/50 dark:hover:bg-slate-800/50 cursor-pointer transition-colors"
                      onDoubleClick={() => onSelectOrder(orden)}
                    >
                      <TableCell>{orden.nroOrdenTrabajo}</TableCell>
                      <TableCell>{orden.cliente.descripcion}</TableCell>
                      <TableCell>{orden.sistema.descripcion}</TableCell>
                      <TableCell className="max-w-xs truncate">{orden.asunto}</TableCell>
                      <TableCell>{orden.usuarioResponsable.apellidos}</TableCell>
                      <TableCell>
                        <Badge 
                          variant="outline"
                          className={getEstadoColor(orden.estado.id)}
                        >
                          {orden.estado.descripcion}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {format(orden.fechaSolicitud, 'dd/MM/yyyy', { locale: es })}
                      </TableCell>
                      <TableCell className="text-right">
                        {orden.cantidadHorasConsumidas.toFixed(1)}
                      </TableCell>
                      <TableCell>
                        <Button
                          size="sm"
                          variant="ghost"
                          onClick={() => onSelectOrder(orden)}
                        >
                          <Eye className="w-4 h-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>

          {/* Summary */}
          {filteredOrders.length > 0 && (
            <div className="mt-4 p-4 bg-slate-50 dark:bg-slate-800/50 rounded-lg">
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <div className="text-gray-500 dark:text-gray-400">Cantidad de OT</div>
                  <div className="text-gray-900 dark:text-gray-100">{filteredOrders.length}</div>
                </div>
                <div>
                  <div className="text-gray-500 dark:text-gray-400">Total Horas Estimadas</div>
                  <div className="text-gray-900 dark:text-gray-100">
                    {filteredOrders.reduce((sum, o) => sum + o.cantidadHorasEstimadas, 0).toFixed(1)}
                  </div>
                </div>
                <div>
                  <div className="text-gray-500 dark:text-gray-400">Total Horas Avance</div>
                  <div className="text-gray-900 dark:text-gray-100">
                    {filteredOrders.reduce((sum, o) => sum + o.cantidadHorasConsumidas, 0).toFixed(1)}
                  </div>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
