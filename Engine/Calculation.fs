module Engine.Calculation

open Domain

let Calculate (inputs : Inputs) =
    let negate originalValue currentValue =
        match originalValue < 0.0 with
        | true -> currentValue * -1.0
        | false -> currentValue

    let round (precision : int) number =
        (floor ((abs (float number) + 0.5 / 10.0 ** precision) * 10.0 ** precision))
        / 10.0 ** precision
        |> negate number

    let funds = inputs.Funds |> List.ofSeq
    let investmentPercentages = inputs.InvestmentPercentages |> List.ofSeq |> Dictionary

    funds
    |> List.map (fun fund ->
        let investmentPercentage = investmentPercentages[fund.Id]
        investmentPercentage * inputs.Investment * (1.0M + fund.GrowthRate) * (1.0M - fund.Charge))
    |> List.sum
    |> float
    |> round 2
    |> decimal