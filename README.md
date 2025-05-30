# Stock Management System

A comprehensive Windows Forms application for inventory and stock management with SQL Server integration.

## Features

- **Product Management**: Track inventory with quantities, thresholds for low stock alerts, and expiry dates
- **Order Management**: Create, edit, and track orders with detailed product information
- **Client Management**: Track clients and their order history
- **Supplier Management**: Manage product suppliers and their contact information
- **Delivery Tracking**: Track deliveries of orders with status updates
- **User Management**: User authentication with role-based access control
- **History Logging**: Complete audit trail of all system actions

## Prerequisites

- Windows 10 or higher
- [SQL Server 2019](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher)
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [.NET SDK 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) or higher

## Database Setup

### Option 1: Create a New Database

1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Right-click on "Databases" in the Object Explorer and select "New Database"
4. Enter "StockManagementDB" as the database name and click "OK"
5. The application will create all necessary tables on first run through Entity Framework migrations

### Option 2: Restore from Backup File

1. Copy your `database_full.bak` file to a location accessible by SQL Server
   - Typical location: `C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\Backup`
   - Replace `MSSQL15.SQLEXPRESS` with your SQL Server instance folder

2. Open SQL Server Management Studio and connect to your SQL Server instance

3. Right-click on "Databases" in the Object Explorer and select tasks then "Restore Database"

4. Select "Device" as the source and click the "..." button

5. In the "Select backup devices" dialog, click "Add"

6. Browse to the location of your .bak file, select it, and click "OK"

7. In the "Destination" section, enter "StockManagementDB" as the database name

8. Click "OK" to start the restore process

9. Wait for the restore operation to complete successfully

## Connecting the Application to the Database

The application uses Entity Framework Core to connect to the database. You may need to modify the connection string in the `StockContext.cs` file:

1. Open the `Models/StockContext.cs` file in your code editor
2. Locate the `OnConfiguring` method
3. Modify the connection string to match your SQL Server instance:

   ```csharp
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       if (!optionsBuilder.IsConfigured)
       {
           optionsBuilder.UseSqlServer(@"Server=YOUR_SERVER_NAME\SQLEXPRESS;Database=StockManagementDB;Trusted_Connection=True;TrustServerCertificate=True;");
       }
   }
   ```

Replace `YOUR_SERVER_NAME\SQLEXPRESS` with your SQL Server instance name:
- For default instance: `localhost` or `.`
- For named instance: `localhost\SQLEXPRESS` or `.\SQLEXPRESS`
- For network instance: `SERVER_IP\INSTANCE_NAME`

## Building and Running the Application

### Using Visual Studio

1. Open the solution file in Visual Studio
2. Restore NuGet packages (right-click on solution > Restore NuGet Packages)
3. Build the solution (Build > Build Solution or F6)
4. Run the application (Debug > Start Debugging or F5)

### Using Command Line

1. Open a command prompt in the project directory
2. Restore packages:
   ```
   dotnet restore
   ```
3. Build the application:
   ```
   dotnet build
   ```
4. Run the application:
   ```
   dotnet run
   ```

## First-Time Login

The application creates a default administrator account on first run:
- Username: `admin`
- Password: `admin`

**Important**: Change the default password after first login for security reasons.

## Troubleshooting

### Database Connection Issues

- Verify SQL Server is running (Services app > SQL Server service should be "Running")
- Check the connection string in `StockContext.cs`
- Ensure your Windows account has permission to access the database
- If using SQL authentication, update the connection string to include username/password

### Entity Framework Migration Errors

If you see migration errors on first run:

1. Open a command prompt in the project directory
2. Run the following commands:
   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

### SQL Server Network Issues

If you cannot connect to a remote SQL Server:
- Check SQL Server Configuration Manager to ensure TCP/IP is enabled
- Verify the SQL Server Browser service is running
- Check Windows Firewall settings to allow SQL Server ports (default: 1433)

## Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)
- [Windows Forms Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)

## License

This project is licensed under the MIT License - see the LICENSE file for details.
