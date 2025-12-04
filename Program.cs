using System;
using System.Collections.Generic;
using System.Text;

namespace MilitaryStrategyPattern
{
    // -----------------------------------------------------------
    // 1. Components (Компоненти сценарію)
    // -----------------------------------------------------------

    /// <summary>
    /// Клас, що описує війська.
    /// Інкапсуляція: властивості доступні тільки для читання ззовні, ініціалізація через конструктор.
    /// </summary>
    public class Troop
    {
        public string Name { get; }
        public int Count { get; }

        public Troop(string name, int count)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Назва військ не може бути порожньою.");
            if (count < 0) throw new ArgumentException("Кількість військ не може бути від'ємною.");

            Name = name;
            Count = count;
        }

        public override string ToString() => $"{Name}: {Count} од.";
    }

    /// <summary>
    /// Клас, що описує ресурси.
    /// </summary>
    public class Resource
    {
        public string Type { get; }
        public double Amount { get; }

        public Resource(string type, double amount)
        {
            Type = type;
            Amount = amount;
        }

        public override string ToString() => $"{Type}: {Amount}";
    }

    /// <summary>
    /// Клас, що описує карту.
    /// </summary>
    public class Map
    {
        public string TerrainName { get; }
        public string WeatherCondition { get; }

        public Map(string terrainName, string weatherCondition)
        {
            TerrainName = terrainName;
            WeatherCondition = weatherCondition;
        }

        public override string ToString() => $"Місцевість: {TerrainName}, Погода: {WeatherCondition}";
    }

    // -----------------------------------------------------------
    // 2. Product (Продукт)
    // -----------------------------------------------------------

    /// <summary>
    /// Складний об'єкт - Військовий сценарій.
    /// </summary>
    public class StrategyScenario
    {
        // Використовуємо приватні поля для списків, щоб запобігти прямій модифікації ззовні (Інкапсуляція)
        private readonly List<Troop> _troops = new List<Troop>();
        private readonly List<Resource> _resources = new List<Resource>();

        public Map ScenarioMap { get; set; }

        public void AddTroop(Troop troop)
        {
            _troops.Add(troop);
        }

        public void AddResource(Resource resource)
        {
            _resources.Add(resource);
        }

        public string GetScenarioDescription()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ВІЙСЬКОВИЙ СЦЕНАРІЙ ===");

            sb.AppendLine("[Карта]");
            sb.AppendLine(ScenarioMap != null ? ScenarioMap.ToString() : "Карту не обрано");

            sb.AppendLine("[Війська]");
            if (_troops.Count > 0)
                _troops.ForEach(t => sb.AppendLine($" - {t}"));
            else
                sb.AppendLine(" - Війська відсутні");

            sb.AppendLine("[Ресурси]");
            if (_resources.Count > 0)
                _resources.ForEach(r => sb.AppendLine($" - {r}"));
            else
                sb.AppendLine(" - Ресурси відсутні");

            return sb.ToString();
        }
    }

    // -----------------------------------------------------------
    // 3. Builder Interface (Інтерфейс Будівельника)
    // -----------------------------------------------------------

    public interface IScenarioBuilder
    {
        IScenarioBuilder SetMap(string terrain, string weather);
        IScenarioBuilder AddTroops(string name, int count);
        IScenarioBuilder AddResources(string type, double amount);
        StrategyScenario Build();
        void Reset();
    }

    // -----------------------------------------------------------
    // 4. Concrete Builder (Конкретний Будівельник)
    // -----------------------------------------------------------

    public class MilitaryScenarioBuilder : IScenarioBuilder
    {
        private StrategyScenario _scenario;

        public MilitaryScenarioBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            _scenario = new StrategyScenario();
        }

        public IScenarioBuilder SetMap(string terrain, string weather)
        {
            _scenario.ScenarioMap = new Map(terrain, weather);
            return this; // Повертаємо this для ланцюгового виклику (Fluent Interface)
        }

        public IScenarioBuilder AddTroops(string name, int count)
        {
            _scenario.AddTroop(new Troop(name, count));
            return this;
        }

        public IScenarioBuilder AddResources(string type, double amount)
        {
            _scenario.AddResource(new Resource(type, amount));
            return this;
        }

        public StrategyScenario Build()
        {
            StrategyScenario result = _scenario;
            Reset(); // Скидаємо будівельник для підготовки до створення наступного об'єкта
            return result;
        }
    }

    // -----------------------------------------------------------
    // 5. Director (Директор) - необов'язковий, але корисний для шаблонів
    // -----------------------------------------------------------

    public class ScenarioDirector
    {
        private readonly IScenarioBuilder _builder;

        public ScenarioDirector(IScenarioBuilder builder)
        {
            _builder = builder;
        }

        public void BuildQuickAttackScenario()
        {
            _builder.SetMap("Рівнина", "Ясно")
                    .AddTroops("Легка піхота", 100)
                    .AddTroops("Бронетехніка", 10)
                    .AddResources("Паливо", 500)
                    .AddResources("Амуніція", 1000);
        }

        public void BuildDefenseScenario()
        {
            _builder.SetMap("Гори", "Туман")
                    .AddTroops("Снайпери", 20)
                    .AddTroops("Артилерія", 5)
                    .AddResources("Медикаменти", 200)
                    .AddResources("Сухпайки", 3000);
        }
    }

    // -----------------------------------------------------------
    // 6. Client Code (Клієнт)
    // -----------------------------------------------------------

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Створення будівельника
            var builder = new MilitaryScenarioBuilder();

            // ВАРІАНТ 1: Використання Директора для стандартних сценаріїв
            var director = new ScenarioDirector(builder);

            Console.WriteLine("1. Будуємо сценарій швидкої атаки (через Директора):");
            director.BuildQuickAttackScenario();
            var attackScenario = builder.Build();
            Console.WriteLine(attackScenario.GetScenarioDescription());

            Console.WriteLine("\n------------------------------------------------\n");

            Console.WriteLine("2. Будуємо сценарій оборони (через Директора):");
            director.BuildDefenseScenario();
            var defenseScenario = builder.Build();
            Console.WriteLine(defenseScenario.GetScenarioDescription());

            Console.WriteLine("\n------------------------------------------------\n");

            // ВАРІАНТ 2: Використання Будівельника напряму (кастомний сценарій)
            Console.WriteLine("3. Будуємо кастомний морський сценарій (без Директора):");

            builder.SetMap("Океан", "Шторм")
                   .AddTroops("Авіаносець", 1)
                   .AddTroops("Винищувачі", 15)
                   .AddTroops("Морська піхота", 200)
                   .AddResources("Ядерне паливо", 100);

            var customScenario = builder.Build();
            Console.WriteLine(customScenario.GetScenarioDescription());

            Console.ReadLine();
        }
    }
}
