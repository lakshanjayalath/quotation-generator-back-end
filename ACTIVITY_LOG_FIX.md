# Recent Activity - Client Add/Delete Fix

## Problem
When users add or delete clients, these actions were not appearing in the "Recent Activity" section of the dashboard, even though the backend was correctly logging these operations. The activity logs had `null` values in the `performedBy` column, causing them to be filtered out.

## Root Cause
The `GetRecentActivitiesAsync()` method in `DashboardService.cs` had a filtering condition that was too strict:

```csharp
// OLD CODE (Line 162)
query = query.Where(a =>
    (userId.HasValue && a.UserId == userId)
    || (email != null && a.PerformedBy != null && a.PerformedBy.ToLower() == email));
```

This filter only checked the `PerformedBy` column, which could be null for older records or in certain scenarios. However, the `ActivityLogger` was correctly populating `PerformedByEmail`, but the filter was ignoring it.

## Solution
Updated the filtering logic to also check `PerformedByEmail`:

```csharp
// NEW CODE (Line 162-164)
query = query.Where(a =>
    (userId.HasValue && a.UserId == userId)
    || (email != null && a.PerformedBy != null && a.PerformedBy.ToLower() == email)
    || (email != null && a.PerformedByEmail != null && a.PerformedByEmail.ToLower() == email));
```

## Files Modified
- **[Services/DashboardService.cs](Services/DashboardService.cs#L162)** - Updated `GetRecentActivitiesAsync()` method

## How Activity Logging Works (End-to-End)

### 1. Backend Activity Logging
When a client is created, updated, or deleted:

**[Controllers/ClientsController.cs](Controllers/ClientsController.cs)**
- `CreateClient()` - Logs activity with `LogAsync("Client", id, "Create", description, currentUserEmail)`
- `UpdateClient()` - Logs activity with `LogAsync("Client", id, "Update", description, currentUserEmail)`
- `DeleteClient()` - Logs activity with `LogAsync("Client", id, "Delete", description, currentUserEmail)`

### 2. Activity Logger Service
**[Services/ActivityLogger.cs](Services/ActivityLogger.cs)**
- Extracts user email from JWT claims
- Sets `PerformedBy` field to the email
- Sets `PerformedByEmail` field from JWT claims
- Sets `PerformedByRole` if available
- Saves activity log to database with timestamp

### 3. Dashboard Retrieval
**[Services/DashboardService.cs](Services/DashboardService.cs)**
- `GetRecentActivitiesAsync()` retrieves recent activities
- Filters by current user's ID or email (admin sees all)
- Returns activities matching the user's email in either `PerformedBy` or `PerformedByEmail` columns

### 4. Frontend Display
The frontend calls `GET /api/dashboard/recent-activities?limit=5` and displays the activities.

## What Gets Logged

### Client Operations
- ✅ Create Client
- ✅ Update Client  
- ✅ Delete Client
- ✅ Bulk Delete Clients

### Quotation Operations
- ✅ Create Quotation
- ✅ Update Quotation
- ✅ Delete Quotation
- ✅ Approve Quotation
- ✅ Reject Quotation

### Item Operations
- ✅ Create Item
- ✅ Update Item
- ✅ Delete Item

## Database Columns in ActivityLog

| Column | Type | Purpose |
|--------|------|---------|
| `Id` | int | Primary key |
| `EntityName` | string | Type of entity (Client, Quotation, Item, etc.) |
| `RecordId` | int | ID of the modified record |
| `ActionType` | string | Create, Update, Delete, Approve, Reject |
| `Description` | string | Human-readable description of the change |
| `PerformedBy` | string | User's email who performed the action |
| `PerformedByEmail` | string | User's email (from JWT claims) |
| `PerformedByRole` | string | User's role (if available) |
| `UserId` | int | Foreign key to User table |
| `Timestamp` | datetime | When the action occurred (Sri Lanka time) |

## Testing the Fix

### 1. Add a new client via frontend
- The activity should appear in "Recent Activity" within seconds

### 2. Delete a client
- The deletion should appear in "Recent Activity"

### 3. Check Dashboard API
```bash
curl -X GET "http://localhost:5264/api/dashboard/recent-activities?limit=5" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

Should return activities with populated `performedBy` field for all recent client operations.

## Frontend Synchronization

The frontend should be calling:
- **Add Client:** Send POST to `/api/clients` with client data
- **Delete Client:** Send DELETE to `/api/clients/{id}`
- **View Activities:** GET from `/api/dashboard/recent-activities?limit=5` or `/api/dashboard/data`

## Notes
- All timestamps are stored in Sri Lanka time (UTC+5:30)
- Activity logs are created even if the main operation succeeds (handled in try-catch)
- Admins can see all activities; regular users see only their own
- The fix maintains backward compatibility with existing activity logs
