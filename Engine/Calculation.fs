module Engine.Calculation

open Domain

let Calculate (inputs : Inputs) =
    let negate originalValue currentValue =
        match originalValue < 0.0 with
        | true  -> currentValue * -1.0
        | false -> currentValue

    let round (precision : int) number =
        (floor ((abs (float number) + 0.5 / 10.0 ** precision) * 10.0 ** precision))
        / 10.0 ** precision
        |> negate number

    let funds = inputs.Funds |>  Array.ofSeq
    let investmentPercentages = inputs.InvestmentPercentages |> Array.ofSeq |> Dictionary

    funds
    |> Array.Parallel.map (fun fund ->
        investmentPercentages[fund.Id] * inputs.Investment * (1.0M + fund.GrowthRate) * (1.0M - fund.Charge))
    |> Array.sum
    |> float
    |> round 2
    |> decimal
