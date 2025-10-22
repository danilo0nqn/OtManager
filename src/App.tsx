import { useState, useEffect } from 'react';
import { LoginPage } from './components/LoginPage';
import { AppLayout } from './components/AppLayout';
import { WorkOrdersList } from './components/WorkOrdersList';
import { WorkOrderDetail } from './components/WorkOrderDetail';
import { RecentHistory } from './components/RecentHistory';
import { OrdenTrabajo } from './types';
import { Toaster } from './components/ui/sonner';

export default function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [currentView, setCurrentView] = useState<'list' | 'detail' | 'history'>('list');
  const [selectedOrder, setSelectedOrder] = useState<OrdenTrabajo | undefined>(undefined);
  const [theme, setTheme] = useState<'light' | 'dark'>(() => {
    const savedTheme = localStorage.getItem('theme');
    return (savedTheme === 'dark' || savedTheme === 'light') ? savedTheme : 'light';
  });

  useEffect(() => {
    if (theme === 'dark') {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
    localStorage.setItem('theme', theme);
  }, [theme]);

  const toggleTheme = () => {
    setTheme(prev => prev === 'light' ? 'dark' : 'light');
  };

  const handleLogin = () => {
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setCurrentView('list');
    setSelectedOrder(undefined);
  };

  const handleSelectOrder = (order: OrdenTrabajo) => {
    setSelectedOrder(order);
    setCurrentView('detail');
  };

  const handleViewChange = (view: 'list' | 'detail' | 'history') => {
    setCurrentView(view);
    if (view !== 'detail') {
      setSelectedOrder(undefined);
    }
  };

  if (!isLoggedIn) {
    return (
      <>
        <LoginPage onLogin={handleLogin} theme={theme} onToggleTheme={toggleTheme} />
        <Toaster />
      </>
    );
  }

  return (
    <>
      <AppLayout 
        currentView={currentView} 
        onViewChange={handleViewChange}
        onLogout={handleLogout}
        theme={theme}
        onToggleTheme={toggleTheme}
      >
        {currentView === 'list' && (
          <WorkOrdersList onSelectOrder={handleSelectOrder} />
        )}
        {currentView === 'detail' && (
          <WorkOrderDetail order={selectedOrder} />
        )}
        {currentView === 'history' && (
          <RecentHistory onSelectOrder={handleSelectOrder} />
        )}
      </AppLayout>
      <Toaster />
    </>
  );
}
