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
    }
}
