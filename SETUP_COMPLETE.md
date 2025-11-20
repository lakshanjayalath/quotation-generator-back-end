# Backend Setup Complete! 🎉

## ✅ What Has Been Created

### 1. **Models**
- `Models/Item.cs` - Item entity with properties: Id, Title, Description, Price, Quantity, IsActive, CreatedAt, UpdatedAt

### 2. **DTOs (Data Transfer Objects)**
- `DTOs/CreateItemDto.cs` - For creating new items
- `DTOs/UpdateItemDto.cs` - For updating existing items
- `DTOs/ItemResponseDto.cs` - For returning items to frontend (matches your React component structure)

### 3. **Database Context**
- `Data/ApplicationDbContext.cs` - Entity Framework DbContext with Items DbSet

### 4. **Controller**
- `Controllers/ItemsController.cs` - Full CRUD API with:
  - GET `/api/items` - List all items with optional filtering
  - GET `/api/items/{id}` - Get single item
  - POST `/api/items` - Create new item
  - PUT `/api/items/{id}` - Update item
  - DELETE `/api/items/{id}` - Delete item
  - DELETE `/api/items/bulk` - Delete multiple items

### 5. **Configuration**
- Updated `Program.cs` with:
  - Entity Framework Core
  - SQL Server connection
  - CORS policy for frontend
  - Swagger/OpenAPI
  - Controllers
- Updated `appsettings.json` and `appsettings.Development.json` with database connection string

### 6. **Database**
- Created SQL Server database: `QuotationGeneratorDb`
- Applied initial migration with Items table

## 🚀 Your Backend is Running!

**Server URL:** `http://localhost:5264`
**Swagger UI:** `http://localhost:5264/swagger` (if you want to test the API)

## 📝 Frontend Integration

Update your frontend API calls to use `http://localhost:5264`:

```javascript
// In your React NewItemForm.jsx, update the axios call:
const handleSave = async () => {
  if (validateForm()) {
    try {
      await axios.post('http://localhost:5264/api/items', {
        title: formData.title,
        description: formData.description,
        price: formData.price,
        quantity: formData.quantity
      });
      setShowSuccess(true);
      
      if (!initialData) {
        setFormData({
          title: "",
          description: "",
          price: 0,
          quantity: 0,
        });
      }
    } catch (error) {
      console.error("Error saving item:", error);
    }
  }
};

// In your ItemPage.jsx, fetch items like this:
useEffect(() => {
  const fetchItems = async () => {
    try {
      const response = await axios.get('http://localhost:5264/api/items');
      setRows(response.data);
    } catch (error) {
      console.error("Error fetching items:", error);
    }
  };
  
  fetchItems();
}, []);

// Delete item:
const handleDelete = async (id) => {
  try {
    await axios.delete(`http://localhost:5264/api/items/${id}`);
    // Refresh the list
  } catch (error) {
    console.error("Error deleting item:", error);
  }
};

// Update item:
const handleEdit = async (id, updates) => {
  try {
    await axios.put(`http://localhost:5264/api/items/${id}`, updates);
    // Refresh the list
  } catch (error) {
    console.error("Error updating item:", error);
  }
};
```

## 🔄 API Response Format

The API returns items in the format expected by your React component:

```json
[
  {
    "id": 1,
    "item": "Wireless Mouse",
    "description": "Ergonomic Bluetooth mouse",
    "price": "3,500 LKR",
    "qty": 12,
    "isActive": true
  }
]
```

## 🛠️ Common Tasks

### Add Sample Data
You can use Swagger UI (`http://localhost:5264/swagger`) to add test items, or use the frontend form.

### Stop the Server
Press `Ctrl+C` in the terminal where the server is running.

### Restart the Server
```bash
cd "d:\Campus Projects\quotation-generator\quotation-generator-back-end"
dotnet run
```

### View Database
Use SQL Server Management Studio or Azure Data Studio to connect to:
- Server: `localhost\SQLEXPRESS`
- Database: `QuotationGeneratorDb`
- Authentication: Windows Authentication

### Make Database Changes
1. Update the model in `Models/Item.cs`
2. Create migration: `dotnet ef migrations add MigrationName`
3. Update database: `dotnet ef database update`

## 📦 Installed Packages
- Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
- Microsoft.EntityFrameworkCore.Design (9.0.0)
- Microsoft.EntityFrameworkCore.Tools (10.0.0)
- Swashbuckle.AspNetCore (6.6.2)

## ✨ Key Features Implemented

1. **Filtering** - Search items by title or description
2. **Active/Inactive** - Filter items by status
3. **Validation** - Input validation for all fields
4. **CORS** - Frontend can connect from localhost:3000, 5173, 5174
5. **Error Handling** - Proper error messages for all operations
6. **Bulk Delete** - Delete multiple items at once
7. **Price Formatting** - Automatically formats prices as "X,XXX LKR"

## 🎯 Next Steps

1. **Test the API** - Open Swagger UI and try the endpoints
2. **Update Frontend** - Change axios URLs to point to `http://localhost:5264`
3. **Add Sample Data** - Create a few items to test the UI
4. **Implement Edit** - Connect the Edit button in your ItemRow component
5. **Implement Delete** - Connect the Delete button in your ItemRow component

Your backend is ready to go! 🚀
