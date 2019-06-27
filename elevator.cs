/*
Elevator (V1.0, 2019-06-27)
This script was written as a solution to a simple elevator setup with only 2 floors. By default
the distance the piston travels is 4 large grid blocks tall, but you could manually set that value
to what ever you'd prefer. The elevator doors are optional and can be disabled in the configuration.

This script does not support multi floor stops and does not support multi piston setups.

[ Setup ]

This setup involves the following components:
* 1 Piston
* 2 Sensors
* 1 Programming block
* 2 Doors (optional)

Luckily the build instructions can be represented by a simple 2d ASCII diagram. The diagram
shows the piston fully extended at 10 meters or 4 large grid blocks.

[ Design ]

NOTE: The ascii should only be viewed in the programming blocks interface.

PP - piston
__ - piston top fully extended
|| - piston shaft piece
AB - any block / armor block
SS - sensor shaft 
SB - sensor bottom
ED - elevator door (optional)

ED __
      | |
      | |
      | | 
      | |  ED
      PP ZZ
      SS SB

The shaft sensor (SS) will need to detect every block in the shaft of the elevator reaching
just above the piston top (__). The bottom sensor (SB) will only need to reach slightly above the
armor block to detect the person wanting to call the elevator down. The elevator doors are
optional, but prevent other players from falling into the elevator shaft.
*/ 

// [Configuration]
const string LcdTopName = "SGM1 - Corner LCD Flat Top - Elevator Top";

const string LcdBottomName = "SGM1 - Corner LCD Flat Top - Elevator Bottom";

const string PistonElevatorName = "SGM1 - Piston Elevator Maintenance";

const string ElevatorSensorShaftName = "SGM1 - Sensor - Elevator - Top";
const string ElevatorSensorBottomName = "SGM1 - Sensor - Elevator - Bottom";

const string ElevatorDoorTopName = "SGM1 - Door (Maintenance Top)";
const string ElevatorDoorBottomName = "SGM1 - Door (Maintenance Bottom)";

const byte ElevatorMaxDistance = 10;
const byte ElevatorMinDistance = 0;

const bool UseElevatorDoors = true;

// If debug is set to true, another LCD display will be avaliable to play with the variables of this script. 
const bool Debug = false;
const string LcdDebugName = "lcddebug";

/* END OF CONFIG */


string elevator_status;
string elevatorError;
bool shaftSensorTriggered;
bool bottomSensorTriggered;
public Program()
{
  Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main()
{
  // internal variables
  var lcdTop = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(LcdTopName);
  var lcdBottom = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(LcdBottomName);

  var pistonElevator = (IMyPistonBase) GridTerminalSystem.GetBlockWithName(PistonElevatorName);

  var elevatorSensorShaft = (IMySensorBlock) GridTerminalSystem.GetBlockWithName(ElevatorSensorShaftName);
  var elevatorSensorBottom = (IMySensorBlock) GridTerminalSystem.GetBlockWithName(ElevatorSensorBottomName);

  var elevatorDoorTop = (IMyDoor) GridTerminalSystem.GetBlockWithName(ElevatorDoorTopName);
  var elevatorDoorBottom = (IMyDoor) GridTerminalSystem.GetBlockWithName(ElevatorDoorBottomName);

  var elevatorPosition = float.Parse(pistonElevator.DetailedInfo.Split(' ','\n')[3].TrimEnd('m'));

  // elevator helper functions
  string ExtendElevator() 
  {   
    try
    {
    pistonElevator.ApplyAction("Extend");
    return elevator_status = "Raising";
    }
    catch (Exception)
    {
      return elevatorError = "An error occured when extending the elevator.";
    }
  }

  string RetractElevator()
  {
    try
    {
    pistonElevator.ApplyAction("Retract");
    return elevator_status = "Lowering";
    }
    catch (Exception)
    {
      return elevatorError = "An error occured when retracting the elevator.";
    }
  }

  bool IsElevatorEmpty()
  {
    if (!elevatorSensorBottom.IsActive && !elevatorSensorShaft.IsActive && (elevatorPosition == ElevatorMaxDistance || elevatorPosition == ElevatorMinDistance))
    {
      shaftSensorTriggered = false;
      bottomSensorTriggered = false;
      if (elevatorPosition == 0)
      {
        ExtendElevator();
      }
      return true;
    }
    return false;
  }

  bool IsElevatorIdle() 
  {
    if (elevatorPosition == ElevatorMaxDistance || elevatorPosition == ElevatorMinDistance)
    {
      elevator_status = "Idle";
      return true;
    }
    return false;
  }

  // elevator logic
  if (elevatorSensorShaft.IsActive && !bottomSensorTriggered)
  {
    shaftSensorTriggered = true;
    RetractElevator();
  }

  if (elevatorSensorBottom.IsActive && !elevatorSensorShaft.IsActive)
  {
    bottomSensorTriggered = true;
    RetractElevator();
  }
  
  if (bottomSensorTriggered && elevatorSensorShaft.IsActive)
  {
    shaftSensorTriggered = true;
    ExtendElevator();
  }

  IsElevatorIdle();
  IsElevatorEmpty();
  
  // elevator door logic
  if (UseElevatorDoors)
  {
    if (elevatorPosition == ElevatorMaxDistance)
    {
      elevatorDoorTop.ApplyAction("Open_On");
      elevatorDoorBottom.ApplyAction("Open_Off");
    }
    else if (elevatorPosition == ElevatorMinDistance)
    {
      elevatorDoorTop.ApplyAction("Open_Off");
      elevatorDoorBottom.ApplyAction("Open_On");
    }
    else
    {
      elevatorDoorTop.ApplyAction("Open_Off");
      elevatorDoorBottom.ApplyAction("Open_Off");
    }
  }

  // LCD code
  lcdTop.ContentType=ContentType.TEXT_AND_IMAGE;
  lcdTop.ContentType=ContentType.TEXT_AND_IMAGE;
  lcdTop.FontSize = 2;

  lcdBottom.ContentType=ContentType.TEXT_AND_IMAGE;
  lcdBottom.ContentType=ContentType.TEXT_AND_IMAGE;
  lcdBottom.FontSize = 2;

  IMyTextSurface programmable_block_lcd=Me.GetSurface(0);
  
  programmable_block_lcd.ContentType=ContentType.TEXT_AND_IMAGE;
  programmable_block_lcd.FontSize = 0.75f;
  programmable_block_lcd.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;

  // string piston_animation_vertical ;

  string flavor_text = @"\bin\warez\piston\piston_elevator.cs"+"\npiston_elevator: Running... Piston Elevator\n";
  string stats = "\n\nElevator Status: "+elevator_status+"\nPiston Position: "+elevatorPosition+"m"+"\nTop Elevator Sensor: "+elevatorSensorShaft.IsActive.ToString()+"\nBottom Elevator Sensor: "+elevatorSensorBottom.IsActive.ToString();
  
  programmable_block_lcd.WriteText(flavor_text+stats);
  
  lcdTop.SetValue( "alignment", (Int64)2 );
  lcdBottom.SetValue( "alignment", (Int64)2 );
  lcdTop.WriteText(elevator_status.ToString());
  lcdBottom.WriteText(elevator_status.ToString());
  
  // Echo text
  Echo("elevatorPosition: "+elevatorPosition+"\nelevator_status: "+elevator_status+"\n\nelevatorSensorShaft.Enabled: "+elevatorSensorShaft.Enabled.ToString()+"\nelevatorSensorBottom.Enabled: "+elevatorSensorBottom.Enabled.ToString()+"\n\nelevatorSensorShaft.IsActive: "+elevatorSensorShaft.IsActive.ToString()+"\nelevatorSensorBottom.IsActive: "+elevatorSensorBottom.IsActive.ToString()+"\n\nshaftSensorTriggered: "+shaftSensorTriggered+"\nbottomSensorTriggered: "+bottomSensorTriggered.ToString());
  Echo("elevatorError: "+elevatorError);

  // Debug text
  if (Debug)
  {
    var LcdDebug = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(LcdDebugName);
    LcdDebug.WriteText("elevator_status: "+elevator_status+"\n\nelevatorSensorShaft.Enabled: "+elevatorSensorShaft.Enabled.ToString()+"\nelevatorSensorBottom.Enabled: "+elevatorSensorBottom.Enabled.ToString()+"\n\nelevatorSensorShaft.IsActive: "+elevatorSensorShaft.IsActive.ToString()+"\nelevatorSensorBottom.IsActive: "+elevatorSensorBottom.IsActive.ToString()+"\n\nshaftSensorTriggered: "+shaftSensorTriggered+"\nbottomSensorTriggered: "+bottomSensorTriggered.ToString());
  }
}