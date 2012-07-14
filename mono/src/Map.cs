// -*- mode: csharp; indent-tabs-mode: nil; c-basic-offset: 2; -*-

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
}

public enum Command {
  Left = 'L',
  Right = 'R',
  Up = 'U',
  Down = 'D',
  Wait = 'W',
  Abort = 'A',
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

    for(var i = lineMap.Count - 1; i >= 0; i--) {
      var line = lineMap[i];
      for(var j = 0; j < line.Length; j++) {
        var x = j; var y = lineMap.Count - 1 - i;

        retVal[x,y] = (Item)(line[j]);
        if(retVal[x,y] == Item.Robot) {
          retVal.X = x;
          retVal.Y = y;
        }
        else if(retVal[x,y] == Item.Lambda) {
          retVal.LambdasLeft++;
        }
      }
    }

    // Now read the metadata
    while(src.Peek() >= 0) {
      var dataStr = src.ReadLine();
      var data = dataStr.Split(' ', '\t');
      if(data[0].StartsWith("Water")) {
        // Console.Error.WriteLine("Water tag found!");
        retVal.Water = int.Parse(data[1]);
        continue;
      }
      else if(data[0].StartsWith("Flooding")) {
        // Console.Error.WriteLine("Flooding tag found!");
        retVal.Flooding = int.Parse(data[1]);
        continue;
      }
      else if(data[0].StartsWith("Waterproof")) {
        // Console.Error.WriteLine("Waterproof tag found!");
        retVal.Waterproof = int.Parse(data[1]);
        continue;
      };
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
    return this;
  }

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

  public Item this[int x, int y] {
    get {
      return this.state[y,x];
    }
    set {
      this.state[y,x] = value;
    }
  }

  public override string ToString() {
    var sb = new StringBuilder();
    sb.AppendFormat("Map ({0} x {1}), robot position: ({2} x {3}), score = {4}\n", M, N, X, Y, Score);
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
        } else if((old[x, y] == Item.Rock) && ((y-1) >= 0) && (x+1 < old.N) && (old[x, y-1] == Item.Lambda) && (old[x+1,y] == Item.Empty) && (old[x+1,y-1] == Item.Empty)) {
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

    // Console.Error.WriteLine("Turn done. Score: {0}", this.Score);
    return this;
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
    // Console.Error.WriteLine("Lift entered!");
  }

  private void Abort() {
    Score += (25 * LambdasCollected);
    // Console.Error.WriteLine("Aborted!");
  }
}

}
