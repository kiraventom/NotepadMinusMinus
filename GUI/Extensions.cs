using System;

namespace GUI
{
    public static class Extensions
    {
        public static bool NotNullAndTrue(this bool? nullable)
        {
            return nullable ?? false;
        }
    }
}