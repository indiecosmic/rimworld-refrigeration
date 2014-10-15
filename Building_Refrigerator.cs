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

            if (newItem is Meal)
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

                        Freeze(meal, from, originalAge, t);
                    }

                    stackSizes[index] = meal.stackCount;
                    ages[index] = meal.GetAge();
                }
            }
        }

        private void Freeze(Meal meal)
        {
            int age = meal.GetAge();
            float spoilFactor = ((ThingDef_Refrigerator)this.def).spoilAgeFactor;
            meal.SetAge(age - Mathf.RoundToInt((spoilFactor * meal.def.food.ticksBeforeSpoil)));
        }

        private void Freeze(Meal meal, int from, int unfrozenAge, float t)
        {
            float spoilFactor = ((ThingDef_Refrigerator)this.def).spoilAgeFactor;
            meal.SetAge(Mathf.RoundToInt(Mathf.Lerp((float)from, (float)(unfrozenAge - Mathf.RoundToInt(spoilFactor * meal.def.food.ticksBeforeSpoil)), t)));
        }

        private void Thaw(Meal meal)
        {
            int age = meal.GetAge();
            float spoilFactor = ((ThingDef_Refrigerator)this.def).spoilAgeFactor;
            age += Mathf.RoundToInt(spoilFactor * meal.def.food.ticksBeforeSpoil);

            if (age >= meal.def.food.ticksBeforeSpoil)
            {
                age = meal.def.food.ticksBeforeSpoil / 10;
            }

            meal.SetAge(age);
        }
    }
}
