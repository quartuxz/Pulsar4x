﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulsar4X.ECSLib
{
    public class RefineResourcesAtbDB : BaseDataBlob, IComponentDesignAttribute
    {
        [JsonProperty]
        public List<Guid> RefinableMatsList { get; internal set; }

        [JsonProperty]
        public int RefineryPoints { get; internal set; }

        public RefineResourcesAtbDB()
        {
        }

        /// <summary>
        /// this is for the parser, it takes a dictionary but turns it into a list of keys, ignoring the values.
        /// </summary>
        /// <param name="refinableMatsList">a list of guid that this is capable of refining</param>
        /// <param name="RefineryPoints"></param>
        public RefineResourcesAtbDB(Dictionary<Guid, double> refinableMatsList, double refineryPoints)
        {
            RefinableMatsList = refinableMatsList.Keys.ToList();
            RefineryPoints = (int)refineryPoints;
        }

        public RefineResourcesAtbDB(List<Guid> refinableMatsList, int refineryPoints)
        {
            RefinableMatsList = refinableMatsList;
            RefineryPoints = refineryPoints;
        }

        public RefineResourcesAtbDB(RefineResourcesAtbDB db)
        {
            RefinableMatsList = new List<Guid>(db.RefinableMatsList);
            RefineryPoints = db.RefineryPoints;
        }

        public override object Clone()
        {
            return new RefineResourcesAtbDB(this);
        }

        public void OnComponentInstallation(Entity parentEntity, ComponentInstance componentInstance)
        {
            if (!parentEntity.HasDataBlob<RefiningDB>())
                parentEntity.SetDataBlob(new RefiningDB());
            RefiningProcessor.ReCalcRefiningRate(parentEntity);
        }
    }
}