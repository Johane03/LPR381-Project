﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace LPR381_Project
{
    internal class Knapsack
    {
        private string problemType;
        private int[] objFunction;
        private int[,] constraintsCoefficients;
        private int[] rhsConstraints;
        private string[] signRestrictions;
        private List<(int VariableIndex, int ConstraintIndex, int OriginalCoefficient, double Fraction, double Value, int Rank)> rankingsData;

        public Knapsack(string problemType, int[] objFunction, int[,] constraintsCoefficients, int[] rhsConstraints, string[] signRestrictions)
        {
            this.problemType = problemType;
            this.objFunction = objFunction;
            this.constraintsCoefficients = constraintsCoefficients;
            this.rhsConstraints = rhsConstraints;
            this.signRestrictions = signRestrictions;
            this.rankingsData = new List<(int, int, int, double, double, int)>();
        }

        public string ProblemType { get => problemType; set => problemType = value; }
        public int[] ObjFunction { get => objFunction; set => objFunction = value; }
        public int[,] ConstraintsCoefficients { get => constraintsCoefficients; set => constraintsCoefficients = value; }
        public int[] RhsConstraints { get => rhsConstraints; set => rhsConstraints = value; }
        public string[] SignRestrictions { get => signRestrictions; set => signRestrictions = value; }

        public string RankingTable()
        {
            rankingsData.Clear(); // Clear previous data
            List<(int Numerator, int Denominator, double Value, int Rank, int Row, int Column)> rankings = new List<(int Numerator, int Denominator, double Value, int Rank, int Row, int Column)>();
            int numConstraints = constraintsCoefficients.GetLength(0);
            int numVariables = constraintsCoefficients.GetLength(1);

            // Calculate fractions and values
            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < numVariables; j++)
                {
                    int numerator = objFunction[j];
                    int denominator = constraintsCoefficients[i, j];
                    double value = (double)numerator / denominator; // Value based on fraction only
                    rankings.Add((numerator, denominator, value, 0, i, j));
                }
            }

            // Sort by value (fraction) and assign ranks
            rankings = rankings.OrderByDescending(x => x.Value).ToList();
            for (int i = 0; i < rankings.Count; i++)
            {
                var item = rankings[i];
                rankings[i] = (item.Numerator, item.Denominator, item.Value, i + 1, item.Row, item.Column);
                rankingsData.Add((item.Column, item.Row, constraintsCoefficients[item.Row, item.Column], (double)item.Numerator / item.Denominator, item.Value, i + 1));
            }

            // Create string representation of the ranking table
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Ranking Table:");
            sb.AppendLine("Variable\tFraction\tValue\tRank");
            foreach (var item in rankings)
            {
                string fraction = $"{item.Numerator}/{item.Denominator}";
                sb.AppendLine($"x{item.Column + 1}\t{fraction}\t{item.Value:F2}\t{item.Rank}");
            }

            return sb.ToString();
        }

        public string RankingEvaluation()
        {
            // Initialize StringBuilder for output
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Sub-Problem 0:");
            sb.AppendLine("Variable\tSmallerThanRHS\t0/1\tNew RHS");

            // Create a copy of rhsConstraints to modify during evaluation
            int[] modifiedRhsConstraints = (int[])rhsConstraints.Clone();
            bool cutFlag = false;

            // Iterate through the stored ranking data
            foreach (var data in rankingsData)
            {
                try
                {
                    int variableIndex = data.VariableIndex;
                    int constraintIndex = data.ConstraintIndex;
                    int originalCoefficient = data.OriginalCoefficient;

                    if (cutFlag)
                    {
                        sb.AppendLine($"x{variableIndex + 1}\tNo\t0\t0");
                        continue;
                    }

                    // Perform the check
                    bool isSmaller = originalCoefficient < modifiedRhsConstraints[constraintIndex];
                    int flag = isSmaller ? 1 : 0;

                    if (isSmaller)
                    {
                        modifiedRhsConstraints[constraintIndex] -= originalCoefficient;
                    }
                    else
                    {
                        cutFlag = true;
                        sb.AppendLine($"x{variableIndex + 1}\tNo\t0\t{modifiedRhsConstraints[constraintIndex]}/{originalCoefficient} (cut)");
                        continue;
                    }

                    // Append results to the output
                    sb.AppendLine($"x{variableIndex + 1}\t{(isSmaller ? "Yes" : "No")}\t{flag}\t{modifiedRhsConstraints[constraintIndex]}");
                }
                catch (Exception ex)
                {
                    // Handle any unexpected errors
                    sb.AppendLine($"Error processing data. Exception: {ex.Message}");
                }
            }

            return sb.ToString();
        }

        public string Iterations()
        {
            return "in progress";
        }
    }
}
