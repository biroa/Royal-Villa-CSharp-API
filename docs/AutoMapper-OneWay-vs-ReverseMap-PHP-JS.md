# AutoMapper: One-Way vs ReverseMap (PHP & JavaScript perspective)

This guide explains the two mapping styles registered in `Program.cs` for the Royal Villa API. It is written for developers who know **PHP** and **JavaScript** and are new to **C#**, **ASP.NET Core**, and **AutoMapper**.

## The configuration in this project

```csharp
builder.Services.AddAutoMapper(cfg =>
{
    // Approach 1: one-way maps (write path → database entity)
    cfg.CreateMap<VillaCreateDTO, Villa>();
    cfg.CreateMap<VillaUpdateDTO, Villa>()
        .ForMember(dest => dest.Id, opt => opt.Ignore());

    // Approach 2: two-way maps (response / DTO-to-DTO)
    cfg.CreateMap<Villa, VillaDTO>().ReverseMap();
    cfg.CreateMap<VillaCreateDTO, VillaDTO>().ReverseMap();
    cfg.CreateMap<VillaUpdateDTO, VillaDTO>().ReverseMap();
});
```

## Types you are mapping between

| C# type | Role | PHP analogy | JavaScript analogy |
|---------|------|-------------|-------------------|
| `Villa` | Entity persisted by EF Core | Eloquent `Villa` model (`app/Models/Villa.php`) | Prisma/Sequelize row model or DB document shape |
| `VillaCreateDTO` | POST body | Laravel **Form Request** / validated input array for `store()` | Zod/`joi` schema or TypeScript `CreateVillaInput` for `POST` |
| `VillaUpdateDTO` | PUT/PATCH body | Update Form Request / `$request->validated()` | `UpdateVillaInput` — same fields, no server-owned keys |
| `VillaDTO` | GET/list response | **API Resource** output (`VillaResource::toArray()`) | Response DTO / `VillaResponse` interface returned from controller |

`Villa` includes server fields clients should not set on create: `Id`, `CreatedDate`, `UpdatedDate`.  
`VillaDTO` exposes `Id` and business fields but hides audit columns — similar to returning only selected keys from a Resource, not the full model.

---

## What AutoMapper is (in PHP/JS terms)

AutoMapper is a **convention-based object copier** registered in DI (like a Laravel singleton service). You declare allowed type pairs once; at runtime you call:

```csharp
var villa = _mapper.Map<Villa>(villaCreateDTO);
```

It copies properties with the **same name** and compatible types from source to destination.

| Ecosystem | Similar idea |
|-----------|----------------|
| **PHP (Laravel)** | `array_merge` / manual assignment from `$request->validated()` into `new Villa([...])`, or packages like Spatie Laravel Data `VillaData::from($request)` |
| **PHP (generic)** | `array_intersect_key` + loop, or a small `mapCreateDtoToEntity()` function |
| **JavaScript** | `{ ...pick(dto, ['name', 'rate']) }`, or a `toVilla(dto)` helper; libraries like `class-transformer` `plainToInstance` |
| **Java** (if you know it) | MapStruct — compile-time mapper; AutoMapper is runtime/convention-based |

It is **not** JSON encoding. The HTTP layer already deserialized JSON into `VillaCreateDTO`. AutoMapper runs **after** binding, **before** `SaveChanges`.

---

## Approach 1: One-way maps (first three `cfg` lines)

```csharp
cfg.CreateMap<VillaCreateDTO, Villa>();
cfg.CreateMap<VillaUpdateDTO, Villa>()
    .ForMember(dest => dest.Id, opt => opt.Ignore());
```

### Purpose

**Inbound / write path only:** turn what the client sent into a `Villa` entity for `AddAsync` or in-place update.

### PHP mental model

```php
// VillaCreateDTO ≈ $validated from Form Request
$villa = new Villa([
    'name' => $validated['name'],
    'details' => $validated['details'],
    // ... no 'id', no 'created_at' from client
]);
$villa->save();
```

In Laravel you normally **do not** run `VillaResource` backward into the model on create. You build the model from input, set timestamps in the controller or model events, and save.

### JavaScript mental model

```javascript
// createVillaInput ≈ VillaCreateDTO (validated body)
const villa = {
  name: createVillaInput.name,
  details: createVillaInput.details,
  rate: createVillaInput.rate,
  // id, createdAt: set by server / DB
};
await prisma.villa.create({ data: villa });
```

Or with spread, being careful **not** to spread client `id`:

```javascript
const { id, ...safe } = body; // strip forbidden fields
await prisma.villa.create({ data: safe });
```

### Why one direction only

You register **`VillaCreateDTO → Villa`**, not **`Villa → VillaCreateDTO`**, because:

- On create, `Id` and dates come from the database or server, not the request.
- Exposing a reverse map would suggest you can rebuild “create input” from an entity — rarely what you want on the API.

### The `ForMember(..., Ignore())` on update

```csharp
.ForMember(dest => dest.Id, opt => opt.Ignore());
```

When mapping **`VillaUpdateDTO` → `Villa`**, never write into `dest.Id`.

PHP equivalent: when filling an existing model for update, you use route `id` or `$villa->id`, not `$request->input('id')`:

```php
$villa = Villa::findOrFail($id);
$villa->fill($request->validated()); // validated array has no 'id'
$villa->save();
```

JavaScript equivalent: `update` uses `where: { id: params.id }` and omits `id` from `data`.

### How the controller uses it today

| Action | Call | Meaning |
|--------|------|---------|
| Create | `_mapper.Map<Villa>(villaCreateDTO)` | New entity from POST body |
| Update | `_mapper.Map(villaUpdateDTO, existingVilla)` | Patch existing entity (like `fill()` on the same instance) |

---

## Approach 2: Maps with `.ReverseMap()` (last three `cfg` lines)

```csharp
cfg.CreateMap<Villa, VillaDTO>().ReverseMap();
cfg.CreateMap<VillaCreateDTO, VillaDTO>().ReverseMap();
cfg.CreateMap<VillaUpdateDTO, VillaDTO>().ReverseMap();
```

### What `.ReverseMap()` does

One registration creates **two** allowed directions:

1. **Forward:** `Villa` → `VillaDTO`
2. **Reverse:** `VillaDTO` → `Villa`

Same for create/update DTOs ↔ `VillaDTO`.

PHP analogy: you defined `toArray()` on a Resource **and** a factory that rebuilds a model from that array — two directions in one config.

JavaScript analogy:

```javascript
// forward
function toVillaResponse(villa) { return { id: villa.id, name: villa.name, ... }; }
// reverse (only register if you really need it)
function fromVillaResponse(dto) { return { ...dto }; }
```

`.ReverseMap()` is AutoMapper’s way of saying: “register both functions.”

### Purpose

1. **`Villa` ↔ `VillaDTO`** — **read path**  
   - Forward: entity from DB → JSON-friendly response (hide `CreatedDate` / `UpdatedDate`).  
   - Like `return new VillaResource($villa);` or `res.json(toVillaResponse(villa))`.

2. **`VillaCreateDTO` / `VillaUpdateDTO` ↔ `VillaDTO`** — **convert between API shapes** without touching the database  
   - Useful if create/update payloads and GET responses should share the same public field set but remain different types in C#.

### PHP: Laravel API Resource

```php
// Forward only (typical GET)
return new VillaResource($villa);

// VillaResource::toArray($request) might return:
// ['id' => $this->id, 'name' => $this->name, ...]  // no created_at
```

`VillaDTO` is that array shape as a **typed class**.  
`CreateMap<Villa, VillaDTO>()` is “Resource transformation as a service.”

Reverse (`VillaDTO` → `Villa`) is like doing `Villa::create($resourceArray)` — you would only do that if you accept the risk of missing audit fields and overwriting defaults.

### JavaScript: separate request/response types

```typescript
interface VillaResponse {
  id: number;
  name: string;
  rate: number;
  // no createdAt in public API
}

function toVillaResponse(row: VillaRow): VillaResponse {
  const { createdAt, updatedAt, ...publicFields } = row;
  return publicFields;
}
```

Reverse map ≈ `function fromVillaResponse(dto: VillaResponse): VillaRow` — uncommon for writes; updates usually use a dedicated `UpdateVillaInput`.

---

## Side-by-side: two approaches

| | **Approach 1 (no ReverseMap)** | **Approach 2 (with ReverseMap)** |
|--|--------------------------------|----------------------------------|
| **Lines** | `CreateMap<Source, Villa>()` | `CreateMap<A, B>().ReverseMap()` |
| **Directions** | One-way: DTO → entity | Two-way: A ↔ B |
| **Typical HTTP use** | POST, PUT/PATCH | GET, list, optional response shaping |
| **PHP** | `$model->fill($validated)` | `VillaResource`, `toArray()` / optional `fromArray()` |
| **JavaScript** | `prisma.create({ data: input })` | `toVillaResponse(entity)` for `res.json()` |
| **Hides DB-only fields** | N/A (building entity) | `Villa` → `VillaDTO` drops audit fields |
| **Custom rule in this API** | Ignore `Id` on update → `Villa` | Mirrored on reverse where AutoMapper applies rules |

---

## Data flow diagram

```text
  Client (JSON)          C# API layer                    Database
  -------------          ------------                    --------

  POST body      →   VillaCreateDTO   ──map (1-way)──►   Villa   ──►  EF Core
  PUT body       →   VillaUpdateDTO   ──map (1-way)──►   Villa

  GET response   ←   VillaDTO         ◄──map (2-way)──   Villa
                     (no CreatedDate/UpdatedDate)
```

**Do not** replace the create/update one-way maps with a chain `VillaCreateDTO` → `VillaDTO` → `Villa` unless you have a reason. Writes should map **directly** to `Villa`.

---

## Comparison table (quick reference)

| Question | Approach 1 | Approach 2 |
|----------|------------|------------|
| When? | Saving to DB | Returning JSON; converting DTO variants |
| Map example | `Map<Villa>(createDto)` | `Map<VillaDTO>(villa)` |
| PHP | Form Request → Eloquent model | Model → API Resource array |
| JS | Body → Prisma `data` | Row → `VillaResponse` |
| Risk if misused | Low | Reverse `VillaDTO` → `Villa` may leave dates default or overwrite entity |

---

## Practical rules for this API

1. **Creates and updates** — keep using Approach 1 only (`Map<Villa>`, `Map(dto, existing)`).
2. **GET endpoints** — prefer `return Ok(_mapper.Map<VillaDTO>(villa));` so clients never see `CreatedDate`/`UpdatedDate` unless you add them to `VillaDTO`.
3. **Avoid** `Map<Villa>(villaDto)` for updates; use `VillaUpdateDTO` → `Villa` with `Id` ignored — same as never taking `id` from `$request` on update in PHP.
4. **After create** — if you want a consistent response shape, map `Villa` → `VillaDTO` instead of returning the raw entity.

Example (conceptual):

```csharp
// GET
return Ok(_mapper.Map<VillaDTO>(villa));

// POST (after save)
return CreatedAtAction(
    nameof(GetVilla),
    new { id = villa.Id },
    _mapper.Map<VillaDTO>(villa));
```

---

## AutoMapper vs doing it manually in PHP/JS

| | Manual (PHP/JS habit) | AutoMapper |
|--|----------------------|------------|
| **Pros** | Obvious, full control, no package | Less boilerplate, central rules in `Program.cs` |
| **Cons** | Repeats in every action | Learning curve; reverse maps need care |
| **When manual wins** | Renamed fields, heavy logic | Same property names on both types |

If property names differ (`price` vs `rate`), configure `ForMember` in C# — similar to a custom key map in PHP or a dedicated mapper function in JS.

---

## Related files in this repo

| File | Contents |
|------|----------|
| `Program.cs` | All `CreateMap` registrations |
| `Models/Villa.cs` | Entity |
| `Models/DTO/VillaDTO.cs` | Response shape |
| `Models/DTO/VillaCreateDTO.cs` | POST body |
| `Models/DTO/VillaUpdateDTO.cs` | PUT body |
| `Controllers/VillaController.cs` | `_mapper` usage on create/update |
| `docs/AutoMapper-and-DTOs.md` | Intro to DTOs + basic create mapping |

---

## Summary

| Concept | PHP | JavaScript | C# (this project) |
|---------|-----|------------|-----------------|
| Request shape | Form Request / validated array | Request body type / Zod schema | `VillaCreateDTO`, `VillaUpdateDTO` |
| DB shape | Eloquent model | Prisma model / DB row | `Villa` |
| Response shape | API Resource | Response interface | `VillaDTO` |
| Write mapping | `fill()` / `new Model($validated)` | `create({ data })` / spread | `CreateMap<*DTO, Villa>()` one-way |
| Read mapping | `VillaResource` | `toVillaResponse()` | `CreateMap<Villa, VillaDTO>()` |
| Both directions | Rare for writes | Rare for writes | `.ReverseMap()` — use forward often, reverse with care |

**Approach 1** = “client input → entity” (POST/PUT).  
**Approach 2** = “entity ↔ public JSON shape” and optional DTO conversions (GET and consistent responses).

Together they separate **what you persist** from **what you accept** and **what you return** — the same layering you already use in Laravel or in a typed Node API, expressed with AutoMapper profiles instead of hand-written assignment blocks.
