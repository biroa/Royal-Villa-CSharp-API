# DbSet vs HashSet

Quick reference for **Entity Framework Core** `DbSet<T>` versus the .NET **`HashSet<T>`** collection type.

## Comparison

| | `DbSet<Villa>` | `HashSet<Villa>` |
|---|----------------|------------------|
| **Purpose** | ORM access to a **database table** | In-memory collection of **unique** items |
| **Storage** | PostgreSQL / SQL Server / etc. | RAM only |
| **Lifetime** | Tied to `AppDbContext` and connection | Lives for as long as you hold the instance |
| **Uniqueness** | One row per primary key in the table | One instance per `Equals` / `GetHashCode` |
| **Queries** | LINQ translated to **SQL** | LINQ over objects **in memory** |
| **Persistence** | `SaveChanges()` writes to the database | Lost when the `HashSet` is discarded |
| **Typical use** | CRUD for API / app data | Deduping, graphs, algorithms, caches |

## What “Set” means in `DbSet`

The word **set** in `DbSet` means “the set of entities of this type in the model” (like all rows in the `Villas` table). It does **not** mean a hash-based data structure.

## Examples

### DbSet (database)

```csharp
// Registered on AppDbContext
public DbSet<Villa> Villas => Set<Villa>();

// In a controller or service
var all = await _context.Villas.ToListAsync();
var one = await _context.Villas.FindAsync(1);

_context.Villas.Add(new Villa { Name = "Sunset" });
await _context.SaveChangesAsync();
```

### HashSet (in memory)

```csharp
var villas = new HashSet<Villa>();

villas.Add(new Villa { Id = 1, Name = "Sunset" });
villas.Add(new Villa { Id = 1, Name = "Sunset" }); // duplicate may be ignored, depending on equality

// No database — data exists only in this HashSet until you copy it elsewhere
```

## Summary

| Question | Answer |
|----------|--------|
| Is `DbSet<Villa>` a hash? | **No.** |
| Can it hold many `Villa` objects? | **Yes** — as rows in a table, loaded or tracked by EF. |
| Is it the same as `HashSet<Villa>`? | **No** — different type, different storage, different purpose. |
