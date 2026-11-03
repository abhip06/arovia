import { AdminProductManager } from "@/features/dashboard/components/admin-product-manager";

export default async function AdminProductPage({ params }: { params: Promise<{ slug: string; sku: string }> }) {
  const { slug, sku } = await params;
  return <AdminProductManager slug={slug} sku={sku} />;
}
