﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace Pulsar4X.ECSLib
{

    public class ConstructEntitiesProcessor : IHotloopProcessor
    {
        public TimeSpan RunFrequency {
            get {
                return TimeSpan.FromDays(1);
            }
        }

        public TimeSpan FirstRunOffset => TimeSpan.FromHours(3);

        public Type GetParameterType => typeof(ConstructionDB);

        public void Init(Game game)
        {
            //unneeded.
        }

        public void ProcessEntity(Entity entity, int deltaSeconds)
        {
            ConstructionProcessor.ConstructStuff(entity);
        }

        public void ProcessManager(EntityManager manager, int deltaSeconds)
        {
            foreach(var entity in manager.GetAllEntitiesWithDataBlob<ConstructionDB>()) 
            {
                ProcessEntity(entity, deltaSeconds);
            }
        }
    }

    public static class ConstructionProcessor
    {
        internal static void ConstructStuff(Entity colony)
        {
            CargoStorageDB stockpile = colony.GetDataBlob<CargoStorageDB>();
            Entity faction;
            colony.Manager.FindEntityByGuid(colony.FactionOwner, out faction);
            var factionInfo = faction.GetDataBlob<FactionInfoDB>();
            var colonyConstruction = colony.GetDataBlob<ConstructionDB>();

            var pointRates = new Dictionary<ConstructionType, int>(colonyConstruction.ConstructionRates);
            int maxPoints = colonyConstruction.PointsPerTick;

            List<ConstructionJob> constructionJobs = colonyConstruction.JobBatchList;
            foreach (ConstructionJob batchJob in constructionJobs.ToArray())
            {
                var designInfo = factionInfo.ComponentDesigns[batchJob.ItemGuid];
                ConstructionType conType = batchJob.ConstructionType;
                //total number of resources requred for a single job in this batch
                int resourcePoints = designInfo.MineralCosts.Sum(item => item.Value);
                resourcePoints += designInfo.MaterialCosts.Sum(item => item.Value);
                resourcePoints += designInfo.ComponentCosts.Sum(item => item.Value);

                while ((pointRates[conType] > 0) && (maxPoints > 0) && (batchJob.NumberCompleted < batchJob.NumberOrdered))
                {
                    //gather availible resorces for this job.
                    
                    ConsumeResources(stockpile, batchJob.MineralsRequired);
                    ConsumeResources(stockpile, batchJob.MaterialsRequired);
                    ConsumeResources(stockpile, batchJob.ComponentsRequired);

                    int useableResourcePoints = designInfo.MineralCosts.Sum(item => item.Value) - batchJob.MineralsRequired.Sum(item => item.Value);
                    useableResourcePoints += designInfo.MaterialCosts.Sum(item => item.Value) - batchJob.MaterialsRequired.Sum(item => item.Value);
                    useableResourcePoints += designInfo.ComponentCosts.Sum(item => item.Value) - batchJob.ComponentsRequired.Sum(item => item.Value);
                    //how many construction points each resourcepoint is worth.
                    int pointPerResource = designInfo.BuildPointCost / resourcePoints;
                    
                    //calculate how many construction points each resource we've got stored for this job is worth
                    int pointsToUse = Math.Min(pointRates[conType], maxPoints);
                    pointsToUse = Math.Min(pointsToUse, batchJob.ProductionPointsLeft);
                    pointsToUse = Math.Min(pointsToUse, useableResourcePoints * pointPerResource);
                    
                    //construct only enough for the amount of resources we have. 
                    batchJob.ProductionPointsLeft -= pointsToUse;
                    pointRates[conType] -= pointsToUse;                    
                    maxPoints -= pointsToUse;

                    if (batchJob.ProductionPointsLeft == 0)
                    {
                        BatchJobItemComplete(colony, stockpile, batchJob, designInfo);
                    }
                }
            }
        }

        private static void BatchJobItemComplete(Entity colonyEntity, CargoStorageDB storage, ConstructionJob batchJob, ComponentDesign designInfo)
        {
            var colonyConstruction = colonyEntity.GetDataBlob<ConstructionDB>();
            batchJob.NumberCompleted++;
            batchJob.ProductionPointsLeft = designInfo.BuildPointCost;
            batchJob.MineralsRequired = designInfo.MineralCosts;
            batchJob.MineralsRequired = designInfo.MaterialCosts;
            batchJob.MineralsRequired = designInfo.ComponentCosts;

            ComponentInstance specificComponent = new ComponentInstance(designInfo);
            if (batchJob.InstallOn != null)
            {
                if (batchJob.InstallOn == colonyEntity || StorageSpaceProcessor.HasEntity(storage, colonyEntity.GetDataBlob<CargoAbleTypeDB>()))
                {
                    EntityManipulation.AddComponentToEntity(batchJob.InstallOn, specificComponent);
                    ReCalcProcessor.ReCalcAbilities(batchJob.InstallOn);
                }
            }
            else
            {
                StorageSpaceProcessor.AddCargo(storage, specificComponent, 1);
            }

            if (batchJob.NumberCompleted == batchJob.NumberOrdered)
            {
                colonyConstruction.JobBatchList.Remove(batchJob);
                if (batchJob.Auto)
                {
                    colonyConstruction.JobBatchList.Add(batchJob);
                }
            }
        }

        /// <summary>
        /// consumes resources in the stockpile, and updates the dictionary.
        /// </summary>
        /// <param name="stockpile"></param>
        /// <param name="toUse"></param>
        private static void ConsumeResources(CargoStorageDB fromCargo, IDictionary<Guid, int> toUse)
        {   
            foreach (KeyValuePair<Guid, int> kvp in toUse.ToArray())
            {             
                ICargoable cargoItem = fromCargo.OwningEntity.Manager.Game.StaticData.GetICargoable(kvp.Key);
                Guid cargoTypeID = cargoItem.CargoTypeID;
                int amountUsedThisTick = 0;
                if (fromCargo.StoredCargoTypes.ContainsKey(cargoTypeID))
                {
                    if (fromCargo.StoredCargoTypes[cargoTypeID].ItemsAndAmounts.ContainsKey(cargoItem.ID))
                    {
                        if (fromCargo.StoredCargoTypes[cargoTypeID].ItemsAndAmounts[cargoItem.ID] >= kvp.Value)
                        {
                            amountUsedThisTick = kvp.Value;
                        }
                        else
                        {
                            amountUsedThisTick = (int)fromCargo.StoredCargoTypes[cargoTypeID].ItemsAndAmounts[cargoItem.ID];
                        }
                    }
                }
                StorageSpaceProcessor.RemoveCargo(fromCargo, cargoItem, amountUsedThisTick);            
                toUse[kvp.Key] -= amountUsedThisTick;                      
            }         
        }

        /// <summary>
        /// called by ReCalcProcessor
        /// </summary>
        /// <param name="colonyEntity"></param>
        public static void ReCalcConstructionRate(Entity colonyEntity)
        {

            //List<Entity> installations = colonyEntity.GetDataBlob<ColonyInfoDB>().Installations.Keys.ToList();
            
            var factories = new List<Entity>();

            Dictionary<ConstructionType, int> typeRates = new Dictionary<ConstructionType, int>
            {
                {ConstructionType.Ordnance, 0},
                {ConstructionType.Installations, 0},
                {ConstructionType.Fighters, 0},
                {ConstructionType.ShipComponents, 0},
                {ConstructionType.Ships, 0},
            };
            var instancesDB = colonyEntity.GetDataBlob<ComponentInstancesDB>();
            
            if (instancesDB.TryGetComponentsByAttribute<ConstructionAtbDB>(out var instances))
            {
                foreach (var instance in instances)
                {
                    float healthPercent = instance.HealthPercent();
                    var designInfo = instance.Design.GetAttribute<ConstructionAtbDB>();
                    foreach (var item in designInfo.InternalConstructionPoints)
                    {
                        typeRates.SafeValueAdd(item.Key, (int)(item.Value * healthPercent));
                    }
                }
            }
            

            colonyEntity.GetDataBlob<ConstructionDB>().ConstructionRates = typeRates;
            int maxPoints = 0;
            foreach (int p in typeRates.Values)
            {
                if (p > maxPoints)
                    maxPoints = p;
            }
            colonyEntity.GetDataBlob<ConstructionDB>().PointsPerTick = maxPoints;
        }


        #region PlayerInteraction

        /// <summary>
        /// Adds a job to a colonys ColonyConstructionDB.JobBatchList
        /// </summary>
        /// <param name="colonyEntity"></param>
        /// <param name="job"></param>
        [PublicAPI]
        public static void AddJob(FactionInfoDB factionInfo, Entity colonyEntity, ConstructionJob job)
        {
            var constructingDB = colonyEntity.GetDataBlob<ConstructionDB>();
            //var factionInfo = colonyEntity.GetDataBlob<OwnedDB>().OwnedByFaction.GetDataBlob<FactionInfoDB>();
            lock (constructingDB.JobBatchList) //prevent threaded race conditions
            {
                //check that this faction does have the design on file. I *think* all this type of construction design will get stored in factionInfo.ComponentDesigns
                if (factionInfo.ComponentDesigns.ContainsKey(job.ItemGuid))
                    constructingDB.JobBatchList.Add(job);
            }
        }

        
        /// <summary>
        /// Moves a job up or down the ColonyRefiningDB.JobBatchList. 
        /// </summary>
        /// <param name="colonyEntity">the colony that's being interacted with</param>
        /// <param name="job">the job that needs re-prioritising</param>
        /// <param name="delta">How much to move it ie: 
        /// -1 moves it down the list and it will be done later
        /// +1 moves it up the list andit will be done sooner
        /// this will safely handle numbers larger than the list size, 
        /// placing the item either at the top or bottom of the list.
        /// </param>
        [PublicAPI]
        public static void ChangeJobPriority(Entity colonyEntity, ConstructionJob job, int delta)
        {
            var constructingDB = colonyEntity.GetDataBlob<ConstructionDB>();
            lock (constructingDB.JobBatchList) //prevent threaded race conditions
            {
                //first check that the job does still exsist in the list.
                if (constructingDB.JobBatchList.Contains(job))
                {
                    var currentIndex = constructingDB.JobBatchList.IndexOf(job);
                    var newIndex = currentIndex + delta;
                    if (newIndex <= 0)
                    {
                        constructingDB.JobBatchList.RemoveAt(currentIndex);
                        constructingDB.JobBatchList.Insert(0, job);
                    }
                    else if (newIndex >= constructingDB.JobBatchList.Count - 1)
                    {
                        constructingDB.JobBatchList.RemoveAt(currentIndex);
                        constructingDB.JobBatchList.Add(job);
                    }
                    else
                    {
                        constructingDB.JobBatchList.RemoveAt(currentIndex);
                        constructingDB.JobBatchList.Insert(newIndex, job);
                    }
                }
            }
        } 
        #endregion
    }
}