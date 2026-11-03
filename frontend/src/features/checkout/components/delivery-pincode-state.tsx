"use client";

import { useEffect, useState, type FormEvent } from "react";
import { ArrowRight, MapPin } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useLocationStore } from "@/store/location-store";

export function DeliveryPincodeRequiredState() {
    const currentPincode = useLocationStore(
        (state) => state.pincode,
    );

    const setUserLocation = useLocationStore(
        (state) => state.setUserLocation,
    );

    const [pincode, setPincode] = useState(
        currentPincode ?? "",
    );

    const [validationError, setValidationError] =
        useState<string | null>(null);

    useEffect(() => {
        if (currentPincode && !pincode) {
            setPincode(currentPincode);
        }
    }, [currentPincode, pincode]);

    function handlePincodeChange(value: string) {
        const normalized = value.replace(/\D/g, "").slice(0, 6);

        setPincode(normalized);
        setValidationError(null);
    }

    function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        const normalized = pincode.trim();

        if (!/^\d{6}$/.test(normalized)) {
            setValidationError(
                "Enter a valid 6-digit delivery pincode.",
            );
            return;
        }

        // Save the customer-confirmed pincode to Zustand.
        // This changes the source to "User", allowing
        // useRequestPincode() to return the pincode.
        setUserLocation(normalized);

        setValidationError(null);
    }

    return (
        <main className="flex min-h-screen items-center justify-center bg-background px-4 py-12">
            <section className="w-full max-w-md">
                <div className="flex flex-col items-center text-center">
                    <div className="flex size-16 items-center justify-center rounded-2xl border border-primary/10 bg-primary/5 text-primary">
                        <MapPin className="size-7" />
                    </div>

                    <h1 className="mt-6 text-2xl font-semibold tracking-tight">
                        Where should we deliver?
                    </h1>

                    <p className="mt-2 text-sm leading-6 text-muted-foreground">
                        Enter your 6-digit delivery pincode so we can
                        check product availability and delivery options
                        for your area.
                    </p>
                </div>

                <form
                    onSubmit={handleSubmit}
                    className="mt-7 space-y-6 rounded-2xl border bg-card p-5 sm:p-6"
                >
                    <div className="space-y-2">
                        <Label htmlFor="delivery-pincode">
                            Delivery pincode
                        </Label>

                        <Input
                            id="delivery-pincode"
                            name="postalCode"
                            type="text"
                            inputMode="numeric"
                            autoComplete="postal-code"
                            placeholder="Enter 6-digit pincode"
                            maxLength={6}
                            value={pincode}
                            aria-invalid={Boolean(validationError)}
                            aria-describedby={
                                validationError
                                    ? "delivery-pincode-error"
                                    : undefined
                            }
                            onChange={(event) =>
                                handlePincodeChange(event.target.value)
                            }
                        />

                        {validationError && (
                            <p
                                id="delivery-pincode-error"
                                role="alert"
                                className="text-sm text-destructive"
                            >
                                {validationError}
                            </p>
                        )}
                    </div>

                    <Button
                        type="submit"
                        className="h-11 w-full rounded-xl"
                        disabled={!/^\d{6}$/.test(pincode)}
                    >
                        Check delivery availability
                        <ArrowRight className="ml-2 size-4" />
                    </Button>

                    <p className="text-xs leading-5 text-muted-foreground">
                        You can confirm your full delivery address
                        before placing your order.
                    </p>
                </form>
            </section>
        </main>
    );
}