import { DashboardShell } from "@/features/dashboard/components/dashboard-shell";

export default function InventoryLayout({ children }: { children: React.ReactNode }) {
  return <DashboardShell kind="inventory">{children}</DashboardShell>;
}
