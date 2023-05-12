namespace Proficient.Ductulator;

public static class Constants
{
    public const double Rho = 0.0753;
    public const double Pi = 3.1415926525897;
    public const double Sigma = 0.0005; ///material absolute roughness factor
    public static double Fprecision = Math.Pow(10, Main.Settings.FricPrec);
    public static string NumPattern = @"^\d+(\.\d+)?$";
    public static string Pattern = @"^\d*\.?\d+\s*([\+\-\*\/]\s*\d+(\.\d+)?\s*)*$";

}

public static class Functions
{
    public static int WidthSolver(double fTarget, int airflow, int depth)
    {
        var width = Convert.ToInt32(Math.Ceiling(airflow * 144.0 / depth / 1000));
        var f = FrictionSolver(airflow, 0, width, depth, false);

        if (f < fTarget)
        {
            while(f < fTarget)
            {
                f = FrictionSolver(airflow, 0, --width, depth, false);
            }
            width++;
        }
        else if (f > fTarget)
        {
            while(f > fTarget)
            {
                f = FrictionSolver(airflow, 0, ++width, depth, false);
            }
        }

        return width % 2 == 1 ? ++width : width;
    }

    public static double AirflowSolver(double fTarget, int dia, int width, int depth, bool isRnd)
    {

        var airflow = isRnd ? 
            Convert.ToInt32(dia * dia * Math.PI * 1000 / 576.0) : 
            Convert.ToInt32(width * depth * 1000 / 144.0);
        var f = FrictionSolver(airflow, dia, width, depth, isRnd);
        int upper = 1, lower = 1;

        if (f > fTarget)
        {
            do
            {
                upper = airflow;
                airflow = Convert.ToInt32(upper * 0.9);
                f = FrictionSolver(airflow, dia, width, depth, isRnd);
            } while (f > fTarget);
            lower = airflow;
        }
        else if (f < fTarget)
        {
            do
            {
                lower = airflow;
                airflow = Convert.ToInt32(lower * 1.1);
                f = FrictionSolver(airflow, dia, width, depth, isRnd);
            } while (f < fTarget);
            upper = airflow;
        }

        airflow = Convert.ToInt32((lower + upper) / 2);
            

        for (var i = lower; i < upper; i++)
        {
            if (FrictionSolver(i, dia, width, depth, isRnd) < fTarget) return --i;
        }

        return airflow;
    }

    public static double FrictionSolver(int airflow, int dia, int width, int depth, bool isRnd)
    {
        double f = 1, fNew = 0.02;
        var hyd = isRnd ? dia : 2 * width * depth / (width + depth);
        var velocity = VelocitySolver(airflow, dia, width, depth, isRnd);
        var re = 8.5 * hyd * velocity;
        while (Math.Abs(f - fNew) > 0.0001)
        {
            f = fNew;
            fNew = Math.Pow(-2 * Math.Log(12 * Constants.Sigma / (3.7 * hyd) + 2.51 / (re * Math.Pow(f, 0.5))) / Math.Log(10), -2);
        }
        return 12 * fNew * Constants.Rho * 100 / hyd * Math.Pow(velocity / 1097, 2);
    }

    public static double VelocitySolver(int airflow, int dia, int width, int depth, bool isRnd)
    {
        return isRnd ? 
            576.0 * airflow / (Math.PI * dia * dia) : 
            144.0 * airflow / (width * depth);
    }
}