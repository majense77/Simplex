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
            //throw new NotImplementedException();

            //Print the original obj function and constraints
            //Then compute slack, surplus and artificial vars
            //Print out standard form of matrix (think LPSolve)

            PrintInput(model);
            StandardModel standardModel = StandardizeModel(model);
            Matrix RHSMatrix = MakeRHSMatrix(standardModel);
            Matrix LHSMatrix = MakeLHSMatrix(standardModel);
            Matrix ObjMatrix = MakeObjMatrix(standardModel);
            PrintStandardizedModel(RHSMatrix,LHSMatrix,ObjMatrix);
            return null;
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
                System.Diagnostics.Debug.Write(model.Goal.Coefficients[i] + "x" + (i+1));
                if (model.Goal.ConstantTerm == 0 && (i + 1 == model.Goal.Coefficients.Length)) 
                { }
                else if ((i+1) != model.Goal.Coefficients.Length + 1) 
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

        private StandardModel StandardizeModel(Model model) {
            StandardModel newModel = new StandardModel();
            newModel.Constraints = model.Constraints;
            newModel.Goal = model.Goal;
            newModel.GoalKind = model.GoalKind;
            newModel.SVariables = new double[newModel.Constraints.Count];
            //newModel.ArtificialVars = new double[newModel.Constraints.Count];
            int i = 0;
            foreach(LinearConstraint constraint in model.Constraints) {
                if (constraint.Relationship.Equals(Relationship.LessThanOrEquals)) {
                    newModel.SVariables[i] = 1;
                    //newModel.ArtificialVars[i] = 0;
                    constraint.Relationship = Relationship.Equals;
                }
                else if (constraint.Relationship.Equals(Relationship.GreaterThanOrEquals)) {
                    newModel.SVariables[i] = -1;
                    //newModel.ArtificialVars[i] = 1;
                    constraint.Relationship = Relationship.Equals;
                }
                else {
                    newModel.SVariables[i] = 0;
                    //newModel.ArtificialVars[i] = 0;
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

        private Matrix MakeRHSMatrix(StandardModel standardModel)
        {
            int numConstraints = standardModel.Constraints.Count;
            int numCoefficients = standardModel.Constraints[0].Coefficients.Length;
            int numSVars = standardModel.SVariables.Length;
            double[,] RHSArr = new double[numConstraints, 1];
            /*int numAVars = standardModel.ArtificialVars.Length;*/
            for (int i = 0; i < numConstraints; i++)
            {
                RHSArr[i, 0] = standardModel.Constraints[i].Value;
            }
            Matrix RHSMatrix = new Matrix(RHSArr);
            return RHSMatrix;
        }

        private Matrix MakeLHSMatrix(StandardModel standardModel) {
            int numConstraints = standardModel.Constraints.Count;
            int numCoefficients = standardModel.Constraints[0].Coefficients.Length;
            int numSVars = standardModel.SVariables.Length;
            /*int numAVars = standardModel.ArtificialVars.Length;*/
            double[,] LHSArr = new double[numConstraints,numCoefficients+numSVars/*+numAVars*/];
            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < numCoefficients; j++)
                {
                    LHSArr[i, j] = standardModel.Constraints[i].Coefficients[j];
                }
                for (int j = 0; j < numSVars; j++) {
                    if (i == j)
                    {
                        LHSArr[i, j + numCoefficients] = standardModel.SVariables[j];
                    }
                    else
                    {
                        LHSArr[i, j + numCoefficients] = 0;
                    }
                }
                /*for (int j = 0; j < numAVars; j++)
                {
                    matrix[i, j + numCoefficients + numSVars] = standardModel.ArtificialVars[j];
                }*/
            }
            Matrix LHSMatrix = new Matrix(LHSArr);
            return LHSMatrix;
        }

        private Matrix MakeObjMatrix(StandardModel standardModel)
        {
            int numConstraints = standardModel.Constraints.Count;
            int numCoefficients = standardModel.Constraints[0].Coefficients.Length;
            int numSVars = standardModel.SVariables.Length;
            /*int numAVars = standardModel.ArtificialVars.Length;*/
            double[,] ObjArr = new double[1, numCoefficients + numSVars];
            for (int i = 0; i < numCoefficients; i++)
            {
                ObjArr[0, i] = standardModel.Goal.Coefficients[i];
            }
            for (int i = 0; i < numSVars /*+ numAVars*/; i++)
            {
                ObjArr[0, numCoefficients + i] = 0;
            }
            //matrix[numConstraints, numCoefficients + numSVars /*+ numAVars*/] = standardModel.Goal.ConstantTerm;
            Matrix ObjMatrix = new Matrix(ObjArr);
            return ObjMatrix;
        }

        private void PrintStandardizedModel(Matrix RHS, Matrix LHS, Matrix Z)
        {
            System.Diagnostics.Debug.WriteLine("\nRHS:");
            System.Diagnostics.Debug.Write(RHS.ToString());
            System.Diagnostics.Debug.WriteLine("\nLHS:");
            System.Diagnostics.Debug.Write(LHS.ToString());
            System.Diagnostics.Debug.WriteLine("\nObjective Row:");
            System.Diagnostics.Debug.Write(Z.ToString());

        }

    }
}
