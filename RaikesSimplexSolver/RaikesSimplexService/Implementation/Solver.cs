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
            System.Diagnostics.Debug.WriteLine(" ");
            return null;
        }
    }
}
