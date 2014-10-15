using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IndieSoft.RimWorld.Refrigeration
{
    public static class MealUtility
    {
        public static int GetAge(this Meal meal)
        {
            FieldInfo ageField = typeof(Meal).GetField("age", BindingFlags.NonPublic | BindingFlags.Instance);
            return (int)ageField.GetValue(meal);
        }

        public static void SetAge(this Meal meal, int age)
        {
            FieldInfo ageField = typeof(Meal).GetField("age", BindingFlags.NonPublic | BindingFlags.Instance);
            ageField.SetValue(meal, age);
        }
    }
}
