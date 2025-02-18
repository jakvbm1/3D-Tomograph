using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Tomograph
{
    struct Point3D
    {
    public Point3D(double x, double y ,double z)
    {
    this.x = x; this.y = y; this.z = z;
    }

    public double x, y, z;
    }


    class LinearFunction3D
    {
        private Point3D start;
        private Point3D end;

        public Point3D Start {  get { return start; } }
        public Point3D End { get { return end; } }

        public LinearFunction3D(Point3D startPoint, Point3D endPoint) 
        {
            start = startPoint;
            end = endPoint;
        }
    }


    internal class TomographySimulator3D
    {
        private int matrixSize;
        private Point3D[,] entryPoints;
        private Point3D[,] endPoints;
        private LinearFunction3D[,,,] lineFunctions;

        public TomographySimulator3D(int MatrixSize)
        {
            this.matrixSize = MatrixSize;

            entryPoints = new Point3D[matrixSize, matrixSize];
            endPoints = new Point3D[matrixSize, matrixSize];
            lineFunctions = new LinearFunction3D[matrixSize, matrixSize, matrixSize, matrixSize];

            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    entryPoints[i, j] = new Point3D(-1 + (double)(2*i) / (matrixSize - 1), 0 + (double)(2*j) / (matrixSize - 1), -1);
                    endPoints[i, j] = new Point3D(0 + (double)(2*i) / (matrixSize - 1), 0 + (double)(2*j) / (matrixSize - 1), 1);
                }
            }

            
        }
    }
}
