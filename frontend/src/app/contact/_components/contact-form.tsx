"use client";

import { useState } from "react";

import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { CheckCircle2, Loader2, Send } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

import {
    contactSchema,
    type ContactFormValues,
} from "@/lib/validation/contact.validations";

import type {
    ContactRequest,
    ContactResponse,
} from "@/types/contact.types";

const defaultValues: ContactFormValues = {
    firstName: "",
    lastName: "",
    email: "",
    subject: "",
    message: "",
};

export function ContactForm() {
    const [submitted, setSubmitted] = useState(false);

    const form = useForm<ContactFormValues>({
        resolver: zodResolver(contactSchema),
        mode: "onChange",
        defaultValues,
    });

    const onSubmit = async (values: ContactFormValues) => {
        const payload: ContactRequest = {
            firstName: values.firstName.trim(),
            lastName: values.lastName.trim(),
            email: values.email.trim(),
            subject: values.subject.trim(),
            message: values.message.trim(),
        };

        try {
            // TODO: Replace with your backend API call.
            //
            // const response = await api.post<ContactResponse>(
            //     "/api/contact",
            //     payload,
            // );

            const response: ContactResponse = {
                message:
                    "Thanks for reaching out. Our support team will get back to you soon.",
            };

            console.info("Contact request:", payload);
            console.info("Contact response:", response);

            setSubmitted(true);
            form.reset(defaultValues);
        } catch {
            form.setError("root", {
                message:
                    "We couldn't send your message right now. Please try again.",
            });
        }
    };

    if (submitted) {
        return (
            <div className="flex min-h-120 flex-col items-center justify-center rounded-3xl border border-primary/10 bg-primary/5 px-6 py-12 text-center">
                <div className="flex size-16 items-center justify-center rounded-full bg-primary/10 text-primary">
                    <CheckCircle2 className="size-8" />
                </div>

                <h3 className="mt-6 text-2xl font-semibold text-foreground">
                    Message sent successfully
                </h3>

                <p className="mt-3 max-w-md text-sm leading-6 text-muted-foreground sm:text-base">
                    Thank you for reaching out to Arovia. Our team will review
                    your message and get back to you as soon as possible.
                </p>

                <Button
                    type="button"
                    variant="outline"
                    className="mt-7 h-11 rounded-xl px-6"
                    onClick={() => setSubmitted(false)}
                >
                    Send another message
                </Button>
            </div>
        );
    }

    return (
        <form
            onSubmit={form.handleSubmit(onSubmit)}
            className="space-y-6"
            noValidate
        >
            <div className="grid gap-5 sm:grid-cols-2">
                <Controller
                    name="firstName"
                    control={form.control}
                    render={({ field, fieldState }) => (
                        <div className="space-y-2.5">
                            <label
                                htmlFor="firstName"
                                className="block text-sm font-medium"
                            >
                                First name
                            </label>

                            <Input
                                {...field}
                                id="firstName"
                                placeholder="Enter your first name"
                                autoComplete="given-name"
                                aria-invalid={fieldState.invalid}
                                className="h-11 rounded-xl bg-background"
                            />

                            {fieldState.error && (
                                <p className="text-sm text-destructive">
                                    {fieldState.error.message}
                                </p>
                            )}
                        </div>
                    )}
                />

                <Controller
                    name="lastName"
                    control={form.control}
                    render={({ field, fieldState }) => (
                        <div className="space-y-2.5">
                            <label
                                htmlFor="lastName"
                                className="block text-sm font-medium"
                            >
                                Last name
                            </label>

                            <Input
                                {...field}
                                id="lastName"
                                placeholder="Enter your last name"
                                autoComplete="family-name"
                                aria-invalid={fieldState.invalid}
                                className="h-11 rounded-xl bg-background"
                            />

                            {fieldState.error && (
                                <p className="text-sm text-destructive">
                                    {fieldState.error.message}
                                </p>
                            )}
                        </div>
                    )}
                />
            </div>

            <Controller
                name="email"
                control={form.control}
                render={({ field, fieldState }) => (
                    <div className="space-y-2.5">
                        <label
                            htmlFor="email"
                            className="block text-sm font-medium"
                        >
                            Email address
                        </label>

                        <Input
                            {...field}
                            id="email"
                            type="email"
                            inputMode="email"
                            placeholder="Enter your email"
                            autoComplete="email"
                            aria-invalid={fieldState.invalid}
                            className="h-11 rounded-xl bg-background"
                        />

                        {fieldState.error && (
                            <p className="text-sm text-destructive">
                                {fieldState.error.message}
                            </p>
                        )}
                    </div>
                )}
            />

            <Controller
                name="subject"
                control={form.control}
                render={({ field, fieldState }) => (
                    <div className="space-y-2.5">
                        <label
                            htmlFor="subject"
                            className="block text-sm font-medium"
                        >
                            Subject
                        </label>

                        <Input
                            {...field}
                            id="subject"
                            placeholder="How can we help?"
                            aria-invalid={fieldState.invalid}
                            className="h-11 rounded-xl bg-background"
                        />

                        {fieldState.error && (
                            <p className="text-sm text-destructive">
                                {fieldState.error.message}
                            </p>
                        )}
                    </div>
                )}
            />

            <Controller
                name="message"
                control={form.control}
                render={({ field, fieldState }) => (
                    <div className="space-y-2.5">
                        <div className="flex items-center justify-between gap-4">
                            <label
                                htmlFor="message"
                                className="block text-sm font-medium"
                            >
                                Message
                            </label>

                            <span className="text-xs text-muted-foreground">
                                {field.value.length}/2000
                            </span>
                        </div>

                        <Textarea
                            {...field}
                            id="message"
                            placeholder="Tell us how we can help..."
                            rows={7}
                            aria-invalid={fieldState.invalid}
                            className="resize-none rounded-xl bg-background"
                        />

                        {fieldState.error && (
                            <p className="text-sm text-destructive">
                                {fieldState.error.message}
                            </p>
                        )}
                    </div>
                )}
            />

            {form.formState.errors.root?.message && (
                <p className="text-sm text-destructive">
                    {form.formState.errors.root.message}
                </p>
            )}

            <Button
                type="submit"
                disabled={
                    !form.formState.isValid ||
                    form.formState.isSubmitting
                }
                className="group h-11 w-full rounded-xl sm:w-auto sm:min-w-40"
            >
                {form.formState.isSubmitting ? (
                    <>
                        <Loader2 className="mr-2 size-4 animate-spin" />
                        Sending...
                    </>
                ) : (
                    <>
                        Send message
                        <Send className="ml-2 size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
                    </>
                )}
            </Button>

            <p className="text-xs leading-5 text-muted-foreground">
                Please do not include passwords, payment-card details, or
                other highly sensitive information in this form.
            </p>
        </form>
    );
}