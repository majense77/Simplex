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
            Matrix XbMatrix = mm.MakeRHSMatrix(standardModel);
            Matrix LHSMatrix = mm.MakeLHSMatrix(standardModel);
            Matrix ObjMatrix = mm.MakeZObjMatrix(standardModel);
            PrintStandardizedModel(XbMatrix,LHSMatrix,ObjMatrix);
            Solution solution = SolveModel(standardModel, LHSMatrix, XbMatrix, ObjMatrix);
            return solution;
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

        private Solution SolveModel(StandardModel model, Matrix LHSMatrix, Matrix XbMatrix, Matrix ObjMatrix)
        {
            int[] basic = FindFirstBasic(model);
            Boolean optimized = false;
            if (model.Goal.Coefficients.Min() > 0) {
                optimized = true;
            }
            int numCoefficients = model.Constraints[0].Coefficients.Length;
            int numSVars = model.SVariables.Count;
            int numAVars = model.ArtificialVars.Count;
            int length = numCoefficients+numSVars+numAVars;
            double[] CnPrimes = new double[length];
            List<Matrix> PnPrimes = new List<Matrix>();
            while (!optimized)
            {
                Matrix basicMatrix = CreateBasicMatrix(basic, LHSMatrix);
                Matrix Cb = CreateCbMatrix(basic, ObjMatrix);
                Matrix inverse = Invert(basicMatrix);
                for (int i = 0; i < length; i++)
                {
                    if (!basic.Contains(i))
                    {
                        Matrix Pn = LHSMatrix.Column(i + 1);
                        Matrix PnPrime = inverse * Pn;
                        PnPrimes.Add(PnPrime);
                        double Cn = Double.Parse(ObjMatrix.ColumnSum(i + 1).ToString());
                        double herp = Double.Parse((Cb * PnPrime).ColumnSum(1).ToString());
                        double CnPrime = Cn - herp;
                        CnPrimes[i] = CnPrime;
                    }
                    else 
                    {
                        CnPrimes[i] = 0;
                    }
                }

                Matrix XbPrime = inverse * XbMatrix;
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

               double[] ratios = FindRatios(XbPrime, PnPrimes, model, entering);

               double min = ratios[0];
               int exitingColumn = 0;  //Change for unbounded stuff, eventually
                for (int i = 0; i < ratios.Length; i++)
                {
                    if (ratios[i] < min && ratios[i] > 0)
                    {
                        min = ratios[i];
                        exitingColumn = i;
                    }
                }

                //int exitingIndex = Array.IndexOf(basic, exitingColumn);
                basic[exitingColumn] = entering;
            }
            return null;
        }

        private Solution CreateSolution(int[] basic, int numCoefficients, Matrix XbPrime, StandardModel model)
        {
            Solution solution = new Solution();
            double[] decisions = new double[numCoefficients];
            for (int i = 0; i < basic.Length; i++)
            {
                if (basic[i] < numCoefficients)
                {
                    decisions[basic[i]] = Double.Parse(XbPrime.RowSum(i + 1).ToString());
                }
            }
            solution.Decisions = decisions;
            double optimalVal = 0;
            for (int i = 0; i < numCoefficients; i++)
            {
                optimalVal += decisions[i] * model.Goal.Coefficients[i] * -1;
            }
            solution.OptimalValue = optimalVal;
            solution.AlternateSolutionsExist = false;
            solution.Quality = SolutionQuality.Optimal;
            return solution;
        }

        private double[] FindRatios(Matrix XbPrime, List<Matrix> PnPrimes, StandardModel model, int entering) 
        {
            double[] ratios = new double[model.Constraints.Count];
            for (int i = 0; i < model.Constraints.Count; i++)
            {
                double xbValue = Double.Parse(XbPrime.RowSum(i + 1).ToString());
                double pnValue = Double.Parse(PnPrimes[entering].RowSum(i + 1).ToString());
                ratios[i] = xbValue / pnValue;
            }
            return ratios;
        }

        private int[] FindFirstBasic(StandardModel model)
        {
            int numCoefficients = model.Constraints[0].Coefficients.Length;
            int numSVars = model.SVariables.Count;
            int numAVars = model.ArtificialVars.Count;
            int[] basic = new int[numSVars];
            //bool[] basic = new bool[numCoefficients + numSVars + numAVars];
            for (int i = 0; i < numSVars; i++)
            {
                if (model.ArtificialVars.ContainsKey(i))
                {
                    //basic[numCoefficients + numSVars + i] = true;
                    basic[i] = numCoefficients + numSVars + i;
                }
                else
                {
                    //basic[numCoefficients + i] = true;
                    basic[i] = numCoefficients + i;
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
            return matrix.Inverse();
        }
    }

}
