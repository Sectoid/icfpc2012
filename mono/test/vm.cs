// -*- mode: csharp; indent-tabs-mode: nil; c-basic-offset: 2; -*-

using System;
using System.Collections.Generic;
using System.IO;

namespace ICFPC {
public class VMTest {
  public static int Main(string[] argv) {
    var map = Map.Read(Console.In);
    
    var cmdSeq = argv[0];
    var expectedScore = int.Parse(argv[1]);
    var finState = map.Execute(cmdSeq);
    Console.Error.WriteLine("Result: {0}, expected: {1}, lambdas collected: {2}", finState.Score, expectedScore, finState.LambdasCollected);
    return 0;
  }

}
}
