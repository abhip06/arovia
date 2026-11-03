"use client";

import { useEffect } from "react";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { LoaderCircle, MapPin } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";

import {
    useCreateAddress,
    useUpdateAddress,
} from "@/features/addresses/hooks/use-addresses";

import type {
    Address,
    CreateAddressRequest,
} from "@/types/address.types";

const addressSchema = z.object({
    label: z.string().trim().max(100),
    recipientName: z
        .string()
        .trim()
        .min(2, "Enter the recipient's name.")
        .max(100),
    recipientPhone: z
        .string()
        .trim()
        .max(20, "Phone number is too long."),
    addressLine1: z
        .string()
        .trim()
        .min(3, "Enter the street address.")
        .max(200),
    addressLine2: z.string().trim().max(200),
    city: z
        .string()
        .trim()
        .min(2, "Enter the city.")
        .max(100),
    state: z
        .string()
        .trim()
        .min(2, "Enter the state.")
        .max(100),
    postalCode: z
        .string()
        .trim()
        .regex(/^\d{6}$/, "Enter a valid 6-digit pincode."),
    country: z
        .string()
        .trim()
        .min(2, "Enter the country.")
        .max(50),
});

type AddressFormValues = z.infer<typeof addressSchema>;

const defaultValues: AddressFormValues = {
    label: "",
    recipientName: "",
    recipientPhone: "",
    addressLine1: "",
    addressLine2: "",
    city: "",
    state: "",
    postalCode: "",
    country: "India",
};

interface AddressFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    address?: Address | null;
    onSaved?: (address: Address) => void;
    onCreated?: (address: Address) => void;
    onUpdated?: (address: Address) => void;
}

export function AddressFormDialog({
    open,
    onOpenChange,
    address = null,
    onSaved,
    onCreated,
    onUpdated,
}: AddressFormDialogProps) {
    const createAddress = useCreateAddress();
    const updateAddress = useUpdateAddress();

    const isEditing = Boolean(address);
    const isPending =
        createAddress.isPending || updateAddress.isPending;

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<AddressFormValues>({
        resolver: zodResolver(addressSchema),
        defaultValues,
    });

    useEffect(() => {
        if (!open) {
            return;
        }

        if (address) {
            reset({
                label: address.label ?? "",
                recipientName: address.recipientName,
                recipientPhone: address.recipientPhone ?? "",
                addressLine1: address.addressLine1,
                addressLine2: address.addressLine2 ?? "",
                city: address.city,
                state: address.state,
                postalCode: address.postalCode,
                country: address.country,
            });
        } else {
            reset(defaultValues);
        }
    }, [open, address, reset]);

    const onSubmit = async (values: AddressFormValues) => {
        const request: CreateAddressRequest = {
            label: values.label.trim() || "Home",
            recipientName: values.recipientName.trim(),
            recipientPhone:
                values.recipientPhone.trim() || null,
            addressLine1: values.addressLine1.trim(),
            addressLine2:
                values.addressLine2.trim() || null,
            city: values.city.trim(),
            state: values.state.trim(),
            postalCode: values.postalCode.trim(),
            country: values.country.trim(),
        };

        try {
            const savedAddress = address
                ? await updateAddress.mutateAsync({
                      addressId: address.id,
                      request,
                  })
                : await createAddress.mutateAsync(request);

            toast.success(
                isEditing
                    ? "Address updated successfully."
                    : "Address saved successfully.",
            );

            reset(defaultValues);
            onOpenChange(false);

            if (onSaved) {
                onSaved(savedAddress);
            } else if (isEditing) {
                onUpdated?.(savedAddress);
            } else {
                onCreated?.(savedAddress);
            }
        } catch (error) {
            toast.error(
                error instanceof Error
                    ? error.message
                    : isEditing
                      ? "Could not update the address."
                      : "Could not save the address.",
            );
        }
    };

    return (
        <Dialog
            open={open}
            onOpenChange={(nextOpen) => {
                if (!isPending) {
                    onOpenChange(nextOpen);
                }
            }}
        >
            <DialogContent className="max-h-[90dvh] overflow-y-auto sm:max-w-xl">
                <DialogHeader>
                    <div className="mb-2 flex size-11 items-center justify-center rounded-xl bg-primary/10 text-primary">
                        <MapPin className="size-5" />
                    </div>

                    <DialogTitle>
                        {isEditing
                            ? "Edit delivery address"
                            : "Add a delivery address"}
                    </DialogTitle>

                    <DialogDescription>
                        {isEditing
                            ? "Update the details for this saved address."
                            : "Enter the address where you would like your Arovia order delivered."}
                    </DialogDescription>
                </DialogHeader>

                <form
                    onSubmit={handleSubmit(onSubmit)}
                    className="mt-4 flex flex-col gap-4"
                >
                    <Field
                        id="label"
                        label="Address label"
                        error={errors.label?.message}
                    >
                        <Input
                            id="label"
                            placeholder="Home, Work, Hostel, Other"
                            maxLength={100}
                            {...register("label")}
                        />
                    </Field>

                    <div className="grid gap-4 sm:grid-cols-2">
                        <Field
                            id="recipientName"
                            label="Recipient name"
                            error={errors.recipientName?.message}
                        >
                            <Input
                                id="recipientName"
                                autoComplete="name"
                                {...register("recipientName")}
                            />
                        </Field>

                        <Field
                            id="recipientPhone"
                            label="Phone number (optional)"
                            error={errors.recipientPhone?.message}
                        >
                            <Input
                                id="recipientPhone"
                                type="tel"
                                autoComplete="tel"
                                {...register("recipientPhone")}
                            />
                        </Field>
                    </div>

                    <Field
                        id="addressLine1"
                        label="Address line 1"
                        error={errors.addressLine1?.message}
                    >
                        <Input
                            id="addressLine1"
                            placeholder="House / flat number, street"
                            autoComplete="address-line1"
                            {...register("addressLine1")}
                        />
                    </Field>

                    <Field
                        id="addressLine2"
                        label="Address line 2 (optional)"
                        error={errors.addressLine2?.message}
                    >
                        <Input
                            id="addressLine2"
                            placeholder="Area, locality"
                            autoComplete="address-line2"
                            {...register("addressLine2")}
                        />
                    </Field>

                    <div className="grid gap-4 sm:grid-cols-2">
                        <Field
                            id="city"
                            label="City"
                            error={errors.city?.message}
                        >
                            <Input
                                id="city"
                                autoComplete="address-level2"
                                {...register("city")}
                            />
                        </Field>

                        <Field
                            id="state"
                            label="State"
                            error={errors.state?.message}
                        >
                            <Input
                                id="state"
                                autoComplete="address-level1"
                                {...register("state")}
                            />
                        </Field>
                    </div>

                    <div className="grid gap-4 sm:grid-cols-2">
                        <Field
                            id="postalCode"
                            label="Pincode"
                            error={errors.postalCode?.message}
                        >
                            <Input
                                id="postalCode"
                                inputMode="numeric"
                                autoComplete="postal-code"
                                maxLength={6}
                                {...register("postalCode")}
                            />
                        </Field>

                        <Field
                            id="country"
                            label="Country"
                            error={errors.country?.message}
                        >
                            <Input
                                id="country"
                                autoComplete="country-name"
                                {...register("country")}
                            />
                        </Field>
                    </div>

                    <div className="mt-2 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
                        <Button
                            type="button"
                            variant="outline"
                            disabled={isPending}
                            onClick={() => onOpenChange(false)}
                        >
                            Cancel
                        </Button>

                        <Button
                            type="submit"
                            disabled={isPending}
                        >
                            {isPending && (
                                <LoaderCircle className="mr-2 size-4 animate-spin" />
                            )}

                            {isPending
                                ? "Saving..."
                                : isEditing
                                  ? "Save changes"
                                  : "Save address"}
                        </Button>
                    </div>
                </form>
            </DialogContent>
        </Dialog>
    );
}

function Field({
    id,
    label,
    error,
    children,
}: {
    id: string;
    label: string;
    error?: string;
    children: React.ReactNode;
}) {
    return (
        <div className="flex flex-col gap-1.5">
            <Label htmlFor={id}>{label}</Label>
            {children}

            {error && (
                <p role="alert" className="text-xs text-destructive">
                    {error}
                </p>
            )}
        </div>
    );
}