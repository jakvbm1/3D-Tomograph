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
            Point3D? enterPoint = null;
            Point3D? leavePoint = null;

            bool firstPointPresent = false;
            bool secondPointPresent = false;
            bool enterFront = false;
            double t = (startPoint.z + 1)/2;
            double x = lf.Start.x + t*(lf.End.x - lf.Start.x);
            double y = lf.Start.y + t*(lf.End.y - lf.Start.y);

            if(startPoint.x <= x && x >= endPoint.x && startPoint.y <= y && y >= endPoint.y)
            {
                enterPoint = new Point3D(x, y, startPoint.z);
                firstPointPresent = true;
                enterFront = true;
            }

            else if (lf.Start.x == lf.End.x && lf.Start.y == lf.End.y) { return 0; } //promien idzie prosto, nie trafia

            t = (startPoint.x - lf.Start.x) / (lf.End.x - lf.Start.x);
            y = lf.Start.y + t * (lf.End.y - lf.Start.y);
            double z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            if (startPoint.y <= y && y >= endPoint.y && startPoint.z <= z && z >= endPoint.z)
            {
                if (!firstPointPresent)
                {
                    enterPoint = new Point3D(startPoint.x, y, z);
                    firstPointPresent = true;
                }

                else
                {
                    leavePoint = new Point3D(startPoint.x, y, z);
                    secondPointPresent = true;
                }
            }

            else if (lf.End.y == lf.Start.y && lf.Start.x < lf.End.x && !firstPointPresent) { return 0; } //promien idzie prosto-prawo, nie trafia

            t = (endPoint.x - lf.Start.x) / (lf.End.x - lf.Start.x);
            y = lf.Start.y + t * (lf.End.y - lf.Start.y);
            z = lf.Start.z + t * (lf.End.z - lf.Start.z);

            if (startPoint.y <= y && y >= endPoint.y && startPoint.z <= z && z >= endPoint.z)
            {
                if (!firstPointPresent)
                {
                    enterPoint = new Point3D(endPoint.x, y, z);
                    firstPointPresent = true;
                }

                else
                {
                    leavePoint = new Point3D(endPoint.x, y, z);
                    secondPointPresent = true;
                }
            }

            else if (lf.End.y == lf.Start.y && lf.Start.x > lf.End.x && !firstPointPresent) { return 0; } //promien idzie prosto-lewo, nie trafia


            return 0;
        }
    }
}
