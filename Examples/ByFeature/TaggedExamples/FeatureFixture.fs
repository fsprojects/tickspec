﻿module TickSpec.NUnit

open System.Reflection
open NUnit.Framework
open TickSpec

/// Inherit from FeatureFixture to define a feature fixture
[<TestFixture>]
type FeatureFixture () =
    [<Test>]
    [<TestCaseSource("Scenarios")>]
    member this.TestScenario (scenario:Scenario) =
        if scenario.Tags |> Seq.exists ((=) "ignore") then
            raise (IgnoreException("Ignored: " + scenario.Name))
        scenario.Action.Invoke()

    static member Scenarios =
        let createTestCaseData (feature:Feature) (scenario:Scenario) =
            let testCaseData =
                (new TestCaseData(scenario))
                    .SetName(scenario.Name)
                    .SetProperty("Feature", feature.Name.Substring("Feature: ".Length))

            scenario.Tags |> Seq.fold (fun (data:TestCaseData) (tag:string) -> data.SetProperty("Tag", tag)) testCaseData

        let createFeatureData (feature:Feature) =
            feature.Scenarios
            |> Seq.map (createTestCaseData feature)

        let assembly = typeof<FeatureFixture>.Assembly
        let definitions = new StepDefinitions(assembly)
        [
            "WebTesting.feature" ;
            "HttpServer.feature" ;
        ]
        |> Seq.collect ( fun source ->
            let featureStream = assembly.GetManifestResourceStream(source)
            let feature = definitions.GenerateFeature(source, featureStream)
            createFeatureData feature)