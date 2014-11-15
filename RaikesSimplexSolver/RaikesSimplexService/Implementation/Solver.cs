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
            foreach (LinearConstraint LC in model.Constraints) 
            {
                for (int i = 0; i < LC.Coefficients.Length; i++) 
                {
                    //System.Diagnostics.Debug.WriteLine(LC.Coefficients[i]);
                }
            }
            return null;
        }

        public void PrintInput(Model model) 
        {
            System.Diagnostics.Debug.WriteLine("Original Data:");

            //Objective Function
            int goalCounter = 1;
            System.Diagnostics.Debug.WriteLine("\tObjective Function:");
            System.Diagnostics.Debug.Write("\t     " + model.GoalKind + ": Z = ");
            foreach (Double coeff in model.Goal.Coefficients) 
            {
                System.Diagnostics.Debug.Write(coeff + "x" + goalCounter + " + ");
                goalCounter++;
            }
            System.Diagnostics.Debug.WriteLine(model.Goal.ConstantTerm);
            
            //Linear Constraints
            int counter;
            System.Diagnostics.Debug.WriteLine("\tConstraints:");
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
                StandardModel newModel = (StandardModel)model;
                newModel.SVariables = new double[newModel.Constraints.Count];
                newModel.ArtificialVars = new double[newModel.Constraints.Count];
                int i = 0;
                foreach(LinearConstraint constraint in model.Constraints) {
                    if (constraint.Relationship.Equals(Relationship.LessThanOrEquals)) {
                        newModel.SVariables[i] = 1;
                        newModel.ArtificialVars[i] = 0;
                        constraint.Relationship = Relationship.Equals;
                    }
                    else if (constraint.Relationship.Equals(Relationship.GreaterThanOrEquals)) {
                        newModel.SVariables[i] = -1;
                        newModel.ArtificialVars[i] = 1;
                        constraint.Relationship = Relationship.Equals;
                    }
                    else {
                        newModel.SVariables[i] = 0;
                        newModel.ArtificialVars[i] = 0;
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
    }
}
