using ProiectIA_NSGA2_final;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProiectIA_NSGA2_final
{
    public class Program
    {
        public static Random random = new Random();

        public static List<Car> InitializePopulation(int size)
        {

            var population = new List<Car>();
            for (int i = 0; i < size; i++)
            {
                population.Add(new Car()
                {
                    price = random.Next(1000, 10000),
                    range = random.Next(200, 800)
                });
            }
            return population;
        }
        public static void EvaluatePopulation(List<Car> population)
        {
            foreach (var car in population)
            {
                car.objectives[0] = car.price;
                car.objectives[1] = -car.range;
            }
        }
        public static bool Dominates(Car car1, Car car2)
        {
            bool isBetter = false;
            for (int i = 0; i < car1.objectives.Length; i++)
            {
                if (car1.objectives[i] > car2.objectives[i])
                {
                    return false;
                }
                if (car1.objectives[i] < car2.objectives[i])
                {
                    isBetter = true;
                }
            }
            return isBetter;
        }

        public static List<List<Car>> NonDominatedSort(List<Car> population)
        {
            var fronts = new List<List<Car>>();
            var dominations = new Dictionary<Car, int>();

            foreach (var car in population)
            {
                dominations[car] = 0;
                car.dominatedBy = new List<Car>();
                foreach (var solution in population)
                {
                    if (Dominates(car, solution))
                    {
                        car.dominatedBy.Add(solution);
                    }
                    else if (Dominates(solution, car))
                    {
                        dominations[car]++;
                    }
                }
                if (dominations[car] == 0)
                {
                    car.rank = 0;
                    if (fronts.Count == 0)
                    {
                        fronts.Add(new List<Car>());
                    }
                    fronts[0].Add(car);
                }
            }

            int i = 0;
            while (i < fronts.Count && fronts[i].Count > 0)
            {
                var nextFront = new List<Car>();
                foreach (var car in fronts[i])
                {
                    foreach (var dominated in car.dominatedBy)
                    {
                        dominations[dominated]--;
                        if (dominations[dominated] == 0)
                        {
                            dominated.rank = i + 1;
                            nextFront.Add(dominated);
                        }
                    }
                }
                if (nextFront.Count > 0)
                {
                    fronts.Add(nextFront);
                }
                i++;
            }
            return fronts;
        }


        public static void CalculateCrowdingDistance(List<Car> front)
        {
            int numObjectives = front[0].objectives.Length;
            int f_size = front.Count;

            foreach (var car in front)
            {
                car.crowdingDistance = 0;
            }
            for (int i = 0; i < numObjectives; i++)
            {
                front.Sort((a, b) => a.objectives[i].CompareTo(b.objectives[i]));

                front[0].crowdingDistance = double.PositiveInfinity;
                front[f_size - 1].crowdingDistance = double.PositiveInfinity;

                double min_val = front[0].objectives[i];
                double max_val = front[f_size - 1].objectives[i];

                if (max_val == min_val)
                {
                    continue;
                }
                for (int j = 1; j < f_size - 1; j++)
                {
                    front[j].crowdingDistance += (front[j + 1].objectives[i] - front[j - 1].objectives[i]) / (max_val - min_val);
                }
            }
        }

        public static Car ArithmeticCrossover(Car parent1, Car parent2)
        {
            double crossover_rate = 0.9;
            double coeficient = random.NextDouble();

            if (random.NextDouble() < crossover_rate)
            {
                return new Car()
                {
                    price = (coeficient * parent1.price) + ((1 - coeficient) * parent2.price),
                    range = (coeficient * parent1.range) + ((1 - coeficient) * parent2.range),

                };
            }
            return null;
        }


        public static Car Mutation(Car child)
        {
            double mutation_rate = 0.17;

            if (random.NextDouble() < mutation_rate)
            {
                var mutation_value_price = random.Next(-12, 25);
                var mutation_value_range = random.Next(-3, 12);

                child.price = child.price + mutation_value_price;
                child.range = child.range + mutation_value_range;
            }
            return child;
        }

        public static Car SelectedByRankAndDistance(List<Car> population)
        {
            Car parent = null;
            for (int i = 0; i < population.Count; i++)
            {
                var parent1 = random.Next(population.Count);
                var parent2 = random.Next(population.Count);
                if (parent1 == parent2)
                {
                    parent2 = random.Next(population.Count);
                }
                else
                {
                    if (population[parent1].rank < population[parent2].rank)
                    {
                        parent = population[parent1];
                    }
                    else if (population[parent1].rank > population[parent2].rank)
                    {
                        parent = population[parent2];
                    }
                    else
                    {
                        if (population[parent1].crowdingDistance > population[parent2].crowdingDistance)
                        {
                            parent = population[parent1];
                        }
                        else
                        {
                            parent = population[parent2];
                        }
                    }
                }
            }
            return parent;
        }

        static void Main(string[] args)
        {
            int max_generations = 100;
            int size_population = 60;

            var population = InitializePopulation(size_population);
            EvaluatePopulation(population);

            List<List<Car>> fronts = NonDominatedSort(population);
            var current_generation = 1;

            while (max_generations > 0)
            {
                Console.WriteLine($"Generatia {current_generation}");

                List<Car> selectedParents = new List<Car>();
                List<Car> children = new List<Car>();

                for (int i = 0; i < population.Count; i++)
                {
                    Car parent1 = SelectedByRankAndDistance(population);
                    Car parent2 = SelectedByRankAndDistance(population);

                    Car child = ArithmeticCrossover(parent1, parent2);
                    if (child != null)
                    {
                        child = Mutation(child);
                        children.Add(child);
                    }
                }

                EvaluatePopulation(children);

                List<Car> combinedPopulation = population.Concat(children).ToList();
                fronts = NonDominatedSort(combinedPopulation);

                Console.WriteLine("Front 0:");
                foreach (var solutie in fronts[0])
                {
                    Console.WriteLine($"solutia: Pret: {solutie.price:F0} , Autonomie: {solutie.range:F0}");
                }
                List<Car> nextGeneration = new List<Car>();

                foreach (var front in fronts)
                {
                    CalculateCrowdingDistance(front);
                    if (nextGeneration.Count + front.Count <= size_population)
                    {
                        nextGeneration.AddRange(front);
                    }
                    else
                    {
                        front.Sort((a, b) => b.crowdingDistance.CompareTo(a.crowdingDistance));
                        nextGeneration.AddRange(front.Take(size_population - nextGeneration.Count));
                        break;
                    }
                }
                population = new List<Car>(nextGeneration);

                max_generations--;
                current_generation++;
            }
        }
    }
}
