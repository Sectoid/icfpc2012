// -*- mode: csharp; indent-tabs-mode: nil; c-basic-offset: 2; -*-

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ICFPC {
public static class Util {
  public static long GetObjectSize(this object obj) {
    using (var s = new MemoryStream()) {
      var formatter = new BinaryFormatter();
      formatter.Serialize(s, obj);
      return s.Length;
    }  
  }

  public static bool IsIn(this Item obj) {
    return 
      (obj == Item.InA)
      || (obj == Item.InB)
      || (obj == Item.InC)
      || (obj == Item.InD)
      || (obj == Item.InE)
      || (obj == Item.InF)
      || (obj == Item.InG)
      || (obj == Item.InH)
      || (obj == Item.InI);
  }

  public static bool IsOut(this Item obj) {
    return 
      (obj == Item.Out1)
      || (obj == Item.Out2)
      || (obj == Item.Out3)
      || (obj == Item.Out4)
      || (obj == Item.Out5)
      || (obj == Item.Out6)
      || (obj == Item.Out7)
      || (obj == Item.Out8)
      || (obj == Item.Out9);
  }
}

public struct Point {
  public Point(int x, int y) { X = x; Y = y; }
  public int X;
  public int Y;

  public override bool Equals(object other) {
    return (other is Point) && ((Point)other == this);
  } 

  public override int GetHashCode() {
    return X + Y;
  } 

  public static bool operator==(Point l, Point r) {
    return (l.X == r.X) && (l.Y == r.Y); 
  }

  public static bool operator!=(Point l, Point r) {
    return (l.X != r.X) || (l.Y != r.Y); 
  }
}

}
