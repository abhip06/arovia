import { create } from "zustand";

interface DashboardUiState {
  sidebarCollapsed: boolean;
  toggleSidebar: () => void;
  setSidebarCollapsed: (collapsed: boolean) => void;
  activeWarehouseId: string;
  setActiveWarehouseId: (warehouseId: string) => void;
  activeStockId: string;
  setActiveStockId: (stockId: string) => void;
}

export const useDashboardUiStore = create<DashboardUiState>((set) => ({
  sidebarCollapsed: false,
  toggleSidebar: () =>
    set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),
  setSidebarCollapsed: (sidebarCollapsed) => set({ sidebarCollapsed }),
  activeWarehouseId: "",
  setActiveWarehouseId: (activeWarehouseId) => set({ activeWarehouseId }),
  activeStockId: "",
  setActiveStockId: (activeStockId) => set({ activeStockId }),
}));
