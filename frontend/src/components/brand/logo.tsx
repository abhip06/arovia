import Image from "next/image";
import Link from "next/link";

import { cn } from "@/lib/utils";

interface LogoProps {
    contentType?: "HEADER" | "FOOTER";
    appearance?: "default" | "overlay";
    className?: string;
    imageClassName?: string;
    textClassName?: string;
    imageSize?: number;
    href?: string;
}

export function Logo({
    contentType,
    appearance = "default",
    className,
    imageClassName,
    textClassName,
    imageSize = 32,
    href = "/",
}: LogoProps) {
    const isOverlay = appearance === "overlay";

    return (
        <Link
            href={href}
            aria-label="Arovia home"
            className={cn(
                "inline-flex w-fit items-center gap-2 shrink-0",
                className,
            )}
        >
            <Image
                src="/images/brand-logo.png"
                alt=""
                width={imageSize}
                height={imageSize}
                priority
                className={cn(
                    "shrink-0 object-contain",
                    imageClassName,
                )}
            />

            <div className="flex flex-col">
                <span
                    className={cn(
                        "text-xl font-bold tracking-normal",
                        textClassName,
                    )}
                >
                    <span
                        className={
                            isOverlay
                                ? "text-white"
                                : "text-primary"
                        }
                    >
                        Aro
                    </span>

                    <span
                        className={
                            isOverlay
                                ? "text-white"
                                : "text-foreground"
                        }
                    >
                        via
                    </span>
                </span>

                {contentType === "FOOTER" && (
                    <p className="mt-1 text-xs text-gray-400">
                        Healthcare made simple.
                    </p>
                )}
            </div>
        </Link>
    );
}