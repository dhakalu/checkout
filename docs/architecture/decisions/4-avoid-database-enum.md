# ADR: Store Enumerated Values as VARCHAR in the Database

| Field | Value |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-05-23 |
| **Deciders** | Engineering |
| **Domain** | All |
---
## Context

The application defines enumerated types in the domain layer (e.g. `OrderStatus`). A decision was needed on how to persist these values in the database: as integers, as native database enum types, or as strings.

## Decision

Store enumerated values as `VARCHAR` in the database. EF Core's `HasConversion<string>()` handles the translation between the application enum and the stored string automatically.

## Reasons

**Integer storage is fragile.** Inserting or reordering enum members silently remaps existing rows to the wrong value with no compile-time or runtime warning.

**Native database enums are painful to evolve.** Adding a value requires a schema migration. Renaming or removing a value requires a data migration. The database and application code must be kept in sync manually.

**VARCHAR is safe to evolve.** Adding a new enum member requires only a code change — no migration needed. The stored value is the member name, so existing rows are unaffected.

**Stored values are human-readable.** `"Pending"`, `"Shipped"`, `"Cancelled"` are immediately understandable in queries and logs without needing a lookup table or enum definition.

## Why Not a Lookup Table?

A separate status/lookup table was considered and rejected for fixed enumerated values because:

- The values are **defined in code**, not configured at runtime — a table adds a sync problem without solving anything
- Every query requires a **join** for data that never changes
- Adding a new value still requires a **code change** regardless, so the table buys nothing
- It pollutes the domain model with a meaningless `StatusId int` foreign key instead of a semantic enum

## When a Lookup Table Is the Right Choice

Use a lookup table when the values need to be **managed at runtime without a deployment**:

- An admin can add, rename, or deactivate values via a UI
- Different tenants or customers have different sets of values
- The value carries extra metadata beyond a name — display label, sort order, color, permissions, etc.

```csharp
// Lookup table makes sense when the row carries more than just a name
public class OrderStatus
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string DisplayLabel { get; init; }
    public string HexColor { get; init; }
    public int SortOrder { get; init; }
}
```

If the only column in the table would be `Id` and `Name`, it is an enum in disguise — keep it in code.

## Trade-offs

Renaming an enum member requires a data migration to update existing rows:

```sql
UPDATE orders SET status = 'NewName' WHERE status = 'OldName';
```

Treat enum member names as part of the public contract — rename with care.

## Configuration

```csharp
builder.Property(x => x.Status)
    .HasConversion<string>()
    .HasMaxLength(50);
```