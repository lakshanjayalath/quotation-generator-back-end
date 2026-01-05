# Dashboard User Filtering - Implementation Summary

## Overview
Implemented role-based filtering for dashboard endpoints to ensure users only see their own data while admins see everything.

## Changes Made

### 1. Activity Logger Service ([Services/ActivityLogger.cs](Services/ActivityLogger.cs))
- Added `IHttpContextAccessor` injection to capture current user context
- Automatically records `UserId` from JWT claims when logging activities
- Falls back to email if userId is not available

### 2. Dashboard Service ([Services/DashboardService.cs](Services/DashboardService.cs))
- Added `IHttpContextAccessor` injection for user context
- Implemented three helper methods:
  - `GetCurrentUserId()` - Extracts user ID from JWT claims
  - `GetCurrentUserEmail()` - Extracts user email from JWT claims
  - `IsAdmin()` - Checks if current user has Admin role

#### Recent Activities Filtering
- **Admin**: Sees all activities
- **User**: Only sees activities where `UserId` matches their ID or `PerformedBy` matches their email

#### Recent Quotations Filtering
- **Admin**: Sees all quotations
- **User**: Only sees quotations where:
  - `CreatedById` matches their user ID, OR
  - `CreatedByEmail` matches their email, OR
  - `AssignedUser` matches their email

#### Overview/Pipeline Data Filtering
- **Admin**: Statistics from all quotations in the period
- **User**: Statistics only from their own/assigned quotations in the period

### 3. Activity Logs Controller ([Controllers/ActivityLogsController.cs](Controllers/ActivityLogsController.cs))
- Removed `[AllowAnonymous]` attributes - all endpoints now require authentication
- Added role-based filtering to all three endpoints:
  - `GET /api/ActivityLogs` - Get all activity logs
  - `POST /api/ActivityLogs/filter` - Filter activity logs
  - `GET /api/ActivityLogs/{entityName}/{recordId}` - Get logs for specific entity
- **Admin**: Sees all activity logs including admin activities
- **User**: Filtered to exclude admin-performed activities using:
  - `User.Role != "Admin"` for logs with UserId populated
  - Excludes logs where `PerformedBy` contains "admin" (case-insensitive) for legacy logs

### 4. Dashboard Controller ([Controllers/DashboardController.cs](Controllers/DashboardController.cs))
- Removed all `[AllowAnonymous]` attributes
- All endpoints now require JWT authentication via `[Authorize]` attribute
- This ensures user context is available for filtering

### 5. Program.cs
- Registered `IHttpContextAccessor` to enable services to access the current HTTP request context

## Security Benefits

1. **Data Isolation**: Users can only see their own activities and quotations
2. **Admin Visibility**: Admin users maintain full oversight of all system activities
3. **Privacy**: Admin activities are hidden from regular users
4. **Authentication Required**: All dashboard and activity log endpoints require valid JWT tokens

## Testing

### As a Regular User:
1. Login with user credentials
2. Navigate to dashboard - should only see:
   - Your own created quotations
   - Quotations assigned to you
   - Your own activity logs
   - Statistics based on your quotations only

### As an Admin:
1. Login with admin credentials
2. Navigate to dashboard - should see:
   - All quotations from all users
   - All activity logs including admin activities
   - System-wide statistics

## Database Compatibility

- Works with existing activity logs that don't have `UserId` populated
- Fallback filtering using `PerformedBy` field ensures legacy data is still filtered correctly
- New activity logs will have proper `UserId` for accurate filtering

## Frontend Impact

- Frontend must send valid JWT token with all dashboard API requests
- No changes needed to API request/response structure
- Data returned will be automatically filtered based on logged-in user's role

## Notes

- The server is currently running and will automatically reload with these changes (hot reload enabled)
- No database migration required - uses existing fields
- All filtering happens at the service layer for consistency
- Email comparison is case-insensitive for reliability
