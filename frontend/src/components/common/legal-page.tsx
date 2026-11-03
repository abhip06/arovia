import type { ReactNode } from "react";

import { Separator } from "@/components/ui/separator";

interface LegalSection {
    title: string;
    content: ReactNode;
}

interface LegalPageProps {
    title: string;
    description: string;
    lastUpdated: string;
    sections: LegalSection[];
}

export function LegalPage({
    title,
    description,
    lastUpdated,
    sections,
}: LegalPageProps) {
    return (
        <main className="bg-background pt-12">
            <div className="container mx-auto px-4 py-14 sm:py-18 lg:px-6 lg:py-20">
                <div className="mx-auto max-w-4xl">
                    {/* Header */}
                    <header className="max-w-3xl">
                        <p className="mb-3 text-sm font-semibold uppercase tracking-[0.12em] text-primary">
                            Arovia
                        </p>

                        <h1 className="text-3xl font-bold tracking-normal text-foreground sm:text-4xl lg:text-5xl">
                            {title}
                        </h1>

                        <p className="mt-5 text-base leading-7 text-muted-foreground sm:text-lg">
                            {description}
                        </p>

                        <p className="mt-5 text-sm text-muted-foreground">
                            Last updated: {lastUpdated}
                        </p>
                    </header>

                    <Separator className="my-10 sm:my-12" />

                    {/* Content */}
                    <article className="space-y-10">
                        {sections.map((section, index) => (
                            <section
                                key={section.title}
                                id={`section-${index + 1}`}
                                className="scroll-mt-24"
                            >
                                <h2 className="text-xl font-semibold tracking-normal text-foreground sm:text-2xl">
                                    {section.title}
                                </h2>

                                <div className="mt-4 space-y-4 text-sm leading-7 text-muted-foreground sm:text-base">
                                    {section.content}
                                </div>
                            </section>
                        ))}
                    </article>

                    <Separator className="my-10 sm:my-12" />

                    <p className="text-xs leading-6 text-muted-foreground">
                        This document is provided for informational purposes and
                        should be reviewed by qualified legal counsel before
                        Arovia is launched or used commercially.
                    </p>
                </div>
            </div>
        </main>
    );
}