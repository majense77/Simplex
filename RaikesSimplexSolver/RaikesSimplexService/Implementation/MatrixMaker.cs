using RaikesSimplexService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaikesSimplexService.Joel
{
    class MatrixMaker
    {

        public Matrix MakeRHSMatrix(StandardModel standardModel, bool twoPhase)
        {
            if (!twoPhase)
                return MakeRHS(standardModel);
            else
                return MakeARHS(standardModel);
        }

        private Matrix MakeRHS(StandardModel standardModel)
        {
            int numConstraints = standardModel.Constraints.Count;
            int numCoefficients = standardModel.Constraints[0].Coefficients.Length;
            int numSVars = standardModel.SVariables.ToArray().Length;
            double[,] RHSArr = new double[numConstraints, 1];
            int numAVars = standardModel.ArtificialVars.ToArray().Length;
            for (int i = 0; i < numConstraints; i++)
            {
                RHSArr[i, 0] = standardModel.Constraints[i].Value;
            }
            Matrix RHSMatrix = new Matrix(RHSArr);
            return RHSMatrix;
        }

        private Matrix MakeARHS(StandardModel standardModel) 
        {
            int numConstraints = standardModel.Constraints.Count;
            int numCoefficients = standardModel.Constraints[0].Coefficients.Length;
            int numSVars = standardModel.SVariables.ToArray().Length;
            double[,] RHSArr = new double[numConstraints + 1, 1];
            int numAVars = standardModel.ArtificialVars.ToArray().Length;
            for (int i = 0; i <= numConstraints; i++)
            {
                if (i != numConstraints)
                {
                    RHSArr[i, 0] = standardModel.Constraints[i].Value;
                }
                else 
                {
                    RHSArr[i, 0] = standardModel.Goal.ConstantTerm;
                }
            }
            Matrix RHSMatrix = new Matrix(RHSArr);
            return RHSMatrix;
        }

        public Matrix MakeLHSMatrix(StandardModel standardModel, bool twoPhase) 
        {
            if (!twoPhase)
                return MakeLHS(standardModel);
            else
                return MakeALHS(standardModel);
        }

        public Matrix MakeLHS(StandardModel standardModel)
        {
            int numConstraints = standardModel.Constraints.Count;
            int numCoefficients = standardModel.Constraints[0].Coefficients.Length;
            int numSVars = standardModel.SVariables.ToArray().Length;
            int numAVars = standardModel.ArtificialVars.ToArray().Length;
            double[,] LHSArr = new double[numConstraints, numCoefficients + numSVars + numAVars];
            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < numCoefficients; j++)
                {
                    LHSArr[i, j] = standardModel.Constraints[i].Coefficients[j];
                }
                for (int j = 0; j < numSVars; j++)
                {
                    if (standardModel.SVariables.ContainsKey(j))
                    {
                        if (i == j)
                            LHSArr[i, j + numCoefficients] = standardModel.SVariables[j];
                        else
                            LHSArr[i, j + numCoefficients] = 0;
                    }
                }
                for (int j = 0; j < numAVars; j++)
                {
                    if (standardModel.ArtificialVars.ContainsKey(j))
                    {
                        if (i == j)
                            LHSArr[i, j + numCoefficients + numSVars] = standardModel.ArtificialVars[j];
                        else
                            LHSArr[i, j + numCoefficients + numSVars] = 0;
                    }
                }
            }
            Matrix LHSMatrix = new Matrix(LHSArr);
            return LHSMatrix;
        }

        private Matrix MakeALHS(StandardModel standardModel) 
        {
            int numConstraints = standardModel.Constraints.Count;
            int numCoefficients = standardModel.Constraints[0].Coefficients.Length;
            int numSVars = standardModel.SVariables.ToArray().Length;
            int numAVars = standardModel.ArtificialVars.ToArray().Length;
            double[,] LHSArr = new double[numConstraints + 1, numCoefficients + numSVars + numAVars];
            for (int i = 0; i <= numConstraints; i++)
            {
                //Z-Row
                if (i == numConstraints)
                {
                    for (int j = 0; j < numCoefficients; j++)
                    {
                        LHSArr[i, j] = standardModel.Goal.Coefficients[j];
                    }
                    for (int j = 0; j < numSVars; j++)
                    {
                        LHSArr[i, j + numCoefficients] = 0;
                    }
                    for (int j = 0; j < numAVars; j++)
                    {
                        LHSArr[i, j + numCoefficients + numSVars] = 0;
                    }
                }
                else
                {
                    //Most of the matrix
                    for (int j = 0; j < numCoefficients; j++)
                    {
                        LHSArr[i, j] = standardModel.Constraints[i].Coefficients[j];
                    }
                    for (int j = 0; j < numSVars; j++)
                    {
                        if (standardModel.SVariables.ContainsKey(j))
                        {
                            if (i == j)
                                LHSArr[i, j + numCoefficients] = standardModel.SVariables[j];
                            else
                                LHSArr[i, j + numCoefficients] = 0;
                        }
                    }
                    for (int j = 0; j < numAVars; j++)
                    {
                        if (standardModel.ArtificialVars.ContainsKey(j))
                        {
                            if (i == j)
                                LHSArr[i, j + numCoefficients + numSVars] = standardModel.ArtificialVars[j];
                            else
                                LHSArr[i, j + numCoefficients + numSVars] = 0;
                        }
                    }
                }
            }
            Matrix LHSMatrix = new Matrix(LHSArr);
            return LHSMatrix;
        }

        public Matrix MakeZObjMatrix(StandardModel standardModel)
        {
            int numConstraints = standardModel.Constraints.Count;
            int numCoefficients = standardModel.Constraints[0].Coefficients.Length;
            int numSVars = standardModel.SVariables.ToArray().Length;
            int numAVars = standardModel.ArtificialVars.ToArray().Length;
            double[,] ObjArr = new double[1, numCoefficients + numSVars + numAVars];
            for (int i = 0; i < numCoefficients; i++)
            {
                ObjArr[0, i] = standardModel.Goal.Coefficients[i];
            }
            for (int i = 0; i < numSVars + numAVars; i++)
            {
                ObjArr[0, numCoefficients + i] = 0;
            }
            Matrix ObjMatrix = new Matrix(ObjArr);
            return ObjMatrix;
        }

        public Matrix MakeWObjMatrix(StandardModel standardModel, Matrix LHSMatrix) 
        {
            List<Matrix> ARows = new List<Matrix>();
            for (int i = 0; i < standardModel.SVariables.Count; i++)
            {
                if (standardModel.ArtificialVars.ContainsKey(i))
                {
                    ARows.Add(LHSMatrix.Row(i + 1));
                }
            }
            int Coeff = standardModel.Constraints[0].Coefficients.Count();
            int SVars = standardModel.SVariables.Count;
            int AVars = standardModel.ArtificialVars.Count;
            double[,] WRow = new double[1,Coeff + standardModel.SVariables.Count + AVars];
            for (int i = 0; i < ARows.Count; i++)
            {
                Matrix row = ARows[i];
                for (int j = 0; j < Coeff + SVars; j++)
                {
                    WRow[0,j] -= Double.Parse(row.RowSum(j + 1).ToString());
                }
            }
            Matrix WRowMatrix = new Matrix(WRow);
            return WRowMatrix;
        }
    }
}
