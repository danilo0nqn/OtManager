import { ReactNode, useState } from 'react';
import { 
  LayoutList, 
  FileText, 
  History, 
  ChevronLeft,
  ChevronRight,
  LogOut,
  Moon,
  Sun
} from 'lucide-react';
import { currentUser } from '../data/mockData';
import { Button } from './ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';

interface AppLayoutProps {
  children: ReactNode;
  currentView: 'list' | 'detail' | 'history';
  onViewChange: (view: 'list' | 'detail' | 'history') => void;
  onLogout: () => void;
  theme: 'light' | 'dark';
  onToggleTheme: () => void;
}

export function AppLayout({ children, currentView, onViewChange, onLogout, theme, onToggleTheme }: AppLayoutProps) {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  const menuItems = [
    { id: 'list' as const, label: 'Órdenes de Trabajo', icon: LayoutList },
    { id: 'detail' as const, label: 'Orden de Trabajo', icon: FileText },
    { id: 'history' as const, label: 'Historial', icon: History },
  ];

  return (
    <div className="h-screen w-full flex bg-gray-50 dark:bg-slate-950">
      {/* Sidebar */}
      <aside 
        className={`bg-gradient-to-b from-slate-800 to-slate-900 border-r border-slate-700 transition-all duration-300 flex flex-col ${
          sidebarCollapsed ? 'w-20' : 'w-64'
        }`}
      >
        {/* Logo */}
        <div className="h-16 flex items-center justify-between px-4 border-b border-slate-700">
          {!sidebarCollapsed && (
            <span className="text-white tracking-tight">OT Manager</span>
          )}
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
            className="text-slate-400 hover:text-white hover:bg-slate-700"
          >
            {sidebarCollapsed ? <ChevronRight className="w-5 h-5" /> : <ChevronLeft className="w-5 h-5" />}
          </Button>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-3 space-y-1">
          {menuItems.map((item) => {
            const Icon = item.icon;
            const isActive = currentView === item.id;
            
            return (
              <button
                key={item.id}
                onClick={() => onViewChange(item.id)}
                className={`w-full flex items-center gap-3 px-3 py-3 rounded-lg transition-all ${
                  isActive 
                    ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/30' 
                    : 'text-slate-300 hover:bg-slate-700 hover:text-white'
                }`}
              >
                <Icon className="w-5 h-5 flex-shrink-0" />
                {!sidebarCollapsed && <span className="truncate">{item.label}</span>}
              </button>
            );
          })}
        </nav>

        {/* User section */}
        <div className="p-3 border-t border-slate-700">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button className="w-full flex items-center gap-3 px-3 py-3 rounded-lg text-slate-300 hover:bg-slate-700 hover:text-white transition-all">
                <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center flex-shrink-0">
                  <span className="text-white text-sm">{currentUser.iniciales}</span>
                </div>
                {!sidebarCollapsed && (
                  <div className="flex-1 text-left overflow-hidden">
                    <div className="truncate">{currentUser.nombre}</div>
                    <div className="truncate text-slate-400">{currentUser.id}</div>
                  </div>
                )}
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56">
              <DropdownMenuLabel>Mi Cuenta</DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={onLogout}>
                <LogOut className="mr-2 h-4 w-4" />
                <span>Cerrar Sesión</span>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </aside>

      {/* Main content */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Header */}
        <header className="h-16 bg-white dark:bg-slate-900 border-b border-gray-200 dark:border-slate-700 flex items-center justify-between px-6 shadow-sm">
          <h1 className="text-gray-900 dark:text-white">Gestión de Órdenes de Trabajo</h1>
          
          <div className="flex items-center gap-3">
            <Button
              variant="ghost"
              size="icon"
              onClick={onToggleTheme}
              className="rounded-full hover:bg-gray-100 dark:hover:bg-slate-800"
            >
              {theme === 'light' ? (
                <Moon className="w-5 h-5 text-gray-600 dark:text-gray-300" />
              ) : (
                <Sun className="w-5 h-5 text-gray-300" />
              )}
            </Button>
            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center shadow-md">
              <span className="text-white">{currentUser.iniciales}</span>
            </div>
          </div>
        </header>

        {/* Content */}
        <main className="flex-1 overflow-auto">
          {children}
        </main>
      </div>
    </div>
  );
}
