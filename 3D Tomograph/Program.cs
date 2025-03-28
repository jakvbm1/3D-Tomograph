using _3D_Tomograph;

Sphere sphere = new Sphere(new Point3D(0, 0, 0), 1, 1);
Cuboid2 cuboid = new Cuboid2(1, new Point3D(-0.5, -0.5, -0.5), new Point3D(0.5, 0.5, 0.5));
TomographySimulator3D simulator = new TomographySimulator3D(2, [cuboid]);

double[] array = simulator.CalculateLosses();
Console.WriteLine(array.Length);
foreach  (double value in array)
{
    //Console.WriteLine(value);
}