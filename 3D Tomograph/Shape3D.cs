using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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
            double t = -1* (linearFunction.Start.z - center.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);

            double squareDistance = Math.Pow(((linearFunction.Start.x - center.x) + t * (linearFunction.End.x - linearFunction.Start.x)), 2) +
                Math.Pow(((linearFunction.Start.y - center.y) + t * (linearFunction.End.y - linearFunction.Start.y)), 2) +
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

    class Cuboid : Shape3D
    {
        Point3D startPoint;
        Point3D endPoint;
        double material;
        public double Material => material;

        public double calculateLoss(LinearFunction3D linearFunction)
        {
            throw new NotImplementedException();
        }

        public bool frontEnter(LinearFunction3D lf)
        {
            double t = -1 * (lf.Start.z - startPoint.z) * (lf.End.z - lf.Start.z) / Math.Pow((lf.End.z - lf.Start.z), 2);

            double x = lf.Start.x + t * (lf.End.x - lf.Start.x);
            double y = lf.Start.y + t * (lf.End.y - lf.Start.y);

            return (lf.Start.x < x && lf.End.x > x && y > lf.Start.y && y < lf.End.y);
        }

        public bool frontBack(LinearFunction3D lf) 
        {
            double t = -1 * (lf.Start.z - endPoint.z) * (lf.End.z - lf.Start.z) / Math.Pow((lf.End.z - lf.Start.z), 2);

            double x = lf.Start.x + t * (lf.End.x - lf.Start.x);
            double y = lf.Start.y + t * (lf.End.y - lf.Start.y);

            return (lf.Start.x < x && lf.End.x > x && y > lf.Start.y && y < lf.End.y);

        }


    }
}
