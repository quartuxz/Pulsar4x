﻿using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Pulsar4X.ECSLib;
using Pulsar4X.ImGuiNetUI.EntityManagement;

namespace Pulsar4X.SDL2UI
{
    public class EntityInfoPanel : PulsarGuiWindow
    {
	    private EntityInfoPanel()
	    {
	        _flags =  ImGuiWindowFlags.NoCollapse;
        
        }

        internal static EntityInfoPanel GetInstance() {

            EntityInfoPanel thisItem;
            if (!_state.LoadedWindows.ContainsKey(typeof(EntityInfoPanel)))
            {
                thisItem = new EntityInfoPanel();
            }
            else
            {
                thisItem = (EntityInfoPanel)_state.LoadedWindows[typeof(EntityInfoPanel)];
            }
             

            return thisItem;


        }
        //displays selected entity info
        internal override void Display()
        {
            ImGui.SetNextWindowSize(new Vector2(175, 225), ImGuiCond.Once);
            if (ImGui.Begin("Currently selected", _flags))
            {

                
                

                if(ImGui.Button("see all small bodies")){
                        SmallBodyEntityInfoPanel.GetInstance().SetActive();
                }

                if(_state.LastClickedEntity != null && _state.StarSystemStates.ContainsKey(_state.SelectedStarSysGuid)){

                    if (_state.PrimaryEntity != null)
                    {
                        ImGui.Text("Primary: " + _state.PrimaryEntity.Name);
                    }
                    else
                    {
                        ImGui.Text("(Select primary...)");
                    }

                    ImGui.Text("Subject: "+_state.LastClickedEntity.Name);
                    
                    //ImGui.Text(""+_state.LastClickedEntity.);
                    //gets all children and parent nodes, displays their names and makes them clickable to navigate towards them.

                    

                    if(_state.LastClickedEntity.Entity.HasDataBlob<PositionDB>()){
                        ImGui.Text("Parent entity: ");

                        var parentEntity = _state.LastClickedEntity.Entity.GetDataBlob<PositionDB>().Parent;
                        bool hasParentEntity = false;
                        if(parentEntity != null){
                            //checks if parent exists in the selected star system and has a name
                            //notice that parent can be any bodyType(ex. asteroid, comet, planet etc), unlike childrenEntities, which are more selectively displayed...
                            if(_state.StarSystemStates[_state.SelectedStarSysGuid].EntityStatesWithNames.ContainsKey(parentEntity.Guid)){
                                var tempEntityState = _state.StarSystemStates[_state.SelectedStarSysGuid].EntityStatesWithNames[parentEntity.Guid];
                                hasParentEntity = true;
                                if(ImGui.SmallButton(tempEntityState.Name)){
                                //if(ImGui.SmallButton(parentEntity.GetDataBlob<NameDB>().GetName(_state.Faction.Guid))){
                                    _state.EntityClicked(parentEntity.Guid, _state.SelectedStarSysGuid, MouseButtons.Primary);
                                //}
                                }
                            }
                        }
                        if(!hasParentEntity){
                            ImGui.Text("(...No parent entity)");
                        }
                        bool hasChildrenEntities = false;
                        ImGui.Text("Children entities: ");
                        foreach(var childEntity in _state.LastClickedEntity.Entity.GetDataBlob<PositionDB>().Children){
                            //checks if child exists in the seclted star system and has name
                            if(_state.StarSystemStates[_state.SelectedStarSysGuid].EntityStatesWithNames.ContainsKey(childEntity.Guid)){
                                var tempEntityState = _state.StarSystemStates[_state.SelectedStarSysGuid].EntityStatesWithNames[childEntity.Guid];
                                //only show child entities that arent comets or asteroids if the lastClickedEntity(parent entity) isnt either, if LastClickedEntity(parent entity) is either, then show them always
                                if(_state.LastClickedEntity.BodyType == UserOrbitSettings.OrbitBodyType.Asteroid || _state.LastClickedEntity.BodyType == UserOrbitSettings.OrbitBodyType.Comet ||(tempEntityState.BodyType != UserOrbitSettings.OrbitBodyType.Asteroid && tempEntityState.BodyType != UserOrbitSettings.OrbitBodyType.Comet)){
                                    hasChildrenEntities = true;
                                    if(ImGui.SmallButton(tempEntityState.Name)){
                                        _state.EntityClicked(childEntity.Guid, _state.SelectedStarSysGuid, MouseButtons.Primary);
                                    }
                                }

                            }
                        }
                        if(!hasChildrenEntities){
                            ImGui.Text("(...No children entities)");
                        }
                    }
                }else
                {

                    ImGui.Text("(select subject...)");
                }
                ImGui.End();
            }
        }
    }
}
