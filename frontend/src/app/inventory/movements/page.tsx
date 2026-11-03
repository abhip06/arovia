import { InventoryWorkspace } from "@/features/dashboard/components/inventory-workspace";

export default async function InventoryMovementsPage({ searchParams }: { searchParams: Promise<{ stockId?: string }> }) {
  const { stockId } = await searchParams;
  return <InventoryWorkspace view="movements" initialStockId={stockId ?? ""} />;
}
