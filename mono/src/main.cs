// -*- mode: csharp; indent-tabs-mode: nil; c-basic-offset: 2; -*-

using System;

namespace ICFPC {
public static class Miner {

  public static int Main(string[] argv) {
    Console.Error.WriteLine("Loading map...");

    var map = Map.Read(Console.In);

    Console.Error.WriteLine("Map is loaded: {0}, map takes {1} bytes in memory", map, map.GetObjectSize());

    Console.WriteLine("A"); // Only abort for now ;)

    return 0;
  }
}

}