using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaikesSimplexService.DataModel
{
    class StandardModel : Model
    {

        public Dictionary<int, double> SVariables { get; set; }

        public Dictionary<int, double> ArtificialVars { get; set; }
    }
}
