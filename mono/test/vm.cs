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
    Console.Error.WriteLine("Final state: {0}", finState);
    Console.Error.WriteLine("Result: {0}: got: {1}, expected: {2}, lambdas collected: {3}", 
                            (finState.Score == expectedScore) ? "PASS" : "FAIL",
                            finState.Score, expectedScore, finState.LambdasCollected);
    return (finState.Score == expectedScore) ? 0 : -1;
  }

}
}
