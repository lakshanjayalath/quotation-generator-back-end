# Client Management API - Setup Complete! 🎉

## ✅ What Has Been Created

### 1. **Models**
- `Models/Client.cs` - Client entity with:
  - Client Details: ClientName, ClientIdNumber, ClientContactNumber, ClientAddress, ClientEmail
  - Company Details: Name, Number, Group, AssignedUser, IdNumber, VatNumber, Website, Phone, RoutingId, ValidVat, TaxExempt, Classification
  - Billing Address: BillingStreet, BillingSuite, BillingCity, BillingState, BillingPostalCode, BillingCountry
  - Shipping Address: ShippingStreet, ShippingSuite, ShippingCity, ShippingState, ShippingPostalCode, ShippingCountry
  - Metadata: IsActive, CreatedDate, UpdatedAt
  - Navigation: Contacts collection

- `Models/ClientContact.cs` - Client contact entity with:
  - FirstName, LastName, Email, Phone, AddToInvoices
  - Foreign key relationship to Client

### 2. **DTOs (Data Transfer Objects)**
- `DTOs/CreateClientDto.cs` - For creating new clients
- `DTOs/UpdateClientDto.cs` - For updating existing clients
- `DTOs/ClientResponseDto.cs` - For returning client list (matches your React component)
- `DTOs/ClientContactDto.cs` - For contact data transfer

### 3. **Database Context**
- Updated `Data/ApplicationDbContext.cs` with:
  - Clients DbSet
  - ClientContacts DbSet
  - Client-Contact relationship configuration (cascade delete)

### 4. **Controller**
- `Controllers/ClientsController.cs` - Full CRUD API with:
  - GET `/api/clients` - List all clients with optional filtering by name/company/email and active status
  - GET `/api/clients/{id}` - Get single client with contacts
  - POST `/api/clients` - Create new client with contacts
  - PUT `/api/clients/{id}` - Update client (all fields optional)
  - DELETE `/api/clients/{id}` - Delete single client
  - DELETE `/api/clients/bulk` - Delete multiple clients

### 5. **Database**
- Created and applied migration: `AddClientsTable`
- Database tables created:
  - `Clients` table with all client fields
  - `ClientContacts` table with foreign key to Clients
  - Index on ClientContacts.ClientId for performance

## 🚀 Your Backend is Running!

**Server URL:** `http://localhost:5264`
**Swagger UI:** `http://localhost:5264/swagger`

## 📝 API Endpoints

### Clients API (`/api/clients`)

#### Get All Clients
```
GET /api/clients
Query Parameters:
  - filter (optional): Filter by client name, company name, or email
  - isActive (optional): Filter by active status (true/false)

Example: GET /api/clients?filter=Lakshan&isActive=true
```

**Response:**
```json
[
  {
    "clientId": 1,
    "name": "Lakshan Perera",
    "companyName": "TechLabs Pvt Ltd",
    "email": "lakshan.perera@example.com",
    "contactNumber": "+94711234567",
    "createdDate": "2025-01-05",
    "isActive": true
  }
]
```

#### Get Single Client
```
GET /api/clients/{id}
```

**Response:**
```json
{
  "id": 1,
  "clientName": "Lakshan Perera",
  "clientEmail": "lakshan.perera@example.com",
  "clientContactNumber": "+94711234567",
  "name": "TechLabs Pvt Ltd",
  "contacts": [
    {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "phone": "+94771234567",
      "addToInvoices": true
    }
  ],
  ...all other fields
}
```

#### Create Client
```
POST /api/clients
Content-Type: application/json

{
  "clientName": "Lakshan Perera",
  "clientIdNumber": "C001",
  "clientContactNumber": "+94711234567",
  "clientAddress": "123 Main St, Colombo",
  "clientEmail": "lakshan.perera@example.com",
  "name": "TechLabs Pvt Ltd",
  "number": "COM123",
  "group": "Group A",
  "assignedUser": "User 1",
  "idNumber": "ID123",
  "vatNumber": "VAT456",
  "website": "https://techlabs.lk",
  "phone": "+94112345678",
  "routingId": "R001",
  "validVat": true,
  "taxExempt": false,
  "classification": "Type A",
  "billingStreet": "123 Main St",
  "billingSuite": "Suite 100",
  "billingCity": "Colombo",
  "billingState": "Western",
  "billingPostalCode": "00100",
  "billingCountry": "Sri Lanka",
  "shippingStreet": "456 Park Ave",
  "shippingSuite": "Suite 200",
  "shippingCity": "Colombo",
  "shippingState": "Western",
  "shippingPostalCode": "00200",
  "shippingCountry": "Sri Lanka",
  "contacts": [
    {
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "phone": "+94771234567",
      "addToInvoices": true
    }
  ]
}
```

#### Update Client
```
PUT /api/clients/{id}
Content-Type: application/json

{
  "clientName": "Updated Name",
  "clientEmail": "updated@example.com",
  "isActive": false,
  "contacts": [
    {
      "firstName": "Jane",
      "lastName": "Smith",
      "email": "jane@example.com",
      "phone": "+94779876543",
      "addToInvoices": false
    }
  ]
}
```

Note: All fields are optional. Only include fields you want to update.

#### Delete Single Client
```
DELETE /api/clients/{id}
```

#### Bulk Delete Clients
```
DELETE /api/clients/bulk
Content-Type: application/json

[1, 2, 3, 4, 5]
```

## 🔧 Frontend Integration

Update your React components to use the API:

### Fetch Clients (ClientPage.jsx)
```javascript
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5264/api';

// In your ClientPage component
useEffect(() => {
  const fetchClients = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/clients`);
      setRows(response.data);
    } catch (error) {
      console.error("Error fetching clients:", error);
    }
  };
  
  fetchClients();
}, []);

// Filter with active status
const fetchFilteredClients = async (searchText, isActive) => {
  try {
    const params = new URLSearchParams();
    if (searchText) params.append('filter', searchText);
    if (isActive !== undefined) params.append('isActive', isActive);
    
    const response = await axios.get(`${API_BASE_URL}/clients?${params}`);
    setRows(response.data);
  } catch (error) {
    console.error("Error fetching clients:", error);
  }
};
```

### Create Client (NewClientForm.jsx)
```javascript
const handleSave = async () => {
  try {
    await axios.post(`${API_BASE_URL}/clients`, formData);
    setShowSuccess(true);
    if (onSave) onSave(formData);
    
    // Optionally navigate back
    navigate('/ClientPage');
  } catch (error) {
    console.error("Error saving client:", error);
    // Show error message to user
  }
};
```

### Update Client
```javascript
const handleUpdate = async (id, updates) => {
  try {
    await axios.put(`${API_BASE_URL}/clients/${id}`, updates);
    // Refresh the list
    fetchClients();
  } catch (error) {
    console.error("Error updating client:", error);
  }
};
```

### Delete Client
```javascript
const handleDelete = async (id) => {
  try {
    await axios.delete(`${API_BASE_URL}/clients/${id}`);
    // Refresh the list
    fetchClients();
  } catch (error) {
    console.error("Error deleting client:", error);
  }
};
```

### Bulk Delete
```javascript
const handleBulkDelete = async (selectedIds) => {
  try {
    await axios.delete(`${API_BASE_URL}/clients/bulk`, {
      data: selectedIds
    });
    // Refresh the list
    fetchClients();
  } catch (error) {
    console.error("Error deleting clients:", error);
  }
};
```

## 🧪 Testing with Postman

### Create a Client
1. Method: `POST`
2. URL: `http://localhost:5264/api/clients`
3. Headers: `Content-Type: application/json`
4. Body (raw JSON):
```json
{
  "clientName": "Lakshan Perera",
  "clientIdNumber": "C001",
  "clientContactNumber": "+94711234567",
  "clientAddress": "123 Main St, Colombo",
  "clientEmail": "lakshan.perera@example.com",
  "name": "TechLabs Pvt Ltd",
  "number": "COM123",
  "group": "Group A",
  "assignedUser": "User 1",
  "contacts": [
    {
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@techlabs.lk",
      "phone": "+94771234567",
      "addToInvoices": true
    }
  ]
}
```

### Get All Clients
- Method: `GET`
- URL: `http://localhost:5264/api/clients`

### Get Single Client
- Method: `GET`
- URL: `http://localhost:5264/api/clients/1`

### Update Client
- Method: `PUT`
- URL: `http://localhost:5264/api/clients/1`
- Body: (include only fields to update)

### Delete Client
- Method: `DELETE`
- URL: `http://localhost:5264/api/clients/1`

## 📊 Database Schema

### Clients Table
| Column | Type | Required | Default | Description |
|--------|------|----------|---------|-------------|
| Id | int | Yes | IDENTITY | Primary key |
| ClientName | nvarchar(200) | Yes | - | Client full name |
| ClientIdNumber | nvarchar(max) | No | - | Client ID number |
| ClientContactNumber | nvarchar(max) | No | - | Client phone |
| ClientAddress | nvarchar(max) | No | - | Client address |
| ClientEmail | nvarchar(200) | Yes | - | Client email |
| Name | nvarchar(max) | No | - | Company name |
| ... | ... | ... | ... | ... (all other fields) |
| IsActive | bit | No | true | Active status |
| CreatedDate | datetime2 | No | GETUTCDATE() | Creation date |
| UpdatedAt | datetime2 | Yes | - | Last update |

### ClientContacts Table
| Column | Type | Required | Description |
|--------|------|----------|-------------|
| Id | int | Yes | Primary key |
| FirstName | nvarchar(max) | No | Contact first name |
| LastName | nvarchar(max) | No | Contact last name |
| Email | nvarchar(max) | No | Contact email |
| Phone | nvarchar(max) | No | Contact phone |
| AddToInvoices | bit | No | Include in invoices |
| ClientId | int | Yes | Foreign key to Clients |

## ✨ Features Implemented

1. **Full CRUD Operations** - Create, Read, Update, Delete
2. **Filtering** - Search by name, company, or email
3. **Active/Inactive Status** - Filter by client status
4. **Nested Contacts** - Support for multiple contacts per client
5. **Bulk Delete** - Delete multiple clients at once
6. **Validation** - Input validation for required fields
7. **CORS Enabled** - Frontend can connect from localhost
8. **Cascade Delete** - Deleting a client removes all their contacts
9. **Date Formatting** - Returns dates in YYYY-MM-DD format

## 🎯 What's Working Now

✅ Items API (`/api/items`)
✅ Clients API (`/api/clients`)
✅ Database with 2 main tables (Items, Clients)
✅ Full CRUD operations for both
✅ Filtering and search
✅ Bulk operations
✅ Server running on `http://localhost:5264`

Your backend is fully functional! 🚀
