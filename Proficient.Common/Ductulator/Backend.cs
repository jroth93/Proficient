using static System.Math;

namespace Proficient.Ductulator;

public static class Backend
{
    public static List<string> AirflowFriction(int airflow, double friction, int minDepth, int maxDepth)
    {
        var output = new List<string>()
        {
            "Duct Size\n[in]\n\n", 
            "Velocity\n[FPM]\n\n", 
            "Actual Friction\n[In./100 ft.]\n\n"
        };
        // determine rectangular sizes
        if (minDepth == 0) minDepth += 2;
        for (var i = minDepth; i < maxDepth + 2; i += 2)
        {
            var width = Functions.WidthSolver(friction, airflow, i);
            // print equivalent diameter on first iteration
            double vel, f;
            if (i == minDepth)
            {
                var dia = Convert.ToInt32(1.3 * Pow(width * i, 0.625) / Pow(width + i, 0.25));
                if(Convert.ToInt32(dia) % 2 == 1) dia += 1;
                if(Functions.FrictionSolver(airflow, dia - 2, 0, 0, true) < friction) dia -= 2;
                vel = Ceiling(Functions.VelocitySolver(airflow, dia, 0, 0, true));
                f = Ceiling(Functions.FrictionSolver(airflow, dia, 0, 0, true) * Constants.Fprecision) / Constants.Fprecision;
                output[0] += $"{dia} Ø\n";
                output[1] += $"{vel}\n";
                output[2] += $"{f}\n";
            }
            //output duct dimensions
            vel = Convert.ToInt32(Functions.VelocitySolver(airflow, 0, width, i, false));
            f = Ceiling(Functions.FrictionSolver(airflow, 0, width, i, false) * Constants.Fprecision) / Constants.Fprecision;
            output[0] += $"\n{width} / {i}";
            output[1] += $"\n{vel}";
            output[2] += "\n" + f.ToString("F" + Main.Settings?.FricPrec.ToString() ?? "2");
        }

        return output;
    }

    public static List<string> AirflowVelocity(int airflow, int vel, int minDepth, int maxDepth)
    {
        var output = new List<string>()
        {
            "Duct Size\n[in]\n\n", 
            "Friction\n[In./100 ft.]\n\n", 
            "Actual Velocity\n[FPM]\n\n"
        };

        for (var i = minDepth; i < maxDepth + 2; i += 2)
        {
            int actVel;
            var width = Convert.ToInt32(Ceiling(144.0 * airflow / (vel * i)));
            width = width % 2 == 1 ? width + 1 : width;
            var fRect = Ceiling(Functions.FrictionSolver(airflow, 0, width, i, false) * Constants.Fprecision) / Constants.Fprecision;
            if (i == minDepth)
            {
                var dia = Convert.ToInt32(Ceiling(Pow(576.0 * airflow / (PI * vel), 0.5)));
                if (dia % 2 == 1) dia += 1;
                var fRnd = Ceiling(Functions.FrictionSolver(airflow, dia, 0, 0, true) * Constants.Fprecision) / Constants.Fprecision;
                actVel = Convert.ToInt32(Functions.VelocitySolver(airflow, dia, 0, 0, true));
                output[0] += $"{dia} Ø\n";
                output[1] += $"{fRnd}\n";
                output[2] += $"{actVel}\n";
            }
            actVel = Convert.ToInt32(Functions.VelocitySolver(airflow, 0, width, i, false));
            output[0] += $"\n{width} / {i}";
            output[1] += "\n" + fRect.ToString("F" + Main.Settings?.FricPrec.ToString() ?? "2");
            output[2] += $"\n{actVel}";
        }

        return output;
    }

    public static string EquivalentDuct(int dia, int width, int depth, int minDepth, int maxDepth, bool isRnd)
    {

        var output = "Duct Size\n[in]\n\n";
        var airflow = 1000;
        var friction = Functions.FrictionSolver(airflow, dia, width, depth, isRnd);

        if (!isRnd)
        {
            var dOut = Convert.ToInt32(Ceiling(1.3 * Pow(width * depth, 0.625) / Pow(width + depth, 0.25)));
            if(dOut % 2 == 1) dOut++;
            output += $"{dOut} Ø\n\n";
        }

        for (var i = minDepth; i < maxDepth + 2; i += 2)
        {
            var wOut = Convert.ToInt32(Functions.WidthSolver(friction, airflow, i));
            output += i == depth && isRnd ? "" : $"{wOut}/{i}\n";
        }

        return output;
    }
}