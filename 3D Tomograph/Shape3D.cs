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
        public double calculateLoss(LinearFunction3D lf);
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
            // Direction vector of the line
            double dx = linearFunction.End.x - linearFunction.Start.x;
            double dy = linearFunction.End.y - linearFunction.Start.y;
            double dz = linearFunction.End.z - linearFunction.Start.z;

            // Vector from start of the line to the sphere center
            double sx = center.x - linearFunction.Start.x;
            double sy = center.y - linearFunction.Start.y;
            double sz = center.z - linearFunction.Start.z;

            // Projection factor t, clamped to the line segment
            double t = (sx * dx + sy * dy + sz * dz) / (dx * dx + dy * dy + dz * dz);
            t = Math.Max(0, Math.Min(1, t));  // Clamp t to [0,1] to ensure it's within the segment

            // Closest point on the line segment to the sphere center
            double closestX = linearFunction.Start.x + t * dx;
            double closestY = linearFunction.Start.y + t * dy;
            double closestZ = linearFunction.Start.z + t * dz;

            // Distance from the closest point to the sphere center
            double squareDistance = (closestX - center.x) * (closestX - center.x) +
                                    (closestY - center.y) * (closestY - center.y) +
                                    (closestZ - center.z) * (closestZ - center.z);

            double distance = Math.Sqrt(squareDistance);

            if (distance >= radius)
            {
                return 0; // No intersection
            }

            // Compute the actual loss based on the segment passing through the sphere
            double penetrationDepth = 2 * Math.Sqrt(radius * radius - distance * distance);
            return material * penetrationDepth;
        }

    }

    class Cuboid : Shape3D
    {
        Point3D startPoint;
        Point3D endPoint;
        double material;

        public Cuboid(double material, Point3D start, Point3D end)
        {
            this.material = material;

            startPoint = new Point3D(Math.Min(start.x, end.x), Math.Min(start.y, end.y), Math.Min(start.z, end.z));
            endPoint = new Point3D(Math.Max(start.x, end.x), Math.Max(start.y, end.y), Math.Max(start.z, end.z));
        }

        public double Material => material;

        public double calculateLoss(LinearFunction3D linearFunction)
        {
            //prosto
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

                    else if (TopEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);
                        enterPoint = new Point3D(x, endPoint.y, z);
                    }

                    if (enterPoint is null)
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

                            leavePoint = new Point3D(x, startPoint.y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                        return distance;
                    }
                }

                //prosto - w gore
                else
                {
                    Point3D? enterPoint = null;

                    if (frontEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.z - startPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                        enterPoint = new Point3D(x, y, startPoint.z);
                    }

                    else if (BottomEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.y - startPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);
                        enterPoint = new Point3D(x, startPoint.y, z);
                    }

                    if (enterPoint is null)
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
                            double t = -1 * (linearFunction.Start.y - startPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                            double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(x, endPoint.y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                        return distance;
                    }
                }
            }

            //w prawo
            else if (linearFunction.End.x > linearFunction.Start.x)
            {
                //prosto
                if (linearFunction.End.y == linearFunction.Start.y)
                {
                    Point3D? enterPoint = null;

                    if (frontEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.z - startPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                        enterPoint = new Point3D(x, y, startPoint.z);
                    }

                    else if (LeftEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.x - startPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(startPoint.x, y, z);
                    }

                    if (enterPoint is null) { return 0; }

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
                            double t = -1 * (linearFunction.Start.x - endPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                            double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(endPoint.x, y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                        return distance;
                    }

                }

                //w dol
                else if (linearFunction.End.y < linearFunction.Start.y)
                {
                    Point3D? enterPoint = null;

                    if (frontEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.z - startPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                        enterPoint = new Point3D(x, y, startPoint.z);
                    }

                    else if (LeftEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.x - startPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(startPoint.x, y, z);
                    }

                    else if (TopEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(x, endPoint.y, z);
                    }

                    if (enterPoint is null)
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

                        else if (BottomEnter(linearFunction))
                        {
                            double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                            double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(x, startPoint.y, z);
                        }

                        else
                        {
                            double t = -1 * (linearFunction.Start.x - endPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                            double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(endPoint.x, y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                        return distance;
                    }

                }

                //w gore
                else
                {
                    Point3D? enterPoint = null;

                    if (frontEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.z - startPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                        enterPoint = new Point3D(x, y, startPoint.z);
                    }

                    else if (LeftEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.x - startPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(startPoint.x, y, z);
                    }

                    else if (BottomEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.y - startPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(x, startPoint.y, z);
                    }

                    if (enterPoint is null)
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

                        else if (TopEnter(linearFunction))
                        {
                            double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                            double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(x, endPoint.y, z);
                        }

                        else
                        {
                            double t = -1 * (linearFunction.Start.x - endPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                            double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(endPoint.x, y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                        return distance;
                    }
                }
            }

            //w lewo
            else if (linearFunction.End.x < linearFunction.Start.x)
            {
                //prosto
                if (linearFunction.End.y == linearFunction.Start.y)
                {
                    Point3D? enterPoint = null;

                    if (frontEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.z - startPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                        enterPoint = new Point3D(x, y, startPoint.z);
                    }

                    else if (RightEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.x - endPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(endPoint.x, y, z);
                    }

                    if (enterPoint is null) { return 0; }

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
                            double t = -1 * (linearFunction.Start.x - startPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                            double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(startPoint.x, y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                        return distance;
                    }

                }

                //w dol
                else if (linearFunction.End.y < linearFunction.Start.y)
                {
                    Point3D? enterPoint = null;

                    if (frontEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.z - startPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                        enterPoint = new Point3D(x, y, startPoint.z);
                    }

                    else if (RightEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.x - endPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(endPoint.x, y, z);
                    }

                    else if (TopEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(x, endPoint.y, z);
                    }

                    if (enterPoint is null)
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

                        else if (BottomEnter(linearFunction))
                        {
                            double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                            double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(x, startPoint.y, z);
                        }

                        else
                        {
                            double t = -1 * (linearFunction.Start.x - startPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                            double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(startPoint.x, y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                        return distance;
                    }

                }

                //w gore
                else
                {
                    Point3D? enterPoint = null;

                    if (frontEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.z - startPoint.z) * (linearFunction.End.z - linearFunction.Start.z) / Math.Pow((linearFunction.End.z - linearFunction.Start.z), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);

                        enterPoint = new Point3D(x, y, startPoint.z);
                    }

                    else if (RightEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.x - endPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                        double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(endPoint.x, y, z);
                    }

                    else if (BottomEnter(linearFunction))
                    {
                        double t = -1 * (linearFunction.Start.y - startPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);
                        double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                        double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                        enterPoint = new Point3D(x, startPoint.y, z);
                    }

                    if (enterPoint is null)
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

                        else if (TopEnter(linearFunction))
                        {
                            double t = -1 * (linearFunction.Start.y - endPoint.y) * (linearFunction.End.y - linearFunction.Start.y) / Math.Pow((linearFunction.End.y - linearFunction.Start.y), 2);

                            double x = linearFunction.Start.x + t * (linearFunction.End.x - linearFunction.Start.x);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(x, endPoint.y, z);
                        }

                        else
                        {
                            double t = -1 * (linearFunction.Start.x - startPoint.x) * (linearFunction.End.x - linearFunction.Start.x) / Math.Pow((linearFunction.End.x - linearFunction.Start.x), 2);
                            double y = linearFunction.Start.y + t * (linearFunction.End.y - linearFunction.Start.y);
                            double z = linearFunction.Start.z + t * (linearFunction.End.z - linearFunction.Start.z);

                            leavePoint = new Point3D(startPoint.x, y, z);
                        }

                        double distance = Math.Sqrt(Math.Pow(leavePoint.x - ((Point3D)enterPoint).x, 2) + Math.Pow(leavePoint.y - ((Point3D)enterPoint).y, 2) + Math.Pow(leavePoint.z - ((Point3D)enterPoint).z, 2)) * material;
                        return distance;
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

            return (lf.Start.x <= x && lf.End.x >= x && y >= lf.Start.y && y <= lf.End.y);

        }

        public bool LeftEnter(LinearFunction3D lf) 
        {
            double t = -1 * (lf.Start.x - startPoint.x) * (lf.End.x - lf.Start.x) / Math.Pow((lf.End.x - lf.Start.x), 2);

            double y = lf.Start.y + t * (lf.End.y - lf.Start.y);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            return (lf.Start.z <= z && lf.End.z >= z && y >= lf.Start.y && y <= lf.End.y);
        }

        public bool RightEnter(LinearFunction3D lf)
        {
            double t = -1 * (lf.Start.x - endPoint.x) * (lf.End.x - lf.Start.x) / Math.Pow((lf.End.x - lf.Start.x), 2);

            double y = lf.Start.y + t * (lf.End.y - lf.Start.y);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            return (lf.Start.z <= z && lf.End.z >= z && y >= lf.Start.y && y <= lf.End.y);
        }

        public bool BottomEnter(LinearFunction3D lf) 
        {
            double t = -1 * (lf.Start.y - startPoint.y) * (lf.End.y - lf.Start.y) / Math.Pow((lf.End.y - lf.Start.y), 2);

            double x = lf.Start.x + t * (lf.End.x - lf.Start.x);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            return (lf.Start.z <= z && lf.End.z >= z && z >= lf.Start.z && z <= lf.End.z);
        }

        public bool TopEnter(LinearFunction3D lf)
        {
            double t = -1 * (lf.Start.y - endPoint.y) * (lf.End.y - lf.Start.y) / Math.Pow((lf.End.y - lf.Start.y), 2);

            double x = lf.Start.x + t * (lf.End.x - lf.Start.x);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            return (lf.Start.z <= z && lf.End.z >= z && z >= lf.Start.z && z <= lf.End.z);
        }
    }

    class Cuboid2: Shape3D
    {
        Point3D startPoint;
        Point3D endPoint;
        double material;

        public double Material => material;

        public Cuboid2(double material, Point3D start, Point3D end)
        {
            this.material = material;

            startPoint = new Point3D(Math.Min(start.x, end.x), Math.Min(start.y, end.y), Math.Min(start.z, end.z));
            endPoint = new Point3D(Math.Max(start.x, end.x), Math.Max(start.y, end.y), Math.Max(start.z, end.z));
        }

        public double calculateLoss(LinearFunction3D lf)
        {
            double tMin = double.NegativeInfinity, tMax = double.PositiveInfinity;

            Point3D rayDir = new Point3D(lf.End.x - lf.Start.x, lf.End.y - lf.Start.y, lf.End.z - lf.Start.z);
            Point3D rayOrig = lf.Start;

            double[] boundsX = { startPoint.x, endPoint.x };
            double[] boundsY = { startPoint.y, endPoint.y };
            double[] boundsZ = { startPoint.z, endPoint.z };

            // Iterate over x, y, z axes
            for (int i = 0; i < 3; i++)
            {
                double origin = i == 0 ? rayOrig.x : (i == 1 ? rayOrig.y : rayOrig.z);
                double direction = i == 0 ? rayDir.x : (i == 1 ? rayDir.y : rayDir.z);
                double minBound = i == 0 ? boundsX[0] : (i == 1 ? boundsY[0] : boundsZ[0]);
                double maxBound = i == 0 ? boundsX[1] : (i == 1 ? boundsY[1] : boundsZ[1]);

                if (Math.Abs(direction) < 1e-6)
                {
                    if (origin < minBound || origin > maxBound)
                        return 0; // No intersection
                }
                else
                {
                    double t0 = (minBound - origin) / direction;
                    double t1 = (maxBound - origin) / direction;

                    if (t0 > t1) (t0, t1) = (t1, t0); // Swap if necessary

                    tMin = Math.Max(tMin, t0);
                    tMax = Math.Min(tMax, t1);

                    if (tMin > tMax)
                        return 0; // No intersection
                }
            }

            if (tMin < 0 && tMax < 0)
                return 0; // Ray points away from the box

            // **Compute entry and exit points in 3D space**
            Point3D entryPoint = new Point3D(rayOrig.x + tMin * rayDir.x,
                                             rayOrig.y + tMin * rayDir.y,
                                             rayOrig.z + tMin * rayDir.z);

            Point3D exitPoint = new Point3D(rayOrig.x + tMax * rayDir.x,
                                            rayOrig.y + tMax * rayDir.y,
                                            rayOrig.z + tMax * rayDir.z);

            // **Compute Euclidean distance**
            double distance = Math.Sqrt(Math.Pow(exitPoint.x - entryPoint.x, 2) +
                                        Math.Pow(exitPoint.y - entryPoint.y, 2) +
                                        Math.Pow(exitPoint.z - entryPoint.z, 2));

            return distance * material;
        }


    }
}
