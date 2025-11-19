# VetCareAPI

Small REST API for a veterinary clinic built with **ASP.NET Core (.NET 8)** + **EF Core (MySQL/Pomelo)**.

## What it does
- Domain: **Clinics**, **Pets**, **Visits** (+ **Users** as pet owners)
- CRUD for Clinics/Pets/Visits
- Hierarchical endpoint: **GET /api/clinics/{id}/visits**
- Dates stored/returned in **UTC**
- Swagger/OpenAPI enabled in Development

## Quick start
1) Configure connection string in `appsettings.Development.json`:
```json
{ "ConnectionStrings": { "db": "Server=localhost;Database=vetcare;User=root;Password=;" } }
```

## Endpoints

### Clinics
- `GET    /api/clinics` — list clinics  
- `GET    /api/clinics/{id}` — get clinic by id  
- `POST   /api/clinics` — create clinic  
- `PUT    /api/clinics/{id}` — update clinic  
- `DELETE /api/clinics/{id}` — delete clinic  
- **Hierarchical:** `GET /api/clinics/{id}/visits` — visits for a clinic *(404 if clinic not found)*

### Pets
- `GET    /api/pets` — list pets  
- `GET    /api/pets/{id}` — get pet by id  
- `GET    /api/pets/by-user/{userId}` — list pets for a user  
- `POST   /api/pets` — create pet  
- `PUT    /api/pets/{id}` — update pet  
- `DELETE /api/pets/{id}` — delete pet  

### Visits
- `GET    /api/visits/{id}` — get visit by id  
- `GET    /api/visits/pet/{petId}` — list visits for a pet  
- `POST   /api/visits` — create visit  
- `PUT    /api/visits/{id}` — update visit *(e.g., set Status = Cancelled)*  
- `DELETE /api/visits/{id}` — delete visit  

---

## Status codes

- **201 Created** — successful POST *(with `Location` header)*  
- **204 No Content** — successful PUT/DELETE  
- **404 Not Found** — path resource missing  
- **400 Bad Request** — malformed JSON / binding/type errors / invalid model *(default `[ApiController]`)*  
- **422 Unprocessable Entity** — well-formed request violates domain rules *(e.g., `EndsAt <= StartsAt`, unknown `ClinicId`/`PetId`)*  
