# Quotation Generator Backend

A .NET 9 Web API for managing items and generating quotations.

## Features

- **Items Management**: Complete CRUD operations for items
  - Create new items
  - Read/List items with filtering
  - Update existing items
  - Delete single or multiple items
- **RESTful API** with proper HTTP status codes
- **Entity Framework Core** with SQL Server
- **CORS enabled** for frontend integration
- **Swagger UI** for API documentation

## Prerequisites

- .NET 9 SDK
- SQL Server Express (or SQL Server)
- Visual Studio Code or Visual Studio

## Getting Started

### 1. Database Setup

The connection string is configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=QuotationGeneratorDb;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
}
```

If you need to change the database server, update this connection string.

### 2. Run Migrations

The database has already been created. If you need to recreate it or make changes:

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Update the database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### 3. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Items API (`/api/items`)

#### Get All Items
```
GET /api/items
Query Parameters:
  - filter (optional): Filter items by title or description
  - isActive (optional): Filter by active status (true/false)

Example: GET /api/items?filter=mouse&isActive=true
```

#### Get Single Item
```
GET /api/items/{id}
```

#### Create Item
```
POST /api/items
Content-Type: application/json

{
  "title": "Wireless Mouse",
  "description": "Ergonomic Bluetooth mouse",
  "price": 3500,
  "quantity": 12
}
```

#### Update Item
```
PUT /api/items/{id}
Content-Type: application/json

{
  "title": "Updated Title",
  "description": "Updated Description",
  "price": 4000,
  "quantity": 15,
  "isActive": true
}
```

Note: All fields are optional in the update request.

#### Delete Single Item
```
DELETE /api/items/{id}
```

#### Bulk Delete Items
```
DELETE /api/items/bulk
Content-Type: application/json

[1, 2, 3, 4, 5]
```

## Project Structure

```
quotation-generator-back-end/
├── Controllers/
│   ├── AuthController.cs
│   └── ItemsController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── DTOs/
│   ├── CreateItemDto.cs
│   ├── UpdateItemDto.cs
│   └── ItemResponseDto.cs
├── Models/
│   ├── Item.cs
│   └── User.cs
├── Migrations/
│   └── [EF Core migrations]
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

## Frontend Integration

The API is configured to accept requests from the following origins:
- `http://localhost:3000` (React default)
- `http://localhost:5173` (Vite default)
- `http://localhost:5174` (Vite alternative)

Update the CORS policy in `Program.cs` if you need to add more origins.

### Frontend API Call Example

```javascript
// Fetch all items
const response = await axios.get('http://localhost:5000/api/items');

// Create new item
const newItem = {
  title: "Laptop Stand",
  description: "Adjustable aluminum stand",
  price: 4500,
  quantity: 20
};
const response = await axios.post('http://localhost:5000/api/items', newItem);

// Update item
const updates = { price: 5000, quantity: 25 };
await axios.put(`http://localhost:5000/api/items/${id}`, updates);

// Delete item
await axios.delete(`http://localhost:5000/api/items/${id}`);
```

## Database Schema

### Items Table

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| Id | int | No | IDENTITY | Primary key |
| Title | nvarchar(200) | No | - | Item name |
| Description | nvarchar(1000) | Yes | - | Item description |
| Price | decimal(18,2) | No | - | Item price |
| Quantity | int | No | - | Available quantity |
| IsActive | bit | No | true | Active status |
| CreatedAt | datetime2 | No | GETUTCDATE() | Creation timestamp |
| UpdatedAt | datetime2 | Yes | - | Last update timestamp |

## Development

### Adding New Models

1. Create the model class in the `Models` folder
2. Add DbSet to `ApplicationDbContext`
3. Create a migration: `dotnet ef migrations add AddModelName`
4. Update database: `dotnet ef database update`

### Adding New Controllers

1. Create controller in `Controllers` folder
2. Inherit from `ControllerBase`
3. Add `[Route("api/[controller]")]` and `[ApiController]` attributes
4. Inject `ApplicationDbContext` in constructor

## Troubleshooting

### Database Connection Issues

If you get a connection error:
1. Verify SQL Server Express is installed and running
2. Check the connection string in `appsettings.json`
3. Try using SQL Server Management Studio to test the connection

### CORS Issues

If the frontend can't connect:
1. Check that your frontend URL is in the CORS policy in `Program.cs`
2. Ensure the API is running on the expected port
3. Check browser console for specific CORS errors

## License

MIT
