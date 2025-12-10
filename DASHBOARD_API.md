# Dashboard API Integration Guide

## Backend Endpoints

All dashboard endpoints require JWT authentication.

### 1. Get Complete Dashboard Data
**Endpoint:** `GET /api/dashboard/data`

Retrieves all dashboard information in one call (overview, recent clients, activities, and quotations).

**Request:**
```bash
curl -X GET "http://localhost:5264/api/dashboard/data" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

**Response:**
```json
{
  "overview": {
    "totalClients": 10,
    "totalQuotations": 25,
    "totalItems": 50,
    "totalQuotationAmount": 150000.00,
    "pendingQuotations": 5,
    "approvedQuotations": 15,
    "rejectedQuotations": 5
  },
  "recentClients": [
    {
      "id": 1,
      "clientName": "Client Name",
      "clientEmail": "client@example.com",
      "clientContactNumber": "1234567890",
      "city": "New York",
      "createdDate": "2025-12-08T10:30:00"
    }
  ],
  "recentActivities": [
    {
      "id": 1,
      "entityName": "Quotation",
      "recordId": 5,
      "actionType": "Create",
      "description": "New quotation created",
      "performedBy": "John Doe",
      "timestamp": "2025-12-08T10:25:00"
    }
  ],
  "recentQuotations": [
    {
      "id": 1,
      "quoteNumber": "QT-2025-001",
      "clientName": "Client Name",
      "quoteDate": "2025-12-08T10:00:00",
      "total": 5000.00,
      "status": "Pending",
      "validUntil": "2025-12-15T10:00:00"
    }
  ]
}
```

---

### 2. Get Overview Statistics Only
**Endpoint:** `GET /api/dashboard/overview`

**Request:**
```bash
curl -X GET "http://localhost:5264/api/dashboard/overview" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Response:**
```json
{
  "totalClients": 10,
  "totalQuotations": 25,
  "totalItems": 50,
  "totalQuotationAmount": 150000.00,
  "pendingQuotations": 5,
  "approvedQuotations": 15,
  "rejectedQuotations": 5
}
```

---

### 3. Get Recent Clients
**Endpoint:** `GET /api/dashboard/recent-clients?limit=5`

**Query Parameters:**
- `limit` (optional, default: 5) - Number of recent clients to retrieve

**Request:**
```bash
curl -X GET "http://localhost:5264/api/dashboard/recent-clients?limit=5" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Response:**
```json
[
  {
    "id": 1,
    "clientName": "Client Name",
    "clientEmail": "client@example.com",
    "clientContactNumber": "1234567890",
    "city": "New York",
    "createdDate": "2025-12-08T10:30:00"
  }
]
```

---

### 4. Get Recent Activities
**Endpoint:** `GET /api/dashboard/recent-activities?limit=5`

**Query Parameters:**
- `limit` (optional, default: 5) - Number of recent activities to retrieve

**Request:**
```bash
curl -X GET "http://localhost:5264/api/dashboard/recent-activities?limit=5" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Response:**
```json
[
  {
    "id": 1,
    "entityName": "Quotation",
    "recordId": 5,
    "actionType": "Create",
    "description": "New quotation created",
    "performedBy": "John Doe",
    "timestamp": "2025-12-08T10:25:00"
  }
]
```

---

### 5. Get Recent Quotations
**Endpoint:** `GET /api/dashboard/recent-quotations?limit=5`

**Query Parameters:**
- `limit` (optional, default: 5) - Number of recent quotations to retrieve

**Request:**
```bash
curl -X GET "http://localhost:5264/api/dashboard/recent-quotations?limit=5" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Response:**
```json
[
  {
    "id": 1,
    "quoteNumber": "QT-2025-001",
    "clientName": "Client Name",
    "quoteDate": "2025-12-08T10:00:00",
    "total": 5000.00,
    "status": "Pending",
    "validUntil": "2025-12-15T10:00:00"
  }
]
```

---

## Frontend Implementation Examples

### React Example
```javascript
import { useEffect, useState } from 'react';

function Dashboard() {
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const token = localStorage.getItem('authToken');
        const response = await fetch('http://localhost:5264/api/dashboard/data', {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });

        if (!response.ok) {
          throw new Error('Failed to fetch dashboard data');
        }

        const data = await response.json();
        setDashboardData(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="dashboard">
      {/* Overview Stats */}
      <section className="overview">
        <div className="stat-card">
          <h3>Total Clients</h3>
          <p>{dashboardData.overview.totalClients}</p>
        </div>
        <div className="stat-card">
          <h3>Total Quotations</h3>
          <p>{dashboardData.overview.totalQuotations}</p>
        </div>
        <div className="stat-card">
          <h3>Total Amount</h3>
          <p>${dashboardData.overview.totalQuotationAmount.toFixed(2)}</p>
        </div>
      </section>

      {/* Recent Clients */}
      <section className="recent-clients">
        <h2>Recent Clients</h2>
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Phone</th>
              <th>Date Added</th>
            </tr>
          </thead>
          <tbody>
            {dashboardData.recentClients.map(client => (
              <tr key={client.id}>
                <td>{client.clientName}</td>
                <td>{client.clientEmail}</td>
                <td>{client.clientContactNumber}</td>
                <td>{new Date(client.createdDate).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {/* Recent Activities */}
      <section className="recent-activities">
        <h2>Recent Activities</h2>
        <div className="activity-list">
          {dashboardData.recentActivities.map(activity => (
            <div key={activity.id} className="activity-item">
              <p><strong>{activity.actionType}</strong> - {activity.entityName}</p>
              <p>{activity.description}</p>
              <small>{new Date(activity.timestamp).toLocaleString()}</small>
            </div>
          ))}
        </div>
      </section>

      {/* Recent Quotations */}
      <section className="recent-quotations">
        <h2>Recent Quotations</h2>
        <table>
          <thead>
            <tr>
              <th>Quote #</th>
              <th>Client</th>
              <th>Total</th>
              <th>Status</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            {dashboardData.recentQuotations.map(quote => (
              <tr key={quote.id}>
                <td>{quote.quoteNumber}</td>
                <td>{quote.clientName}</td>
                <td>${quote.total.toFixed(2)}</td>
                <td>{quote.status}</td>
                <td>{new Date(quote.quoteDate).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}

export default Dashboard;
```

### Vue.js Example
```vue
<template>
  <div class="dashboard">
    <div v-if="loading">Loading...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <div v-else>
      <!-- Overview Stats -->
      <section class="overview">
        <div class="stat-card">
          <h3>Total Clients</h3>
          <p>{{ dashboardData.overview.totalClients }}</p>
        </div>
        <div class="stat-card">
          <h3>Total Quotations</h3>
          <p>{{ dashboardData.overview.totalQuotations }}</p>
        </div>
        <div class="stat-card">
          <h3>Pending Quotations</h3>
          <p>{{ dashboardData.overview.pendingQuotations }}</p>
        </div>
      </section>

      <!-- Recent Clients Table -->
      <section class="recent-clients">
        <h2>Recent Clients</h2>
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Phone</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="client in dashboardData.recentClients" :key="client.id">
              <td>{{ client.clientName }}</td>
              <td>{{ client.clientEmail }}</td>
              <td>{{ client.clientContactNumber }}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- Recent Activities -->
      <section class="recent-activities">
        <h2>Recent Activities</h2>
        <div class="activity-list">
          <div v-for="activity in dashboardData.recentActivities" :key="activity.id" class="activity-item">
            <strong>{{ activity.actionType }}</strong> - {{ activity.entityName }}
            <p>{{ activity.description }}</p>
          </div>
        </div>
      </section>

      <!-- Recent Quotations -->
      <section class="recent-quotations">
        <h2>Recent Quotations</h2>
        <table>
          <thead>
            <tr>
              <th>Quote #</th>
              <th>Client</th>
              <th>Total</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="quote in dashboardData.recentQuotations" :key="quote.id">
              <td>{{ quote.quoteNumber }}</td>
              <td>{{ quote.clientName }}</td>
              <td>${{ quote.total.toFixed(2) }}</td>
              <td>{{ quote.status }}</td>
            </tr>
          </tbody>
        </table>
      </section>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      dashboardData: null,
      loading: true,
      error: null
    };
  },
  mounted() {
    this.fetchDashboardData();
  },
  methods: {
    async fetchDashboardData() {
      try {
        const token = localStorage.getItem('authToken');
        const response = await fetch('http://localhost:5264/api/dashboard/data', {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });

        if (!response.ok) {
          throw new Error('Failed to fetch dashboard data');
        }

        this.dashboardData = await response.json();
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    }
  }
};
</script>
```

### TypeScript/Axios Example
```typescript
import axios, { AxiosInstance } from 'axios';

interface OverviewDto {
  totalClients: number;
  totalQuotations: number;
  totalItems: number;
  totalQuotationAmount: number;
  pendingQuotations: number;
  approvedQuotations: number;
  rejectedQuotations: number;
}

interface RecentClientDto {
  id: number;
  clientName: string;
  clientEmail: string;
  clientContactNumber: string;
  city: string;
  createdDate: string;
}

interface RecentActivityDto {
  id: number;
  entityName: string;
  recordId: number;
  actionType: string;
  description: string | null;
  performedBy: string | null;
  timestamp: string;
}

interface RecentQuotationDto {
  id: number;
  quoteNumber: string;
  clientName: string | null;
  quoteDate: string;
  total: number;
  status: string;
  validUntil: string | null;
}

interface DashboardResponseDto {
  overview: OverviewDto;
  recentClients: RecentClientDto[];
  recentActivities: RecentActivityDto[];
  recentQuotations: RecentQuotationDto[];
}

class DashboardService {
  private api: AxiosInstance;

  constructor(baseURL: string, token: string) {
    this.api = axios.create({
      baseURL,
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
  }

  async getDashboardData(): Promise<DashboardResponseDto> {
    const response = await this.api.get<DashboardResponseDto>('/api/dashboard/data');
    return response.data;
  }

  async getOverview(): Promise<OverviewDto> {
    const response = await this.api.get<OverviewDto>('/api/dashboard/overview');
    return response.data;
  }

  async getRecentClients(limit: number = 5): Promise<RecentClientDto[]> {
    const response = await this.api.get<RecentClientDto[]>('/api/dashboard/recent-clients', {
      params: { limit }
    });
    return response.data;
  }

  async getRecentActivities(limit: number = 5): Promise<RecentActivityDto[]> {
    const response = await this.api.get<RecentActivityDto[]>('/api/dashboard/recent-activities', {
      params: { limit }
    });
    return response.data;
  }

  async getRecentQuotations(limit: number = 5): Promise<RecentQuotationDto[]> {
    const response = await this.api.get<RecentQuotationDto[]>('/api/dashboard/recent-quotations', {
      params: { limit }
    });
    return response.data;
  }
}

// Usage
const dashboardService = new DashboardService(
  'http://localhost:5264',
  localStorage.getItem('authToken') || ''
);

const dashboardData = await dashboardService.getDashboardData();
console.log(dashboardData.overview);
console.log(dashboardData.recentClients);
```

---

## Testing with Swagger

1. Navigate to `http://localhost:5264/swagger` in your browser
2. Look for the `Dashboard` section
3. Expand the endpoints and click "Try it out"
4. Click "Execute" to test the endpoints

---

## Error Handling

All endpoints return error responses in the following format:
```json
{
  "message": "Error description",
  "error": "Detailed error message"
}
```

Status codes:
- `200 OK` - Success
- `401 Unauthorized` - Missing or invalid JWT token
- `500 Internal Server Error` - Server error
