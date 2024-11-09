using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Product
{
    public string Name { get; set; }
    public double Weight { get; set; }
    public string PackagingType { get; set; }
}

public class Factory
{
    public string Name { get; set; }
    public double ProductionRate { get; set; } // Единиц в час
    public Product Product { get; set; }

    public async Task ProduceAsync(Warehouse warehouse, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000); // Симуляция времени производства (1 секунда = 1 час)
            warehouse.AddProduct(Product, ProductionRate);
            Console.WriteLine($"{Name} произвел {ProductionRate} единиц продукта {Product.Name}");
        }
    }
}

public class Warehouse
{
    private readonly object _lock = new object();
    public Dictionary<Product, double> Inventory { get; private set; } = new Dictionary<Product, double>();
    public double Capacity { get; private set; }

    public Warehouse(double capacity)
    {
        Capacity = capacity;
    }

    public void AddProduct(Product product, double quantity)
    {
        lock (_lock)
        {
            if (!Inventory.ContainsKey(product))
            {
                Inventory[product] = 0;
            }
            Inventory[product] += quantity;

            if (GetTotalQuantity() >= Capacity * 0.95)
            {
                Console.WriteLine("Склад заполнен на 95% или более. Начинаем освобождать склад.");
                RemoveProducts();
            }
        }
    }

    public double GetTotalQuantity()
    {
        return Inventory.Values.Sum();
    }

    public void RemoveProducts()
    {
        // Логика для удаления продукции со склада (например, отправка грузовикам)
        Console.WriteLine("Продукция была удалена со склада.");
        Inventory.Clear(); // Для простоты очищаем склад, можно добавить логику отправки грузовикам
    }
}

public class Truck
{
    public string Type { get; set; }
    public double Capacity { get; set; } // Вместимость грузовика

    public void Deliver(Warehouse warehouse)
    {
        // Логика доставки продукции со склада
        Console.WriteLine($"Грузовик типа {Type} забирает продукцию со склада.");
        warehouse.RemoveProducts();
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        int numberOfFactories = 5; // Количество заводов
        double n = 50; // Базовая производительность
        double M = 100; // Коэффициент вместимости склада

        List<Factory> factories = new List<Factory>
        {
            new Factory { Name = "Завод A", ProductionRate = n, Product = new Product { Name = "Продукт A", Weight = 1.0, PackagingType = "Box" }},
            new Factory { Name = "Завод B", ProductionRate = 1.1 * n, Product = new Product { Name = "Продукт B", Weight = 1.0, PackagingType = "Box" }},
            new Factory { Name = "Завод C", ProductionRate = 1.2 * n, Product = new Product { Name = "Продукт C", Weight = 1.0, PackagingType = "Box" }},
            new Factory { Name = "Завод D", ProductionRate = 1.3 * n, Product = new Product { Name = "Продукт D", Weight = 1.0, PackagingType = "Box" }},
            new Factory { Name = "Завод E", ProductionRate = 1.4 * n, Product = new Product { Name = "Продукт E", Weight = 1.0, PackagingType = "Box" }}
        };

        Warehouse warehouse = new Warehouse(M * (factories.Sum(f => f.ProductionRate)));

        List<Task> productionTasks = factories.Select(factory => factory.ProduceAsync(warehouse, CancellationToken.None)).ToList();
        await Task.WhenAll(productionTasks);
    }
}
