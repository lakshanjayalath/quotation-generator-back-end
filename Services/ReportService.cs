using System.Data;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.Models;

namespace quotation_generator_back_end.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext db, ILogger<ReportService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Dictionary<string, object>>> GenerateReportAsync(DTOs.ReportRequestDto request)
        {
            var table = await GenerateReportTableAsync(request);
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow r in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn c in table.Columns) dict[c.ColumnName] = r[c];
                rows.Add(dict);
            }
            return rows;
        }

        public async Task<DataTable> GenerateReportTableAsync(DTOs.ReportRequestDto request)
        {
            var dt = new DataTable();
            var reportType = request?.ReportType ?? "Activity";

            // Parse date filters
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (request?.Filters != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Filters.StartDate) && 
                    DateTime.TryParse(request.Filters.StartDate, out var start))
                {
                    startDate = start;
                }

                if (!string.IsNullOrWhiteSpace(request.Filters.EndDate) && 
                    DateTime.TryParse(request.Filters.EndDate, out var end))
                {
                    // Set end date to end of day
                    endDate = end.AddDays(1).AddSeconds(-1);
                }
            }

            // Get action type filter (All, Created, Updated, Deleted)
            // Check both Filters.ActionType and Filters.Activity fields, and fallback to root ActionType
            var actionType = (request?.Filters?.ActionType ?? request?.Filters?.Activity ?? request?.ActionType)?.ToLowerInvariant() ?? "all";
            
            // Debug logging
            _logger.LogInformation($"Report Request - Type: {reportType}, ActionType: {actionType}, ActionTypeField: {request?.Filters?.ActionType}, ActivityField: {request?.Filters?.Activity}, RootActionType: {request?.ActionType}, StartDate: {startDate}, EndDate: {endDate}");

            switch (reportType)
            {
                case "Products":
                    dt.Columns.Add("Product Name");
                    dt.Columns.Add("SKU");
                    dt.Columns.Add("Category");
                    dt.Columns.Add("Price", typeof(decimal));
                    dt.Columns.Add("Stock", typeof(int));
                    dt.Columns.Add("Created Date");

                    var items = await _db.Items.ToListAsync();
                    
                    // Apply date filtering for products (by CreatedAt)
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        items = items.Where(it =>
                        {
                            if (startDate.HasValue && it.CreatedAt < startDate.Value) return false;
                            if (endDate.HasValue && it.CreatedAt > endDate.Value) return false;
                            return true;
                        }).ToList();
                    }

                    // Apply action type filtering for products
                    if (actionType != "all")
                    {
                        var itemIds = items.Select(i => i.Id).ToList();
                        var productLogs = await _db.ActivityLogs
                            .Where(log => log.EntityName == "Item" && itemIds.Contains(log.RecordId))
                            .ToListAsync();

                        if (actionType == "created" || actionType == "create")
                        {
                            var createdIds = productLogs
                                .Where(log => log.ActionType.ToLower() == "create")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            items = items.Where(it => createdIds.Contains(it.Id)).ToList();
                        }
                        else if (actionType == "updated" || actionType == "update")
                        {
                            var updatedIds = productLogs
                                .Where(log => log.ActionType.ToLower() == "update")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            items = items.Where(it => updatedIds.Contains(it.Id)).ToList();
                        }
                        else if (actionType == "deleted" || actionType == "delete")
                        {
                            var deletedIds = productLogs
                                .Where(log => log.ActionType.ToLower() == "delete")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            items = items.Where(it => deletedIds.Contains(it.Id) || !it.IsActive).ToList();
                        }
                    }

                    foreach (var it in items)
                    {
                        var row = dt.NewRow();
                        row["Product Name"] = it.Title ?? string.Empty;
                        row["SKU"] = it.Id.ToString();
                        row["Category"] = it.Description ?? string.Empty;
                        row["Price"] = it.Price;
                        row["Stock"] = it.Quantity;
                        row["Created Date"] = it.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                        dt.Rows.Add(row);
                    }
                    break;

                case "Users":
                    dt.Columns.Add("User Name");
                    dt.Columns.Add("Email");
                    dt.Columns.Add("Role");
                    dt.Columns.Add("Status");
                    dt.Columns.Add("Created Date");

                    var users = await _db.Users.ToListAsync();
                    
                    // Apply date filtering for users (by CreatedAt)
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        users = users.Where(u =>
                        {
                            if (startDate.HasValue && u.CreatedAt < startDate.Value) return false;
                            if (endDate.HasValue && u.CreatedAt > endDate.Value) return false;
                            return true;
                        }).ToList();
                    }

                    // Apply action type filtering for users
                    if (actionType != "all")
                    {
                        var userIds = users.Select(u => u.Id).ToList();
                        var userLogs = await _db.ActivityLogs
                            .Where(log => log.EntityName == "User" && userIds.Contains(log.RecordId))
                            .ToListAsync();

                        if (actionType == "created" || actionType == "create")
                        {
                            var createdIds = userLogs
                                .Where(log => log.ActionType.ToLower() == "create")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            users = users.Where(u => createdIds.Contains(u.Id)).ToList();
                        }
                        else if (actionType == "updated" || actionType == "update")
                        {
                            var updatedIds = userLogs
                                .Where(log => log.ActionType.ToLower() == "update")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            users = users.Where(u => updatedIds.Contains(u.Id)).ToList();
                        }
                        else if (actionType == "deleted" || actionType == "delete")
                        {
                            var deletedIds = userLogs
                                .Where(log => log.ActionType.ToLower() == "delete")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            users = users.Where(u => deletedIds.Contains(u.Id)).ToList();
                        }
                    }

                    foreach (var u in users)
                    {
                        var r = dt.NewRow();
                        r["User Name"] = (u.FirstName + " " + u.LastName).Trim();
                        r["Email"] = u.Email ?? string.Empty;
                        r["Role"] = u.Role ?? "User";
                        r["Status"] = "Active";
                        r["Created Date"] = u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                        dt.Rows.Add(r);
                    }
                    break;

                case "Invoices":
                    dt.Columns.Add("Invoice ID");
                    dt.Columns.Add("Client");
                    dt.Columns.Add("Amount", typeof(decimal));
                    dt.Columns.Add("Date");
                    dt.Columns.Add("Status");
                    dt.Columns.Add("Due Date");

                    // Map Quotation rows as Invoices if present in DB (example: treat quotations as invoices)
                    var invoices = await _db.Quotations.Include(q => q.Client).ToListAsync();
                    
                    // Apply date filtering for invoices (by QuoteDate)
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        invoices = invoices.Where(inv =>
                        {
                            if (startDate.HasValue && inv.QuoteDate < startDate.Value) return false;
                            if (endDate.HasValue && inv.QuoteDate > endDate.Value) return false;
                            return true;
                        }).ToList();
                    }

                    // Apply action type filtering for invoices
                    if (actionType != "all")
                    {
                        var invoiceIds = invoices.Select(q => q.Id).ToList();
                        var invoiceLogs = await _db.ActivityLogs
                            .Where(log => log.EntityName == "Quotation" && invoiceIds.Contains(log.RecordId))
                            .ToListAsync();

                        if (actionType == "created" || actionType == "create")
                        {
                            var createdIds = invoiceLogs
                                .Where(log => log.ActionType.ToLower() == "create")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            invoices = invoices.Where(inv => createdIds.Contains(inv.Id)).ToList();
                        }
                        else if (actionType == "updated" || actionType == "update")
                        {
                            var updatedIds = invoiceLogs
                                .Where(log => log.ActionType.ToLower() == "update")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            invoices = invoices.Where(inv => updatedIds.Contains(inv.Id)).ToList();
                        }
                        else if (actionType == "deleted" || actionType == "delete")
                        {
                            var deletedIds = invoiceLogs
                                .Where(log => log.ActionType.ToLower() == "delete")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            invoices = invoices.Where(inv => deletedIds.Contains(inv.Id)).ToList();
                        }
                    }

                    foreach (var inv in invoices)
                    {
                        var rowInv = dt.NewRow();
                        rowInv["Invoice ID"] = inv.Id;
                        rowInv["Client"] = inv.ClientName ?? inv.Client?.ClientName ?? string.Empty;
                        rowInv["Amount"] = inv.Total;
                        rowInv["Date"] = inv.QuoteDate.ToString("yyyy-MM-dd");
                        rowInv["Status"] = inv.Status;
                        rowInv["Due Date"] = inv.ValidUntil?.ToString("yyyy-MM-dd") ?? string.Empty;
                        dt.Rows.Add(rowInv);
                    }
                    break;

                case "Quotes":
                    dt.Columns.Add("Quote ID");
                    dt.Columns.Add("Client");
                    dt.Columns.Add("Amount", typeof(decimal));
                    dt.Columns.Add("Date");
                    dt.Columns.Add("Status");
                    dt.Columns.Add("Expiry Date");

                    var quotes = await _db.Quotations.Include(q => q.Client).ToListAsync();
                    
                    // Apply date filtering for quotes (by QuoteDate)
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        quotes = quotes.Where(q =>
                        {
                            if (startDate.HasValue && q.QuoteDate < startDate.Value) return false;
                            if (endDate.HasValue && q.QuoteDate > endDate.Value) return false;
                            return true;
                        }).ToList();
                    }

                    // Apply action type filtering for quotes
                    if (actionType != "all")
                    {
                        var quoteIds = quotes.Select(q => q.Id).ToList();
                        var quoteLogs = await _db.ActivityLogs
                            .Where(log => log.EntityName == "Quotation" && quoteIds.Contains(log.RecordId))
                            .ToListAsync();

                        if (actionType == "created" || actionType == "create")
                        {
                            var createdIds = quoteLogs
                                .Where(log => log.ActionType.ToLower() == "create")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            quotes = quotes.Where(q => createdIds.Contains(q.Id)).ToList();
                        }
                        else if (actionType == "updated" || actionType == "update")
                        {
                            var updatedIds = quoteLogs
                                .Where(log => log.ActionType.ToLower() == "update")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            quotes = quotes.Where(q => updatedIds.Contains(q.Id)).ToList();
                        }
                        else if (actionType == "deleted" || actionType == "delete")
                        {
                            var deletedIds = quoteLogs
                                .Where(log => log.ActionType.ToLower() == "delete")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            quotes = quotes.Where(q => deletedIds.Contains(q.Id)).ToList();
                        }
                    }

                    foreach (var q in quotes)
                    {
                        var rq = dt.NewRow();
                        rq["Quote ID"] = q.Id;
                        rq["Client"] = q.ClientName ?? q.Client?.ClientName ?? string.Empty;
                        rq["Amount"] = q.Total;
                        rq["Date"] = q.QuoteDate.ToString("yyyy-MM-dd");
                        rq["Status"] = q.Status;
                        rq["Expiry Date"] = q.ValidUntil?.ToString("yyyy-MM-dd") ?? string.Empty;
                        dt.Rows.Add(rq);
                    }
                    break;

                case "Clients":
                    dt.Columns.Add("Client Name");
                    dt.Columns.Add("Email");
                    dt.Columns.Add("Phone");
                    dt.Columns.Add("Address");
                    dt.Columns.Add("Status");
                    dt.Columns.Add("Created Date");

                    var clients = await _db.Clients.ToListAsync();
                    
                    // Apply date filtering for clients (by CreatedDate)
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        clients = clients.Where(c =>
                        {
                            if (startDate.HasValue && c.CreatedDate < startDate.Value) return false;
                            if (endDate.HasValue && c.CreatedDate > endDate.Value) return false;
                            return true;
                        }).ToList();
                    }

                    // Apply action type filtering for clients
                    if (actionType != "all")
                    {
                        _logger.LogInformation($"Filtering Clients by actionType: {actionType}");
                        var clientIds = clients.Select(c => c.Id).ToList();
                        _logger.LogInformation($"Total clients before filtering: {clients.Count}, IDs: {string.Join(",", clientIds)}");
                        
                        var clientLogs = await _db.ActivityLogs
                            .Where(log => log.EntityName == "Client" && clientIds.Contains(log.RecordId))
                            .ToListAsync();
                        
                        _logger.LogInformation($"Activity logs found for Client entity: {clientLogs.Count}");
                        if (clientLogs.Count > 0)
                        {
                            _logger.LogInformation($"Log details: {string.Join("; ", clientLogs.Select(l => $"ID:{l.RecordId}, Action:{l.ActionType}"))}");
                        }

                        if (actionType == "created" || actionType == "create")
                        {
                            var createdIds = clientLogs
                                .Where(log => log.ActionType.ToLower() == "create")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            _logger.LogInformation($"Created IDs found: {createdIds.Count}, IDs: {string.Join(",", createdIds)}");
                            clients = clients.Where(c => createdIds.Contains(c.Id)).ToList();
                            _logger.LogInformation($"Clients after created filter: {clients.Count}");
                        }
                        else if (actionType == "updated" || actionType == "update")
                        {
                            var updatedIds = clientLogs
                                .Where(log => log.ActionType.ToLower() == "update")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            _logger.LogInformation($"Updated IDs found: {updatedIds.Count}, IDs: {string.Join(",", updatedIds)}");
                            clients = clients.Where(c => updatedIds.Contains(c.Id)).ToList();
                            _logger.LogInformation($"Clients after updated filter: {clients.Count}");
                        }
                        else if (actionType == "deleted" || actionType == "delete")
                        {
                            var deletedIds = clientLogs
                                .Where(log => log.ActionType.ToLower() == "delete")
                                .Select(log => log.RecordId)
                                .Distinct()
                                .ToList();
                            _logger.LogInformation($"Deleted IDs found: {deletedIds.Count}, IDs: {string.Join(",", deletedIds)}");
                            clients = clients.Where(c => deletedIds.Contains(c.Id) || !c.IsActive).ToList();
                            _logger.LogInformation($"Clients after deleted filter: {clients.Count}");
                        }
                    }

                    foreach (var c in clients)
                    {
                        var rc = dt.NewRow();
                        rc["Client Name"] = c.ClientName ?? string.Empty;
                        rc["Email"] = c.ClientEmail ?? string.Empty;
                        rc["Phone"] = c.ClientContactNumber ?? c.Phone ?? string.Empty;
                        rc["Address"] = c.ClientAddress ?? string.Empty;
                        rc["Status"] = c.IsActive ? "Active" : "Inactive";
                        rc["Created Date"] = c.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
                        dt.Rows.Add(rc);
                    }
                    break;

                case "Activity":
                default:
                    dt.Columns.Add("Date");
                    dt.Columns.Add("Entity");
                    dt.Columns.Add("Record ID", typeof(int));
                    dt.Columns.Add("Action");
                    dt.Columns.Add("Description");
                    dt.Columns.Add("Performed By");

                    var activityLogs = await _db.ActivityLogs.ToListAsync();
                    
                    // Apply date filtering for activity logs (by Timestamp)
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        activityLogs = activityLogs.Where(log =>
                        {
                            if (startDate.HasValue && log.Timestamp < startDate.Value) return false;
                            if (endDate.HasValue && log.Timestamp > endDate.Value) return false;
                            return true;
                        }).ToList();
                    }

                    // Apply action type filtering for activity logs (by ActionType)
                    if (actionType != "all")
                    {
                        if (actionType == "created" || actionType == "create")
                        {
                            activityLogs = activityLogs.Where(log => log.ActionType.ToLower() == "create").ToList();
                        }
                        else if (actionType == "updated" || actionType == "update")
                        {
                            activityLogs = activityLogs.Where(log => log.ActionType.ToLower() == "update").ToList();
                        }
                        else if (actionType == "deleted" || actionType == "delete")
                        {
                            activityLogs = activityLogs.Where(log => log.ActionType.ToLower() == "delete").ToList();
                        }
                    }

                    foreach (var log in activityLogs)
                    {
                        var rowLog = dt.NewRow();
                        rowLog["Date"] = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                        rowLog["Entity"] = log.EntityName ?? string.Empty;
                        rowLog["Record ID"] = log.RecordId;
                        rowLog["Action"] = log.ActionType ?? string.Empty;
                        rowLog["Description"] = log.Description ?? string.Empty;
                        rowLog["Performed By"] = log.PerformedBy ?? string.Empty;
                        dt.Rows.Add(rowLog);
                    }
                    break;
            }

            // Apply server-side sorting if requested. Support common SortBy formats:
            // "Column", "Column ASC", "Column DESC", "Column:desc", "Column|desc"
            var sortBy = request?.Options?.SortBy;
            if (!string.IsNullOrWhiteSpace(sortBy) && dt.Columns.Count > 0)
            {
                try
                {
                    // Normalize separators
                    var raw = sortBy.Trim();
                    string column = raw;
                    bool desc = false;

                    // detect separators and direction
                    if (raw.Contains(":"))
                    {
                        var parts = raw.Split(':', 2);
                        column = parts[0].Trim();
                        var dir = parts[1].Trim().ToLowerInvariant();
                        desc = dir.StartsWith("d");
                    }
                    else if (raw.Contains("|"))
                    {
                        var parts = raw.Split('|', 2);
                        column = parts[0].Trim();
                        var dir = parts[1].Trim().ToLowerInvariant();
                        desc = dir.StartsWith("d");
                    }
                    else
                    {
                        // handle "Column DESC" or "Column ASC"
                        var parts = raw.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            column = parts[0].Trim();
                            var dir = parts[1].Trim().ToLowerInvariant();
                            desc = dir.StartsWith("d");
                        }
                    }

                    // If the column name matches an existing column, apply DataView sort
                    // DataTable column names may contain spaces; put them in brackets
                    if (dt.Columns.Contains(column))
                    {
                        var sortExpression = $"[{column}] {(desc ? "DESC" : "ASC")}";
                        var dv = dt.DefaultView;
                        dv.Sort = sortExpression;
                        dt = dv.ToTable();
                    }
                    else
                    {
                        // Try to match ignoring case
                        var match = dt.Columns.Cast<DataColumn>().FirstOrDefault(c => string.Equals(c.ColumnName, column, StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                        {
                            var sortExpression = $"[{match.ColumnName}] {(desc ? "DESC" : "ASC")}";
                            var dv = dt.DefaultView;
                            dv.Sort = sortExpression;
                            dt = dv.ToTable();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to apply sort: {SortBy}", sortBy);
                    // If sorting fails, ignore and return unsorted table
                }
            }

            return dt;
        }
    }
}
