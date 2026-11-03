"use client";

import { useMemo, useState } from "react";
import {
  Check,
  ShieldCheck,
  UserRound,
  Users,
} from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useAuth } from "@/features/auth/context/auth-context";
import type { AdminUser, AdminUserRole } from "../api/admin-users.api";
import { useAdminUsers, useChangeAdminUserRole } from "../hooks/use-admin-users";
import {
  MetricCard,
  PageHeading,
  QueryState,
  SearchField,
  SectionTitle,
  StatusPill,
  Surface,
} from "./dashboard-primitives";
import { CapabilityPage } from "./capability-page";

const roles: AdminUserRole[] = [
  "Customer",
  "Pharmacist",
  "WarehouseStaff",
  "Admin",
];

function formatRole(role: AdminUserRole) {
  return role === "WarehouseStaff" ? "Warehouse staff" : role;
}

function UserRoleDialog({
  user,
  onClose,
}: {
  user: AdminUser | null;
  onClose: () => void;
}) {
  const [role, setRole] = useState<AdminUserRole>(user?.role ?? "Customer");
  const changeRole = useChangeAdminUserRole();

  const saveRole = async () => {
    if (!user || role === user.role) return;

    try {
      await changeRole.mutateAsync({ userId: user.id, role });
      toast.success(`${user.firstName} ${user.lastName}'s role is now ${formatRole(role)}.`);
      onClose();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not update this user's role.");
    }
  };

  return (
    <Dialog open={Boolean(user)} onOpenChange={(open) => !open && !changeRole.isPending && onClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Update access role</DialogTitle>
          <DialogDescription>
            {user ? `${user.firstName} ${user.lastName} · ${user.email}` : "Choose the access role for this account."}
          </DialogDescription>
        </DialogHeader>
        <label className="grid gap-2 text-sm font-medium">
          Account role
          <select
            className="h-10 rounded-lg border border-input bg-background px-3 text-sm outline-none focus:ring-2 focus:ring-ring"
            value={role}
            onChange={(event) => setRole(event.target.value as AdminUserRole)}
          >
            {roles.map((item) => (
              <option key={item} value={item}>{formatRole(item)}</option>
            ))}
          </select>
        </label>
        <div className="rounded-xl border bg-muted/30 p-3 text-xs leading-5 text-muted-foreground">
          Role changes affect the permissions granted to this account. The server verifies that the current administrator is authorized.
        </div>
        <DialogFooter>
          <Button variant="outline" disabled={changeRole.isPending} onClick={onClose}>
            Cancel
          </Button>
          <Button disabled={!user || role === user.role || changeRole.isPending} onClick={() => void saveRole()}>
            {changeRole.isPending ? "Saving…" : "Save role"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function UserRow({
  user,
  isCurrentUser,
  onEdit,
}: {
  user: AdminUser;
  isCurrentUser: boolean;
  onEdit: (user: AdminUser) => void;
}) {
  return (
    <tr className="border-t transition-colors hover:bg-muted/20">
      <td className="px-4 py-3.5 sm:px-5">
        <p className="font-medium">{user.firstName} {user.lastName}</p>
        <p className="mt-1 text-xs text-muted-foreground">{user.email}</p>
      </td>
      <td className="px-4 py-3.5">
        <StatusPill variant={user.role === "Admin" ? "default" : "muted"}>
          {formatRole(user.role)}
        </StatusPill>
        {isCurrentUser && <span className="ml-2 text-[10px] text-muted-foreground">You</span>}
      </td>
      <td className="px-4 py-3.5">
        <StatusPill variant={user.isEmailVerified ? "success" : "warning"}>
          {user.isEmailVerified ? "Verified" : "Pending"}
        </StatusPill>
      </td>
      <td className="px-4 py-3.5 text-right">
        <Button size="sm" variant="outline" disabled={isCurrentUser} onClick={() => onEdit(user)}>
          {isCurrentUser ? "Current account" : "Change role"}
        </Button>
      </td>
    </tr>
  );
}

export function AdminUsersPage() {
  const [search, setSearch] = useState("");
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null);
  const usersQuery = useAdminUsers();
  const { user: currentUser } = useAuth();
  const users = useMemo(() => usersQuery.data?.users ?? [], [usersQuery.data?.users]);
  const filteredUsers = useMemo(() => {
    const query = search.trim().toLocaleLowerCase();
    if (!query) return users;

    return users.filter((user) =>
      `${user.firstName} ${user.lastName} ${user.email} ${user.role}`
        .toLocaleLowerCase()
        .includes(query),
    );
  }, [search, users]);

  const adminCount = users.filter((item) => item.role === "Admin").length;
  const staffCount = users.filter((item) => item.role === "Pharmacist" || item.role === "WarehouseStaff").length;
  const verifiedCount = users.filter((item) => item.isEmailVerified).length;

  return (
    <>
      <PageHeading
        eyebrow="People and access"
        title="Users & staff"
        description="Review accounts, check verification status, and assign the platform roles supported by your access policy."
        action={(
          <Button variant="outline" onClick={() => void usersQuery.refetch()} disabled={usersQuery.isFetching}>
            <Users className="mr-2 size-4" />
            {usersQuery.isFetching ? "Refreshing…" : "Refresh directory"}
          </Button>
        )}
      />

      <div className="mb-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Total accounts" value={usersQuery.isError ? "—" : usersQuery.data?.totalCount ?? "—"} note="Directory count from the admin API" icon={Users} />
        <MetricCard label="Administrators" value={usersQuery.isLoading || usersQuery.isError ? "—" : adminCount} note="Current users with admin role" icon={ShieldCheck} tone="primary" />
        <MetricCard label="Operational staff" value={usersQuery.isLoading || usersQuery.isError ? "—" : staffCount} note="Pharmacists and warehouse staff" icon={UserRound} tone="green" />
        <MetricCard label="Email verified" value={usersQuery.isLoading || usersQuery.isError ? "—" : verifiedCount} note="Accounts verified on loaded directory" icon={Check} tone="amber" />
      </div>

      <Surface>
        <SectionTitle
          title="Account directory"
          description="Roles are changed through the protected admin endpoint. Your own role is locked for this session."
          action={(
            <SearchField
              value={search}
              onChange={setSearch}
              placeholder="Search name, email or role"
            />
          )}
        />
        <QueryState
          loading={usersQuery.isLoading}
          error={usersQuery.error}
          empty={!usersQuery.isLoading && !usersQuery.error && filteredUsers.length === 0}
          onRetry={() => void usersQuery.refetch()}
        />
        {!usersQuery.isLoading && !usersQuery.error && filteredUsers.length > 0 && (
          <>
            <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[680px] text-left text-sm">
                <thead className="bg-muted/35 text-[10px] uppercase tracking-[0.12em] text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 sm:px-5">Account</th>
                    <th className="px-4 py-3 sm:px-5">Role</th>
                    <th className="px-4 py-3 sm:px-5">Email status</th>
                    <th className="px-4 py-3 text-right sm:px-5">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredUsers.map((item) => (
                    <UserRow
                      key={item.id}
                      user={item}
                      isCurrentUser={item.id === currentUser?.id}
                      onEdit={setSelectedUser}
                    />
                  ))}
                </tbody>
              </table>
            </div>
            <div className="divide-y md:hidden">
              {filteredUsers.map((item) => {
                const isCurrentUser = item.id === currentUser?.id;

                return (
                  <article key={item.id} className="space-y-3 p-4">
                    <div className="flex items-start justify-between gap-3">
                      <div className="min-w-0">
                        <p className="truncate text-sm font-semibold">{item.firstName} {item.lastName}</p>
                        <p className="mt-1 truncate text-xs text-muted-foreground">{item.email}</p>
                      </div>
                      <StatusPill variant={item.isEmailVerified ? "success" : "warning"}>
                        {item.isEmailVerified ? "Verified" : "Pending"}
                      </StatusPill>
                    </div>
                    <div className="flex items-center justify-between gap-3 border-t pt-3">
                      <div className="flex items-center gap-2">
                        <StatusPill variant={item.role === "Admin" ? "default" : "muted"}>
                          {formatRole(item.role)}
                        </StatusPill>
                        {isCurrentUser && <span className="text-[10px] text-muted-foreground">You</span>}
                      </div>
                      <Button size="sm" variant="outline" disabled={isCurrentUser} onClick={() => setSelectedUser(item)}>
                        {isCurrentUser ? "Current account" : "Change role"}
                      </Button>
                    </div>
                  </article>
                );
              })}
            </div>
            <div className="border-t px-4 py-3 text-xs text-muted-foreground sm:px-5">
              Showing {filteredUsers.length} of {usersQuery.data?.totalCount ?? users.length} accounts
            </div>
          </>
        )}
      </Surface>

      <UserRoleDialog
        key={selectedUser?.id ?? "no-selection"}
        user={selectedUser}
        onClose={() => setSelectedUser(null)}
      />
    </>
  );
}

export function AdminShippingPage() {
  return (
    <CapabilityPage
      eyebrow="Fulfillment"
      title="Shipping"
      description="Review carrier handoffs, shipment progress, and delivery exceptions."
      apiArea="Shipment and carrier tracking"
    />
  );
}

export function AdminReportsPage() {
  return (
    <CapabilityPage
      eyebrow="Insights"
      title="Reports"
      description="Review trusted sales, inventory, and fulfillment reporting."
      apiArea="Analytics and reporting"
    />
  );
}

export function AdminSettingsPage() {
  return (
    <CapabilityPage
      eyebrow="Configuration"
      title="Settings"
      description="Manage store and workspace configuration."
      apiArea="Platform settings"
    />
  );
}
