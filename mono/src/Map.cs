// -*- mode: csharp; indent-tabs-mode: nil; c-basic-offset: 2; -*-

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ICFPC {

public enum Item {
  Robot,
  Empty,
  Earth,
  Wall,
  Rock,
  Lambda,
  ClosedLift,
  OpenLift,
}

public class Map {
  private Item[,] state = null;

  public static Item Char2Item(char ch) {
    switch(ch) {
      case 'R': return Item.Robot;
      case ' ': return Item.Empty;
      case '.': return Item.Earth;
      case '#': return Item.Wall;
      case '*': return Item.Rock;
      case '\\': return Item.Lambda;
      case 'L': return Item.ClosedLift;
      case 'O': return Item.OpenLift;
      default:
        throw new Exception(String.Format("Bad map character: '{0'}", ch));
    }
  }

  public static char Item2Char(Item it) {
    switch(it) {
      case Item.Robot: return 'R';
      case Item.Empty: return ' ';
      case Item.Earth: return '.';
      case Item.Wall: return '#';
      case Item.Rock: return '*';
      case Item.Lambda: return '\\';
      case Item.ClosedLift: return 'L';
      case Item.OpenLift: return 'O';
      default:
        throw new Exception(String.Format("Bad item: '{0'}", it));
    }
  }

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
        retVal.state[i,j] = Char2Item(line[j]);
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

  public int Water { get; private set; }
  public int Flooding { get; private set; }
  public int Waterproof { get; private set; }

  public Item this[int x, int y] {
    get {
      return this.state[x,y];
    }
    set {
      this.state[x,y] = value;
    }
  }

  public override string ToString() {
    var sb = new StringBuilder();
    sb.AppendFormat("Map ({0} x {1})\n", M, N);
    for(var i = M - 1; i >= 0; i--) {
      for(var j = 0; j < N; j++) {
        sb.Append(Item2Char(state[i,j]));
      }
      sb.AppendLine();
    }
    sb.AppendFormat("Water: {0}, Flooding: {1}, Waterproof: {2}", Water, Flooding, Waterproof);
    return sb.ToString();
  }
}

}
