// -*- mode: csharp; indent-tabs-mode: nil; c-basic-offset: 2; -*-

using System;

namespace ICFPC {
public static class Miner {

  public static int Main(string[] argv) {
    Console.Error.WriteLine("Loading map...");

    var map = Map.Read(Console.In);

    Console.Error.WriteLine("Map is loaded. Size is: ({0} x {1})", map.M, map.N);

    Console.WriteLine("A"); // Only abort for now ;)

    return 0;
  }
}

}