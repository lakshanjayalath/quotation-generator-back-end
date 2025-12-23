# Frontend Integration Guide for User Profile API

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/users/profile` | Get current user's profile |
| `PUT` | `/api/users/profile` | Update profile details |
| `PUT` | `/api/users/profile/password` | Change password |
| `PUT` | `/api/users/profile/notes` | Update notes |
| `POST` | `/api/users/profile/image` | Upload profile image |
| `DELETE` | `/api/users/profile/image` | Delete profile image |
| `GET` | `/api/users/profile/quotations?page=1&pageSize=10&status=` | Get quotation history |

## Updated React Component Code

Replace your `handleSave` and other handlers with:

```javascript
// API Base URL
const API_URL = "http://localhost:5264/api";

// Get auth token from your auth context/storage
const getAuthHeaders = () => ({
  headers: {
    Authorization: `Bearer ${localStorage.getItem("token")}`,
    "Content-Type": "application/json",
  },
});

// Save profile changes
const handleSave = async () => {
  try {
    // Update profile details
    await axios.put(
      `${API_URL}/users/profile`,
      {
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        phone: user.phone,
        street: user.street,
        city: user.city,
        state: user.state,
        postalCode: user.postalCode,
        country: user.country,
        language: user.language,
        preferredContactMethod: user.preferredContactMethod,
        notifications: user.notifications,
      },
      getAuthHeaders()
    );

    // Change password if provided
    if (user.newPassword) {
      if (user.newPassword !== user.confirmPassword) {
        alert("New passwords do not match!");
        return;
      }
      
      await axios.put(
        `${API_URL}/users/profile/password`,
        {
          currentPassword: user.currentPassword,
          newPassword: user.newPassword,
          confirmPassword: user.confirmPassword,
        },
        getAuthHeaders()
      );
      
      // Clear password fields after successful change
      setUser(prev => ({
        ...prev,
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
      }));
    }

    alert("Profile updated successfully!");
  } catch (error) {
    console.error("Save error:", error);
    alert(error.response?.data?.message || "Failed to save changes");
  }
};

// Save notes
const handleSaveNotes = async () => {
  try {
    await axios.put(
      `${API_URL}/users/profile/notes`,
      { notes: user.notes },
      getAuthHeaders()
    );
    alert("Notes saved successfully!");
  } catch (error) {
    console.error("Notes save error:", error);
    alert(error.response?.data?.message || "Failed to save notes");
  }
};

// Upload profile image
const handleImageUpload = async (e) => {
  const file = e.target.files[0];
  if (!file) return;

  // Show preview immediately
  setProfileImage(URL.createObjectURL(file));

  // Upload to server
  const formData = new FormData();
  formData.append("image", file);

  try {
    const res = await axios.post(
      `${API_URL}/users/profile/image`,
      formData,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
          "Content-Type": "multipart/form-data",
        },
      }
    );
    
    if (res.data.success) {
      setProfileImage(`${API_URL.replace('/api', '')}${res.data.imageUrl}`);
    }
  } catch (error) {
    console.error("Image upload error:", error);
    alert(error.response?.data?.message || "Failed to upload image");
  }
};

// Load profile on mount
useEffect(() => {
  const loadProfile = async () => {
    try {
      const res = await axios.get(
        `${API_URL}/users/profile`,
        getAuthHeaders()
      );
      
      setUser(prev => ({
        ...prev,
        firstName: res.data.firstName || "",
        lastName: res.data.lastName || "",
        email: res.data.email || "",
        phone: res.data.phone || "",
        street: res.data.street || "",
        city: res.data.city || "",
        state: res.data.state || "",
        postalCode: res.data.postalCode || "",
        country: res.data.country || "",
        language: res.data.language || "English",
        preferredContactMethod: res.data.preferredContactMethod || "Email",
        notifications: res.data.notifications ?? true,
        notes: res.data.notes || "",
      }));
      
      if (res.data.profileImageUrl) {
        setProfileImage(`http://localhost:5264${res.data.profileImageUrl}`);
      }
      
      // Set quotation summary
      setQuotationSummary(res.data.quotationSummary);
      setQuotations(res.data.quotations || []);
    } catch (err) {
      console.error("Profile load error", err);
    } finally {
      setLoading(false);
    }
  };
  loadProfile();
}, []);
```

## Updated Quotation Summary Cards

```javascript
// Add state for quotation summary
const [quotationSummary, setQuotationSummary] = useState({
  total: 0,
  pending: 0,
  approved: 0,
  rejected: 0,
  totalValue: 0,
});

// Update the cards to show real data
<Box sx={{ display: "flex", flexDirection: "row", gap: 2, mb: 3, overflowX: "auto" }}>
  <Card sx={{ p: 2, minWidth: 120, flex: "0 0 auto" }}>
    <Typography variant="subtitle2">Total</Typography>
    <Typography variant="h5">{quotationSummary.total}</Typography>
  </Card>
  <Card sx={{ p: 2, minWidth: 120, flex: "0 0 auto" }}>
    <Typography variant="subtitle2">Pending</Typography>
    <Typography variant="h5">{quotationSummary.pending}</Typography>
  </Card>
  <Card sx={{ p: 2, minWidth: 120, flex: "0 0 auto" }}>
    <Typography variant="subtitle2">Approved</Typography>
    <Typography variant="h5">{quotationSummary.approved}</Typography>
  </Card>
  <Card sx={{ p: 2, minWidth: 120, flex: "0 0 auto" }}>
    <Typography variant="subtitle2">Rejected</Typography>
    <Typography variant="h5">{quotationSummary.rejected}</Typography>
  </Card>
  <Card sx={{ p: 2, minWidth: 120, flex: "0 0 auto" }}>
    <Typography variant="subtitle2">Value</Typography>
    <Typography variant="h5">
      ${quotationSummary.totalValue?.toLocaleString() || 0}
    </Typography>
  </Card>
</Box>
```

## Notes Tab - Updated Save Button

```javascript
{tab === 2 && (
  <Paper sx={{ p: 4, mt: 3 }}>
    <TextField 
      label="Personal Notes" 
      multiline 
      rows={5} 
      fullWidth 
      value={user.notes} 
      onChange={handleChange("notes")} 
      placeholder="Add your personal notes here..."
    />
    <Box mt={3} display="flex" justifyContent="flex-end">
      <Button variant="contained" onClick={handleSaveNotes}>Save Notes</Button>
    </Box>
  </Paper>
)}
```

## Important Notes

1. **Authentication Required**: All profile endpoints require a valid JWT token in the Authorization header.

2. **CORS**: Make sure your backend has CORS configured to accept requests from your frontend origin.

3. **Static Files**: Profile images are served from `/uploads/profiles/` - ensure your backend is configured to serve static files from `wwwroot`.

4. **Error Handling**: The API returns structured error responses with a `message` field.
