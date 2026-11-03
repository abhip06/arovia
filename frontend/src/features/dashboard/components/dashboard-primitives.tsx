import type { ElementType, ReactNode } from "react";
import {
  AlertCircle,
  ArrowDown,
  ArrowUp,
  CheckCircle2,
  LoaderCircle,
  Search,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

type MetricTone = "primary" | "amber" | "red" | "green";

const metricToneClasses: Record<MetricTone, string> = {
  primary: "bg-primary/10 text-primary",
  amber: "bg-amber-500/10 text-amber-700 dark:text-amber-300",
  red: "bg-destructive/10 text-destructive",
  green: "bg-emerald-500/10 text-emerald-700 dark:text-emerald-300",
};

export function PageHeading({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow?: string;
  title: string;
  description?: string;
  action?: ReactNode;
}) {
  return (
    <div className="mb-7 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div>
        {eyebrow && (
          <p className="mb-2 text-xs font-semibold uppercase tracking-[0.16em] text-primary">
            {eyebrow}
          </p>
        )}
        <h1 className="text-2xl font-semibold tracking-tight sm:text-3xl">{title}</h1>
        {description && (
          <p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground">
            {description}
          </p>
        )}
      </div>
      {action && <div className="shrink-0">{action}</div>}
    </div>
  );
}

export function MetricCard({
  label,
  value,
  note,
  icon: Icon,
  tone = "primary",
  badge,
}: {
  label: string;
  value: ReactNode;
  note: string;
  icon: ElementType;
  tone?: MetricTone;
  badge?: string;
}) {
  return (
    <article className="rounded-2xl border bg-card p-4 shadow-sm shadow-black/[0.02] sm:p-5">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-muted-foreground">{label}</p>
          <p className="mt-3 text-2xl font-semibold tracking-tight">{value}</p>
        </div>
        <span className={`grid size-10 shrink-0 place-items-center rounded-xl ${metricToneClasses[tone]}`}>
          <Icon className="size-5" />
        </span>
      </div>
      <div className="mt-3 flex items-center justify-between gap-2">
        <p className="text-xs text-muted-foreground">{note}</p>
        {badge && (
          <span className="rounded-full border border-amber-500/25 bg-amber-500/10 px-2 py-0.5 text-[9px] font-semibold uppercase tracking-wide text-amber-700 dark:text-amber-300">
            {badge}
          </span>
        )}
      </div>
    </article>
  );
}

export function Surface({
  children,
  className = "",
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <section className={`rounded-2xl border bg-card shadow-sm shadow-black/[0.02] ${className}`}>
      {children}
    </section>
  );
}

export function SectionTitle({
  title,
  description,
  action,
}: {
  title: string;
  description?: string;
  action?: ReactNode;
}) {
  return (
    <div className="flex flex-wrap items-start justify-between gap-3 border-b px-4 py-4 sm:px-5">
      <div>
        <h2 className="text-sm font-semibold">{title}</h2>
        {description && (
          <p className="mt-1 text-xs leading-5 text-muted-foreground">{description}</p>
        )}
      </div>
      {action}
    </div>
  );
}

type StatusVariant = "default" | "success" | "warning" | "danger" | "muted";

const statusClasses: Record<StatusVariant, string> = {
  default: "border-primary/20 bg-primary/5 text-primary",
  success: "border-emerald-500/25 bg-emerald-500/10 text-emerald-700 dark:text-emerald-300",
  warning: "border-amber-500/25 bg-amber-500/10 text-amber-700 dark:text-amber-300",
  danger: "border-destructive/20 bg-destructive/10 text-destructive",
  muted: "border-border bg-muted text-muted-foreground",
};

export function StatusPill({
  children,
  variant = "default",
}: {
  children: ReactNode;
  variant?: StatusVariant;
}) {
  return (
    <span className={`inline-flex items-center rounded-full border px-2.5 py-1 text-[10px] font-semibold capitalize ${statusClasses[variant]}`}>
      {children}
    </span>
  );
}

export function QueryState({
  loading,
  error,
  empty,
  onRetry,
}: {
  loading?: boolean;
  error?: unknown;
  empty?: boolean;
  onRetry?: () => void;
}) {
  if (loading) {
    return (
      <div className="flex items-center justify-center gap-2 px-4 py-12 text-sm text-muted-foreground">
        <LoaderCircle className="size-4 animate-spin" />
        Loading live data…
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center px-4 py-10 text-center">
        <AlertCircle className="size-5 text-destructive" />
        <p className="mt-2 text-sm font-medium">Could not load this data</p>
        <p className="mt-1 max-w-md text-xs leading-5 text-muted-foreground">
          {error instanceof Error ? error.message : "The API request failed. Please retry."}
        </p>
        {onRetry && (
          <Button className="mt-4" size="sm" variant="outline" onClick={onRetry}>
            Try again
          </Button>
        )}
      </div>
    );
  }

  if (empty) {
    return (
      <div className="flex flex-col items-center px-4 py-12 text-center">
        <CheckCircle2 className="size-5 text-muted-foreground" />
        <p className="mt-2 text-sm font-medium">Nothing to show yet</p>
        <p className="mt-1 text-xs text-muted-foreground">Live data will appear here when available.</p>
      </div>
    );
  }

  return null;
}

export function SearchField({
  value,
  onChange,
  placeholder = "Search…",
}: {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
}) {
  return (
    <label className="relative block w-full sm:max-w-xs">
      <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        className="pl-9"
      />
    </label>
  );
}

export function PageControls({
  page,
  totalPages,
  onChange,
}: {
  page: number;
  totalPages: number;
  onChange: (page: number) => void;
}) {
  const lastPage = Math.max(totalPages, 1);

  return (
    <div className="flex items-center justify-between gap-3 border-t px-4 py-3 text-xs text-muted-foreground sm:px-5">
      <span>Page {page} of {lastPage}</span>
      <div className="flex gap-2">
        <Button size="sm" variant="outline" disabled={page <= 1} onClick={() => onChange(page - 1)}>
          <ArrowDown className="mr-1 size-3.5 rotate-90" /> Previous
        </Button>
        <Button size="sm" variant="outline" disabled={page >= lastPage} onClick={() => onChange(page + 1)}>
          Next <ArrowUp className="ml-1 size-3.5 rotate-90" />
        </Button>
      </div>
    </div>
  );
}

export function DemoNotice({
  children = "This workspace uses local demo data. Changes stay in this browser session and are not sent to the backend.",
}: {
  children?: ReactNode;
}) {
  return (
    <div className="mb-5 flex items-start gap-3 rounded-xl border border-amber-500/25 bg-amber-500/5 px-4 py-3 text-xs leading-5 text-amber-900 dark:text-amber-200">
      <span className="mt-0.5 size-2 shrink-0 rounded-full bg-amber-500" />
      <p><strong className="font-semibold">Demo workspace.</strong> {children}</p>
    </div>
  );
}
