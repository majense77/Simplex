using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;
using RaikesSimplexService.Contracts;
using RaikesSimplexService.DataModel;

namespace RaikesSimplexService.Joel
{

    public class Solver : ISolver
    {

        public Solution Solve(Model model)
        {
            PrintInput(model);
            StandardModel standardModel = StandardizeModel(model);
            MatrixMaker mm = new MatrixMaker();
            Matrix XbMatrix, LHSMatrix, ObjMatrix;
            bool twoPhase = TwoPhase(standardModel);
            if (!twoPhase)
            {
                XbMatrix = mm.MakeRHSMatrix(standardModel, false);
                LHSMatrix = mm.MakeLHSMatrix(standardModel, false);
                ObjMatrix = mm.MakeZObjMatrix(standardModel);
                PrintStandardizedModel(XbMatrix, LHSMatrix, ObjMatrix);
            }
            else
            {
                XbMatrix = mm.MakeRHSMatrix(standardModel, true);
                LHSMatrix = mm.MakeLHSMatrix(standardModel, true);
                ObjMatrix = mm.MakeWObjMatrix(standardModel, LHSMatrix);
                PrintStandardizedModel(XbMatrix, LHSMatrix, ObjMatrix);
            }
            return SolveModel(standardModel, LHSMatrix, XbMatrix, ObjMatrix, twoPhase);
        }

        private bool TwoPhase(StandardModel model) 
        {
            bool twoPhase = false;
            if (model.ArtificialVars.ToArray().Length > 0) 
            {
                twoPhase = true;
            }
            return twoPhase;
        }

        private StandardModel StandardizeModel(Model model) {
            StandardModel newModel = new StandardModel();
            newModel.Constraints = model.Constraints;
            newModel.Goal = model.Goal;
            newModel.GoalKind = model.GoalKind;
            newModel.SVariables = new Dictionary<int, double>();
            newModel.ArtificialVars = new Dictionary<int, double>();
            int i = 0;
            foreach(LinearConstraint constraint in model.Constraints) {
                if (constraint.Relationship.Equals(Relationship.LessThanOrEquals)) {
                    newModel.SVariables.Add(i,1);
                    constraint.Relationship = Relationship.Equals;
                }
                else if (constraint.Relationship.Equals(Relationship.GreaterThanOrEquals)) {
                    newModel.SVariables.Add(i,-1);
                    newModel.ArtificialVars.Add(i, 1);
                    constraint.Relationship = Relationship.Equals;
                }
                i++;
            }
            if (newModel.GoalKind.Equals(GoalKind.Minimize))
            {
                for (i = 0; i < newModel.Goal.Coefficients.Length; i++)
                {
                    newModel.Goal.Coefficients[i] = newModel.Goal.Coefficients[i] * -1;
                }
                newModel.GoalKind = GoalKind.Maximize;
            }
            return newModel;
        }

        public void PrintInput(Model model)
        {
            System.Diagnostics.Debug.WriteLine("Original Data:");

            //Objective Function
            int goalCounter = 1;
            System.Diagnostics.Debug.WriteLine("\tObjective Function:");
            System.Diagnostics.Debug.Write("\t     " + model.GoalKind + ": Z = ");
            for (int i = 0; i < model.Goal.Coefficients.Length; i++)
            {
                System.Diagnostics.Debug.Write(model.Goal.Coefficients[i] + "x" + (i + 1));
                if (model.Goal.ConstantTerm == 0 && (i + 1 == model.Goal.Coefficients.Length))
                { }
                else if ((i + 1) != model.Goal.Coefficients.Length + 1)
                {
                    System.Diagnostics.Debug.Write(" + ");
                }
                goalCounter++;
            }

            if (model.Goal.ConstantTerm != 0)
                System.Diagnostics.Debug.WriteLine(model.Goal.ConstantTerm);

            //Linear Constraints
            int counter;
            System.Diagnostics.Debug.WriteLine("\n\tConstraints:");
            foreach (LinearConstraint LC in model.Constraints)
            {
                counter = 1;
                System.Diagnostics.Debug.Write("\t     ");
                foreach (Double coeff in LC.Coefficients)
                {
                    System.Diagnostics.Debug.Write(coeff + "x" + counter);
                    if (counter != goalCounter - 1)
                    {
                        System.Diagnostics.Debug.Write(" + ");
                    }
                    else
                    {
                        String relation = "";
                        if (LC.Relationship == Relationship.Equals)
                        {
                            relation = "=";
                        }
                        else if (LC.Relationship == Relationship.GreaterThanOrEquals)
                        {
                            relation = ">=";
                        }
                        else
                        {
                            relation = "<=";
                        }

                        System.Diagnostics.Debug.WriteLine(" " + relation + " " + LC.Value);
                    }
                    counter++;
                }
            }
        }

        private void PrintStandardizedModel(Matrix Xb, Matrix LHS, Matrix Z)
        {
            System.Diagnostics.Debug.Write("\nStandard Matrx Form: ");
            System.Diagnostics.Debug.WriteLine("\n\tLHS:");
            System.Diagnostics.Debug.Write("\t  [\t" + LHS.ToString());
            System.Diagnostics.Debug.WriteLine("\n\tXb:");
            System.Diagnostics.Debug.Write("\t  [\t" + Xb.ToString());
            System.Diagnostics.Debug.WriteLine("\n\tObjective Row:");
            System.Diagnostics.Debug.Write("\t  [\t" + Z.ToString());
        }

        private Solution SolveModel(StandardModel model, Matrix LHSMatrix, Matrix XbMatrix, Matrix ObjMatrix, bool twoPhase)
        {
            int count = 0;
            int[] basic = FindFirstBasic(model, twoPhase);
            Boolean optimized = true;
            int numCoefficients = model.Constraints[0].Coefficients.Length;
            int numSVars = model.SVariables.Count;
            int numAVars = model.ArtificialVars.Count;
            int length = numCoefficients+numSVars+numAVars;
            double objMin = 0;
            if (twoPhase)
            {
                length++;
            }
            for (int i = 0; i < length; i++)
            {
                if (ObjMatrix[1, i + 1].Re < objMin)
                {
                    objMin = ObjMatrix[1, i + 1].Re;
                }
            }
            if (objMin < 0)
            {
                optimized = false;
            }
            double[] CnPrimes = new double[length];
            Dictionary<int, Matrix> PnPrimes = new Dictionary<int, Matrix>();
            while (!optimized && count < 50000)
            {
                Matrix basicMatrix = CreateBasicMatrix(basic, LHSMatrix);
                Matrix Cb = CreateCbMatrix(basic, ObjMatrix);
                Matrix inverse = RoundMatrix(Invert(basicMatrix), basic.Length, basic.Length);
                PnPrimes.Clear();
                for (int i = 0; i < length; i++)
                {
                    if (!basic.Contains(i))
                    {
                        Matrix Pn = LHSMatrix.Column(i + 1);
                        Matrix PnPrime = RoundMatrix(inverse * Pn, basic.Length, 1);
                        PnPrimes.Add(i, PnPrime);
                        double Cn = ObjMatrix[1, i+1].Re;
                        double herp = Math.Round(((Cb * PnPrime)[1, 1].Re), 5);
                        double CnPrime = Cn - herp;
                        CnPrimes[i] = CnPrime;
                    }
                    else 
                    {
                        CnPrimes[i] = 0;
                    }
                }

                Matrix XbPrime = RoundMatrix(inverse * XbMatrix, basic.Length, 1);
                //infeasible - RHS constraint value is negative
                int rows = model.Constraints.Count;
                if (twoPhase)
                {
                    rows++;
                }
                if (!twoPhase)
                {
                    double XbPrimeMin = FindMatrixMin(XbPrime, rows, 1);
                    if (XbPrimeMin < 0)
                    {
                        return CreateSolution(SolutionQuality.Infeasible);
                    }
                }
                int entering = 0;
                if (CnPrimes.Min() < 0)
                {
                    entering = Array.IndexOf(CnPrimes, CnPrimes.Min());
                }
                else 
                {
                    Solution solution = new Solution();
                    if (model.ArtificialVars.Count > 0)
                    {
                        //checking if artificial variables are still basic
                        if (basic.Max() > numCoefficients + numSVars)
                        {
                            return CreateSolution(SolutionQuality.Infeasible);
                        }
                        for (int i = 0; i < numAVars; i++)
                        {
                            LHSMatrix.DeleteColumn(numCoefficients + numSVars + 2);
                        }
                        model.ArtificialVars.Clear();
                        ObjMatrix = LHSMatrix.ExtractRow(model.Constraints.Count + 1);
                        Matrix temp = new Matrix();
                        Matrix temp2;
                        for (int i = 1; i < numCoefficients + numSVars + 1; i++)
                        {
                            temp2 = new Matrix(ObjMatrix[i+1, 1].Re);
                            temp.InsertColumn(temp2, i);
                        }
                        ObjMatrix = temp;
                        LHSMatrix.DeleteColumn(1);
                        XbMatrix.DeleteRow(model.Constraints.Count + 1);
                        PrintStandardizedModel(XbMatrix, LHSMatrix, ObjMatrix);
                        int[] newBasic = new int[basic.Length-1];
                        for (int i = 0; i < basic.Length - 1; i++)
                        {
                            newBasic[i] = basic[i + 1] - 1;
                        }
                        solution = SolveModel(model, LHSMatrix, XbMatrix, ObjMatrix, newBasic, false, count);
                        return solution;
                    }
                    optimized = true;
                    return CreateSolution(basic, numCoefficients, XbPrime, model);
                }

               double[] ratios = FindRatios(XbPrime, PnPrimes, model, entering, twoPhase);

               double min = ratios[0];
               int exitingColumn = 0;
                for (int i = 0; i < ratios.Length; i++)
                {
                    if ((min <= 0 && ratios[i] > 0 ) || (ratios[i] < min && ratios[i] > 0))
                    {
                        min = ratios[i];
                        exitingColumn = i;
                    }
                    if (twoPhase) {
                    if (min == ratios[i])
                        {
                            if (!(basic[exitingColumn] > numCoefficients + numSVars) && (basic[i] > numCoefficients + numSVars))
                            {
                                min = ratios[i];
                                exitingColumn = i;
                            }
                        }
                    }

                }
                //unbounded
                if (min <= 0)
                {
                    return CreateSolution(SolutionQuality.Unbounded);
                }
                //int exitingIndex = Array.IndexOf(basic, exitingColumn);
                basic[exitingColumn] = entering;
                count++;
            }
            if (count >= 50000)
            {
                return CreateSolution(SolutionQuality.TimedOut);
            }
            return null;
        }

        /**
         * For the second part of a two-phase revised
         */
        private Solution SolveModel(StandardModel model, Matrix LHSMatrix, Matrix XbMatrix, Matrix ObjMatrix, int[] basic, bool twoPhase, int count)
        {
            bool optimized = true;
            int numCoefficients = model.Constraints[0].Coefficients.Length;
            int numSVars = model.SVariables.Count;
            int length = numCoefficients + numSVars;
            double objMin = 0;
            if (twoPhase)
            {
                length++;
            }
            for (int i = 0; i < length; i++)
            {
                if (ObjMatrix[1, i + 1].Re < objMin)
                {
                    objMin = ObjMatrix[1, i + 1].Re;
                }
            }
            if (objMin < 0)
            {
                optimized = false;
            }
            double[] CnPrimes = new double[length];
            Dictionary<int, Matrix> PnPrimes = new Dictionary<int, Matrix>();
            while (!optimized && count < 50000)
            {
                Matrix basicMatrix = CreateBasicMatrix(basic, LHSMatrix);
                Matrix Cb = CreateCbMatrix(basic, ObjMatrix);
                Matrix inverse = RoundMatrix(Invert(basicMatrix), basic.Length, basic.Length);
                PnPrimes.Clear();
                for (int i = 0; i < length; i++)
                {
                    if (!basic.Contains(i))
                    {
                        Matrix Pn = LHSMatrix.Column(i + 1);
                        Matrix PnPrime = RoundMatrix(inverse * Pn, basic.Length, 1);
                        PnPrimes.Add(i, PnPrime);
                        double Cn = ObjMatrix[1, i + 1].Re;
                        double herp = Math.Round(((Cb * PnPrime)[1, 1].Re), 5);
                        double CnPrime = Cn - herp;
                        CnPrimes[i] = CnPrime;
                    }
                    else
                    {
                        CnPrimes[i] = 0;
                    }
                }

                Matrix XbPrime = RoundMatrix(inverse * XbMatrix, basic.Length, 1);
                int rows = model.Constraints.Count;
                if (twoPhase) {
                    rows++;
                }
                double XbPrimeMin = FindMatrixMin(XbPrime, rows, 1);
                //infeasible - RHS constraint value is negative
                if (XbPrimeMin < 0)
                {
                    return CreateSolution(SolutionQuality.Infeasible);
                }
                int entering = 0;
                if (CnPrimes.Min() < 0)
                {
                    entering = Array.IndexOf(CnPrimes, CnPrimes.Min());
                }
                else
                {
                    optimized = true;
                    return CreateSolution(basic, numCoefficients, XbPrime, model);
                }

                double[] ratios = FindRatios(XbPrime, PnPrimes, model, entering, twoPhase);

                double min = ratios[0];
                int exitingColumn = 0;
                for (int i = 0; i < ratios.Length; i++)
                {
                    if ((min <= 0 && ratios[i] > 0) || (ratios[i] < min && ratios[i] > 0))
                    {
                        min = ratios[i];
                        exitingColumn = i;
                    }
                }
                //unbounded
                if (min <= 0)
                {
                    return CreateSolution(SolutionQuality.Unbounded);
                }
                //int exitingIndex = Array.IndexOf(basic, exitingColumn);
                basic[exitingColumn] = entering;
                count++;
            }
            //timed out
            if (count >= 50000)
            {
                return CreateSolution(SolutionQuality.TimedOut);
            }
            if (optimized)
            {
                Matrix basicMatrix = CreateBasicMatrix(basic, LHSMatrix);
                Matrix inverse = Invert(basicMatrix);
                Matrix XbPrime = RoundMatrix(inverse * XbMatrix, basic.Length, 1);
                return CreateSolution(basic, numCoefficients, XbPrime, model);
            }
            return null;
        }

        private double FindMatrixMin(Matrix matrix, int rows, int columns)
        {
            double min = matrix[1,1].Re;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (matrix[i + 1, j + 1].Re < min)
                    {
                        min = matrix[i + 1, j + 1].Re;
                    }
                   
                }
            }
            return min;
        }

        private Matrix RoundMatrix(Matrix matrix, int rows, int columns)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i + 1, j + 1].Re = Math.Round(matrix[i + 1, j + 1].Re, 5);
                }
            }
            return matrix;
        }

        private Solution CreateSolution(int[] basic, int numCoefficients, Matrix XbPrime, StandardModel model)
        {
            Solution solution = new Solution();
            double[] decisions = new double[numCoefficients];
            for (int i = 0; i < basic.Length; i++)
            {
                if (basic[i] < numCoefficients)
                {
                    decisions[basic[i]] = XbPrime[i + 1, 1].Re;
                }
            }
            solution.Decisions = decisions;
            double optimalVal = 0;
            for (int i = 0; i < numCoefficients; i++)
            {
                optimalVal += decisions[i] * model.Goal.Coefficients[i] * -1;
            }
            if (optimalVal < 0)
            {
                optimalVal = optimalVal * -1;
            }
            solution.OptimalValue = optimalVal;
            solution.AlternateSolutionsExist = false;
            solution.Quality = SolutionQuality.Optimal;
            return solution;
        }

        private Solution CreateSolution(SolutionQuality quality)
        {
            Solution solution = new Solution();
            solution.Quality = quality;
            return solution;
        }

        private double[] FindRatios(Matrix XbPrime, Dictionary<int, Matrix> PnPrimes, StandardModel model, int entering, bool twoPhase) 
        {
            int length = model.Constraints.Count;
            if (twoPhase)
            {
                length++;
            }
            double[] ratios = new double[length];
            for (int i = 0; i < length; i++)
            {
                double xbValue = XbPrime[i + 1, 1].Re;
                double pnValue = PnPrimes[entering][i+1,1].Re;
                if (pnValue != 0)
                {
                    ratios[i] = Math.Round((xbValue / pnValue), 5);
                } else {
                    ratios[i] = double.PositiveInfinity;
                }
            }
            return ratios;
        }

        private int[] FindFirstBasic(StandardModel model, bool twoPhase)
        {
            int numCoefficients = model.Constraints[0].Coefficients.Length;
            int numSVars = model.SVariables.Count;
            int numAVars = model.ArtificialVars.Count;
            int basicLength = numSVars;
            if (twoPhase)
            {
                basicLength++;
            }
            int[] basic = new int[basicLength];
            //bool[] basic = new bool[numCoefficients + numSVars + numAVars];
            int accForTwoPhase = 0;
            if (twoPhase)
            {
                basic[0] = 0;
                accForTwoPhase = 1;
            }
            int basicAVars = 0;
            for (int i = 0; i < numSVars; i++)
            {
                
                if (model.ArtificialVars.ContainsKey(i))
                {
                    //basic[numCoefficients + numSVars + i] = true;
                    basic[i + accForTwoPhase] = numCoefficients + numSVars + basicAVars + accForTwoPhase;
                    basicAVars++;
                }
                else
                {
                    //basic[numCoefficients + i] = true;
                    basic[i + accForTwoPhase] = numCoefficients + i + accForTwoPhase;
                }
            }
            return basic;
        }

        private Matrix CreateBasicMatrix(int[] basic, Matrix LHSMatrix)
        {
            Matrix basicMatrix = new Matrix();
            for (int i = 0; i < basic.Length; i++)
            {
                Matrix temp = LHSMatrix.Column(basic[i]+1);
                basicMatrix.InsertColumn(temp, i+1);
            }
            //System.Diagnostics.Debug.Write(basicMatrix.ToString());
            return basicMatrix;
        }

        private Matrix CreateCbMatrix(int[] basic, Matrix ObjMatrix)
        {
            Matrix CbMatrix = new Matrix();
            for (int i = 0; i < basic.Length; i++)
            {
                Matrix temp = ObjMatrix.Column(basic[i] + 1);
                CbMatrix.InsertColumn(temp, i + 1);
            }
            return CbMatrix;
        }

        private Matrix Invert(Matrix matrix)
        {
            return matrix.InverseLeverrier();
        }
    }

}
