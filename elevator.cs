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
string lcd_top_name = "SGM1 - Corner LCD Flat Top - Elevator Top";

string lcd_bottom_name = "SGM1 - Corner LCD Flat Top - Elevator Bottom";

string piston_elevator_name = "SGM1 - Piston Elevator Maintenance";

string elevator_sensor_shaft_name = "SGM1 - Sensor - Elevator - Top";
string elevator_sensor_bottom_name = "SGM1 - Sensor - Elevator - Bottom";

string elevator_door_top_name = "SGM1 - Door (Maintenance Top)";
string elevator_door_bottom_name = "SGM1 - Door (Maintenance Bottom)";

ushort elevator_max_distance = 10;
ushort elevator_min_distance = 0;

bool use_elevator_doors = true;

// If debug is set to true, another LCD display will be avaliable to play with the variables of this script. 
bool debug = false;
string lcd_debug_name = "lcddebug";

/* END OF CONFIG */


string elevator_status;

bool shaft_sensor_triggered;
bool bottom_sensor_triggered;
public Program()
{
  Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main()
{
  // internal variables
  var lcd_top = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(lcd_top_name);
  var lcd_bottom = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(lcd_bottom_name);

  var piston_elevator = (IMyPistonBase) GridTerminalSystem.GetBlockWithName(piston_elevator_name);

  var elevator_sensor_shaft = (IMySensorBlock) GridTerminalSystem.GetBlockWithName(elevator_sensor_shaft_name);
  var elevator_sensor_bottom = (IMySensorBlock) GridTerminalSystem.GetBlockWithName(elevator_sensor_bottom_name);

  var elevator_door_top = (IMyDoor) GridTerminalSystem.GetBlockWithName(elevator_door_top_name);
  var elevator_door_bottom = (IMyDoor) GridTerminalSystem.GetBlockWithName(elevator_door_bottom_name);

  var elevator_position_float = float.Parse(piston_elevator.DetailedInfo.Split(' ','\n')[3].TrimEnd('m'));

  // elevator helper functions
  string ExtendElevator() 
  {
    piston_elevator.ApplyAction("Extend");
    return elevator_status = "Raising";
  }
  
  string RetractElevator()
  {
    piston_elevator.ApplyAction("Retract");
    return elevator_status = "Lowering";
  }

  bool IsElevatorEmpty()
  {
    if (!elevator_sensor_bottom.IsActive && !elevator_sensor_shaft.IsActive && (elevator_position_float == elevator_max_distance || elevator_position_float == elevator_min_distance))
    {
      shaft_sensor_triggered = false;
      bottom_sensor_triggered = false;
      if (elevator_position_float == 0)
      {
        ExtendElevator();
      }
      return true;
    }
    return false;
  }

  bool IsElevatorIdle() 
  {
    if (elevator_position_float == elevator_max_distance || elevator_position_float == elevator_min_distance)
    {
      elevator_status = "Idle";
      return true;
    }
    return false;
  }

  // elevator logic
  if (elevator_sensor_shaft.IsActive && !bottom_sensor_triggered)
  {
    shaft_sensor_triggered = true;
    RetractElevator();
  }

  if (elevator_sensor_bottom.IsActive && !elevator_sensor_shaft.IsActive)
  {
    bottom_sensor_triggered = true;
    RetractElevator();
  }
  
  if (bottom_sensor_triggered && elevator_sensor_shaft.IsActive)
  {
    shaft_sensor_triggered = true;
    ExtendElevator();
  }

  IsElevatorIdle();
  IsElevatorEmpty();
  
  // elevator door logic
  if (use_elevator_doors)
  {
    if (elevator_position_float == elevator_max_distance)
    {
      elevator_door_top.ApplyAction("Open_On");
      elevator_door_bottom.ApplyAction("Open_Off");
    }
    else if (elevator_position_float == elevator_min_distance)
    {
      elevator_door_top.ApplyAction("Open_Off");
      elevator_door_bottom.ApplyAction("Open_On");
    }
    else
    {
      elevator_door_top.ApplyAction("Open_Off");
      elevator_door_bottom.ApplyAction("Open_Off");
    }
  }

  // LCD code
  lcd_top.ContentType=ContentType.TEXT_AND_IMAGE;
  lcd_top.ContentType=ContentType.TEXT_AND_IMAGE;
  lcd_top.FontSize = 2;

  lcd_bottom.ContentType=ContentType.TEXT_AND_IMAGE;
  lcd_bottom.ContentType=ContentType.TEXT_AND_IMAGE;
  lcd_bottom.FontSize = 2;

  IMyTextSurface programmable_block_lcd=Me.GetSurface(0);
  
  programmable_block_lcd.ContentType=ContentType.TEXT_AND_IMAGE;
  programmable_block_lcd.FontSize = 0.75f;
  programmable_block_lcd.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;

  // string piston_animation_vertical ;

  string flavor_text = @"\bin\warez\piston\piston_elevator.cs"+"\npiston_elevator: Running... Piston Elevator\n";
  string stats = "\n\nElevator Status: "+elevator_status+"\nPiston Position: "+elevator_position_float+"m"+"\nTop Elevator Sensor: "+elevator_sensor_shaft.IsActive.ToString()+"\nBottom Elevator Sensor: "+elevator_sensor_bottom.IsActive.ToString();
  
  programmable_block_lcd.WriteText(flavor_text+stats);
  
  lcd_top.SetValue( "alignment", (Int64)2 );
  lcd_bottom.SetValue( "alignment", (Int64)2 );
  lcd_top.WriteText(elevator_status.ToString());
  lcd_bottom.WriteText(elevator_status.ToString());
  
  // Echo text
  Echo("elevator_position_float: "+elevator_position_float+"\nelevator_status: "+elevator_status+"\n\nelevator_sensor_shaft.Enabled: "+elevator_sensor_shaft.Enabled.ToString()+"\nelevator_sensor_bottom.Enabled: "+elevator_sensor_bottom.Enabled.ToString()+"\n\nelevator_sensor_shaft.IsActive: "+elevator_sensor_shaft.IsActive.ToString()+"\nelevator_sensor_bottom.IsActive: "+elevator_sensor_bottom.IsActive.ToString()+"\n\nshaft_sensor_triggered: "+shaft_sensor_triggered+"\nbottom_sensor_triggered: "+bottom_sensor_triggered.ToString());

  // Debug text
  if (debug)
  {
    var lcd_debug = (IMyTextPanel) GridTerminalSystem.GetBlockWithName(lcd_debug_name);
    lcd_debug.WriteText("elevator_status: "+elevator_status+"\n\nelevator_sensor_shaft.Enabled: "+elevator_sensor_shaft.Enabled.ToString()+"\nelevator_sensor_bottom.Enabled: "+elevator_sensor_bottom.Enabled.ToString()+"\n\nelevator_sensor_shaft.IsActive: "+elevator_sensor_shaft.IsActive.ToString()+"\nelevator_sensor_bottom.IsActive: "+elevator_sensor_bottom.IsActive.ToString()+"\n\nshaft_sensor_triggered: "+shaft_sensor_triggered+"\nbottom_sensor_triggered: "+bottom_sensor_triggered.ToString());
  }
}