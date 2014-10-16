using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace IndieSoft.RimWorld.Refrigeration
{
    public class Building_Refrigerator : Building_Storage, SlotGroupParent
    {
        private List<Thing> frozenThings = new List<Thing>();
        private List<int> stackSizes = new List<int>();
        private List<int> ages = new List<int>();
        private CompPowerTrader powerComp;

        public bool IsOn
        {
            get
            {
                return this.powerComp != null && this.powerComp.PowerOn;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (Thing thing in GetSlotGroup().HeldThings)
            {
                if (thing is Meal)
                {
                    Meal meal = (Meal)thing;
                    if (frozenThings.Contains(meal))
                    {
                        Thaw(meal);
                        frozenThings.Remove(meal);
                    }
                }
            }

            base.Destroy(mode);
        }

        public override void Notify_LostThing(Thing newItem)
        {
            base.Notify_LostThing(newItem);

            if (newItem is Meal && frozenThings.Contains(newItem))
            {
                Thaw((Meal)newItem);
                int index = frozenThings.IndexOf(newItem);
                frozenThings.Remove(newItem);
                stackSizes.RemoveAt(index);
                ages.RemoveAt(index);
            }
        }

        public override void Tick()
        {
            base.Tick();

            foreach (Thing thing in GetSlotGroup().HeldThings)
            {
                if (thing is Meal)
                {
                    Meal meal = (Meal)thing;
                    if (this.IsOn)
                    {
                        if (!frozenThings.Contains(meal))
                        {
                            Freeze(meal);
                            frozenThings.Add(meal);
                            stackSizes.Add(meal.stackCount);
                            ages.Add(meal.GetAge());
                        }
                        int index = frozenThings.IndexOf(meal);
                        if (meal.stackCount != stackSizes[index])
                        {
                            int diff = meal.stackCount - stackSizes[index];
                            float t = (diff / (float)meal.stackCount);
                            int lerp = meal.GetAge();
                            int from = ages[index];

                            int originalAge = Mathf.RoundToInt(((lerp - from) + (t * from)) / t);

                            FreezeAbsorbedStack(meal, from, originalAge, t);
                        }

                        stackSizes[index] = meal.stackCount;
                        ages[index] = meal.GetAge();
                    }
                    else
                    {
                        if (frozenThings.Contains(meal))
                        {
                            Thaw(meal);
                            int index = frozenThings.IndexOf(meal);
                            frozenThings.Remove(meal);
                            stackSizes.RemoveAt(index);
                            ages.RemoveAt(index);
                        }
                    }
                }
            }
        }

        private void Freeze(Meal meal)
        {
            int age = meal.GetAge();
            float multiplier = ((ThingDef_Refrigerator)this.def).spoilAgeFactor;
            int maxAge = meal.def.food.ticksBeforeSpoil;
            int newMaxAge = Mathf.RoundToInt(multiplier * maxAge);

            int result = CalcAge(maxAge, newMaxAge, age);
            meal.SetAge(result);
        }

        private void FreezeAbsorbedStack(Meal meal, int from, int ageOfAddedMeal, float t)
        {
            float multiplier = ((ThingDef_Refrigerator)this.def).spoilAgeFactor;
            int maxAge = meal.def.food.ticksBeforeSpoil;
            int newMaxAge = Mathf.RoundToInt(multiplier * maxAge);

            int addedMealFrozen = CalcAge(maxAge, newMaxAge, ageOfAddedMeal);

            int result = Mathf.RoundToInt(Mathf.Lerp((float)from, (float)addedMealFrozen, t));
            meal.SetAge(result);
        }

        private int CalcAge(int maxAge, int newMaxAge, int currentAge)
        {
            return maxAge - (newMaxAge - Mathf.RoundToInt((currentAge / (float)maxAge) * newMaxAge));
        }

        private int CalcAgeReversed(int maxAge, int newMaxAge, int currentAge)
        {
            return maxAge + Mathf.RoundToInt(((currentAge - maxAge) / (float)newMaxAge) * maxAge);
        }

        private void Thaw(Meal meal)
        {
            int age = meal.GetAge();
            float multiplier = ((ThingDef_Refrigerator)this.def).spoilAgeFactor;
            int maxAge = meal.def.food.ticksBeforeSpoil;
            int newMaxAge = Mathf.RoundToInt(multiplier * maxAge);

            int result = CalcAgeReversed(maxAge, newMaxAge, age);
            meal.SetAge(result);
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.powerComp = base.GetComp<CompPowerTrader>();
        }
    }
}
