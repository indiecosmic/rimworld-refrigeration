using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace IndieSoft.RimWorld.Refrigeration
{
    public class CompTemperatureBasedPowerTrader : CompPowerTrader
    {
        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            float temperature = Find.Map.WorldSquare.temperature;
            if (temperature >= 30f)
                this.powerOutput *= 2f;
            else if (temperature >= 20f)
                this.powerOutput *= 1.5f;
            else if (temperature <= 0f)
                this.powerOutput *= 0.75f;
            else if (temperature <= -10f)
                this.powerOutput *= 0.5f;
        }
    }
}
