# Order Service UI - Blazor WebAssembly

A modern Blazor WebAssembly UI for managing orders in the OrderService microservice.

## Features

- ✅ **Create** new orders
- 📝 **Read** and view all orders
- ✏️ **Update** existing orders
- 🗑️ **Delete** orders
- 🎨 Modern, responsive UI with gradient design
- ⚡ Real-time updates
- 🔄 Async operations with proper error handling

## Prerequisites

- .NET 8.0 SDK or later
- OrderService API running on `http://localhost:5000`

## Project Structure

```
OrderUI/
├── Models/
│   └── Order.cs                 # Order data model
├── Services/
│   └── OrderService.cs          # HTTP client service for API calls
├── Pages/
│   └── Orders.razor             # Main CRUD page
├── wwwroot/
│   ├── css/
│   │   └── app.css             # Styling
│   └── index.html              # HTML entry point
├── App.razor                    # App router
├── Program.cs                   # App configuration
└── _Imports.razor              # Global imports
```

## Getting Started

### 1. Ensure the OrderService API is Running

First, make sure your OrderService API is running:

```bash
cd ../OrderService
dotnet run
```

The API should be accessible at `http://localhost:5000`.

### 2. Enable CORS on the API

Add CORS support to your OrderService API in `Program.cs`:

```csharp
// Add before builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        policy => policy
            .WithOrigins("http://localhost:5001", "https://localhost:5001")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add after app.Build() but before app.Run()
app.UseCors("AllowBlazorApp");
```

### 3. Run the Blazor UI

```bash
cd OrderUI
dotnet restore
dotnet run
```

The application will start at `http://localhost:5001`.

### 4. Open in Browser

Navigate to `http://localhost:5001` in your web browser.

## Usage

### Creating an Order

1. Fill in the form fields:
   - **Product ID**: The ID of the product to order
   - **Quantity**: Number of items
   - **Total Price**: Total cost of the order
   - **User ID**: The user placing the order
2. Click **"Create Order"**

### Editing an Order

1. Click the **"Edit"** button on any order in the table
2. Modify the fields as needed
3. Click **"Update Order"** to save changes
4. Click **"Cancel"** to discard changes

### Deleting an Order

1. Click the **"Delete"** button on any order
2. Confirm the deletion

## API Configuration

The UI connects to the OrderService API at `http://localhost:5000`. To change this:

1. Open `Program.cs`
2. Modify the HttpClient BaseAddress:

```csharp
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("YOUR_API_URL") });
```

## Customization

### Styling

Edit `wwwroot/css/app.css` to customize colors, fonts, and layout.

### API Endpoint

The default API endpoint is `/api/orders`. This matches the OrderService controller route.

## Troubleshooting

### CORS Errors

If you see CORS errors in the browser console:
- Ensure CORS is properly configured in the OrderService API
- Check that the allowed origins match the Blazor app URL

### Connection Errors

If the UI can't connect to the API:
- Verify the API is running on `http://localhost:5000`
- Check the HttpClient BaseAddress in `Program.cs`
- Ensure no firewall is blocking the connection

### Build Errors

If you encounter build errors:
```bash
dotnet clean
dotnet restore
dotnet build
```

## Development

### Hot Reload

The application supports hot reload. Changes to `.razor` and `.cs` files will automatically refresh the browser.

### Debug Mode

To run in debug mode with browser developer tools:
```bash
dotnet run --configuration Debug
```

## Production Build

To create a production build:

```bash
dotnet publish -c Release -o ./publish
```

The output will be in the `publish/wwwroot` folder and can be served by any static web server.

## Technologies Used

- **Blazor WebAssembly** - Client-side web framework
- **.NET 8.0** - Runtime and SDK
- **HttpClient** - API communication
- **CSS3** - Styling with gradients and animations

## License

This project is part of the OrderService microservice system.
