"use client";

import { ChevronLeft, ChevronRight, MoreHorizontal } from "lucide-react";

import { Button } from "@/components/ui/button";

function getVisiblePages(currentPage: number, totalPages: number) {
    if (totalPages <= 5) {
        return Array.from({ length: totalPages }, (_, index) => index + 1);
    }

    if (currentPage <= 3) {
        return [1, 2, 3, 4, "ellipsis", totalPages] as const;
    }

    if (currentPage >= totalPages - 2) {
        return [1, "ellipsis", totalPages - 3, totalPages - 2, totalPages - 1, totalPages] as const;
    }

    return [1, "ellipsis", currentPage - 1, currentPage, currentPage + 1, "ellipsis", totalPages] as const;
}

export function CatalogPagination({
    page,
    pageSize,
    totalCount,
    totalPages,
    onPageChange,
}: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    onPageChange: (page: number) => void;
}) {
    if (totalPages <= 1) {
        return null;
    }

    const start = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
    const end = Math.min(page * pageSize, totalCount);
    const pages = getVisiblePages(page, totalPages);

    return (
        <nav
            aria-label="Product catalog pagination"
            className="flex flex-col items-center justify-between gap-4 border-t border-border/70 pt-6 sm:flex-row"
        >
            <p className="text-sm text-muted-foreground">
                Showing <span className="font-medium text-foreground">{start}–{end}</span> of{" "}
                <span className="font-medium text-foreground">{totalCount.toLocaleString("en-IN")}</span> products
            </p>

            <div className="flex items-center gap-1.5">
                <Button
                    type="button"
                    variant="outline"
                    size="icon"
                    aria-label="Go to previous page"
                    disabled={page <= 1}
                    onClick={() => onPageChange(page - 1)}
                    className="size-9 rounded-xl"
                >
                    <ChevronLeft className="size-4" />
                </Button>

                {pages.map((pageNumber, index) =>
                    pageNumber === "ellipsis" ? (
                        <span
                            key={`ellipsis-${index}`}
                            aria-hidden="true"
                            className="flex size-9 items-center justify-center text-muted-foreground"
                        >
                            <MoreHorizontal className="size-4" />
                        </span>
                    ) : (
                        <Button
                            key={pageNumber}
                            type="button"
                            variant={pageNumber === page ? "default" : "outline"}
                            aria-label={`Go to page ${pageNumber}`}
                            aria-current={pageNumber === page ? "page" : undefined}
                            onClick={() => onPageChange(pageNumber)}
                            className="size-9 rounded-xl px-0"
                        >
                            {pageNumber}
                        </Button>
                    ),
                )}

                <Button
                    type="button"
                    variant="outline"
                    size="icon"
                    aria-label="Go to next page"
                    disabled={page >= totalPages}
                    onClick={() => onPageChange(page + 1)}
                    className="size-9 rounded-xl"
                >
                    <ChevronRight className="size-4" />
                </Button>
            </div>
        </nav>
    );
}
