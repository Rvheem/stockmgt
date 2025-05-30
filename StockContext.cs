using Microsoft.EntityFrameworkCore;
using System.Configuration;
using StockManagementApp.Models;

namespace StockManagementApp
{
    // This class simply inherits from the Models.StockContext
    // Acting as a wrapper to make it accessible from the root namespace
    public class StockContext : Models.StockContext
    {
        // Inherits all configuration from the base class
    }
}
