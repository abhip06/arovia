import Link from "next/link";
import { ArrowRight, Blocks, Database, ShieldAlert } from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { PageHeading, SectionTitle, Surface } from "./dashboard-primitives";

interface CapabilityState {
  icon: LucideIcon;
  title: string;
  description: string;
}

export function CapabilityPage({
  title,
  eyebrow,
  description,
  apiArea,
}: {
  title: string;
  eyebrow: string;
  description: string;
  apiArea: string;
}) {
  const states: CapabilityState[] = [
    {
      icon: Database,
      title: "No endpoint",
      description: `${apiArea} is not exposed by the current backend.`,
    },
    {
      icon: ShieldAlert,
      title: "No local writes",
      description: "This page does not save pretend changes in the browser.",
    },
    {
      icon: Blocks,
      title: "Ready to connect",
      description: "Add the API contract and this screen can become a live workspace.",
    },
  ];

  return (
    <>
      <PageHeading eyebrow={eyebrow} title={title} description={description} />
      <div className="grid gap-4 lg:grid-cols-[1fr_320px]">
        <Surface>
          <SectionTitle
            title="Not connected to a backend workflow"
            description="This workspace does not display example records or actions that would look like live data."
          />
          <div className="grid gap-3 p-4 sm:grid-cols-3 sm:p-5">
            {states.map(({ icon: Icon, title: stateTitle, description: stateDescription }) => (
              <article key={stateTitle} className="rounded-xl border bg-background p-4">
                <span className="grid size-9 place-items-center rounded-lg bg-muted text-muted-foreground">
                  <Icon className="size-4" />
                </span>
                <h3 className="mt-3 text-sm font-semibold">{stateTitle}</h3>
                <p className="mt-1 text-xs leading-5 text-muted-foreground">{stateDescription}</p>
              </article>
            ))}
          </div>
        </Surface>

        <Surface>
          <div className="p-5">
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Available areas</p>
            <h2 className="mt-2 text-lg font-semibold">Continue with live data</h2>
            <p className="mt-2 text-sm leading-6 text-muted-foreground">
              Catalog, inventory, orders, payments, and account access use the existing API surface.
            </p>
            <Link href="/admin" className={cn(buttonVariants(), "mt-5 w-full gap-2")}>
              Back to overview <ArrowRight className="size-4" />
            </Link>
          </div>
        </Surface>
      </div>
    </>
  );
}
