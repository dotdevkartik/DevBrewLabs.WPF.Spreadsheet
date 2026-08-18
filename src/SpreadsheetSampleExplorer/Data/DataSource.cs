using Newtonsoft.Json;
using SpreadsheetSampleExplorer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace SpreadsheetSampleExplorer.Data
{
    public static class DataSource
    {
        public static IEnumerable<Customer> GetCustomers(int count = 100)
        {
            var customers = new List<Customer>();
            string[] firstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            string[] genders = { "Male", "Female", "Non-Binary", "Other" };
            var rnd = new Random(42);

            for (int i = 0; i < count; i++)
            {
                string fName = firstNames[rnd.Next(firstNames.Length)];
                string lName = lastNames[rnd.Next(lastNames.Length)];
                customers.Add(new Customer
                {
                    Id = 1000 + i,
                    Age = rnd.Next(18, 70),
                    FirstName = fName,
                    LastName = lName,
                    Email = $"{fName.ToLower()}.{lName.ToLower()}@example.com",
                    Gender = genders[rnd.Next(genders.Length)],
                    Phone = $"+1 555-{rnd.Next(100, 999)}-{rnd.Next(1000, 9999)}"
                });
            }

            return customers;
        }

        public static DataTable GetCustomersTable(int count = 100)
        {
            var customers = GetCustomers(count);
            var customersTable = new DataTable();
            var properties = typeof(Customer).GetProperties();
            foreach (var property in properties)
            {
                customersTable.Columns.Add(property.Name, property.PropertyType);
            }

            foreach(var customer in customers)
            {
                var row = customersTable.NewRow();
                foreach (var property in properties)
                {
                    row[property.Name] = property.GetValue(customer);
                }
                customersTable.Rows.Add(row);
            }    

            return customersTable;
        }

        public static List<StockData> GetStocks()
        {
            return new List<StockData>()
            {
                new StockData { Ticker = "MSFT", Company = "Microsoft Corp.", Shares = 250, BasePrice = 415.50, CurrentPrice = 443.75 },
                new StockData { Ticker = "AAPL", Company = "Apple Inc.", Shares = 300, BasePrice = 224.20, CurrentPrice = 226.54 },
                new StockData { Ticker = "NVDA", Company = "NVIDIA Corp.", Shares = 180, BasePrice = 122.80, CurrentPrice = 125.46 },
                new StockData { Ticker = "GOOGL", Company = "Alphabet Inc.", Shares = 200, BasePrice = 175.40, CurrentPrice = 182.17 },
                new StockData { Ticker = "AMZN", Company = "Amazon.com Inc.", Shares = 150, BasePrice = 186.10, CurrentPrice = 201.21 },
                new StockData { Ticker = "TSLA", Company = "Tesla Inc.", Shares = 120, BasePrice = 248.50, CurrentPrice = 255.16 },
                new StockData { Ticker = "META", Company = "Meta Platforms", Shares = 100, BasePrice = 485.30, CurrentPrice = 502.77 },
                new StockData { Ticker = "JPM", Company = "JPMorgan Chase", Shares = 350, BasePrice = 210.15, CurrentPrice = 218.17 }
            };
        }

        public static object[,] GetSortableData(int totalRows, int totalCols)
        {
            string[] firstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };
            string[] categories = { "Electronics", "Software", "Hardware", "Cloud Services", "Consulting", "Office Supplies" };
            string[] countries = { "United States", "Germany", "United Kingdom", "Canada", "Japan", "Australia", "France", "India" };
            string[] statuses = { "Completed", "Pending", "Processing", "Cancelled", "Shipped" };

            var rnd = new Random(123);
            var data = new object[totalRows + 1, totalCols];

            string[] headers = { "Order ID", "Customer Name", "Category", "Country", "Units Sold", "Unit Price ($)", "Total Revenue ($)", "Rating (1-5)", "Status" };
            for (int col = 0; col < totalCols; col++)
            {
                data[0, col] = headers[col];
            }

            for (int row = 1; row <= totalRows; row++)
            {
                string name = $"{firstNames[rnd.Next(firstNames.Length)]} {lastNames[rnd.Next(lastNames.Length)]}";
                int units = rnd.Next(1, 200);
                double price = Math.Round(10.0 + rnd.NextDouble() * 490.0, 2);
                double total = Math.Round(units * price, 2);

                data[row, 0] = 1000 + row;
                data[row, 1] = name;
                data[row, 2] = categories[rnd.Next(categories.Length)];
                data[row, 3] = countries[rnd.Next(countries.Length)];
                data[row, 4] = units;
                data[row, 5] = price;
                data[row, 6] = total;
                data[row, 7] = rnd.Next(1, 6);
                data[row, 8] = statuses[rnd.Next(statuses.Length)];
            }

            return data;
        }

        public static int[,] GetSpanningSalesData()
        {
            return new int[,] {
                { 150000, 165000, 120000, 115000, 90000, 95000 },
                { 80000, 85000, 60000, 72000, 40000, 48000 },
                { 45000, 42000, 35000, 38000, 25000, 22000 },
                { 12000, 15000, 8000, 11000, 5000, 4500 }
            };
        }

        public static object[][] GetRegionalSalesData()
        {
            return new object[][]
            {
                new object[] { "North Region", 1250, 45.00 },
                new object[] { "South Region", 980,  52.50 },
                new object[] { "East Region",  1420, 40.00 },
                new object[] { "West Region",  1100, 60.00 }
            };
        }

        public static object[][] GetExpenseData()
        {
            return new object[][]
            {
                new object[] { "Research & Development",  15000, 18000, 16000, 20000 },
                new object[] { "Sales & Marketing",       22000, 25000, 21000, 28000 },
                new object[] { "Payroll & Benefits",      45000, 46000, 47000, 48000 },
                new object[] { "IT & Infrastructure",     8000,  8500,  9000,  9500  },
                new object[] { "Office & Admin",          5000,  5200,  5100,  5500  }
            };
        }

        public static object[,] GetCellTypesData(int totalRows, int totalCols)
        {
            var data = new object[totalRows, totalCols];
            Random rnd = new Random();
            for (int row = 0; row < totalRows; row++)
            {
                data[row, 0] = $"Text {row + 1}";
                data[row, 1] = rnd.Next(1, 10) % 2 == 0 ? true : rnd.NextDouble() < 0.5 ? false : (bool?)null;
                data[row, 2] = rnd.Next(10000, 20000);
                data[row, 3] = new DateTime(rnd.Next(2001, 2020), rnd.Next(1, 12), rnd.Next(1, 28));
            }
            return data;
        }

        public static object[,] GetEmployeesData(int rowCount, int colCount)
        {
            string[] departments = { "Engineering", "Sales", "Marketing", "Finance", "Human Resources", "Operations", "Legal", "Product" };
            string[] regions = { "North America", "Europe", "Asia Pacific", "Latin America", "Middle East" };
            string[] statuses = { "Active", "Pending", "Completed", "On Hold", "Archived" };

            var rnd = new Random(42);
            var data = new object[rowCount, colCount];

            string[] headers = { "ID", "Employee Ref", "Department", "Region", "Salary ($)", "Score", "Projects", "Status", "Year Joined", "Security Code" };
            for (int col = 0; col < colCount; col++)
            {
                data[0, col] = headers[col];
            }

            for (int row = 1; row < rowCount; row++)
            {
                data[row, 0] = row;
                data[row, 1] = $"EMP-{100000 + row}";
                data[row, 2] = departments[rnd.Next(departments.Length)];
                data[row, 3] = regions[rnd.Next(regions.Length)];
                data[row, 4] = rnd.Next(45000, 185000);
                data[row, 5] = Math.Round(3.0 + rnd.NextDouble() * 2.0, 1);
                data[row, 6] = rnd.Next(1, 15);
                data[row, 7] = statuses[rnd.Next(statuses.Length)];
                data[row, 8] = rnd.Next(2010, 2026);
                data[row, 9] = $"SEC-{rnd.Next(1000, 9999)}";
            }
            return data;
        }

        public static object[,] GetSpreadPropertiesData(int rowCount, int colCount)
        {
            object[,] data = new object[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    data[r, c] = $"Data {r},{c}";
                }
            }
            return data;
        }

        public static object[,] GetZoomingData(int rowCount, int colCount)
        {
            var data = new object[rowCount, colCount];
            
            data[0, 0] = "Quarter";
            data[0, 1] = "Region";
            data[0, 2] = "Sales Rep";
            data[0, 3] = "Revenue ($)";
            data[0, 4] = "Target ($)";
            data[0, 5] = "Status";

            string[] regions = { "North", "South", "East", "West" };
            string[] reps = { "Alice Smith", "Bob Jones", "Carol Vance", "David Miller", "Eva Green" };

            Random rand = new Random(42);
            for (int r = 1; r < rowCount; r++)
            {
                data[r, 0] = $"Q{(r % 4) + 1}";
                data[r, 1] = regions[rand.Next(regions.Length)];
                data[r, 2] = reps[rand.Next(reps.Length)];
                int rev = rand.Next(15000, 95000);
                int tgt = rand.Next(20000, 80000);
                data[r, 3] = rev;
                data[r, 4] = tgt;
                data[r, 5] = rev >= tgt ? "Met Target" : "Under Target";
            }

            return data;
        }
    }
}
