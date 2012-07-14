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

  public static Map Read(TextReader src) {
    var lineMap = new List<string>();
    var m = 0;
    var n = 0;

    while(src.Peek() >= 0) {
      var nextLine = src.ReadLine();
      n = Math.Max(n, nextLine.Length);
      lineMap.Add(nextLine);
      m++;
    }
      
    var retVal = new Map {
      state = new Item[m, n],
      M = m,
      N = n,
    };
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

    System.GC.Collect(); // SPIKE!

    return retVal;
  }

  public int M { get; private set; }
  public int N { get; private set; }

  public Item this[int x, int y] {
    get {
      return this.state[x,y];
    }
    set {
      this.state[x,y] = value;
    }
  }
}

}
