namespace SuperThreadPool.Tools
{
    public class Checker
    {
        public static void CheckIsNotZero(string argName, int value)
        {
            if(value == 0)
            {
                throw new ArgumentException("Parameter cannot be zero", argName);
            }
        }
    }
}
