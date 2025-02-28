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
            if (linearFunction.End.x == linearFunction.Start.x)
            {

                //prosto - prosto
                if (linearFunction.End.y == linearFunction.Start.y)
                {
                    if (frontEnter(linearFunction))
                    {
                        return material * (endPoint.z - startPoint.z);
                    }

                    else { return 0; }
                }
                //prosto - w dol
                else if (linearFunction.Start.y > linearFunction.End.y) 
                {
                    Point3D? enterPoint = null;

                    if (frontEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.z - startPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                        enterPoint = new Point3D(x, y, startPoint.z);
                    }

                    else if(TopEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);
                        enterPoint = new Point3D(x, startPoint.y, z);
                    }

                    if(enterPoint is null)
                    {
                        return 0;
                    }

                    else
                    {
                        Point3D leavePoint = new Point3D(0, 0, 0);
                        if (BackEnter(linearFunction))
                        {
                            double t = -1 * (linearFunction.Start.z - endPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                            double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                            double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                            leavePoint = new Point3D(x, y, endPoint.z);
                        }

                        else
                        {
                            double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                            double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(x, endPoint.y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                    }
                }
            }

            return 0;
        }

        public bool frontEnter(LinearFunction3D lf)
        {
            double t = -1 * (lf.Start.z - startPoint.z) * (lf.End.z - lf.Start.z) / Math.Pow((lf.End.z - lf.Start.z), 2);

            double x = lf.Start.x + t * (lf.End.x - lf.Start.x);
            double y = lf.Start.y + t * (lf.End.y - lf.Start.y);

            return (lf.Start.x < x && lf.End.x > x && y > lf.Start.y && y < lf.End.y);
        }

        public bool BackEnter(LinearFunction3D lf) 
        {
            double t = -1 * (lf.Start.z - endPoint.z) * (lf.End.z - lf.Start.z) / Math.Pow((lf.End.z - lf.Start.z), 2);

            double x = lf.Start.x + t * (lf.End.x - lf.Start.x);
            double y = lf.Start.y + t * (lf.End.y - lf.Start.y);

            return (lf.Start.x < x && lf.End.x > x && y > lf.Start.y && y < lf.End.y);

        }

        public bool LeftEnter(LinearFunction3D lf) 
        {
            double t = -1 * (lf.Start.x - startPoint.x) * (lf.End.x - lf.Start.x) / Math.Pow((lf.End.x - lf.Start.x), 2);

            double y = lf.Start.y + t * (lf.End.y - lf.Start.y);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            return (lf.Start.z < z && lf.End.z > z && y > lf.Start.y && y < lf.End.y);
        }

        public bool RightEnter(LinearFunction3D lf)
        {
            double t = -1 * (lf.Start.x - endPoint.x) * (lf.End.x - lf.Start.x) / Math.Pow((lf.End.x - lf.Start.x), 2);

            double y = lf.Start.y + t * (lf.End.y - lf.Start.y);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            return (lf.Start.z < z && lf.End.z > z && y > lf.Start.y && y < lf.End.y);
        }

        public bool BottomEnter(LinearFunction3D lf) 
        {
            double t = -1 * (lf.Start.y - startPoint.y) * (lf.End.y - lf.Start.y) / Math.Pow((lf.End.y - lf.Start.y), 2);

            double x = lf.Start.x + t * (lf.End.x - lf.Start.x);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            return (lf.Start.z < z && lf.End.z > z && z > lf.Start.z && z < lf.End.z);
        }

        public bool TopEnter(LinearFunction3D lf)
        {
            double t = -1 * (lf.Start.y - endPoint.y) * (lf.End.y - lf.Start.y) / Math.Pow((lf.End.y - lf.Start.y), 2);

            double x = lf.Start.x + t * (lf.End.x - lf.Start.x);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            return (lf.Start.z < z && lf.End.z > z && z > lf.Start.z && z < lf.End.z);
        }
    }
}
