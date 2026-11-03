import { DashboardShell } from "@/features/dashboard/components/dashboard-shell";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return <DashboardShell kind="admin">{children}</DashboardShell>;
}
