"use client";

import { Search } from "lucide-react";
import { useEffect, useRef, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";

interface SearchDialogProps {
    variant?: "icon" | "field";
    appearance?: "default" | "overlay";
    onBeforeOpen?: () => void;
}

export function SearchDialog({
    variant = "icon",
    appearance = "default",
    onBeforeOpen,
}: SearchDialogProps) {
    const [open, setOpen] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const inputRef = useRef<HTMLInputElement>(null);
    const router = useRouter();

    const isOverlay = appearance === "overlay";

    const handleOpen = () => {
        onBeforeOpen?.();
        setOpen(true);
    };

    const handleSearch = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        const query = searchTerm.trim();
        setOpen(false);
        router.push(
            query
                ? `/otc/search?q=${encodeURIComponent(query)}`
                : "/otc/search",
        );
    };

    useEffect(() => {
        if (!open) {
            return;
        }

        const frame = requestAnimationFrame(() => {
            inputRef.current?.focus();
        });

        return () => cancelAnimationFrame(frame);
    }, [open]);

    return (
        <>
            {variant === "icon" ? (
                <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    aria-label="Search healthcare products"
                    onClick={handleOpen}
                    className={
                        isOverlay
                            ? "size-9 cursor-pointer rounded-lg text-white hover:bg-white/10 hover:text-white"
                            : "size-9 cursor-pointer rounded-lg"
                    }
                >
                    <Search className="size-5" />
                </Button>
            ) : (
                <Button
                    type="button"
                    variant="outline"
                    onClick={handleOpen}
                    className="h-11 w-full justify-start gap-3 rounded-lg px-3.5 font-normal text-muted-foreground"
                >
                    <Search className="size-4 shrink-0" />
                    <span>
                        Search healthcare products...
                    </span>
                </Button>
            )}

            <Dialog
                open={open}
                onOpenChange={setOpen}
            >
                <DialogContent className="gap-0 overflow-hidden p-0 sm:max-w-2xl">
                    <DialogHeader className="border-b px-6 py-5">
                        <DialogTitle>
                            Search Arovia
                        </DialogTitle>

                        <DialogDescription>
                            Find medicines, wellness products,
                            personal care and healthcare devices.
                        </DialogDescription>
                    </DialogHeader>

                    <form onSubmit={handleSearch} className="p-6">
                        <div className="relative">
                            <Search
                                aria-hidden="true"
                                className="pointer-events-none absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
                            />

                            <Input
                                ref={inputRef}
                                type="search"
                                value={searchTerm}
                                onChange={(event) => setSearchTerm(event.target.value)}
                                className="h-12 rounded-lg pl-10 pr-24"
                                placeholder="Search medicines, wellness & healthcare products..."
                                aria-label="Search healthcare products"
                            />
                            <Button
                                type="submit"
                                className="absolute right-1.5 top-1.5 h-9 rounded-md px-3"
                            >
                                Search
                            </Button>
                        </div>

                        <p className="mt-3 text-xs text-muted-foreground">
                            Search our medicine, wellness and care catalog.
                        </p>
                    </form>
                </DialogContent>
            </Dialog>
        </>
    );
}
