﻿using RaikesSimplexService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaikesSimplexService.Joel
{
    class MatrixMaker
    {
        public Matrix MakeRHSMatrix(StandardModel standardModel)
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

        public Matrix MakeLHSMatrix(StandardModel standardModel)
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
    }
}