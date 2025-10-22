import { ordenesTrabajo } from '../data/mockData';
import { OrdenTrabajo } from '../types';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Badge } from './ui/badge';
import { Button } from './ui/button';
import { Clock, Eye, Calendar, User } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

interface RecentHistoryProps {
  onSelectOrder: (order: OrdenTrabajo) => void;
}

export function RecentHistory({ onSelectOrder }: RecentHistoryProps) {
  // Get last 10 orders sorted by date
  const recentOrders = [...ordenesTrabajo]
    .sort((a, b) => b.fechaSolicitud.getTime() - a.fechaSolicitud.getTime())
    .slice(0, 10);

  const getEstadoColor = (estadoId: number) => {
    switch (estadoId) {
      case 1: return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 2: return 'bg-blue-100 text-blue-800 border-blue-200';
      case 3: return 'bg-orange-100 text-orange-800 border-orange-200';
      case 4: return 'bg-green-100 text-green-800 border-green-200';
      case 5: return 'bg-red-100 text-red-800 border-red-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  return (
    <div className="p-6 space-y-6 max-w-[1200px] mx-auto">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Clock className="w-5 h-5" />
            Historial Reciente de Órdenes de Trabajo
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {recentOrders.map((orden) => (
              <Card 
                key={orden.nroOrdenTrabajo}
                className="hover:shadow-md transition-shadow cursor-pointer"
                onClick={() => onSelectOrder(orden)}
              >
                <CardContent className="p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 space-y-2">
                      <div className="flex items-center gap-3">
                        <div className="flex items-center gap-2">
                          <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center shadow-sm">
                            <span className="text-white">OT</span>
                          </div>
                          <div>
                            <div className="text-gray-900">
                              Orden #{orden.nroOrdenTrabajo}
                            </div>
                            <div className="text-gray-500">
                              {orden.cliente.descripcion}
                            </div>
                          </div>
                        </div>
                      </div>

                      <p className="text-gray-700">{orden.asunto}</p>

                      <div className="flex flex-wrap gap-4 text-gray-600">
                        <div className="flex items-center gap-1">
                          <Calendar className="w-4 h-4" />
                          <span>{format(orden.fechaSolicitud, 'dd/MM/yyyy', { locale: es })}</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <User className="w-4 h-4" />
                          <span>{orden.usuarioResponsable.apellidos}</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <Clock className="w-4 h-4" />
                          <span>{orden.cantidadHorasConsumidas.toFixed(1)} hs</span>
                        </div>
                      </div>

                      <div className="flex flex-wrap gap-2">
                        <Badge 
                          variant="outline"
                          className={getEstadoColor(orden.estado.id)}
                        >
                          {orden.estado.descripcion}
                        </Badge>
                        <Badge variant="outline" className="bg-slate-100 text-slate-800 border-slate-200">
                          {orden.sistema.descripcion}
                        </Badge>
                        {orden.modulo && (
                          <Badge variant="outline" className="bg-purple-100 text-purple-800 border-purple-200">
                            {orden.modulo}
                          </Badge>
                        )}
                      </div>
                    </div>

                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={(e) => {
                        e.stopPropagation();
                        onSelectOrder(orden);
                      }}
                    >
                      <Eye className="w-4 h-4" />
                    </Button>
                  </div>

                  {/* Progress bar */}
                  {orden.porcentajeAvance > 0 && (
                    <div className="mt-3">
                      <div className="flex items-center justify-between mb-1">
                        <span className="text-gray-600">Progreso</span>
                        <span className="text-gray-900">{orden.porcentajeAvance}%</span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div 
                          className="bg-gradient-to-r from-blue-500 to-blue-600 h-2 rounded-full transition-all"
                          style={{ width: `${orden.porcentajeAvance}%` }}
                        ></div>
                      </div>
                    </div>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>

          {recentOrders.length === 0 && (
            <div className="text-center text-gray-500 py-12">
              <Clock className="w-16 h-16 mx-auto mb-4 text-gray-300" />
              <p>No hay órdenes recientes</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Summary Card */}
      <Card>
        <CardHeader>
          <CardTitle>Resumen</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="p-4 bg-blue-50 rounded-lg">
              <div className="text-blue-600">Total Órdenes</div>
              <div className="text-blue-900 mt-1">{recentOrders.length}</div>
            </div>
            <div className="p-4 bg-green-50 rounded-lg">
              <div className="text-green-600">Terminadas</div>
              <div className="text-green-900 mt-1">
                {recentOrders.filter(o => o.estado.id === 4).length}
              </div>
            </div>
            <div className="p-4 bg-orange-50 rounded-lg">
              <div className="text-orange-600">En Desarrollo</div>
              <div className="text-orange-900 mt-1">
                {recentOrders.filter(o => o.estado.id === 2).length}
              </div>
            </div>
            <div className="p-4 bg-yellow-50 rounded-lg">
              <div className="text-yellow-600">Pendientes</div>
              <div className="text-yellow-900 mt-1">
                {recentOrders.filter(o => o.estado.id === 1).length}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
