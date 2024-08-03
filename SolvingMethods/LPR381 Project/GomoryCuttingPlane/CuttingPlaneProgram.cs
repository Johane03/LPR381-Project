using System;
using System.Collections.Generic;
using System.IO;
using Google.OrTools.LinearSolver;

public class Program
{
    public static void Main(string[] args)
    {
        ModelInput input = null;

        while (true)
        {
            Console.WriteLine("Gomory Cutting Plane Algorithm");
            Console.WriteLine("1. Load Model from File");
            Console.WriteLine("2. Solve Model");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Enter file path: ");
                var filePath = Console.ReadLine();
                try
                {
                    input = new ModelInput(filePath);
                    Console.WriteLine("Model loaded successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading model: " + ex.Message);
                }
            }
            else if (choice == "2")
            {
                if (input == null)
                {
                    Console.WriteLine("Please load a model first.");
                    continue;
                }

                var solver = new SolverWrapper();
                solver.Solve(input);
                Console.WriteLine("Model solved. Results written to output.txt.");
            }
            else if (choice == "3")
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid choice. Try again.");
            }
        }
    }
}

public class ModelInput
{
    public string OptimizationType { get; set; }
    public List<double> ObjectiveCoefficients { get; set; }
    public List<Constraint> Constraints { get; set; }
    public List<string> SignRestrictions { get; set; }

    public ModelInput(string filePath)
    {
        ObjectiveCoefficients = new List<double>();
        Constraints = new List<Constraint>();
        SignRestrictions = new List<string>();
        ParseInputFile(filePath);
    }

    private void ParseInputFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var firstLine = lines[0].Split(' ');

        OptimizationType = firstLine[0];
        for (int i = 1; i < firstLine.Length; i += 2)
        {
            string sign = firstLine[i];
            string coefficientStr = firstLine[i + 1];
            double coefficient;
            if (sign == "+")
            {
                coefficient = double.Parse(coefficientStr);
            }
            else if (sign == "-")
            {
                coefficient = -double.Parse(coefficientStr);
            }
            else
            {
                throw new FormatException("Invalid sign in input file.");
            }
            ObjectiveCoefficients.Add(coefficient);
        }

        for (int i = 1; i < lines.Length - 1; i++)
        {
            var constraintParts = lines[i].Split(' ');
            var constraint = new Constraint
            {
                Coefficients = new List<double>(),
                Relation = constraintParts[constraintParts.Length - 2],
                RHS = double.Parse(constraintParts[constraintParts.Length - 1])
            };

            for (int j = 0; j < constraintParts.Length - 2; j += 2)
            {
                string sign = constraintParts[j];
                string coefficientStr = constraintParts[j + 1];
                double coefficient;
                if (sign == "+")
                {
                    coefficient = double.Parse(coefficientStr);
                }
                else if (sign == "-")
                {
                    coefficient = -double.Parse(coefficientStr);
                }
                else
                {
                    throw new FormatException("Invalid sign in input file.");
                }
                constraint.Coefficients.Add(coefficient);
            }

            Constraints.Add(constraint);
        }

        SignRestrictions.AddRange(lines[^1].Split(' '));
    }
}

public class Constraint
{
    public List<double> Coefficients { get; set; }
    public string Relation { get; set; }
    public double RHS { get; set; }
}

public class SolverWrapper
{
    public void Solve(ModelInput input)
    {
        var solver = Google.OrTools.LinearSolver.Solver.CreateSolver("GLOP");
        if (solver == null) throw new Exception("Could not create solver.");

        var variables = new List<Variable>();
        for (int i = 0; i < input.ObjectiveCoefficients.Count; i++)
        {
            var variable = solver.MakeNumVar(0.0, double.PositiveInfinity, $"x{i}");
            variables.Add(variable);
        }

        // Set up the objective function
        var objective = solver.Objective();
        for (int i = 0; i < input.ObjectiveCoefficients.Count; i++)
        {
            objective.SetCoefficient(variables[i], input.ObjectiveCoefficients[i]);
        }
        if (input.OptimizationType == "max")
        {
            objective.SetMaximization();
        }
        else
        {
            objective.SetMinimization();
        }

        // Add constraints
        foreach (var constraint in input.Constraints)
        {
            var ct = solver.MakeConstraint(constraint.RHS, constraint.RHS);
            for (int i = 0; i < constraint.Coefficients.Count; i++)
            {
                ct.SetCoefficient(variables[i], constraint.Coefficients[i]);
            }
        }

        // Solve the problem
        solver.Solve();

        // Write the solution to a file
        using (var writer = new StreamWriter("output.txt"))
        {
            writer.WriteLine($"Objective Value: {solver.Objective().Value()}");
            for (int i = 0; i < variables.Count; i++)
            {
                writer.WriteLine($"{variables[i].Name()} = {variables[i].SolutionValue()}");
            }
        }
    }
}
