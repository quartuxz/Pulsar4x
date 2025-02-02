﻿using Pulsar4X.ECSLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Pulsar4X.ECSLib
{
    public class ShipOrderVM : IViewModel
    {

        private DictionaryVM<StarSystem, string> _starSystems = new DictionaryVM<StarSystem, string>();
        public DictionaryVM<StarSystem, string> StarSystems 
        { 
            get 
            { 
                return _starSystems; 
            } 
            set 
            { 
                _starSystems = value;
                _starSystems.SelectedIndex = 0;
                RefreshShips(0, 0); 
            } 
        }

        private DictionaryVM<Entity, string> _shipList = new DictionaryVM<Entity, string>();
        public DictionaryVM<Entity, string> ShipList 
        { 
            get 
            { 
                return _shipList; 
            } 
            set 
            { 
                _shipList = value;
                _shipList.SelectedIndex = 0;
                RefreshOrders(0,0); 
            } 
        }

        private DictionaryVM<Entity, string> _moveTargetList = new DictionaryVM<Entity, string>();
        public DictionaryVM<Entity, string> MoveTargetList 
        { 
            get 
            { 
                return _moveTargetList; 
            } 
            set 
            {
                _moveTargetList = value;
                _moveTargetList.SelectedIndex = 0;
                OnPropertyChanged(nameof(SelectedMoveTarget));
            } 
        } 

        private DictionaryVM<Entity, string> _attackTargetList = new DictionaryVM<Entity, string>();
        public DictionaryVM<Entity, string> AttackTargetList
        {
            get
            {
                return _attackTargetList;
            }
            set
            {
                _attackTargetList = value;
                _attackTargetList.SelectedIndex = 0;
                OnPropertyChanged(nameof(SelectedAttackTarget));
            }
        } //not fields!

        private DictionaryVM<BaseOrder, string> _moveOrdersPossible = new DictionaryVM<BaseOrder, string>();
        public DictionaryVM<BaseOrder, string> MoveOrdersPossible 
        { 
            get
            { 
                return _moveOrdersPossible; 
            } 
            set
            {
                _moveOrdersPossible = value;
                _moveOrdersPossible.SelectedIndex = 0;
                OnPropertyChanged(nameof(SelectedPossibleMoveOrder));
            }
        }
        private DictionaryVM<BaseOrder, string> _moveOrderList = new DictionaryVM<BaseOrder, string>();
        public DictionaryVM<BaseOrder, string> MoveOrderList 
        {
            get
            { 
                return _moveOrderList;
            }
            set
            {
                _moveOrderList = value;
                _moveOrderList.SelectedIndex = 0;
                OnPropertyChanged(nameof(SelectedMoveOrder));
            }
        }

        private DictionaryVM<ComponentInstance, string> _fireControlList = new DictionaryVM<ComponentInstance, string>();
        public DictionaryVM<ComponentInstance, string> FireControlList
        {
            get
            {
                return _fireControlList;
            }
            set
            {
                _fireControlList = value;
                _fireControlList.SelectedIndex = 0;
                OnPropertyChanged(nameof(SelectedFireControl));
            }
        }

        private DictionaryVM<ComponentInstance, string> _attachedBeamList = new DictionaryVM<ComponentInstance, string>();
        public DictionaryVM<ComponentInstance, string> AttachedBeamList
        {
            get
            {
                return _attachedBeamList;
            }
            set
            {
                _attachedBeamList = value;
                _attachedBeamList.SelectedIndex = 0;
                OnPropertyChanged(nameof(SelectedAttachedBeam));
            }
        }

        private DictionaryVM<ComponentInstance, string> _freeBeamList = new DictionaryVM<ComponentInstance, string>();
        public DictionaryVM<ComponentInstance, string> FreeBeamList
        {
            get
            {
                return _freeBeamList;
            }
            set
            {
                _freeBeamList = value;
                _freeBeamList.SelectedIndex = 0;
                OnPropertyChanged(nameof(SelectedFreeBeam));
            }
        }

        public StarSystem SelectedSystem { get { return _starSystems.SelectedKey; }}
        public Entity SelectedShip { get { return _shipList.SelectedKey; }}
        public BaseOrder SelectedPossibleMoveOrder { get { return _moveOrdersPossible.SelectedKey; } }
        public BaseOrder SelectedMoveOrder { get { return _moveOrderList.SelectedKey; } }
        public Entity SelectedMoveTarget { get { return _moveTargetList.SelectedKey; } }
        public Entity SelectedAttackTarget { get { return _attackTargetList.SelectedKey; } }
        public ComponentInstance SelectedFireControl { get { return _fireControlList.SelectedKey; } }
        public ComponentInstance SelectedAttachedBeam { get { return _attachedBeamList.SelectedKey; } }
        public ComponentInstance SelectedFreeBeam { get { return _freeBeamList.SelectedKey; } }

        private Entity _targetedEntity;
        public string TargetedEntity {
            get
            { if (_targetedEntity == null)
                    return "None";
              else
                    return _targetedEntity.GetDataBlob<NameDB>().DefaultName;
            }
        }

        public Boolean TargetShown { get; internal set; }
        public int TargetAreaWidth { get; internal set; }

        

        public string ShipSpeed
        {
            get
            {
                if (SelectedShip == null)
                    return "";
                return "Defunct";//Distance.AuToKm(Entity.GetVelocity_m(SelectedShip).ToString("N2");
            }
        }

        public string XSpeed
        {
            get
            {
                if (SelectedShip == null)
                    return "";
                return "Defunct";//Distance.AuToKm(SelectedShip.GetDataBlob<PropulsionAbilityDB>().CurrentVectorMS.X).ToString("N2");
            }
        }

        public string YSpeed
        {
            get
            {
                if (SelectedShip == null)
                    return "";
                return "Defunct";//Distance.AuToKm(SelectedShip.GetDataBlob<PropulsionAbilityDB>().CurrentVectorMS.Y).ToString("N2");
            }
        }

        public string XPos
        {
            get
            {
                if (SelectedShip == null)
                    return "";
                return SelectedShip.GetDataBlob<PositionDB>().X_AU.ToString("N5");
            }
        }

        public string YPos
        {
            get
            {
                if (SelectedShip == null)
                    return "";
                return SelectedShip.GetDataBlob<PositionDB>().Y_AU.ToString("N5");
            }
        }

        public string MaxSpeed
        {
            get
            {
                if (SelectedShip == null)
                    return "";
                return"Defunct";// SelectedShip.GetDataBlob<PropulsionAbilityDB>().MaximumSpeed_MS.ToString("N5");
            }
        }

        public string MoveTargetDistance
        {
            get
            {
                if (SelectedShip == null)
                    return "N/A";
                if (SelectedMoveTarget == null)
                    return "N/A";

                Vector3 delta = SelectedShip.GetDataBlob<PositionDB>().AbsolutePosition_AU - SelectedMoveTarget.GetDataBlob<PositionDB>().AbsolutePosition_AU;
                return Distance.AuToKm(delta.Length()).ToString("N2") ;
            }
        }

        private GameVM _gameVM;
        public GameVM GameVM { get { return _gameVM; } }

        public ShipOrderVM(GameVM game)
        {
            _gameVM = game;

            FactionInfoDB finfo = _gameVM.CurrentFaction.GetDataBlob<FactionInfoDB>();
            foreach (StarSystem starSystem in _gameVM.StarSystemSelectionViewModel.StarSystems.Keys)
            {
                if(finfo.KnownSystems.Contains(starSystem.Guid))
                {
                    _starSystems.Add(starSystem, starSystem.NameDB.GetName(_gameVM.CurrentFaction));
                }
            }

            _starSystems.SelectedIndex = 0;

            TargetShown = false;
            TargetAreaWidth = 2;

            RefreshShips(0, 0);

            //PropertyChanged += ShipOrderVM_PropertyChanged;
            SelectedSystem.ManagerSubpulses.SystemDateChangedEvent += UpdateInterface_SystemDateChangedEvent;

            _starSystems.SelectionChangedEvent += RefreshShips;
            _shipList.SelectionChangedEvent += RefreshOrders;
            _shipList.SelectionChangedEvent += RefreshFireControlList;
            _moveOrdersPossible.SelectionChangedEvent += RefreshTarget;
            _moveTargetList.SelectionChangedEvent += RefreshTargetDistance;
            _fireControlList.SelectionChangedEvent += RefreshBeamWeaponsList;
            _fireControlList.SelectionChangedEvent += RefreshFCTarget;

            OnPropertyChanged(nameof(StarSystems));
            OnPropertyChanged(nameof(SelectedSystem));
        }


        // Not 100% on events, but hopefully this will do
        public void UpdateInterface_SystemDateChangedEvent(DateTime newDate)
        {
            OnPropertyChanged(nameof(ShipSpeed));
            OnPropertyChanged(nameof(XSpeed));
            OnPropertyChanged(nameof(YSpeed));
            OnPropertyChanged(nameof(XPos));
            OnPropertyChanged(nameof(YPos));
            OnPropertyChanged(nameof(MaxSpeed));
            OnPropertyChanged(nameof(MoveTargetDistance));
            RefreshOrderList(0, 0);
            RefreshFireControlList(0, 0);
        }

        public static ShipOrderVM Create(GameVM game)
        {
            
            return new ShipOrderVM(game);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh(bool partialRefresh = false)
        {
            OnPropertyChanged(nameof(StarSystems));
            RefreshShips(0, 0);

        }

        private void StarSystems_SelectionChangedEvent(int oldSelection, int newSelection)
        {
            RefreshShips(0, 0);
        }

        // Updates the list of ships to give orders to and targets when the system changes
        public void RefreshShips(int a, int b)
        {
            if (SelectedSystem == null || _starSystems.SelectedIndex == -1)
                return;

            _shipList.Clear();
            foreach(Entity ship in SelectedSystem.GetAllEntitiesWithDataBlob<ShipInfoDB>(_gameVM.CurrentAuthToken))
            {
                if (ship.HasDataBlob<WarpAbilityDB>())
                    ShipList.Add(ship, ship.GetDataBlob<NameDB>().GetName(_gameVM.CurrentFaction));
            }

            _shipList.SelectedIndex = 0;

            //RefreshTarget(0, 0);

            OnPropertyChanged(nameof(ShipList));
            OnPropertyChanged(nameof(MoveTargetList));

            OnPropertyChanged(nameof(SelectedShip));
            OnPropertyChanged(nameof(SelectedMoveTarget));

            return;
        }

        public void RefreshTarget(int a, int b)
        {

            if (_starSystems.SelectedIndex == -1) //if b is not a valid selection
                return;

            _moveTargetList.Clear();
            _attackTargetList.Clear();

            int moveTargetIndex = _moveTargetList.SelectedIndex;
            int attackTargetIndex = _attackTargetList.SelectedIndex;

            foreach (Entity target in SelectedSystem.GetAllEntitiesWithDataBlob<PositionDB>(_gameVM.CurrentAuthToken))
            {
                if(target != SelectedShip)
                {
                    _moveTargetList.Add(target, target.GetDataBlob<NameDB>().GetName(_gameVM.CurrentFaction));
                    if (target.HasDataBlob<SensorProfileDB>())
                        _attackTargetList.Add(target, target.GetDataBlob<NameDB>().GetName(_gameVM.CurrentFaction));

                }
                    
            }

            _moveTargetList.SelectedIndex = moveTargetIndex;
            _attackTargetList.SelectedIndex = attackTargetIndex;

            if (SelectedPossibleMoveOrder == null)
                TargetShown = false;
            else if (SelectedPossibleMoveOrder.OrderType == orderType.MOVETO)
                TargetShown = true;
            else
                TargetShown = false;

            if (TargetShown)
                TargetAreaWidth = 200;
            else
                TargetAreaWidth = 2;

            OnPropertyChanged(nameof(TargetShown));
            OnPropertyChanged(nameof(TargetAreaWidth));
        }

        public void RefreshTargetDistance(int a, int b)
        {
            OnPropertyChanged(nameof(MoveTargetDistance));
        }

        public void RefreshFCTarget(int a, int b)
        {
            if (SelectedFireControl == null || _fireControlList.SelectedIndex == -1)
                return; 

            _targetedEntity = SelectedFireControl.GetAbilityState<FireControlAbilityState>().Target;
            OnPropertyChanged(TargetedEntity);
        }

        public void RefreshOrders(int a, int b)
        {
            if (SelectedShip == null)
                return;

            _moveOrdersPossible.Clear();

            //if (SelectedShip.HasDataBlob<PropulsionDB>())
            //    _moveOrdersPossible.Add(new MoveOrder(), "Move to");

            _moveOrdersPossible.SelectedIndex = 0;

            RefreshOrderList(0, 0);



            OnPropertyChanged(nameof(SelectedMoveOrder));
            OnPropertyChanged(nameof(SelectedPossibleMoveOrder));

            OnPropertyChanged(nameof(ShipSpeed));
            OnPropertyChanged(nameof(XSpeed));
            OnPropertyChanged(nameof(YSpeed));
            OnPropertyChanged(nameof(XPos));
            OnPropertyChanged(nameof(YPos));
            OnPropertyChanged(nameof(MaxSpeed));

            return;
        }

        public void RefreshOrderList(int a, int b)
        {
            if (SelectedShip == null)
                return;
            List<BaseOrder> orders = new List<BaseOrder>();//(SelectedShip.GetDataBlob<ShipInfoDB>().Orders);

            _moveOrderList.Clear();

            foreach (BaseOrder order in orders)
            {
                string orderDescription = "";

                switch (order.OrderType)
                {
                    case orderType.MOVETO:
                        //MoveOrder moveOrder = (MoveOrder)order;
                        orderDescription += "Move to ";
                        //orderDescription += moveOrder.Target.GetDataBlob<NameDB>().GetName(_gameVM.CurrentFaction);
                        break;
                    default:
                        break;
                }
                _moveOrderList.Add(order, orderDescription);
            }

            OnPropertyChanged(nameof(MoveOrderList));
            OnPropertyChanged(nameof(MoveOrdersPossible));
        }

        public void RefreshFireControlList(int a, int b)
        {
            _fireControlList.Clear();

            if (SelectedShip == null)
                return;

            if (!SelectedShip.HasDataBlob<BeamWeaponsDB>())
            {
                _fireControlList.Clear();
                return;
            }



        
            var instanceDB = SelectedShip.GetDataBlob<ComponentInstancesDB>();
            if (instanceDB.TryGetComponentsByAttribute<BeamFireControlAtbDB>(out var fcList))
            {
                int fcCount = 0;
                foreach (var item in fcList)
                {
                    fcCount++;
                    _fireControlList.Add(item, item.GetName());
                }
            }

            //List<KeyValuePair<Entity, List<Entity>>> fcList = EntityStoreHelpers.GetComponentsOfType<BeamFireControlAtbDB>(instanceDB.SpecificInstances);
            //new List<KeyValuePair<Entity, List<Entity>>>(instanceDB.SpecificInstances.ToDictionary().Where(item => item.Key.HasDataBlob<BeamFireControlAtbDB>()).ToList());
            /*
            foreach (KeyValuePair<Entity, List<Entity>> kvp in fcList)
            {
                int fcCount = 0;
                if (kvp.Key.HasDataBlob<BeamFireControlAtbDB>())
                foreach(Entity instance in kvp.Value)
                {
                    fcCount++;
                    _fireControlList.Add(instance, kvp.Key.GetDataBlob<NameDB>().DefaultName + fcCount);
                }
                        
                
            }
*/
            _fireControlList.SelectedIndex = 0;

            

            RefreshBeamWeaponsList(0, 0);

//            OnPropertyChanged(nameof(FireControlList));

        }

        public void RefreshBeamWeaponsList(int a, int b)
        {
            _attachedBeamList.Clear();
            _freeBeamList.Clear();

            if (SelectedShip == null || _shipList.SelectedIndex == -1)
                return;

            if (_fireControlList.Count > 0 && _fireControlList.SelectedIndex != -1)
            {
                int beamCount = 0;
                foreach (ComponentInstance beam in SelectedFireControl.GetAbilityState<FireControlAbilityState>().AssignedWeapons)
                {
                    beamCount++;
                    _attachedBeamList.Add(beam, beam.GetName());
                }

            }
            else
                _attachedBeamList.Clear();
            var instancesDB = SelectedShip.GetDataBlob<ComponentInstancesDB>();

            var designs = instancesDB.GetDesignsByType(typeof(BeamWeaponAtbDB));
            _freeBeamList.Clear();
            foreach (var design in designs)
            {
                foreach (var instance in instancesDB.GetComponentsBySpecificDesign(design.Guid))
                {
                    int beamCount = 0;
                    if (instance.GetAbilityState<WeaponState>().FireControl == null)
                        _freeBeamList.Add(new KeyValuePair<ComponentInstance, string>(instance, instance.GetName()));

                }
            }


            /*
            List<KeyValuePair<Entity, List<Entity>>> beamList = EntityStoreHelpers.GetComponentsOfType<BeamWeaponAtbDB>(instancesDB.SpecificInstances);
            beamList.AddRange(EntityStoreHelpers.GetComponentsOfType<SimpleBeamWeaponAtbDB>(instancesDB.SpecificInstances));
            //new List<KeyValuePair<Entity, List<Entity>>>(SelectedShip.GetDataBlob<ComponentInstancesDB>().SpecificInstances.Where(item => item.Key.HasDataBlob<BeamWeaponAtbDB>() || item.Key.HasDataBlob<SimpleBeamWeaponAtbDB>()).ToList());

            bool isBeamControlled = false;
            */


            // Get a list of all beam weapons not currently controlled by a fire control
            // @todo: make sure you check all fire controls - currently only lists
            // beams not set to the current fire control
            /*
            foreach (KeyValuePair<Entity, List<Entity>> kvp in beamList)
            {
                int beamCount = 0;
                foreach (Entity instance in kvp.Value)
                {
                    if (instance.GetDataBlob<WeaponStateDB>().FireControl == null)
                        _freeBeamList.Add(new KeyValuePair<Entity, string>(instance, kvp.Key.GetDataBlob<NameDB>().DefaultName + " " + ++beamCount));

                }
            }*/

            OnPropertyChanged(nameof(AttachedBeamList));
            OnPropertyChanged(nameof(FreeBeamList));

        }

        private bool IsBeamInFireControlList(ComponentInstance beam)
        {
            if (SelectedFireControl == null || _fireControlList.SelectedIndex == -1)
                return false;

            var instancesDB = SelectedShip.GetDataBlob<ComponentInstancesDB>();

            var designs = instancesDB.GetDesignsByType(typeof(BeamFireControlAtbDB));

            foreach (var design in designs)
            {
                foreach (var fc in instancesDB.GetComponentsBySpecificDesign(design.Guid))
                {
                    if (fc.GetAbilityState<FireControlAbilityState>().AssignedWeapons.Contains(beam))
                        return true;
                }

            }



            //List<KeyValuePair<Entity, List<Entity>>> fcList = EntityStoreHelpers.GetComponentsOfType<BeamFireControlAtbDB>(instancesDB.SpecificInstances);
            //new List<KeyValuePair<Entity, List<Entity>>>(SelectedShip.GetDataBlob<ComponentInstancesDB>().SpecificInstances.Where(item => item.Key.HasDataBlob<BeamFireControlAtbDB>()).ToList());
            /*
            foreach (KeyValuePair<Entity, List<Entity>> kvp in fcList)
            {
                foreach (Entity instance in kvp.Value)
                {
                    if (SelectedFireControl.GetDataBlob<FireControlInstanceAbilityDB>().AssignedWeapons.Contains(beam))
                        return true;
                }
            }
            */
            return false;
        }


        public void OnAddOrder()
        {
            // Check if Ship, Target, and Order are set
            if (SelectedShip == null  || SelectedMoveTarget == null || SelectedPossibleMoveOrder == null) 
                return;
            switch(SelectedPossibleMoveOrder.OrderType)
            {
                case orderType.MOVETO:
                    //_gameVM.CurrentPlayer.Orders.MoveOrder(SelectedShip, SelectedMoveTarget);
                    break;
                case orderType.INVALIDORDER:
                    break;
                default:
                    break;
            }

            //_gameVM.CurrentPlayer.ProcessOrders();

            RefreshOrders(0,0);
            
        }

        public void OnRemoveOrder()
        {


            if (SelectedShip == null)
                return;

            BaseOrder nextOrder;
            //Queue<BaseOrder> orderList = SelectedShip.GetDataBlob<ShipInfoDB>().Orders;

            /*
            int totalOrders = orderList.Count;

            for (int i = 0; i < totalOrders; i++)
            {
                nextOrder = orderList.Dequeue();
                if(nextOrder != SelectedMoveOrder)
                    orderList.Enqueue(nextOrder);
            }
*/
            
            RefreshOrders(0,0);
        }

        public void OnAddBeam()
        {
            ComponentInstance beam = SelectedFreeBeam;

            if (SelectedFireControl == null || _fireControlList.SelectedIndex == -1)
                return;

            if (SelectedFreeBeam == null || _freeBeamList.SelectedIndex == -1)
                return;

            List<ComponentInstance> weaponList = SelectedFireControl.GetAbilityState<FireControlAbilityState>().AssignedWeapons;
            weaponList.Add(SelectedFreeBeam);

            // @todo: set the fire control for the beam
            beam.GetAbilityState<WeaponState>().FireControl = SelectedFireControl;

            RefreshBeamWeaponsList(0, 0);
        }

        public void OnRemoveBeam()
        {
            ComponentInstance beam = SelectedAttachedBeam;


            if (SelectedFireControl == null || _fireControlList.SelectedIndex == -1)
                return;

            if (SelectedAttachedBeam == null || _attachedBeamList.SelectedIndex == -1)
                return;

            List<ComponentInstance> weaponList = SelectedFireControl.GetAbilityState<FireControlAbilityState>().AssignedWeapons;
            weaponList.Remove(SelectedAttachedBeam);

            // @todo: unset the fire control for the beam

            beam.GetAbilityState<WeaponState>().FireControl = null;

            RefreshBeamWeaponsList(0, 0);
        }

        public void OnAddTarget()
        {
            ComponentInstance fc = SelectedFireControl;
            Entity target = SelectedAttackTarget;

            if (SelectedFireControl == null || _fireControlList.SelectedIndex == -1)
                return;

            if (SelectedAttackTarget == null || _attackTargetList.SelectedIndex == -1)
                return;

            fc.GetAbilityState<FireControlAbilityState>().Target = target;
            // Get the currently selected ship and fire control and the currently selected list of targets
            // Add the currently selected target to the selected ship's target
            // Update GUI

            RefreshFireControlList(0, 0);
        }

        public void OnRemoveTarget()
        {
            // Get the currently selected ship fire control
            // Clear its selected target
            // Update GUI

        }

        private ICommand _addOrder;
        public ICommand AddOrder
        {
            get
            {
                return _addOrder ?? (_addOrder = new CommandHandler(OnAddOrder, true));
            }
        }

        private ICommand _removeOrder;
        public ICommand RemoveOrder
        {
            get
            {
                return _removeOrder ?? (_removeOrder = new CommandHandler(OnRemoveOrder, true));
            }
        }

        private ICommand _addBeam;
        public ICommand AddBeam
        {
            get
            {
                return _addBeam ?? (_addBeam = new CommandHandler(OnAddBeam, true));
            }
        }

        private ICommand _removeBeam;
        public ICommand RemoveBeam
        {
            get
            {
                return _removeBeam ?? (_removeBeam = new CommandHandler(OnRemoveBeam, true));
            }
        }

        private ICommand _addTarget;
        public ICommand AddTarget
        {
            get
            {
                return _addTarget ?? (_addTarget = new CommandHandler(OnAddTarget, true));
            }
        }

        private ICommand _removeTarget;
        public ICommand RemoveTarget
        {
            get
            {
                return _removeTarget ?? (_removeTarget = new CommandHandler(OnRemoveTarget, true));
            }
        }

    }
}
