module Engine.Calculation

open Domain
open System

let Calculate (inputs : Inputs) =
    let negate originalValue currentValue =
        match originalValue < 0.0 with
        | true  -> currentValue * -1.0
        | false -> currentValue

    let round (precision : int) number =
        (floor ((abs (float number) + 0.5 / 10.0 ** precision) * 10.0 ** precision))
        / 10.0 ** precision
        |> negate number

    let roundToSignificantFigures (precision : int) number =
        match number = 0.0 with
        | true ->
            number
        | false ->
            let scale = 10.0 ** floor (log10 (abs number) + 1.0)
            scale * round precision (number / scale)

    let funds = inputs.Funds |>  Array.ofSeq
    let investmentPercentages = inputs.InvestmentPercentages |> Array.ofSeq |> Dictionary

    funds |> Array.Parallel.map (fun fund ->
        let now = DateTime.UtcNow
        let future = now.AddYears(10)

        let startMonth = now.Year * 12 + now.Month
        let endMonth = future.Year * 12 + future.Month
        let months = Array.init (endMonth - startMonth) (fun index -> index + startMonth)
        months |> (investmentPercentages[fund.Id] / 100.0M * inputs.Investment |> Array.fold (fun fundValue totalMonth ->
            let month =
                let value = totalMonth % 12
                match value with
                | 0 -> 12
                | _ -> value
            let year = (totalMonth - month) / 12
            let days = DateTime.DaysInMonth(year, month)
            fundValue * decimal ((1.0 + float fund.GrowthRate) ** (float days / 365.25)) * decimal ((1.0 - float fund.Charge) ** (float days / 365.25)))))
    |> Array.sum
    |> float
    |> roundToSignificantFigures 3
    |> decimal
