using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Tomograph
{
    internal interface Shape3D
    {
        public double Material { get; }
        public double calculateLoss(LinearFunction3D linearFunction);
    }

    class Sphere : Shape3D
    {
        Point3D center;
        double radius;
        double material;
        public Sphere(Point3D center, double radius, double material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        public double Material { get => material; }

        public double calculateLoss(LinearFunction3D linearFunction)
        {
            double t = -1* (linearFunction.Start.x - center.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);

            double squareDistance = Math.Pow(((linearFunction.Start.x - center.x) + t * (linearFunction.End.x - linearFunction.Start.x)), 2) +
                Math.Pow(((linearFunction.Start.y - center.y) + t * (linearFunction.End.y - linearFunction.Start.x)), 2) +
                Math.Pow(((linearFunction.Start.z - center.z) + t * (linearFunction.End.z - linearFunction.Start.z)), 2);


            double distance = Math.Sqrt(squareDistance);

            if (distance >= radius) 
            {
                return 0;
            }

            else
            {
                return material * 2 * Math.Sqrt(radius * radius + distance * distance);
            }
        }
    }
}
