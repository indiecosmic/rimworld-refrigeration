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
        public List<int> frozenThings = new List<int>();
        public List<int> stackSizes = new List<int>();
        public List<int> ages = new List<int>();
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
                    if (frozenThings.Contains(meal.thingIDNumber))
                    {
                        Thaw(meal);
                        frozenThings.Remove(meal.thingIDNumber);
                    }
                }
            }

            base.Destroy(mode);
        }

        public override void Notify_LostThing(Thing newItem)
        {
            base.Notify_LostThing(newItem);

            if (newItem is Meal && frozenThings.Contains(newItem.thingIDNumber))
            {
                Thaw((Meal)newItem);
                int index = frozenThings.IndexOf(newItem.thingIDNumber);
                frozenThings.Remove(newItem.thingIDNumber);
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
                        if (!frozenThings.Contains(meal.thingIDNumber))
                        {
                            Freeze(meal);
                            frozenThings.Add(meal.thingIDNumber);
                            stackSizes.Add(meal.stackCount);
                            ages.Add(meal.GetAge());
                        }
                        int index = frozenThings.IndexOf(meal.thingIDNumber);
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
                        if (frozenThings.Contains(meal.thingIDNumber))
                        {
                            Thaw(meal);
                            int index = frozenThings.IndexOf(meal.thingIDNumber);
                            frozenThings.Remove(meal.thingIDNumber);
                            stackSizes.RemoveAt(index);
                            ages.RemoveAt(index);
                        }
                    }
                }
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.powerComp = base.GetComp<CompPowerTrader>();

            if (frozenThings.Count != ages.Count || frozenThings.Count != stackSizes.Count)
            {
                frozenThings.Clear();
                ages.Clear();
                stackSizes.Clear();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (this.frozenThings == null) this.frozenThings = new List<int>();
            if (this.ages == null) this.ages = new List<int>();
            if (this.stackSizes == null) this.stackSizes = new List<int>();
            
            Scribe_Collections.LookList<int>(ref this.frozenThings, "frozenThings", LookMode.Value);
            Scribe_Collections.LookList<int>(ref this.ages, "ages", LookMode.Value);
            Scribe_Collections.LookList<int>(ref this.stackSizes, "stackSizes", LookMode.Value);
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
    }
}
