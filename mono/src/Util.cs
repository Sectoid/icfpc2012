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
}

}
