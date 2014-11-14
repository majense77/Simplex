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

            foreach (LinearConstraint LC in model.Constraints) 
            {
                for (int i = 0; i < LC.Coefficients.Length; i++) 
                {
                    System.Diagnostics.Debug.WriteLine(LC.Coefficients[i]);
                }
            }
            return null;
        }
    }
}
