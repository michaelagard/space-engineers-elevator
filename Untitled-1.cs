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

// try {
//   var lcdTop = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(LcdTopName);
// }
// catch (Exception e)
// {

// }
// try {
// var lcdBottom = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(LcdBottomName);
// }
// catch (Exception e)
// {

// }
// try { var pistonElevator = (IMyPistonBase) GridTerminalSystem.GetBlockWithName(PistonElevatorName); } catch (Exception e) {};
// try { var elevatorSensorShaft = (IMySensorBlock) GridTerminalSystem.GetBlockWithName(ElevatorSensorShaftName); } catch (Exception e) {};
// try { var elevatorSensorBottom = (IMySensorBlock) GridTerminalSystem.GetBlockWithName(ElevatorSensorBottomName); } catch (Exception e) {};
// try { var elevatorDoorTop = (IMyDoor) GridTerminalSystem.GetBlockWithName(ElevatorDoorTopName); } catch (Exception e) {};


public Program()
{
  Runtime.UpdateFrequency = UpdateFrequency.Update10;
  try { var elevatorDoorBottom = (IMyDoor) GridTerminalSystem.GetBlockWithName(ElevatorDoorBottomName); } catch (Exception e) {Echo("Did this work?");}
}

public void Main()
{
  // internal variables

  

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
  
  try {IsElevatorIdle();} catch (Exception) {elevatorError = "An error occured while checking if the elevator is idle.";}
  try {IsElevatorEmpty();} catch (Exception) {elevatorError = "An error occured while checking if the elevator is empty.";}
  
  // elevator door logic
  try
  {
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
  }
  catch (Exception)
  {
    elevatorError = "An error occured in the elevator door logic.";
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
  if (elevatorError != null)
  {
    Echo("elevatorError: "+elevatorError);
  }

  // Debug text
  if (Debug)
  {
    var LcdDebug = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(LcdDebugName);
    LcdDebug.WriteText("elevator_status: "+elevator_status+"\n\nelevatorSensorShaft.Enabled: "+elevatorSensorShaft.Enabled.ToString()+"\nelevatorSensorBottom.Enabled: "+elevatorSensorBottom.Enabled.ToString()+"\n\nelevatorSensorShaft.IsActive: "+elevatorSensorShaft.IsActive.ToString()+"\nelevatorSensorBottom.IsActive: "+elevatorSensorBottom.IsActive.ToString()+"\n\nshaftSensorTriggered: "+shaftSensorTriggered+"\nbottomSensorTriggered: "+bottomSensorTriggered.ToString());
  }
}