﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace Pulsar4X.ECSLib
{
    public static class PopulationProcessor
    {
        internal static void GrowPopulation(Entity colony)
        {
            // Get current population
            Dictionary<Entity, long> currentPopulation = colony.GetDataBlob<ColonyInfoDB>().Population;
            var instancesDB = colony.GetDataBlob<ComponentInstancesDB>();

            var infrustructureDesigns = instancesDB.GetDesignsByType(typeof(PopulationSupportAtbDB));

            //List<KeyValuePair<Entity, PrIwObsList<Entity>>> infrastructure = instancesDB.ComponentsByDesign.GetInternalDictionary().Where(item => item.Key.HasDataBlob<PopulationSupportAtbDB>()).ToList();
            long popSupportValue;

            //  Pop Cap = Total Population Support Value / Colony Cost
            // Get total popSupport
            popSupportValue = 0;


            foreach (var design in infrustructureDesigns)
            {
                //if(installations[kvp.Key]
                foreach (var component in instancesDB.GetComponentsBySpecificDesign(design.Guid))
                {
                    if (component.IsEnabled)
                    {
                        popSupportValue += (long)(design.GetAttribute<PopulationSupportAtbDB>().PopulationCapacity * component.HealthPercent());
                    }
                }

            }

            long needsSupport = 0;

            foreach (KeyValuePair<Entity, long> kvp in currentPopulation)
            {
                // count the number of different population groups that need infrastructure support
                if (SpeciesProcessor.ColonyCost(colony.GetDataBlob<ColonyInfoDB>().PlanetEntity, kvp.Key.GetDataBlob<SpeciesDB>()) > 1.0)
                    needsSupport++;
            }

            // find colony cost, divide the population support value by it
            foreach (KeyValuePair<Entity, long> kvp in currentPopulation.ToArray())
            {
                double colonyCost = SpeciesProcessor.ColonyCost(colony.GetDataBlob<ColonyInfoDB>().PlanetEntity, kvp.Key.GetDataBlob<SpeciesDB>());
                long maxPopulation;
                double growthRate;
                long newPop;

                if (colonyCost > 1.0)
                {
                    maxPopulation = (long)((double)(popSupportValue / needsSupport) / colonyCost) ;
                    if (currentPopulation[kvp.Key] > maxPopulation) // People will start dying
                    {
                        long excessPopulation = currentPopulation[kvp.Key] - maxPopulation;
                        // @todo: figure out better formula
                        growthRate = -50.0;
                        newPop = (long)(kvp.Value * (1.0 + growthRate));
                        if (newPop < 0)
                            newPop = 0;
                        currentPopulation[kvp.Key] = newPop;
                    }
                    else
                    {
                        // Colony Growth Rate = 20 / (CurrentPop ^ (1 / 3))
                        // Capped at 10% before modifiers for planetary and sector governors, also affected by radiation
                        growthRate = (20.0 / (Math.Pow(kvp.Value, (1.0 / 3.0))));
                        if (growthRate > 10.0)
                            growthRate = 10.0;
                        // @todo: get external factors in population growth (or death)
                        newPop = (long)(kvp.Value * (1.0 + growthRate));
                        if (newPop > maxPopulation)
                            newPop = maxPopulation;
                        if (newPop < 0)
                            newPop = 0;
                        currentPopulation[kvp.Key] = newPop;
                    }
                }
                else
                {
                    // Colony Growth Rate = 20 / (CurrentPop ^ (1 / 3))
                    // Capped at 10% before modifiers for planetary and sector governors, also affected by radiation
                    growthRate = (20.0 / (Math.Pow(kvp.Value, (1.0 / 3.0))));
                    if (growthRate > 10.0)
                        growthRate = 10.0;
                    // @todo: get external factors in population growth (or death)
                    newPop = (long)(kvp.Value * (1.0 + growthRate));
                    if (newPop < 0)
                        newPop = 0;
                    currentPopulation[kvp.Key] = newPop;
                }
            }
        }

        public static void ReCalcMaxPopulation(Entity colonyEntity)
        {

            var infrastructure = new List<Entity>();
            var instancesDB = colonyEntity.GetDataBlob<ComponentInstancesDB>();

            //List<KeyValuePair<Entity, PrIwObsList<Entity>>> infrastructureEntities = instancesDB.ComponentsByDesign.GetInternalDictionary().Where(item => item.Key.HasDataBlob<PopulationSupportAtbDB>()).ToList();
            var infrastructureDesigns = instancesDB.GetDesignsByType(typeof(PopulationSupportAtbDB));

            long totalMaxPop = 0;

            foreach (var design in infrastructureDesigns)
            {
                int designPopCap = design.GetAttribute<PopulationSupportAtbDB>().PopulationCapacity;
                foreach (var infrastructureInstance in instancesDB.GetComponentsBySpecificDesign(design.Guid))
                {
                    if (infrastructureInstance.IsEnabled)
                    {
                        totalMaxPop += (int)(designPopCap * infrastructureInstance.HealthPercent());
                    }
                }
            }

            colonyEntity.GetDataBlob<ColonyLifeSupportDB>().MaxPopulation = totalMaxPop;

        }
    }
}
