"use client";

import { useState, type Dispatch, type ReactNode, type SetStateAction } from "react";
import { Check, ChevronRight, Copy, PackagePlus, Tag } from "lucide-react";
import {
  useForm,
  useWatch,
  type UseFormRegisterReturn,
  type UseFormReturn,
} from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { productTypes } from "@/features/catalog/types/catalog.types";
import { queryKeys } from "@/lib/query/query-keys";
import { adminCatalogApi, type CreatedProduct } from "../api/admin-catalog.api";

const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

const productSchema = z.object({
  name: z.string().trim().min(2, "Enter at least 2 characters.").max(160),
  productType: z.string().regex(/^[1-6]$/, "Choose a product type."),
  manufacturerId: z.string().regex(uuidPattern, "Create a manufacturer above or enter its UUID."),
  brandId: z.string().optional().refine(
    (value) => !value || uuidPattern.test(value),
    "Enter a valid brand UUID.",
  ),
  requiresPrescription: z.boolean(),
  hsnCode: z.string().trim().regex(/^\d{4,8}$/, "Enter a 4–8 digit HSN code."),
  taxRate: z.string().refine(
    (value) => Number.isFinite(Number(value)) && Number(value) >= 0 && Number(value) <= 100,
    "Enter a tax rate from 0 to 100.",
  ),
  returnable: z.boolean(),
  returnWindowDays: z.string().refine(
    (value) => Number.isInteger(Number(value)) && Number(value) >= 0 && Number(value) <= 365,
    "Enter 0–365 days.",
  ),
});

const variantSchema = z.object({
  barcode: z.string().trim().max(80).optional(),
  packageQuantity: z.string().refine(
    (value) => Number.isFinite(Number(value)) && Number(value) > 0,
    "Enter a quantity greater than zero.",
  ),
  packageUnit: z.string().trim().min(1, "Enter a package unit.").max(40),
  netQuantity: z.string().optional().refine(
    (value) => !value || (Number.isFinite(Number(value)) && Number(value) > 0),
    "Enter a quantity greater than zero.",
  ),
  netQuantityUnit: z.string().trim().max(40).optional(),
});

const priceSchema = z.object({
  sellingPrice: z.string().refine(
    (value) => Number.isFinite(Number(value)) && Number(value) > 0,
    "Enter a price greater than zero.",
  ),
  effectiveFrom: z.string().optional(),
});

type ProductValues = z.infer<typeof productSchema>;
type VariantValues = z.infer<typeof variantSchema>;
type PriceValues = z.infer<typeof priceSchema>;
type CreatedVariant = Awaited<ReturnType<typeof adminCatalogApi.createVariant>>;
type Step = "product" | "variant" | "price" | "complete";

const defaultProductValues: ProductValues = {
  name: "",
  productType: "",
  manufacturerId: "",
  brandId: "",
  requiresPrescription: false,
  hsnCode: "",
  taxRate: "0",
  returnable: false,
  returnWindowDays: "0",
};

const defaultVariantValues: VariantValues = {
  barcode: "",
  packageQuantity: "1",
  packageUnit: "pack",
  netQuantity: "",
  netQuantityUnit: "",
};

const defaultPriceValues: PriceValues = {
  sellingPrice: "",
  effectiveFrom: "",
};

function Field({
  label,
  error,
  children,
}: {
  label: string;
  error?: string;
  children: ReactNode;
}) {
  return (
    <label className="grid gap-1.5 text-xs font-medium text-foreground">
      {label}
      {children}
      {error && <span className="font-normal leading-4 text-destructive">{error}</span>}
    </label>
  );
}

function StepMark({
  active,
  complete,
  children,
}: {
  active: boolean;
  complete: boolean;
  children: ReactNode;
}) {
  const textStyle = active
    ? "font-semibold text-primary"
    : complete
      ? "font-medium text-foreground"
      : "text-muted-foreground";
  const markStyle = active
    ? "border-primary bg-primary/10"
    : complete
      ? "border-emerald-600/30 bg-emerald-500/10 text-emerald-700 dark:text-emerald-300"
      : "border-border";

  return (
    <div className={`flex items-center gap-2 text-xs ${textStyle}`}>
      <span className={`grid size-6 place-items-center rounded-full border ${markStyle}`}>
        {complete && <Check className="size-3.5" />}
      </span>
      {children}
    </div>
  );
}

function ProductIdentityFields({
  form,
  referenceName,
  setReferenceName,
  addManufacturer,
  addBrand,
}: {
  form: UseFormReturn<ProductValues>;
  referenceName: { manufacturer: string; brand: string };
  setReferenceName: Dispatch<SetStateAction<{ manufacturer: string; brand: string }>>;
  addManufacturer: { isPending: boolean; mutate: (name: string) => void };
  addBrand: { isPending: boolean; mutate: (name: string) => void };
}) {
  return (
    <section className="space-y-3">
      <div>
        <h3 className="text-sm font-semibold">Product identity</h3>
        <p className="mt-1 text-xs leading-5 text-muted-foreground">
          Reference list APIs omit IDs. Create a manufacturer or brand here, or enter an existing UUID.
        </p>
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        <Field label="Product name" error={form.formState.errors.name?.message}>
          <Input {...form.register("name")} placeholder="e.g. Daily Vitamin C" />
        </Field>
        <Field label="Product type" error={form.formState.errors.productType?.message}>
          <select
            className="h-9 rounded-lg border border-input bg-background px-3 text-sm outline-none focus:ring-2 focus:ring-ring"
            {...form.register("productType")}
          >
            <option value="">Choose type</option>
            {productTypes.map((item, index) => (
              <option key={item.apiType} value={index + 1}>{item.label}</option>
            ))}
          </select>
        </Field>
      </div>

      <div className="grid gap-3 rounded-xl border bg-background p-3 sm:grid-cols-2">
        <ReferenceField
          label="Manufacturer UUID"
          newLabel="New manufacturer name"
          placeholder="Create below or paste an ID"
          newValue={referenceName.manufacturer}
          isPending={addManufacturer.isPending}
          error={form.formState.errors.manufacturerId?.message}
          onNewValue={(value) => setReferenceName((current) => ({ ...current, manufacturer: value }))}
          onAdd={() => addManufacturer.mutate(referenceName.manufacturer.trim())}
          canAdd={referenceName.manufacturer.trim().length >= 2}
          registration={form.register("manufacturerId")}
        />
        <ReferenceField
          label="Brand UUID (optional)"
          newLabel="New brand name"
          placeholder="Leave blank if not branded"
          newValue={referenceName.brand}
          isPending={addBrand.isPending}
          error={form.formState.errors.brandId?.message}
          onNewValue={(value) => setReferenceName((current) => ({ ...current, brand: value }))}
          onAdd={() => addBrand.mutate(referenceName.brand.trim())}
          canAdd={referenceName.brand.trim().length >= 2}
          registration={form.register("brandId")}
        />
      </div>
    </section>
  );
}

function ReferenceField({
  label,
  newLabel,
  placeholder,
  newValue,
  isPending,
  error,
  onNewValue,
  onAdd,
  canAdd,
  registration,
}: {
  label: string;
  newLabel: string;
  placeholder: string;
  newValue: string;
  isPending: boolean;
  error?: string;
  onNewValue: (value: string) => void;
  onAdd: () => void;
  canAdd: boolean;
  registration: UseFormRegisterReturn;
}) {
  return (
    <div className="space-y-2">
      <Field label={label} error={error}>
        <Input {...registration} placeholder={placeholder} />
      </Field>
      <div className="flex gap-2">
        <Input
          aria-label={newLabel}
          value={newValue}
          onChange={(event) => onNewValue(event.target.value)}
          placeholder={newLabel}
        />
        <Button type="button" variant="outline" disabled={isPending || !canAdd} onClick={onAdd}>
          {isPending ? "Adding…" : "Add"}
        </Button>
      </div>
    </div>
  );
}

export function AdminProductWizard({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const queryClient = useQueryClient();
  const [step, setStep] = useState<Step>("product");
  const [product, setProduct] = useState<CreatedProduct | null>(null);
  const [variant, setVariant] = useState<CreatedVariant | null>(null);
  const [referenceName, setReferenceName] = useState({ manufacturer: "", brand: "" });
  const [activeProduct, setActiveProduct] = useState(false);
  const [activeVariant, setActiveVariant] = useState(false);

  const productForm = useForm<ProductValues>({
    resolver: zodResolver(productSchema),
    defaultValues: defaultProductValues,
  });
  const variantForm = useForm<VariantValues>({
    resolver: zodResolver(variantSchema),
    defaultValues: defaultVariantValues,
  });
  const priceForm = useForm<PriceValues>({
    resolver: zodResolver(priceSchema),
    defaultValues: defaultPriceValues,
  });
  const returnable = useWatch({ control: productForm.control, name: "returnable" });

  const refreshCatalog = () => {
    void queryClient.invalidateQueries({ queryKey: queryKeys.catalog.all });
  };

  const createManufacturer = useMutation({
    mutationFn: adminCatalogApi.createManufacturer,
    onSuccess: (created) => {
      productForm.setValue("manufacturerId", created.id, { shouldValidate: true });
      setReferenceName((current) => ({ ...current, manufacturer: "" }));
      void queryClient.invalidateQueries({ queryKey: queryKeys.catalog.manufacturers });
      toast.success(`${created.name} added as manufacturer`);
    },
    onError: (error) => toast.error(error.message || "Could not create the manufacturer."),
  });

  const createBrand = useMutation({
    mutationFn: adminCatalogApi.createBrand,
    onSuccess: (created) => {
      productForm.setValue("brandId", created.id, { shouldValidate: true });
      setReferenceName((current) => ({ ...current, brand: "" }));
      void queryClient.invalidateQueries({ queryKey: queryKeys.catalog.brands });
      toast.success(`${created.name} added as brand`);
    },
    onError: (error) => toast.error(error.message || "Could not create the brand."),
  });

  const createProduct = useMutation({
    mutationFn: (value: ProductValues) => adminCatalogApi.createProduct({
      name: value.name.trim(),
      productType: Number(value.productType),
      manufacturerId: value.manufacturerId,
      brandId: value.brandId || null,
      requiresPrescription: value.requiresPrescription,
      hsnCode: value.hsnCode.trim(),
      taxRate: Number(value.taxRate),
      returnable: value.returnable,
      returnWindowDays: value.returnable ? Number(value.returnWindowDays) : 0,
    }),
    onSuccess: (created) => {
      setProduct(created);
      setStep("variant");
      refreshCatalog();
      toast.success("Product created. Add its first sellable variant.");
    },
    onError: (error) => toast.error(error.message || "Product creation failed."),
  });

  const createVariant = useMutation({
    mutationFn: (value: VariantValues) => adminCatalogApi.createVariant(product!.id, {
      barcode: value.barcode?.trim() || null,
      packageQuantity: Number(value.packageQuantity),
      packageUnit: value.packageUnit.trim(),
      netQuantity: value.netQuantity ? Number(value.netQuantity) : null,
      netQuantityUnit: value.netQuantityUnit?.trim() || null,
    }),
    onSuccess: (created) => {
      setVariant(created);
      setStep("price");
      refreshCatalog();
      toast.success(`Variant ${created.sku} created`);
    },
    onError: (error) => toast.error(error.message || "Variant creation failed."),
  });

  const createPrice = useMutation({
    mutationFn: (value: PriceValues) => adminCatalogApi.createPrice(product!.id, variant!.id, {
      sellingPrice: Number(value.sellingPrice),
      effectiveFrom: value.effectiveFrom ? new Date(value.effectiveFrom).toISOString() : null,
    }),
    onSuccess: () => {
      setStep("complete");
      refreshCatalog();
      toast.success("Initial selling price saved");
    },
    onError: (error) => toast.error(error.message || "Could not save the initial price."),
  });

  const activateProduct = useMutation({
    mutationFn: () => adminCatalogApi.setProductStatus(product!.id, "activate"),
    onSuccess: () => {
      setActiveProduct(true);
      refreshCatalog();
      toast.success("Product activated");
    },
    onError: (error) => toast.error(error.message || "Product activation failed."),
  });

  const activateVariant = useMutation({
    mutationFn: () => adminCatalogApi.setVariantStatus(variant!.id, "activate"),
    onSuccess: () => {
      setActiveVariant(true);
      refreshCatalog();
      toast.success("Variant activated");
    },
    onError: (error) => toast.error(error.message || "Variant activation failed."),
  });

  const pending = createProduct.isPending || createVariant.isPending || createPrice.isPending
    || createManufacturer.isPending || createBrand.isPending
    || activateProduct.isPending || activateVariant.isPending;

  const resetWizard = () => {
    setStep("product");
    setProduct(null);
    setVariant(null);
    setReferenceName({ manufacturer: "", brand: "" });
    setActiveProduct(false);
    setActiveVariant(false);
    productForm.reset(defaultProductValues);
    variantForm.reset(defaultVariantValues);
    priceForm.reset(defaultPriceValues);
  };

  const close = (next: boolean) => {
    if (!next && pending) return;
    onOpenChange(next);
  };

  const copyId = async (value: string, label: string) => {
    try {
      await navigator.clipboard.writeText(value);
      toast.success(`${label} copied`);
    } catch {
      toast.error(`Could not copy the ${label.toLowerCase()}.`);
    }
  };

  const stepKeys: Step[] = ["product", "variant", "price", "complete"];
  const stepLabels = ["Product", "Variant", "Price", "Ready"];
  const typeLabel = productTypes.find((item) => item.apiType === product?.productType)?.label ?? product?.productType;

  return (
    <Dialog open={open} onOpenChange={close}>
      <DialogContent className="max-h-[92vh] overflow-y-auto sm:max-w-3xl">
        <DialogHeader>
          <DialogTitle className="text-xl">Add a product</DialogTitle>
          <DialogDescription>
            Create a product, its first package variant, and the initial selling price using the catalog APIs.
          </DialogDescription>
        </DialogHeader>

        <ol className="grid grid-cols-2 gap-3 rounded-xl border bg-muted/25 p-3 sm:grid-cols-4">
          {stepLabels.map((label, index) => (
            <li key={label}>
              <StepMark
                active={stepKeys[index] === step}
                complete={stepKeys.indexOf(step) > index}
              >
                {label}
              </StepMark>
            </li>
          ))}
        </ol>

        {step === "product" && (
          <form
            onSubmit={productForm.handleSubmit(
              (value) => createProduct.mutate(value),
              () => toast.error("Review the product fields and fix the highlighted values."),
            )}
            className="space-y-5"
          >
            <ProductIdentityFields
              form={productForm}
              referenceName={referenceName}
              setReferenceName={setReferenceName}
              addManufacturer={createManufacturer}
              addBrand={createBrand}
            />

            <section className="space-y-3">
              <h3 className="text-sm font-semibold">Compliance & returns</h3>
              <div className="grid gap-3 sm:grid-cols-2">
                <Field label="HSN code" error={productForm.formState.errors.hsnCode?.message}>
                  <Input inputMode="numeric" {...productForm.register("hsnCode")} placeholder="8 digit code" />
                </Field>
                <Field label="Tax rate (%)" error={productForm.formState.errors.taxRate?.message}>
                  <Input type="number" min="0" max="100" step="0.01" {...productForm.register("taxRate")} />
                </Field>
                <Field label="Return window (days)" error={productForm.formState.errors.returnWindowDays?.message}>
                  <Input type="number" min="0" max="365" step="1" disabled={!returnable} {...productForm.register("returnWindowDays")} />
                </Field>
                <div className="grid gap-2 sm:pt-5">
                  <label className="flex items-center gap-2 text-xs">
                    <input type="checkbox" className="accent-primary" {...productForm.register("requiresPrescription")} />
                    Prescription required
                  </label>
                  <label className="flex items-center gap-2 text-xs">
                    <input type="checkbox" className="accent-primary" {...productForm.register("returnable")} />
                    Allow returns
                  </label>
                </div>
              </div>
            </section>

            <DialogFooter>
              <Button type="button" variant="outline" disabled={pending} onClick={() => close(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={pending}>
                {createProduct.isPending ? "Creating product…" : "Continue to variant"}
                <ChevronRight className="ml-1 size-4" />
              </Button>
            </DialogFooter>
          </form>
        )}

        {step === "variant" && (
          <form
            onSubmit={variantForm.handleSubmit(
              (value) => createVariant.mutate(value),
              () => toast.error("Review the variant details."),
            )}
            className="space-y-4"
          >
            <div className="rounded-xl border bg-primary/[0.035] p-4">
              <div className="flex items-center gap-2 text-sm font-semibold">
                <PackagePlus className="size-4 text-primary" /> {product?.name}
              </div>
              <p className="mt-1 text-xs text-muted-foreground">
                {typeLabel} · Product ID {product?.id}
              </p>
            </div>

            <div className="grid gap-3 sm:grid-cols-2">
              <Field label="Package quantity" error={variantForm.formState.errors.packageQuantity?.message}>
                <Input type="number" min="0.01" step="0.01" {...variantForm.register("packageQuantity")} />
              </Field>
              <Field label="Package unit" error={variantForm.formState.errors.packageUnit?.message}>
                <Input {...variantForm.register("packageUnit")} placeholder="tablet, bottle, pack" />
              </Field>
              <Field label="Net quantity (optional)" error={variantForm.formState.errors.netQuantity?.message}>
                <Input type="number" min="0.01" step="0.01" {...variantForm.register("netQuantity")} />
              </Field>
              <Field label="Net quantity unit">
                <Input {...variantForm.register("netQuantityUnit")} placeholder="ml, g, tablets" />
              </Field>
              <Field label="Barcode (optional)" error={variantForm.formState.errors.barcode?.message}>
                <Input {...variantForm.register("barcode")} />
              </Field>
            </div>

            <DialogFooter>
              <Button type="submit" disabled={pending}>
                {createVariant.isPending ? "Creating variant…" : "Continue to price"}
                <ChevronRight className="ml-1 size-4" />
              </Button>
            </DialogFooter>
          </form>
        )}

        {step === "price" && (
          <form
            onSubmit={priceForm.handleSubmit(
              (value) => createPrice.mutate(value),
              () => toast.error("Enter a valid selling price."),
            )}
            className="space-y-4"
          >
            <div className="rounded-xl border bg-primary/[0.035] p-4">
              <div className="flex items-center gap-2 text-sm font-semibold">
                <Tag className="size-4 text-primary" /> {product?.name} · {variant?.sku}
              </div>
              <p className="mt-1 text-xs text-muted-foreground">
                {variant?.packageQuantity} {variant?.packageUnit} · Set the initial selling price.
              </p>
            </div>

            <div className="grid gap-3 sm:grid-cols-2">
              <Field label="Selling price (₹)" error={priceForm.formState.errors.sellingPrice?.message}>
                <Input type="number" min="0.01" step="0.01" {...priceForm.register("sellingPrice")} />
              </Field>
              <Field label="Effective from (optional)">
                <Input type="datetime-local" {...priceForm.register("effectiveFrom")} />
              </Field>
            </div>

            <DialogFooter>
              <Button type="submit" disabled={pending}>
                {createPrice.isPending ? "Saving price…" : "Save initial price"}
                <Check className="ml-1 size-4" />
              </Button>
            </DialogFooter>
          </form>
        )}

        {step === "complete" && product && variant && (
          <ProductSetupComplete
            product={product}
            variant={variant}
            price={Number(priceForm.getValues("sellingPrice"))}
            activeProduct={activeProduct}
            activeVariant={activeVariant}
            activateProduct={activateProduct.isPending}
            activateVariant={activateVariant.isPending}
            onActivateProduct={() => activateProduct.mutate()}
            onActivateVariant={() => activateVariant.mutate()}
            onCopyProduct={() => void copyId(product.id, "Product ID")}
            onCopyVariant={() => void copyId(variant.id, "Variant ID")}
            onDone={() => {
              resetWizard();
              close(false);
            }}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

function ProductSetupComplete({
  product,
  variant,
  price,
  activeProduct,
  activeVariant,
  activateProduct,
  activateVariant,
  onActivateProduct,
  onActivateVariant,
  onCopyProduct,
  onCopyVariant,
  onDone,
}: {
  product: CreatedProduct;
  variant: CreatedVariant;
  price: number;
  activeProduct: boolean;
  activeVariant: boolean;
  activateProduct: boolean;
  activateVariant: boolean;
  onActivateProduct: () => void;
  onActivateVariant: () => void;
  onCopyProduct: () => void;
  onCopyVariant: () => void;
  onDone: () => void;
}) {
  return (
    <div className="space-y-4">
      <div className="rounded-xl border border-emerald-600/20 bg-emerald-500/[0.06] p-4">
        <div className="flex items-center gap-2 font-semibold">
          <span className="grid size-7 place-items-center rounded-full bg-emerald-500/15 text-emerald-700 dark:text-emerald-300">
            <Check className="size-4" />
          </span>
          Product setup is complete
        </div>
        <p className="mt-2 text-sm">
          {product.name} · {variant.sku} · ₹{price.toLocaleString("en-IN")}
        </p>
        <p className="mt-2 break-all text-[10px] text-muted-foreground">Product ID · {product.id}</p>
        <p className="mt-1 break-all text-[10px] text-muted-foreground">Variant ID · {variant.id}</p>
      </div>

      <p className="text-xs leading-5 text-muted-foreground">
        The product and variant are created in their backend status. Activate both when they are ready to appear in the storefront.
      </p>

      <div className="flex flex-wrap gap-2">
        <Button variant={activeProduct ? "secondary" : "outline"} disabled={activeProduct || activateProduct} onClick={onActivateProduct}>
          {activeProduct ? "Product active" : activateProduct ? "Activating…" : "Activate product"}
        </Button>
        <Button variant={activeVariant ? "secondary" : "outline"} disabled={activeVariant || activateVariant} onClick={onActivateVariant}>
          {activeVariant ? "Variant active" : activateVariant ? "Activating…" : "Activate variant"}
        </Button>
        <Button variant="ghost" size="sm" onClick={onCopyProduct}>
          <Copy className="mr-1 size-3.5" /> Copy product ID
        </Button>
        <Button variant="ghost" size="sm" onClick={onCopyVariant}>
          <Copy className="mr-1 size-3.5" /> Copy variant ID
        </Button>
      </div>

      <div className="rounded-lg bg-muted/40 p-3 text-xs text-muted-foreground">
        {activeProduct && activeVariant
          ? "Both records are active. Refresh the catalog to review the published SKU."
          : "Activation can be done now or later from the catalog detail page. Keep these IDs for unpublished records."}
      </div>

      <DialogFooter>
        <Button onClick={onDone}>Done</Button>
      </DialogFooter>
    </div>
  );
}
