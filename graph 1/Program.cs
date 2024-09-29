using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Graph
{
    private Dictionary<string, List<(string, int)>> adjacencyList;
    private bool isDirected;
    private bool isWeighted;

    // Конструктор по умолчанию: создаёт пустой граф
    public Graph(bool isDirected = false, bool isWeighted = false)
    {
        this.adjacencyList = new Dictionary<string, List<(string, int)>>();
        this.isDirected = isDirected;
        this.isWeighted = isWeighted;
    }

    // Конструктор, загружающий граф из файла
    public Graph(string filename, bool isDirected = false, bool isWeighted = false) : this(isDirected, isWeighted)
    {
        LoadFromFile(filename);
    }

    // Конструктор-копия
    public Graph(Graph other)
    {
        this.isDirected = other.isDirected;
        this.isWeighted = other.isWeighted;
        this.adjacencyList = new Dictionary<string, List<(string, int)>>(other.adjacencyList);
    }

    // Метод добавления вершины
    public void AddVertex(string vertex)
    {
        if (!adjacencyList.ContainsKey(vertex))
        {
            adjacencyList[vertex] = new List<(string, int)>();
        }
        else
        {
            Console.WriteLine($"Вершина {vertex} уже существует.");
        }
    }

    // Метод добавления ребра (дуги)
    public void AddEdge(string from, string to, int weight = 1)
    {
        if (!adjacencyList.ContainsKey(from)) AddVertex(from);
        if (!adjacencyList.ContainsKey(to)) AddVertex(to);

        adjacencyList[from].Add((to, weight));

        if (!isDirected)
        {
            adjacencyList[to].Add((from, weight));
        }
    }

    // Метод удаления вершины
    public void RemoveVertex(string vertex)
    {
        if (adjacencyList.ContainsKey(vertex))
        {
            adjacencyList.Remove(vertex);

            foreach (var neighbors in adjacencyList)
            {
                neighbors.Value.RemoveAll(edge => edge.Item1 == vertex);
            }
        }
        else
        {
            Console.WriteLine($"Вершина {vertex} не существует.");
        }
    }

    // Метод удаления ребра (дуги)
    public void RemoveEdge(string from, string to)
    {
        if (adjacencyList.ContainsKey(from))
        {
            adjacencyList[from].RemoveAll(edge => edge.Item1 == to);
        }

        if (!isDirected && adjacencyList.ContainsKey(to))
        {
            adjacencyList[to].RemoveAll(edge => edge.Item1 == from);
        }
    }

    // Вывод списка смежности в файл
    public void SaveToFile(string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            foreach (var vertex in adjacencyList)
            {
                writer.Write($"{vertex.Key}: ");
                foreach (var edge in vertex.Value)
                {
                    writer.Write($"({edge.Item1}, {edge.Item2}) ");
                }
                writer.WriteLine();
            }
        }
    }

   
    public void LoadFromFile(string filename)
    {
        // Очищаем текущий список смежности
        adjacencyList.Clear();

        // Списки для хранения вершин и рёбер (дуг)
        var vertices = new HashSet<string>();  // Используем HashSet, чтобы избежать дубликатов вершин
        var edges = new HashSet<(string from, string to, int?)>(); // Используем HashSet для рёбер, чтобы избежать дубликатов

        foreach (var line in File.ReadLines(filename))
        {
            var parts = line.Split(':');
            var vertex = parts[0].Trim();

            // Добавляем вершину в список, если её там ещё нет
            vertices.Add(vertex);

            if (parts.Length > 1)
            {
                // Разбиваем оставшуюся часть строки на рёбра или дуги
                var edgesData = parts[1].Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < edgesData.Length; i++)
                {
                    var edgeParts = edgesData[i].Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (edgeParts.Length == 0) continue;

                    var neighbor = edgeParts[0].Trim();
                    int? weight = null;  // Изначально вес равен null для невзвешенного графа

                    // Если граф взвешенный, пытаемся прочитать вес
                    if (isWeighted && edgeParts.Length > 1 && int.TryParse(edgeParts[1], out int parsedWeight))
                    {
                        weight = parsedWeight;  // Присваиваем вес
                    }

                    // Добавляем соседнюю вершину в список, если её там ещё нет
                    vertices.Add(neighbor);

                    // Для неориентированного графа добавляем ребро только один раз
                    if (isDirected || !edges.Contains((neighbor, vertex, weight)))
                    {
                        edges.Add((vertex, neighbor, weight));  // Добавляем ребро или дугу
                    }
                }
            }
        }

        // Добавляем все вершины в граф
        foreach (var v in vertices)
        {
            AddVertex(v);
        }

        // Добавляем все рёбра или дуги в граф
        foreach (var (from, to, weight) in edges)
        {
            if (isWeighted)
            {
                AddEdge(from, to, weight ?? 1);  // Добавляем ребро с весом (если вес не указан, используем 1)
            }
            else
            {
                AddEdge(from, to);  // Добавляем ребро без веса
            }
        }
    }

   

    // Вывод списка смежности на консоль
    public void PrintAdjacencyList()
    {
        foreach (var vertex in adjacencyList)
        {
            Console.Write(vertex.Key + ": ");
            var edges = vertex.Value;
            foreach (var edge in edges)
            {
                if (isWeighted)
                {
                    // Если граф взвешенный, выводим вес
                    Console.Write($"({edge.Item1}, {edge.Item2}) ");
                }
                else
                {
                    // Если граф невзвешенный, выводим только вершину
                    Console.Write($"{edge.Item1} ");
                }
            }
            Console.WriteLine();
        }
    }


    // Метод для генерации списка рёбер на основе списка смежности
    public List<(string, string, int)> GetEdges()
    {
        var edges = new List<(string, string, int)>();

        foreach (var vertex in adjacencyList)
        {
            foreach (var edge in vertex.Value)
            {
                if (isDirected || !edges.Any(e => (e.Item1 == edge.Item1 && e.Item2 == vertex.Key)))
                {
                    edges.Add((vertex.Key, edge.Item1, edge.Item2));
                }
            }
        }

        return edges;
    }

    // Минималистичный консольный интерфейс пользователя
    public static void UserInterface()
    {
        bool isWeighted = false;
        bool isDirected = false;
        Graph graph = null;

        // Вопрос о загрузке графа или вводе вручную
        Console.WriteLine("Хотите загрузить граф из файла? (y/n):");
        bool loadFromFile = Console.ReadLine().ToLower() == "y";

        // Общие вопросы для обоих вариантов
        Console.WriteLine("Граф взвешенный? (y/n):");
        isWeighted = Console.ReadLine().ToLower() == "y";

        Console.WriteLine("Граф ориентированный? (y/n):");
        isDirected = Console.ReadLine().ToLower() == "y";

        if (loadFromFile)
        {
            // Загрузка графа из файла
            Console.WriteLine("Введите имя файла:");
            string filename = Console.ReadLine();

            try
            {
                graph = new Graph(filename, isDirected, isWeighted);
                Console.WriteLine("Граф успешно загружен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке графа: {ex.Message}");
                return; // Выход, если ошибка при загрузке
            }
        }
        else
        {
            // Ввод графа вручную
            graph = new Graph(isDirected, isWeighted);
        }

        string command;

        Console.WriteLine("Введите команду (help для списка команд):");

        while ((command = Console.ReadLine()) != "exit")
        {
            string[] parts = command.Split(' ');

            switch (parts[0])
            {
                case "help":
                    Console.WriteLine("Доступные команды:");
                    Console.WriteLine("addvertex: добавить вершину [имя вершины]");
                    if (isDirected == false && isWeighted == true)
                        Console.WriteLine("addedge: добавить ребро [из вершины] [в вершину] [вес]");
                    else if (isDirected == false && isWeighted == false)
                        Console.WriteLine("addedge: добавить ребро [из вершины] [в вершину]");
                    else if (isDirected == true && isWeighted == false)
                        Console.WriteLine("addedge: добавить дугу [из вершины] [в вершину]");
                    else 
                        Console.WriteLine("addedge: добавить дугу [из вершины] [в вершину] [вес]");
                    Console.WriteLine("removevertex: удалить вершину [имя вершины]");
                    if (isDirected == false )
                        Console.WriteLine("removeedge: удалить ребро [из вершины] [в вершину]");
                    else
                        Console.WriteLine("removeedge: удалить дугу [из вершины] [в вершину]");
                    Console.WriteLine("print - вывести список смежности");
                    Console.WriteLine("save [имя файла] - сохранить граф в файл");
                    Console.WriteLine("exit - выход");
                    break;

                case "addvertex":
                    graph.AddVertex(parts[1]);
                    break;

                case "addedge":
                    if (isWeighted)
                    {
                        if (parts.Length == 4)
                        {
                            int weight = int.Parse(parts[3]);
                            graph.AddEdge(parts[1], parts[2], weight);
                        }
                        else
                        {
                            Console.WriteLine("Для взвешенного графа нужно указать вес ребра.");
                        }
                    }
                    else
                    {
                        graph.AddEdge(parts[1], parts[2]);
                    }
                    break;

                case "removevertex":
                    graph.RemoveVertex(parts[1]);
                    break;

                case "removeedge":
                    graph.RemoveEdge(parts[1], parts[2]);
                    break;

                case "print":
                    graph.PrintAdjacencyList();
                    break;

                case "save":
                    if (parts.Length == 2)
                    {
                        graph.SaveToFile(parts[1]);
                        Console.WriteLine($"Граф сохранён в файл {parts[1]}.");
                    }
                    else
                    {
                        Console.WriteLine("Введите имя файла для сохранения.");
                    }
                    break;

                default:
                    Console.WriteLine("Неизвестная команда. Введите help для списка команд.");
                    break;
            }
        }
    }

}

// Пример использования консольного интерфейса
public class Program
{
    public static void Main()
    {
        Graph.UserInterface();
    }
}
