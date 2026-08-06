# Music-Database

Web application for managing database with music (artists, tracks, users, etc)

## Stack

* **Backend**: ASP.NET Core (.NET10)

* **ORM**: Entity Framework Core

* **Database**: PostgreSQL

* **Frontend**: HTML, CSS

## Key Features

* **Library Management:** Complete CRUD functionality for tracks, albums, artists, and users.
* **Automatic Data Orchestration:** Intelligent system that auto-creates related artist and album records when a new track is imported.
* **Smart Search:** Search across tracks, albums, artists, and users.
* **Role-Based Access:** Basic administrative control over the music library content.
* **Entity Relationships:** Robust relational database structure managed via EF Core, ensuring data integrity across the music library.

## Technical Implementation Highlights

* **Data Integrity**: Albums are automatically classified as *Single*, *EP* or *Album* basic on the count of tracks using a Postgre *Trigger*. This ensures data-consistancy regardless of data source

* **Query Performance**: Using *AsNoTracking* in Read-Only operations to reduce memory overhead and improve performance by bypassing the EFCore Change Tracker

* **Auto-Relationship Resolution**: When adding a new track, the system automatically detects and creates missing related entities (Artists/Albums), simplifying data entry and preventing duplicates.

## Development Status

- [x] Data models and enity relations

- [x] Basic autentification (Signup/Login)

- [x] User roles (admin/user)

- [x] Adding, removing and getting tracks, albums and artists

- [x] Search functionality

- [x] Basic UI for navigation and data entry

- [ ] UI/UX styling with CSS

- [ ] Editing functionality for entities

- [ ] Admin panel for removal operations

- [ ] Playlist management

## Installation

1. Clone repository:

```bash
git clone https://github.com/AlexHysel/Music-Database.git
```

2. Enter the project directory:

```bash
cd Music-Database/MusicDatabase
```

3. Apply db migrations:

```bash
dotnet ef database update
```

4. Run:

```bash
dotnet run
```
