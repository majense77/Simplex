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
            Matrix RHSMatrix = mm.MakeRHSMatrix(standardModel);
            Matrix XbMatrix = mm.MakeLHSMatrix(standardModel);
            Matrix ObjMatrix = mm.MakeZObjMatrix(standardModel);
            PrintStandardizedModel(RHSMatrix,XbMatrix,ObjMatrix);
            SolveModel(StandardModel model, Matrix RHSMatrix, Matrix XbMatrix, Matrix ObjMatrix);
            return null;
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

        private void SolveModel(StandardModel model, Matrix RHSMatrix, Matrix XbMatrix, Matrix ObjMatrix)
        {
            bool[] basicMatrix = findFirstBasic(model);
        }

        private bool[] findFirstBasic(StandardModel model)
        {
            int numCoefficients = model.Constraints[0].Coefficients.Length;
            int numSVars = model.SVariables.Count;
            int numAVars = model.ArtificialVars.Count;
            bool[] basic = new bool[numCoefficients + numSVars + numAVars];
            for (int i = 0; i < numSVars; i++)
            {
                if (model.ArtificialVars.ContainsKey(i))
                {
                    basic[numCoefficients + numSVars + i] = true;
                }
                else
                {
                    basic[numCoefficients + i] = true;
                }
            }
            return basic;
        }

    }
}
