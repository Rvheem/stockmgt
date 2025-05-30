# Stock Management System

A comprehensive Windows Forms C# application for stock management.

## Features

- **Product Management**: Track inventory with quantities, thresholds for low stock alerts, and expiry dates
- **Order Management**: Create, edit, and track orders with detailed product information
- **Client Management**: Track clients and their order history
- **Supplier Management**: Manage product suppliers and their contact information
- **Delivery Tracking**: Track deliveries of orders with status updates
- **User Management**: User authentication with role-based access control
- **History Logging**: Complete audit trail of all system actions

## Installation

1. Ensure you have SQL Server (Express or higher) installed
2. Ensure you have .NET SDK 9.0 or higher installed
3. Clone the repository
4. Open the solution in Visual Studio or VS Code
5. Build and run the application
6. The application will create the database automatically on first run

## Technologies Used

- C# Windows Forms
- Entity Framework Core
- SQL Server

## Security Features

- Password hashing using SHA-256
- Role-based access control (Administrator, Manager, Employee roles)
- Complete action history logging

## Modules

### Products Module
- Add, edit, delete products
- Set stock thresholds for automatic alerts
- Track expiry dates with alerts for soon-to-expire products
- View current stock levels

### Orders Module
- Create orders with multiple products
- Calculate order totals
- Print invoices
- View order statistics

### Clients Module
- Manage client information
- View client order history
- Handle client invoices

### Suppliers Module
- Manage supplier information
- View products supplied by each supplier

### Deliveries Module
- Track order deliveries
- Update delivery status (Pending, In Transit, Delivered, Canceled)

### Users Module
- Manage system users
- Set access roles
- Reset passwords

### History Module
- View all system actions
- Filter by date, user, or action
- Export history to CSV

## License

This project is licensed under the MIT License.

---
This project was generated with .NET CLI and is ready for further development.
