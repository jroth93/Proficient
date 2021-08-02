using System;

namespace Proficient
{

    public static class Constants
    {
        public const double rho = 0.0753;
        public const double pi = 3.1415926525897;
        public const double sigma = 0.0005; ///material absolute roughness factor
        public static double fprecision = Math.Pow(10, Main.Settings.fricPrec);
        public static string numpattern = @"^\d+(\.\d+)?$";
        public static string pattern = @"^\d*\.?\d+\s*([\+\-\*\/]\s*\d+(\.\d+)?\s*)*$";

    }

    public static class Functions
    {
        public static double Widthsolver(double friction, double airflow, int depth)
        {
            double width = airflow * 144.0 / (depth * 1000);
            double tempf = Frictionsolver(airflow, 0, width, depth, false);
            double upper = 1, lower = 1;

            if (tempf < friction)
            {
                do
                {
                    upper = width;
                    width = upper * 0.9;
                    tempf = Frictionsolver(airflow, 0, width, depth, false);
                } while (tempf < friction);
                lower = width;
            }
            else if (tempf > friction)
            {
                do
                {
                    lower = width;
                    width = lower * 1.1;
                    tempf = Frictionsolver(airflow, 0, width, depth, false);
                } while (tempf > friction);
                upper = width;
            }
            else if (tempf == friction)
            {
                return Convert.ToInt32(Math.Ceiling(width));
            }

            width = (lower + upper) / 2;
            tempf = Frictionsolver(airflow, 0, width, depth, false);

            do
            {
                if (tempf < friction) { upper = width; }
                else if (tempf > friction) { lower = width; }
                width = (lower + upper) / 2;
                tempf = Frictionsolver(airflow, 0, width, depth, false);
            } while (Math.Abs(tempf - friction) > 0.0001);
            width = Convert.ToInt32(Math.Ceiling(width));
            return width % 2 == 1 ? width += 1 : width;
        }

        public static double Airflowsolver(double friction, int dia, int width, int depth, bool boolrnd)
        {

            double airflow = boolrnd ? Convert.ToDouble(dia * dia * Math.PI * 1000 / 576.0) : Convert.ToDouble(width * depth * 1000 / 144.0);
            double tempf = Frictionsolver(airflow, dia, width, depth, boolrnd);
            double upper = 1, lower = 1;

            if (tempf > friction)
            {
                do
                {
                    upper = airflow;
                    airflow = upper * 0.9;
                    tempf = Frictionsolver(airflow, dia, width, depth, boolrnd);
                } while (tempf > friction);
                lower = airflow;
            }
            else if (tempf < friction)
            {
                do
                {
                    lower = airflow;
                    airflow = lower * 1.1;
                    tempf = Frictionsolver(airflow, dia, width, depth, boolrnd);
                } while (tempf < friction);
                upper = airflow;
            }
            else if (tempf == friction)
            {
                return Convert.ToInt32(airflow / 2) * 2 + 2;
            }

            airflow = Convert.ToInt32((lower + upper) / 2);
            tempf = Frictionsolver(airflow, dia, width, depth, boolrnd);

            do
            {
                if (tempf > friction)
                { upper = airflow; }
                else if (tempf < friction)
                { lower = airflow; }
                airflow = (lower + upper) / 2;
                tempf = Frictionsolver(airflow, dia, width, depth, boolrnd);
            } while (Math.Abs(tempf - friction) > 0.0001);

            return airflow;
        }

        public static double Frictionsolver(double airflow, double dia, double width, int depth, bool boolrnd)
        {
            double f, new_f = 0.02;
            double hyd = boolrnd ? dia : 2 * width * depth / (width + depth);
            double velocity = Velocitysolver(airflow, dia, width, depth, boolrnd);
            double Re = 8.5 * hyd * velocity;
            do
            {
                f = new_f;
                new_f = Math.Pow(-2 * Math.Log(12 * Constants.sigma / (3.7 * hyd) + 2.51 / (Re * Math.Pow(f, 0.5))) / Math.Log(10), -2);
            } while (Math.Abs(f - new_f) > 0.0001);
            return 12 * new_f * Constants.rho * 100 / hyd * Math.Pow(velocity / 1097, 2);
        }

        public static double Velocitysolver(double airflow, double dia, double width, int depth, bool boolrnd)
        {
            double velocity = boolrnd ? 576 * airflow / (Math.PI * dia * dia) : 144 * airflow / (width * depth);
            return velocity;
        }
    }
}
