﻿#region Copyright/License
/* 
 *Copyright© 2017 Daniel Phelps
    This file is part of Pulsar4x.

    Pulsar4x is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Pulsar4x is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Pulsar4x.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Pulsar4X.ECSLib
{
    public class AsteroidDamageDB : BaseDataBlob
    {
        /// <summary>
        /// Asteroids are damageable and need to store their health value.
        /// </summary>
        [JsonProperty]
        private Int32 _health = 100;

        [PublicAPI]
        public int Health { get { return _health; } internal set { SetField(ref _health, value); } }


        /// <summary>
        /// Default constructor.
        /// </summary>
        public AsteroidDamageDB()
        {
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="clone"></param>
        public AsteroidDamageDB(AsteroidDamageDB clone)
        {
            _health = clone._health;
        }

        // Datablobs must implement the IClonable interface.
        // Most datablobs simply call their own constructor like so:
        public override object Clone()
        {
            return new AsteroidDamageDB(this);
        }
    }
}
