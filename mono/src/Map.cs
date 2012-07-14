// -*- mode: csharp; indent-tabs-mode: nil; c-basic-offset: 2; -*-

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace ICFPC {

public enum Item {
  Robot = 'R',
  Empty = ' ',
  Earth = '.',
  Wall = '#',
  Rock = '*',
  Lambda = '\\',
  ClosedLift = 'L',
  OpenLift = 'O',  
  
  InA = 'A',
  InB = 'B',
  InC = 'C',
  InD = 'D',
  InE = 'E',
  InF = 'F',
  InG = 'G',
  InH = 'H',
  InI = 'I',

  Out1 = '1',
  Out2 = '2',
  Out3 = '3',
  Out4 = '4',
  Out5 = '5',
  Out6 = '6',
  Out7 = '7',
  Out8 = '8',
  Out9 = '9',
}

public enum Command {
  Left = 'L',
  Right = 'R',
  Up = 'U',
  Down = 'D',
  Wait = 'W',
  Abort = 'A',
}

public enum RobotState {
  Mining,
  Abort,
  Finish,
  Smashed,
  Sank,
}

[Serializable]
public class Map {
  private static Map first = null;
  private static Map second = null;

  private Item[,] state = null;

  public static Map Read(TextReader src) {
    var lineMap = new List<string>();
    var m = 0;
    var n = 0;


    while(src.Peek() >= 0) {
      var nextLine = src.ReadLine();
      if(nextLine == "") {
        break; // End of map loading, only metadata left
      }

      n = Math.Max(n, nextLine.Length);
      lineMap.Add(nextLine);
      m++;
    }
      
    var retVal = new Map(m,n);

    for(var i = 0; i < m; i++) {
      for(var j = 0; j < n; j++) {
        retVal.state[i,j] = Item.Empty;
      }
    }

    var inOutMap = new Dictionary<Item, Point>();
    for(var i = lineMap.Count - 1; i >= 0; i--) {
      var line = lineMap[i];
      for(var j = 0; j < line.Length; j++) {
        var x = j; var y = lineMap.Count - 1 - i;
        var item = (Item)(line[j]);
        retVal[x,y] = item;
        if(item == Item.Robot) {
          retVal.X = x;
          retVal.Y = y;
        }
        else if(item == Item.Lambda) {
          retVal.LambdasLeft++;
        }
        else if(item.IsIn() || item.IsOut()) {
          inOutMap[item] = new Point(x, y);
        }
      }
    }

    // Now read the metadata
    while(src.Peek() >= 0) {
      var dataStr = src.ReadLine();
      var data = dataStr.Split(' ', '\t');
      if(data[0] == "Water ") {
        // Console.Error.WriteLine("Water tag found!");
        retVal.Water = int.Parse(data[1]);
        continue;
      }
      else if(data[0] == "Flooding ") {
        // Console.Error.WriteLine("Flooding tag found!");
        retVal.Flooding = int.Parse(data[1]);
        continue;
      }
      else if(data[0] == "Waterproof") {
        // Console.Error.WriteLine("Waterproof tag found!");
        retVal.Waterproof = int.Parse(data[1]);
        continue;
      }
      else if(data[0] == "Trampoline") {
        var inp = (Item)data[1][0]; var inPoint = inOutMap[inp];
        var outp = (Item)data[3][0]; var outPoint = inOutMap[outp];

        retVal.LinkedOutPoints[inp] = new KeyValuePair<Point, List<Point>>(inOutMap[outp], 
                                                                           new List<Point>( new [] { inOutMap[inp], } )); 

        foreach(var regInp in retVal.LinkedOutPoints) {
          if(regInp.Key == inp) continue; // Skip self;

          if(regInp.Value.Key == outPoint) {
            regInp.Value.Value.Add(inPoint);
            retVal.LinkedOutPoints[inp].Value.Add(inOutMap[regInp.Key]);
          }
        }
      }
    }
    
    Map.first = retVal;
    Map.second = retVal.Clone();

    System.GC.Collect(0); // SPIKE!

    return retVal;
  }

  public Map(int m, int n) {
    state = new Item[m, n];
    M = m; N = n; X = 0; Y = 0;
    Score = 0; LambdasLeft = 0; LambdasCollected = 0;
    Water = 0; Flooding = 0; Waterproof = 10;
    State = RobotState.Mining;
    TurnsUnderWater = 0; TurnNumber = 0;
    LinkedOutPoints = new Dictionary<Item, KeyValuePair<Point, List<Point>>>();
  }

  public Map Clone() {
    var other = new Map(M, N);
    other.CopyFrom(this);
    Array.Copy(state, 0, other.state, 0, state.Length);
    return other;
  }

  private Map CopyFrom(Map other) {
    X = other.X; Y = other.Y;
    Score = other.Score; 
    LambdasLeft = other.LambdasLeft; LambdasCollected = other.LambdasCollected;
    Water = other.Water; Flooding = other.Flooding; Waterproof = other.Waterproof;
    State = other.State; 
    TurnsUnderWater = other.TurnsUnderWater; TurnNumber = other.TurnNumber;

    foreach(var record in other.LinkedOutPoints) {
      LinkedOutPoints[record.Key] = record.Value;
    }

    return this;
  }

  public RobotState State { get; private set; }
  public int LambdasCollected { get; private set; }
  public int LambdasLeft { get; private set; }
  public int Score { get; private set; }
  public int M { get; private set; }
  public int N { get; private set; }

  public int X { get; private set; }
  public int Y { get; private set; }

  public int Water { get; private set; }
  public int Flooding { get; private set; }
  public int Waterproof { get; private set; }
  public int TurnsUnderWater {get; private set; }
  public int TurnNumber {get; private set; }

  public Dictionary<Item, KeyValuePair<Point, List<Point>>> LinkedOutPoints { get; private set; }
  // public Dictionary<Item, Point> Out { get; private set; }

  public Item this[int x, int y] {
    get {
      return this.state[y,x];
    }
    set {
      this.state[y,x] = value;
    }
  }

  public Item this[Point p] {
    get {
      return this[p.X, p.Y];
    }
    set {
      this[p.X, p.Y] = value;
    }
  }

  public override string ToString() {
    var sb = new StringBuilder();
    sb.AppendFormat("Map ({0} x {1}), robot position: ({2} x {3}), robot state: {4}, score = {5}\n", M, N, X, Y, State, Score);
    for(var i = M - 1; i >= 0; i--) {
      for(var j = 0; j < N; j++) {
        sb.Append((char)(state[i,j]));
      }
      sb.AppendLine();
    }
    sb.AppendFormat("Water: {0}, Flooding: {1}, Waterproof: {2}", Water, Flooding, Waterproof);
    return sb.ToString();
  }
  
  private Map Rotate() {
    return (Map.first == this) ? Map.second : Map.first;
  }

  public Map Execute(string cmdSeq) {
    var tmp = this;

    foreach(var cmdCh in cmdSeq) {
      tmp = tmp.Execute(cmdCh);
      // Console.Error.WriteLine("State: {0}", tmp.ToString());
    }
    return tmp;
  }

  public Map Execute(char cmdCh) {
    return Execute((Command)cmdCh);
  }

  public Map Execute(Command cmd) {
    if(State != RobotState.Mining) {
      return this;
    }

    var next = this.Rotate();

    if(cmd == Command.Abort) {
      Abort(); return this;
    }

    var nX = X;
    var nY = Y;

    switch(cmd) {
      case Command.Left: nX--; break;
      case Command.Right: nX++; break;
      case Command.Up: nY++; break;
      case Command.Down: nY--; break;
    }

    this.Score--; // Reducing score.

    // Can't move out of map boundaries
    if((nX < 0) || (nX >= this.N)) {
      cmd = Command.Wait;
    }
    if((nY < 0) || (nY >= this.M)) {
      cmd = Command.Wait;
    }
    
    var nItem = this[nX,nY];
    if((nItem == Item.Empty)
       || (nItem == Item.Earth)
       || (nItem == Item.Lambda)
       || (nItem == Item.OpenLift)) {
      
      if(nItem == Item.Lambda) {
        this.CollectLambda();
      }

      if(nItem == Item.OpenLift) {
        this.EnterLift();
      }
      
      this.Move(nX, nY);
    }
    else if(nItem == Item.Rock) {
      if((cmd == Command.Right) && ((nX + 1) < this.N) && (this[nX + 1, nY] == Item.Empty)) {
        // Console.Error.WriteLine("Moving Rock right");
        this.Move(nX, nY);
        this[nX + 1, nY] = Item.Rock;
      }
      else if((cmd == Command.Left) && ((nX - 1) >= 0) && (this[nX - 1, nY] == Item.Empty)) {
        // Console.Error.WriteLine("Moving Rock left");
        this.Move(nX, nY);
        this[nX - 1, nY] = Item.Rock;
      }
    }
    else if(nItem.IsIn()) {
      var teleportData = this.LinkedOutPoints[nItem];
      
      foreach(var item in teleportData.Value) {
        this[item] = Item.Empty;
      }
      this.Move(teleportData.Key);
    }

    // Underwater checks
    if(Y <= Water) {
      TurnsUnderWater++;
    } else {
      TurnsUnderWater = 0;
    }

    TurnNumber++;
    
    return next.Update(this);
  }

  private Map Update(Map old) {
    CopyFrom(old);

    // Map update loop
    for(int i = 0; i < this.M; i++) {
      for(int j = 0; j < this.N; j++) {
        var x = j; var y = i;
        
        // Console.Error.WriteLine("{0}x{1} Processing ({2};{3})", M, N, x, y);
        if((old[x, y] == Item.Rock) && ((y - 1) >= 0) && (old[x,y - 1] == Item.Empty)) {
          this[x,y] = Item.Empty;
          this[x,y - 1] = Item.Rock;
        } else if((old[x, y] == Item.Rock) && ((y-1) >= 0) && (x+1 < old.N) && (old[x, y-1] == Item.Rock) && (old[x+1,y] == Item.Empty) && (old[x+1,y-1] == Item.Empty)) {
          this[x,y] = Item.Empty;
          this[x+1, y-1] = Item.Rock;
        } else if((old[x, y] == Item.Rock) && ((y-1) >= 0) && (x-1 >= 0) && ((y+1) < old.M) && ((x+1) < old.N) && (old[x, y-1] == Item.Rock) && ((old[x+1,y] != Item.Empty) || (old[x+1,y-1] != Item.Empty)) && (old[x-1,y] == Item.Empty) && (old[x-1,y-1] == Item.Empty)) {
          this[x,y] = Item.Empty;
          this[x-1, y-1] = Item.Rock;
        } else if((old[x, y] == Item.Rock) && ((y-1) >= 0) && (old[x, y-1] == Item.Lambda) && ((x+1) < old.N) && (old[x+1,y] == Item.Empty) && (old[x+1,y-1] == Item.Empty)) {
          this[x,y] = Item.Empty;
          this[x+1, y-1] = Item.Rock;
        } else if((old[x,y] == Item.ClosedLift) && (old.LambdasLeft == 0)) {
          this[x,y] = Item.OpenLift;
        } else {
          this[x,y] = old[x,y];
        }
      }
    }

    // Check 'Death' conditions
    if(((old.Y+1) < old.M) && (old[X, Y+1] != Item.Rock) && (this[X, Y+1] == Item.Rock)) {
      this.State = RobotState.Smashed;
    }
    if(old.TurnsUnderWater > old.Waterproof) {
      this.State = RobotState.Sank; // Она утонула. (с) ВВП
    }

    // Rise water!
    if((old.Flooding != 0) && (TurnNumber % Flooding == 0)) {
      Water++;
    }

    // Console.Error.WriteLine("Turn done. Score: {0}", this.Score);
    return this;
  }

  private void Move(Point to) {
    this[X,Y] = Item.Empty;
    this[to.X,to.Y] = Item.Robot;
    X = to.X; Y = to.Y;
  }

  private void Move(int nX, int nY) {
    this[X,Y] = Item.Empty;
    this[nX,nY] = Item.Robot;
    X = nX; Y = nY;
  }

  private void CollectLambda() {
    LambdasCollected++; LambdasLeft--;
    Score += 25;
  }

  private void EnterLift() {
    Score += (50 * LambdasCollected);
    State = RobotState.Finish;
    // Console.Error.WriteLine("Lift entered!");
  }

  private void Abort() {
    Score += (25 * LambdasCollected);
    State = RobotState.Abort;
    // Console.Error.WriteLine("Aborted!");
  }
}

}
