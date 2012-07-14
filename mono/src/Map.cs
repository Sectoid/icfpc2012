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
        retVal.state[i,j] = (Item)(line[j]);
        if(retVal.state[i,j] == Item.Robot) {
          retVal.X = j;
          retVal.Y = i;
        };
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
    

    System.GC.Collect(); // SPIKE!

    return retVal;
  }

  public Map(int m, int n) {
    state = new Item[m, n];
    M = m; N = n;
    Water = 0; Flooding = 0; Waterproof = 10;
  }

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
    sb.AppendFormat("Map ({0} x {1}), robot position: ({2} x {3})\n", M, N, X, Y);
    for(var i = M - 1; i >= 0; i--) {
      for(var j = 0; j < N; j++) {
        sb.Append((char)(state[i,j]));
      }
      sb.AppendLine();
    }
    sb.AppendFormat("Water: {0}, Flooding: {1}, Waterproof: {2}", Water, Flooding, Waterproof);
    return sb.ToString();
  }
}

}
