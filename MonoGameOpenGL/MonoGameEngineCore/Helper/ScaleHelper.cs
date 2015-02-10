namespace MonoGameEngineCore.Helper
{
    public static class ScaleHelper
    {
        public static float Tenths(float x)
        {
            return x/10f;
        }

        public static float Thousandths(float x)
        {
            return x/1000f;
        }

        public static float Millionths(float x)
        {
            return x/1000000f;
        }

        public static float Hundreds(float x)
        {
            return x*100;
        }

        public static float Thousands(float x)
        {
            return x*1000;
        }

        public static float Millions(float x)
        {
            return x*1000000;
        }

        public static float Billions(float x)
        {
            return x * 1000000000;
        }
    }
}