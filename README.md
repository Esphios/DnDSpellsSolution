# DnD 5e Spell WebAPI

A simple .NET 6+ Web API for upserting and retrieving D&D 5e spells. This project demonstrates:

- A layered architecture with:
  - ApplicationCore (domain entities, DTOs, interfaces)
  - Infrastructure (EF Core data access, repositories)
  - WebAPIHost (controllers, endpoints)
- A repository pattern  
- A “DTO-based” approach to upserting spells (via SpellRequest) without exposing entity internals  
- Many-to-many relationship management in EF (Classes ↔ Spells, Subclasses ↔ Spells)

---

## Table of Contents

1. [Prerequisites](#prerequisites)  
2. [Project Structure](#project-structure)  
3. [Installation & Setup](#installation--setup)  
4. [Database Migrations](#database-migrations)  
5. [Running the API](#running-the-api)  
6. [API Endpoints](#api-endpoints)  
7. [Data Models](#data-models)  
8. [Example JSON Payload](#example-json-payload)  
9. [Frontend](#frontend)  
10. [License](#license)

---

## Prerequisites

- .NET 6 SDK or newer  
- SQL Server / PostgreSQL / MySQL (configure your provider in `ApplicationDbContext`)  
- An IDE or text editor (e.g., Visual Studio, VS Code, Rider)

---

## Project Structure

Below is an overview of the main folders:

```
├── ApplicationCore
│   ├── Entities
│   │   ├── Spell.cs
│   │   ├── Class.cs
│   │   ├── Subclass.cs
│   │   ├── School.cs
│   │   ├── Damage.cs
│   │   ├── DamageType.cs
│   │   └── DamageAtSlotLevel.cs
│   ├── Dtos
│   │   └── SpellRequest.cs
│   └── Interfaces
│       └── Repositories
│           └── ISpellRepository.cs
├── Infrastructure
│   ├── Data
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations (Auto-generated after you create DB migrations)
│   └── Repositories
│       └── SpellRepository.cs
├── WebAPIHost
│   ├── Controllers
│   │   └── SpellsController.cs
│   ├── Program.cs
│   └── appsettings.json
├── FrontEnd
│   ├── src
│   │   ├── components
│   │   ├── interfaces
│   │   ├── services
│   │   ├── stores
│   │   ├── App.vue
│   │   ├── router.ts
│   │   ├── main.ts
│   │   ├── style.css
│   ├── public
│   ├── package.json
│   ├── vite.config.ts
└── README.md
```

---

## Installation & Setup

1. **Clone** this repository:  
   ```bash
   git clone https://github.com/your-username/dnd-spell-webapi.git
   ```
2. **Restore NuGet packages**:  
   ```bash
   cd dnd-spell-webapi
   dotnet restore
   ```
3. **Adjust appsettings.json** in `WebAPIHost` and in `WorkerServiceHost` to match your database settings, e.g.:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=DnDSpells;Trusted_Connection=True;"
     }
   }
   ```
   Or whichever provider you use (SQL Server/PostgreSQL/MySQL, etc.).  
4. **(Optional) Adjust EF Configuration**: If you prefer a different DB or EF Core approach, update `ApplicationDbContext` accordingly.

---

## Database Migrations

If you haven’t run migrations yet, you can generate and apply them:

1. **Create Migration** (from the `Infrastructure` project root or solution root):
   ```bash
   dotnet ef migrations add InitialCreate --project Infrastructure --startup-project WebAPIHost
   ```
2. **Apply Migration**:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project WebAPIHost
   ```
This will create/update the database tables.

---

## Running the API

After configuring your connection string and running migrations, start the project:

```bash
cd WebAPIHost
dotnet run
```

The Web API will typically start on `http://localhost:5000` (or `http://localhost:7020` in some cases).

---

## API Endpoints

| Method | Endpoint                 | Description                          |
|-------:|:-------------------------|:-------------------------------------|
| GET    | /api/spells             | Get all spells                       |
| GET    | /api/spells/{id}        | Get a single spell by Id             |
| POST   | /api/spells             | Creates or updates a spell           |

### 1) GET /api/spells

Returns a paginated list of spells from the database.

**Query String Parameters:**

- `page` (int, optional): The page number to return (default is 1).
- `pageSize` (int, optional): The number of items per page (default is 10).
- `sortBy` (string, optional): The field to sort by. Valid values are: `name`, `level`, `class` (default is `level`).
- `sortDirection` (string, optional): The sort order. Must be either `asc` or `desc` (default is `asc`).
- `name` (string, optional): Filter spells by name (partial match).

**Example Request:**

```
GET /Spells?page=1&pageSize=10&sortBy=level&sortDirection=asc&name=arcane
```

**Response** (HTTP 200):
```json
"spells": {
  "$values": [
    {
      "id": "mage-hand",
      "name": "Mage Hand",
      "desc": {
          "$values": [
              "A spectral, floating hand appears at a point you choose
               within range. The hand lasts for the duration..."
          ]
      },
      "range": "30 feet",
      "components": {"$values": ["V", "S"]},
      "material": "Not Available",
      "ritual": false,
      "duration": "1 minute",
      "concentration": false,
      "castingTime": "1 action",
      "level": 0,
      "attackType": "Not Available",
      "url": "/api/spells/mage-hand",
      "updatedAt": "2025-01-17T23:07:25.031",
      "classes": {...},
      "subclasses": {...},
      "school": {...},
      "damage": {...},
      "higherLevel": {...}
    },
    ...
  ]
} 
```

### 2) GET /api/spells/{id}

Returns details of the specified spell.

- **Path Parameter**: `{id}` is the `Spell.Id`.
- **Response (HTTP 200)**: JSON object representing the spell.  
- **Response (HTTP 404)**: If no spell with `{id}` is found.

### 3) POST /api/spells

Creates or updates a spell.  
It expects a `SpellRequest` JSON body. If the given `id` exists, the spell is updated. Otherwise, a new one is created.

- **Body**: JSON (see [Example JSON Payload](#example-json-payload))
- **Response (HTTP 200)**: Echoes the payload (or you can customize).  

---

## Data Models

### Spell Entity  
Represents the core data for a spell:
- Id, Name, Desc, Range, Components, etc.  
- A many-to-many relationship to Class and Subclass.  
- References a single School.  
- Optionally references a Damage object with nested references.  

### Class & Subclass Entities  
- Each has an Id, Name, and a navigational property “Spells.”  

### School Entity  
- Has its own Id, Name, and list of spells referencing that school.  

---

## Example JSON Payload

Below is an example for creating or updating the “Arcane Hand” spell:

```json
{
  "id": "arcane-hand",
  "name": "Arcane Hand",
  "desc": [
    "You create a Large hand of shimmering, translucent force in an unoccupied space..."
  ],
  "higherLevel": [
    "When you cast this spell using a spell slot of 6th level or higher..."
  ],
  "range": "120 feet",
  "components": [
    "V",
    "S",
    "M"
  ],
  "material": "An eggshell and a snakeskin glove.",
  "ritual": false,
  "duration": "Up to 1 minute",
  "concentration": true,
  "castingTime": "1 action",
  "level": 5,
  "attackType": "Melee Spell Attack",
  "schoolId": "evocation",
  "classIds": [
    "wizard"
  ],
  "subclassIds": [],
  "url": "/api/spells/arcane-hand",
  "updatedAt": "2025-01-19T23:41:53.953Z"
}
```

## Important Notice for Forked Repositories

The repository currently includes files such as `appsettings` and `migrations` for documentation and reference purposes. If you are forking this repository or using it as a base for your project, **it is strongly recommended to add these files to your `.gitignore`** to prevent unintentional exposure of sensitive information or unnecessary tracking of database-related changes.

### Suggested `.gitignore` Entries:
```plaintext
# Configuration files
appsettings.json
appsettings.Development.json

# Database migrations
migrations/
```


---

## Frontend

The frontend is a Vue 3 application built with Vite and styled using TailwindCSS. It serves as a user interface for interacting with the DnD 5e Spell WebAPI.

### Features

- **Search & Filtering**: Users can search for spells by name and filter results.
- **Pagination**: Results are paginated for better usability.
- **Sorting**: Users can sort spells by different columns.
- **Spell Details**: Clicking on a spell row opens a modal with more detailed information.
- **Axios Integration**: The frontend fetches spell data from the backend API using Axios.

### Prerequisites

- Node.js (16+ recommended)
- Yarn package manager (or npm if preferred)

### Installation & Setup

1. **Navigate to the frontend directory:**
   ```bash
   cd frontend
   ```
2. **Install dependencies:**
   ```bash
   yarn install
   ```
3. **Set up environment variables:**
   Create a `.env` file in the `frontend` directory with the following content:
   ```env
   VITE_API_BASE_URL=http://localhost:5000/api/spells
   ```
   Adjust the URL if your backend is running on a different port.

4. **Run the development server:**
   ```bash
   yarn run dev
   ```
   The app will be available at `http://localhost:5173/` by default.

### Project Structure

```
FrontEnd/
├── src/
│   ├── components/   # Reusable UI components (e.g., search bar, table, modal)
│   ├── interfaces/   # TypeScript interfaces for API responses and state management
│   ├── services/     # API service layer (Axios calls to backend)
│   ├── stores/       # State management (e.g., Pinia)
│   ├── App.vue       # Root component
│   ├── router.ts     # Vue Router configuration
│   ├── main.ts       # Application entry point
│   ├── style.css     # Global styles
├── public/           # Static assets (e.g., icons, images)
├── package.json      # Project dependencies and scripts
└──  vite.config.ts   # Vite configuration
```

### API Integration

The frontend interacts with the backend using Axios. API requests are handled in a centralized service file:

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
});

export const fetchSpells = async (params) => {
  const response = await api.get('/', { params });
  return response.data;
};
```

### Build & Deployment

To build the project for production, run:

```bash
yarn run build
```

This will generate optimized static files in the `dist/` directory, which can be deployed to any static hosting provider.

---

## License

Distributed under the MIT License. See `LICENSE` file for more details.
